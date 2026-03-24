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

    public async Task<PageLease> AcquirePageAsync()
    {
        if (!await _semaphore.WaitAsync(TimeSpan.FromSeconds(120)))
        {
            throw new TimeoutException("Timed out waiting for browser from pool");
        }
        try
        {
            var browser = await GetHealthyBrowserAsync();
            var page = await browser.NewPageAsync();
            return new PageLease(page, browser, this);
        }
        catch
        {
            _semaphore.Release();
            throw;
        }
    }

    /// <summary>
    /// Returns a healthy browser to the pool and releases the semaphore.
    /// </summary>
    internal void ReturnBrowser(IBrowser browser)
    {
        if (browser.IsConnected)
        {
            _pool.Enqueue(browser);
        }
        else
        {
            try { browser.Dispose(); } catch { }
        }
        _semaphore.Release();
    }

    /// <summary>
    /// Kills a browser (don't reuse) and releases the semaphore.
    /// Used when Chrome is hung and can't be trusted.
    /// Kills the OS process directly since Dispose() also hangs on stuck Chrome.
    /// </summary>
    internal void KillBrowser(IBrowser browser)
    {
        try
        {
            var process = browser.Process;
            if (process != null && !process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch { }
        try { browser.Dispose(); } catch { }
        _semaphore.Release();
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
                "--disable-extensions",
                "--disable-background-networking",
                "--no-first-run",
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
/// RAII wrapper for a page. On normal dispose, returns browser to pool.
/// On timeout, kills the browser so it's not reused in a broken state.
/// </summary>
public sealed class PageLease : IAsyncDisposable
{
    public IPage Page { get; }
    private readonly IBrowser _browser;
    private readonly BrowserPool _pool;
    private int _disposed;
    private bool _timedOut;

    public PageLease(IPage page, IBrowser browser, BrowserPool pool)
    {
        Page = page;
        _browser = browser;
        _pool = pool;
    }

    /// <summary>
    /// Mark this lease as timed out — browser will be killed instead of reused.
    /// </summary>
    public void MarkTimedOut()
    {
        _timedOut = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            if (_timedOut)
            {
                // Chrome is hung — kill the browser process, don't try to close gracefully
                try { Page.Dispose(); } catch { }
                _pool.KillBrowser(_browser);
            }
            else
            {
                // Normal path — close page and return browser to pool
                try
                {
                    var closeTask = Page.CloseAsync();
                    if (await Task.WhenAny(closeTask, Task.Delay(5000)) != closeTask)
                    {
                        // CloseAsync hung too — kill browser
                        try { Page.Dispose(); } catch { }
                        _pool.KillBrowser(_browser);
                        return;
                    }
                }
                catch { }
                try { Page.Dispose(); } catch { }
                _pool.ReturnBrowser(_browser);
            }
        }
    }
}
