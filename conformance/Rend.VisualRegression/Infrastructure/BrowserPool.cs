using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace Rend.VisualRegression.Infrastructure;

/// <summary>
/// Pools Chrome browser processes for reuse across parallel test workers.
/// Each worker acquires a dedicated browser, uses it for one test, then returns it.
/// </summary>
public sealed class BrowserPool : IAsyncDisposable
{
    private readonly ConcurrentQueue<IBrowser> _pool = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly string _chromeExePath;

    public BrowserPool(string chromeExePath, int maxConcurrent)
    {
        _chromeExePath = chromeExePath;
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    /// <summary>
    /// Acquires a browser and creates a fresh page on it.
    /// The page is closed and the browser returned to the pool on dispose.
    /// </summary>
    public async Task<PageLease> AcquirePageAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            var browser = await GetHealthyBrowserAsync();
            var page = await browser.NewPageAsync();
            return new PageLease(page, browser, b =>
            {
                if (b.IsConnected)
                {
                    _pool.Enqueue(b);
                }
                else
                {
                    try { b.Dispose(); } catch { }
                }
                _semaphore.Release();
            });
        }
        catch
        {
            _semaphore.Release();
            throw;
        }
    }

    private async Task<IBrowser> GetHealthyBrowserAsync()
    {
        while (_pool.TryDequeue(out var browser))
        {
            if (browser.IsConnected)
            {
                return browser;
            }
            try { browser.Dispose(); } catch { }
        }
        return await LaunchAsync();
    }

    private Task<IBrowser> LaunchAsync()
    {
        return Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            ExecutablePath = _chromeExePath,
            Args = new[] {
                "--no-sandbox", "--disable-gpu", "--disable-dev-shm-usage",
                "--disable-lcd-text",
            },
        });
    }

    public async ValueTask DisposeAsync()
    {
        while (_pool.TryDequeue(out var browser))
        {
            try { await browser.CloseAsync(); browser.Dispose(); } catch { }
        }
        _semaphore.Dispose();
    }
}

/// <summary>
/// RAII wrapper for a page. Closes the page and returns the browser to the pool on dispose.
/// </summary>
public sealed class PageLease : IAsyncDisposable
{
    public IPage Page { get; }
    private readonly IBrowser _browser;
    private readonly Action<IBrowser> _release;
    private int _disposed;

    public PageLease(IPage page, IBrowser browser, Action<IBrowser> release)
    {
        Page = page;
        _browser = browser;
        _release = release;
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            try { await Page.CloseAsync(); } catch { }
            try { Page.Dispose(); } catch { }
            _release(_browser);
        }
    }
}
