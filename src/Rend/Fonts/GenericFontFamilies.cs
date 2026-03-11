using System;
using System.Collections.Generic;

namespace Rend.Fonts
{
    /// <summary>
    /// Shared generic font family fallback lists.
    /// </summary>
    internal static class GenericFontFamilies
    {
        internal static readonly Dictionary<string, string[]> FallbackMap = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["sans-serif"] = new[] { "Helvetica", "Helvetica Neue", "Arial", "Segoe UI", "DejaVu Sans", "Liberation Sans", "FreeSans", "Noto Sans" },
            ["serif"] = new[] { "Times New Roman", "Times", "Georgia", "DejaVu Serif", "Liberation Serif", "FreeSerif", "Noto Serif" },
            ["monospace"] = new[] { "Consolas", "Courier New", "Courier", "Menlo", "DejaVu Sans Mono", "Liberation Mono", "FreeMono", "Noto Sans Mono" },
            ["cursive"] = new[] { "Comic Sans MS", "Apple Chancery", "Snell Roundhand" },
            ["fantasy"] = new[] { "Impact", "Papyrus" },
            ["system-ui"] = new[] { ".AppleSystemUIFont", "Segoe UI", "Roboto", "Helvetica Neue", "Helvetica", "Arial" },
            ["ui-sans-serif"] = new[] { ".AppleSystemUIFont", "Segoe UI", "Roboto", "Helvetica Neue", "Helvetica", "Arial" },
            ["ui-serif"] = new[] { "New York", "Georgia", "Times New Roman" },
            ["ui-monospace"] = new[] { "SF Mono", "Menlo", "Consolas", "Courier New" },
        };
    }
}
