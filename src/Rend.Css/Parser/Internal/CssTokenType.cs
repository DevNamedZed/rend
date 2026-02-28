namespace Rend.Css.Parser.Internal
{
    /// <summary>
    /// CSS token types per CSS Syntax Module Level 3, §4 Tokenization.
    /// </summary>
    internal enum CssTokenType : byte
    {
        /// <summary>An identifier (e.g. "color", "auto").</summary>
        Ident,

        /// <summary>A function token: identifier followed by '(' (e.g. "rgb(").</summary>
        Function,

        /// <summary>An at-keyword (e.g. "@media").</summary>
        AtKeyword,

        /// <summary>A hash token (e.g. "#fff", "#main"). May be id or unrestricted type.</summary>
        Hash,

        /// <summary>A quoted string (e.g. "hello", 'world').</summary>
        String,

        /// <summary>A bad string (unclosed).</summary>
        BadString,

        /// <summary>A url() token (unquoted URL).</summary>
        Url,

        /// <summary>A bad url() token (contains invalid characters).</summary>
        BadUrl,

        /// <summary>A single delimiter character (e.g. '.', '+', '/').</summary>
        Delim,

        /// <summary>A number (integer or float, e.g. "42", "3.14").</summary>
        Number,

        /// <summary>A percentage (e.g. "50%").</summary>
        Percentage,

        /// <summary>A dimension (number with unit, e.g. "10px", "2em").</summary>
        Dimension,

        /// <summary>Whitespace (spaces, tabs, newlines).</summary>
        Whitespace,

        /// <summary>CDO token: &lt;!-- </summary>
        CDO,

        /// <summary>CDC token: --&gt; </summary>
        CDC,

        /// <summary>Colon ':'.</summary>
        Colon,

        /// <summary>Semicolon ';'.</summary>
        Semicolon,

        /// <summary>Comma ','.</summary>
        Comma,

        /// <summary>Left square bracket '['.</summary>
        LeftBracket,

        /// <summary>Right square bracket ']'.</summary>
        RightBracket,

        /// <summary>Left parenthesis '('.</summary>
        LeftParen,

        /// <summary>Right parenthesis ')'.</summary>
        RightParen,

        /// <summary>Left curly brace '{'.</summary>
        LeftBrace,

        /// <summary>Right curly brace '}'.</summary>
        RightBrace,

        /// <summary>End of file.</summary>
        EOF
    }
}
