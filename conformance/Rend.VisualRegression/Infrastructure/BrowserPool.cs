using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace Rend.VisualRegression.Infrastructure;

public sealed class BrowserPool : IAsyncDisposable
{
    private readonly ConcurrentQueue<IBrowser> _pool = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly string _chromeExePath;
    private readonly int _maxBrowsers;

    public BrowserPool(string chromeExePath, int maxBrowsers)
    {
        _chromeExePath = chromeExePath;
        _maxBrowsers = maxBrowsers;
        _semaphore = new SemaphoreSlim(maxBrowsers, maxBrowsers);
    }

    public async Task<BrowserLease> AcquireAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            var browser = await GetHealthyBrowserAsync();
            return new BrowserLease(browser, Release);
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
                return browser;
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

    private void Release(IBrowser browser)
    {
        if (browser.IsConnected)
            _pool.Enqueue(browser);
        else
            try { browser.Dispose(); } catch { }
        _semaphore.Release();
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

public sealed class BrowserLease : IAsyncDisposable
{
    public IBrowser Browser { get; }
    private readonly Action<IBrowser> _release;
    private int _disposed;

    public BrowserLease(IBrowser browser, Action<IBrowser> release)
    {
        Browser = browser;
        _release = release;
    }

    public ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
            _release(Browser);
        return ValueTask.CompletedTask;
    }
}
