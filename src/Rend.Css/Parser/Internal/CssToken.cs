namespace Rend.Css.Parser.Internal
{
    /// <summary>
    /// A CSS token produced by the tokenizer. Mutable struct reused between reads.
    /// </summary>
    internal struct CssToken
    {
        /// <summary>The token type.</summary>
        public CssTokenType Type;

        /// <summary>
        /// The string value of the token.
        /// For Ident: the identifier value. For Hash: the hash value (without '#').
        /// For String: the string content (without quotes). For Url: the URL value.
        /// For Number/Percentage/Dimension: the numeric representation string.
        /// For AtKeyword: the keyword (without '@'). For Function: the name (without '(').
        /// For Delim: the delimiter character as a string.
        /// </summary>
        public string Value;

        /// <summary>
        /// The numeric value for Number, Percentage, and Dimension tokens.
        /// </summary>
        public float NumericValue;

        /// <summary>
        /// The unit string for Dimension tokens (e.g. "px", "em", "%").
        /// </summary>
        public string? Unit;

        /// <summary>
        /// For Hash tokens: true if the hash is an ID type (starts with ident), false if unrestricted.
        /// For Number/Dimension/Percentage: true if the value was an integer.
        /// </summary>
        public bool Flag;

        /// <summary>Reset the token for reuse.</summary>
        public void Reset()
        {
            Type = CssTokenType.EOF;
            Value = "";
            NumericValue = 0;
            Unit = null;
            Flag = false;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case CssTokenType.Ident: return $"Ident({Value})";
                case CssTokenType.Function: return $"Function({Value})";
                case CssTokenType.AtKeyword: return $"AtKeyword({Value})";
                case CssTokenType.Hash: return $"Hash(#{Value})";
                case CssTokenType.String: return $"String(\"{Value}\")";
                case CssTokenType.Url: return $"Url({Value})";
                case CssTokenType.Number: return $"Number({NumericValue})";
                case CssTokenType.Percentage: return $"Percentage({NumericValue}%)";
                case CssTokenType.Dimension: return $"Dimension({NumericValue}{Unit})";
                case CssTokenType.Delim: return $"Delim({Value})";
                case CssTokenType.Whitespace: return "Whitespace";
                case CssTokenType.Colon: return "Colon";
                case CssTokenType.Semicolon: return "Semicolon";
                case CssTokenType.Comma: return "Comma";
                case CssTokenType.LeftBrace: return "LeftBrace";
                case CssTokenType.RightBrace: return "RightBrace";
                case CssTokenType.LeftParen: return "LeftParen";
                case CssTokenType.RightParen: return "RightParen";
                case CssTokenType.LeftBracket: return "LeftBracket";
                case CssTokenType.RightBracket: return "RightBracket";
                case CssTokenType.CDO: return "CDO";
                case CssTokenType.CDC: return "CDC";
                case CssTokenType.EOF: return "EOF";
                default: return Type.ToString();
            }
        }
    }
}
