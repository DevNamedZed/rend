namespace Rend.Html.Parser
{
    /// <summary>
    /// Options for the HTML parser.
    /// </summary>
    public sealed class HtmlParserOptions
    {
        /// <summary>Maximum tree depth before the parser stops nesting. Default is 512.</summary>
        public int MaxTreeDepth { get; set; } = 512;

        /// <summary>Maximum number of attributes per element. Default is 512.</summary>
        public int MaxAttributes { get; set; } = 512;

        /// <summary>Whether scripting is enabled (affects noscript parsing). Default is false.</summary>
        public bool Scripting { get; set; }

        /// <summary>Default options.</summary>
        public static readonly HtmlParserOptions Default = new HtmlParserOptions();
    }
}
