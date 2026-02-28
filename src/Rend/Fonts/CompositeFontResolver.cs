using System;
using System.Collections.Generic;

namespace Rend.Fonts
{
    /// <summary>
    /// Chains multiple font path resolvers, yielding results from each in order.
    /// </summary>
    public sealed class CompositeFontResolver
    {
        private readonly List<Func<IEnumerable<string>>> _resolvers;

        /// <summary>
        /// Creates a new empty composite resolver.
        /// </summary>
        public CompositeFontResolver()
        {
            _resolvers = new List<Func<IEnumerable<string>>>();
        }

        /// <summary>
        /// Adds a <see cref="SystemFontResolver"/> to the chain.
        /// </summary>
        public CompositeFontResolver AddSystem()
        {
            var resolver = new SystemFontResolver();
            _resolvers.Add(resolver.GetFontPaths);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="DirectoryFontResolver"/> to the chain.
        /// </summary>
        public CompositeFontResolver AddDirectory(string directoryPath)
        {
            var resolver = new DirectoryFontResolver(directoryPath);
            _resolvers.Add(resolver.GetFontPaths);
            return this;
        }

        /// <summary>
        /// Adds a custom resolver function that returns font file paths.
        /// </summary>
        public CompositeFontResolver Add(Func<IEnumerable<string>> fontPathProvider)
        {
            if (fontPathProvider == null) throw new ArgumentNullException(nameof(fontPathProvider));
            _resolvers.Add(fontPathProvider);
            return this;
        }

        /// <summary>
        /// Returns all font file paths from all chained resolvers.
        /// </summary>
        public IEnumerable<string> GetFontPaths()
        {
            foreach (var resolver in _resolvers)
            {
                foreach (string path in resolver())
                {
                    yield return path;
                }
            }
        }
    }
}
