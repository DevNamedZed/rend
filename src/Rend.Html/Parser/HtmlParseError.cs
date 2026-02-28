namespace Rend.Html.Parser
{
    /// <summary>
    /// Represents an HTML parse error with location information.
    /// </summary>
    public readonly struct HtmlParseError
    {
        /// <summary>A short description of the error.</summary>
        public readonly string Message;

        /// <summary>One-based line number.</summary>
        public readonly int Line;

        /// <summary>One-based column number.</summary>
        public readonly int Column;

        public HtmlParseError(string message, int line, int column)
        {
            Message = message;
            Line = line;
            Column = column;
        }

        public override string ToString() => $"({Line}:{Column}) {Message}";
    }
}
