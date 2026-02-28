using System;
using System.Collections.Generic;
using System.IO;

namespace Rend.Fonts
{
    /// <summary>
    /// Discovers font files from a specified directory.
    /// </summary>
    public sealed class DirectoryFontResolver
    {
        private static readonly string[] FontExtensions = { ".ttf", ".otf", ".woff", ".woff2" };

        private readonly string _directoryPath;

        /// <summary>
        /// Creates a new resolver that scans the given directory.
        /// </summary>
        public DirectoryFontResolver(string directoryPath)
        {
            _directoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
        }

        /// <summary>
        /// Returns all font file paths found in the directory (including subdirectories).
        /// </summary>
        public IEnumerable<string> GetFontPaths()
        {
            if (!Directory.Exists(_directoryPath))
                yield break;

            string[] files;
            try
            {
                files = Directory.GetFiles(_directoryPath, "*.*", SearchOption.AllDirectories);
            }
            catch (UnauthorizedAccessException)
            {
                yield break;
            }
            catch (IOException)
            {
                yield break;
            }

            foreach (string file in files)
            {
                string ext = Path.GetExtension(file);
                if (IsFontExtension(ext))
                    yield return file;
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
