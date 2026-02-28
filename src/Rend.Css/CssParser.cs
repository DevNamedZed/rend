using System;
using System.IO;
using System.Text;
using Rend.Css.Parser.Internal;

namespace Rend.Css
{
    /// <summary>
    /// Public API for parsing CSS stylesheets.
    /// </summary>
    public static class CssParser
    {
        /// <summary>Parse a CSS string into a Stylesheet.</summary>
        public static Stylesheet Parse(string css)
        {
            if (css == null) throw new ArgumentNullException(nameof(css));
            var parser = new CssParserCore(css);
            return parser.ParseStylesheet();
        }

        /// <summary>Parse a CSS string with options.</summary>
        public static Stylesheet Parse(string css, CssParserOptions options)
        {
            if (css == null) throw new ArgumentNullException(nameof(css));
            var parser = new CssParserCore(css);
            return parser.ParseStylesheet();
        }

        /// <summary>Parse CSS from a stream (UTF-8).</summary>
        public static Stylesheet Parse(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true))
            {
                var css = reader.ReadToEnd();
                return Parse(css);
            }
        }

        /// <summary>Parse CSS from a stream with options.</summary>
        public static Stylesheet Parse(Stream stream, CssParserOptions options)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true))
            {
                var css = reader.ReadToEnd();
                return Parse(css, options);
            }
        }
    }
}
