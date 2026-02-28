using System;
using System.IO;
using Rend.Html.Parser.Internal;

namespace Rend.Html.Parser
{
    /// <summary>
    /// HTML5 parser. Parses HTML strings or streams into a DOM Document.
    /// </summary>
    public static class HtmlParser
    {
        /// <summary>
        /// Parse an HTML string into a Document.
        /// </summary>
        public static Document Parse(string html)
        {
            return Parse(html, HtmlParserOptions.Default);
        }

        /// <summary>
        /// Parse an HTML string into a Document with options.
        /// </summary>
        public static Document Parse(string html, HtmlParserOptions options)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var document = new Document();
            var tokenizer = new HtmlTokenizer(html);
            var treeBuilder = new HtmlTreeBuilder(document, tokenizer);
            return treeBuilder.Run();
        }

        /// <summary>
        /// Parse HTML from a stream into a Document.
        /// Reads the entire stream into memory as a string.
        /// </summary>
        public static Document Parse(Stream stream)
        {
            return Parse(stream, HtmlParserOptions.Default);
        }

        /// <summary>
        /// Parse HTML from a stream into a Document with options.
        /// </summary>
        public static Document Parse(Stream stream, HtmlParserOptions options)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true))
            {
                var html = reader.ReadToEnd();
                return Parse(html, options);
            }
        }

        /// <summary>
        /// Parse an HTML fragment in the context of a given element.
        /// Returns a DocumentFragment containing the parsed nodes.
        /// </summary>
        public static DocumentFragment ParseFragment(string html, Element? contextElement = null)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));

            // For fragment parsing, we parse as a full document and then
            // extract the body's children into a DocumentFragment.
            // This is a simplified approach; full fragment parsing per spec
            // would use the context element to determine initial state.
            var document = Parse(html);
            var fragment = document.CreateDocumentFragment();

            var body = document.Body;
            if (body != null)
            {
                while (body.FirstChild != null)
                    fragment.AppendChild(body.FirstChild);
            }

            return fragment;
        }
    }
}
