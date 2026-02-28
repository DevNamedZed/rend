using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Rend.Fonts
{
    /// <summary>
    /// Discovers font files from well-known system directories based on the current platform.
    /// </summary>
    public sealed class SystemFontResolver
    {
        private static readonly string[] FontExtensions = { ".ttf", ".otf", ".woff", ".woff2" };

        /// <summary>
        /// Returns all font file paths found in platform-specific system font directories.
        /// </summary>
        public IEnumerable<string> GetFontPaths()
        {
            foreach (string directory in GetSystemFontDirectories())
            {
                if (!Directory.Exists(directory))
                    continue;

                string[] files;
                try
                {
                    files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                catch (IOException)
                {
                    continue;
                }

                foreach (string file in files)
                {
                    string ext = Path.GetExtension(file);
                    if (IsFontExtension(ext))
                        yield return file;
                }
            }
        }

        private static IEnumerable<string> GetSystemFontDirectories()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                yield return "/System/Library/Fonts";
                yield return "/Library/Fonts";
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!string.IsNullOrEmpty(home))
                    yield return Path.Combine(home, "Library", "Fonts");
            }
            else
            {
                // Linux / other Unix.
                yield return "/usr/share/fonts";
                yield return "/usr/local/share/fonts";
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!string.IsNullOrEmpty(home))
                    yield return Path.Combine(home, ".fonts");
            }
        }

        private static bool IsFontExtension(string extension)
        {
            for (int i = 0; i < FontExtensions.Length; i++)
            {
                if (string.Equals(extension, FontExtensions[i], StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
