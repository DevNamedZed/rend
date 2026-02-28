using System;
using System.Text;

namespace Rend.Css.Parser.Internal
{
    /// <summary>
    /// CSS3 tokenizer per CSS Syntax Module Level 3 §4.
    /// Consumes a string input and produces CssTokens on demand.
    /// Handles: escape sequences, url() tokens, comments, CDC/CDO, numbers with sign/exponent.
    /// </summary>
    internal sealed class CssTokenizer
    {
        private readonly string _input;
        private int _pos;
        private readonly StringBuilder _sb = new StringBuilder(64);

        public CssTokenizer(string input)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _pos = 0;
            // Preprocess: replace \r\n, \r, \f with \n per §3.3
            // We do this lazily during Peek/Consume instead for zero-allocation on well-formed input.
        }

        /// <summary>
        /// Read the next token. Returns false only for EOF (token.Type == EOF).
        /// </summary>
        public bool Read(ref CssToken token)
        {
            token.Reset();

            // §4.3.1 Consume comments
            ConsumeComments();

            if (_pos >= _input.Length)
            {
                token.Type = CssTokenType.EOF;
                return false;
            }

            char c = Peek();

            // Whitespace
            if (IsWhitespace(c))
            {
                ConsumeWhitespace();
                token.Type = CssTokenType.Whitespace;
                return true;
            }

            // String tokens
            if (c == '"' || c == '\'')
            {
                ConsumeString(c, ref token);
                return true;
            }

            // Hash token
            if (c == '#')
            {
                Advance();
                if (_pos < _input.Length && (IsNameChar(Peek()) || StartsValidEscape(_pos)))
                {
                    token.Type = CssTokenType.Hash;
                    token.Flag = StartsIdentifier(_pos); // id type
                    token.Value = ConsumeName();
                    return true;
                }
                token.Type = CssTokenType.Delim;
                token.Value = "#";
                return true;
            }

            // Left paren
            if (c == '(')
            {
                Advance();
                token.Type = CssTokenType.LeftParen;
                return true;
            }

            // Right paren
            if (c == ')')
            {
                Advance();
                token.Type = CssTokenType.RightParen;
                return true;
            }

            // Plus sign — could be number
            if (c == '+')
            {
                if (StartsNumber(_pos))
                {
                    ConsumeNumericToken(ref token);
                    return true;
                }
                Advance();
                token.Type = CssTokenType.Delim;
                token.Value = "+";
                return true;
            }

            // Comma
            if (c == ',')
            {
                Advance();
                token.Type = CssTokenType.Comma;
                return true;
            }

            // Minus — could be number, identifier, or CDC
            if (c == '-')
            {
                if (StartsNumber(_pos))
                {
                    ConsumeNumericToken(ref token);
                    return true;
                }
                if (StartsIdentifier(_pos))
                {
                    ConsumeIdentLikeToken(ref token);
                    return true;
                }
                // CDC -->
                if (_pos + 2 < _input.Length && _input[_pos + 1] == '-' && _input[_pos + 2] == '>')
                {
                    _pos += 3;
                    token.Type = CssTokenType.CDC;
                    return true;
                }
                Advance();
                token.Type = CssTokenType.Delim;
                token.Value = "-";
                return true;
            }

            // Period — could be number
            if (c == '.')
            {
                if (StartsNumber(_pos))
                {
                    ConsumeNumericToken(ref token);
                    return true;
                }
                Advance();
                token.Type = CssTokenType.Delim;
                token.Value = ".";
                return true;
            }

            // Colon
            if (c == ':')
            {
                Advance();
                token.Type = CssTokenType.Colon;
                return true;
            }

            // Semicolon
            if (c == ';')
            {
                Advance();
                token.Type = CssTokenType.Semicolon;
                return true;
            }

            // Less-than — CDO <!--
            if (c == '<')
            {
                if (_pos + 3 < _input.Length && _input[_pos + 1] == '!' &&
                    _input[_pos + 2] == '-' && _input[_pos + 3] == '-')
                {
                    _pos += 4;
                    token.Type = CssTokenType.CDO;
                    return true;
                }
                Advance();
                token.Type = CssTokenType.Delim;
                token.Value = "<";
                return true;
            }

            // At-keyword
            if (c == '@')
            {
                Advance();
                if (StartsIdentifier(_pos))
                {
                    token.Type = CssTokenType.AtKeyword;
                    token.Value = ConsumeName();
                    return true;
                }
                token.Type = CssTokenType.Delim;
                token.Value = "@";
                return true;
            }

            // Left bracket
            if (c == '[')
            {
                Advance();
                token.Type = CssTokenType.LeftBracket;
                return true;
            }

            // Backslash — escape or delim
            if (c == '\\')
            {
                if (StartsValidEscape(_pos))
                {
                    ConsumeIdentLikeToken(ref token);
                    return true;
                }
                Advance();
                token.Type = CssTokenType.Delim;
                token.Value = "\\";
                return true;
            }

            // Right bracket
            if (c == ']')
            {
                Advance();
                token.Type = CssTokenType.RightBracket;
                return true;
            }

            // Left brace
            if (c == '{')
            {
                Advance();
                token.Type = CssTokenType.LeftBrace;
                return true;
            }

            // Right brace
            if (c == '}')
            {
                Advance();
                token.Type = CssTokenType.RightBrace;
                return true;
            }

            // Digit — number
            if (c >= '0' && c <= '9')
            {
                ConsumeNumericToken(ref token);
                return true;
            }

            // Name-start char — ident-like
            if (IsNameStartChar(c))
            {
                ConsumeIdentLikeToken(ref token);
                return true;
            }

            // Anything else is a Delim token
            Advance();
            token.Type = CssTokenType.Delim;
            token.Value = c.ToString();
            return true;
        }

        #region Character consumption helpers

        private char Peek()
        {
            if (_pos >= _input.Length) return '\0';
            char c = _input[_pos];
            // §3.3 preprocessing: treat \r\n, \r, \f as \n
            if (c == '\r' || c == '\f') return '\n';
            return c;
        }

        private char PeekAt(int offset)
        {
            int i = _pos + offset;
            if (i >= _input.Length) return '\0';
            char c = _input[i];
            if (c == '\r' || c == '\f') return '\n';
            return c;
        }

        private void Advance()
        {
            if (_pos < _input.Length)
            {
                // Handle \r\n as single newline
                if (_input[_pos] == '\r' && _pos + 1 < _input.Length && _input[_pos + 1] == '\n')
                    _pos += 2;
                else
                    _pos++;
            }
        }

        private char Consume()
        {
            char c = Peek();
            Advance();
            return c;
        }

        #endregion

        #region §4.3.2 Consume comments

        private void ConsumeComments()
        {
            while (_pos + 1 < _input.Length && _input[_pos] == '/' && _input[_pos + 1] == '*')
            {
                _pos += 2;
                while (_pos < _input.Length)
                {
                    if (_input[_pos] == '*' && _pos + 1 < _input.Length && _input[_pos + 1] == '/')
                    {
                        _pos += 2;
                        break;
                    }
                    _pos++;
                }
                // If we reach EOF without closing, that's a parse error but we continue
            }
        }

        #endregion

        #region §4.3.3 Consume a numeric token

        private void ConsumeNumericToken(ref CssToken token)
        {
            ConsumeNumber(out float value, out bool isInteger);

            // Check if followed by an identifier (dimension)
            if (StartsIdentifier(_pos))
            {
                token.Type = CssTokenType.Dimension;
                token.NumericValue = value;
                token.Unit = ConsumeName();
                token.Flag = isInteger;
                return;
            }

            // Check for percentage
            if (_pos < _input.Length && Peek() == '%')
            {
                Advance();
                token.Type = CssTokenType.Percentage;
                token.NumericValue = value;
                token.Flag = isInteger;
                return;
            }

            token.Type = CssTokenType.Number;
            token.NumericValue = value;
            token.Flag = isInteger;
        }

        /// <summary>
        /// §4.3.12 Consume a number. Returns the value and whether it's integer.
        /// </summary>
        private void ConsumeNumber(out float value, out bool isInteger)
        {
            _sb.Clear();
            isInteger = true;

            // Optional sign
            char c = Peek();
            if (c == '+' || c == '-')
            {
                _sb.Append(c);
                Advance();
            }

            // Integer part
            while (_pos < _input.Length && Peek() >= '0' && Peek() <= '9')
            {
                _sb.Append(Peek());
                Advance();
            }

            // Decimal part
            if (_pos < _input.Length && Peek() == '.' &&
                _pos + 1 < _input.Length && PeekAt(1) >= '0' && PeekAt(1) <= '9')
            {
                isInteger = false;
                _sb.Append('.');
                Advance();
                while (_pos < _input.Length && Peek() >= '0' && Peek() <= '9')
                {
                    _sb.Append(Peek());
                    Advance();
                }
            }

            // Exponent part
            c = Peek();
            if ((c == 'e' || c == 'E') && _pos < _input.Length)
            {
                char next = PeekAt(1);
                bool hasExponent = false;
                if (next >= '0' && next <= '9')
                    hasExponent = true;
                else if ((next == '+' || next == '-') && PeekAt(2) >= '0' && PeekAt(2) <= '9')
                    hasExponent = true;

                if (hasExponent)
                {
                    isInteger = false;
                    _sb.Append(c);
                    Advance();
                    c = Peek();
                    if (c == '+' || c == '-')
                    {
                        _sb.Append(c);
                        Advance();
                    }
                    while (_pos < _input.Length && Peek() >= '0' && Peek() <= '9')
                    {
                        _sb.Append(Peek());
                        Advance();
                    }
                }
            }

            if (_sb.Length > 0 && float.TryParse(_sb.ToString(),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float result))
            {
                value = result;
            }
            else
            {
                value = 0;
            }
        }

        #endregion

        #region §4.3.4 Consume an ident-like token

        private void ConsumeIdentLikeToken(ref CssToken token)
        {
            var name = ConsumeName();

            // Check for function: name followed by '('
            if (_pos < _input.Length && Peek() == '(')
            {
                Advance(); // consume '('

                // Special case: url(
                if (name.Equals("url", StringComparison.OrdinalIgnoreCase))
                {
                    ConsumeUrlToken(ref token);
                    return;
                }

                token.Type = CssTokenType.Function;
                token.Value = name;
                return;
            }

            token.Type = CssTokenType.Ident;
            token.Value = name;
        }

        #endregion

        #region §4.3.6 Consume a url token

        private void ConsumeUrlToken(ref CssToken token)
        {
            // Skip whitespace after url(
            ConsumeWhitespace();

            if (_pos >= _input.Length)
            {
                token.Type = CssTokenType.Url;
                token.Value = "";
                return;
            }

            // If next is a quote, this is url("...") which is a Function token + String
            char c = Peek();
            if (c == '"' || c == '\'')
            {
                // Actually, per spec: if url( is followed by a string, it's a Function("url") not a Url token.
                // We need to put back the '(' conceptually and return as function.
                token.Type = CssTokenType.Function;
                token.Value = "url";
                return;
            }

            // Consume unquoted URL
            _sb.Clear();
            while (_pos < _input.Length)
            {
                c = Peek();
                if (c == ')')
                {
                    Advance();
                    token.Type = CssTokenType.Url;
                    token.Value = _sb.ToString();
                    return;
                }
                if (IsWhitespace(c))
                {
                    ConsumeWhitespace();
                    if (_pos < _input.Length && Peek() == ')')
                    {
                        Advance();
                        token.Type = CssTokenType.Url;
                        token.Value = _sb.ToString();
                        return;
                    }
                    // Bad URL — whitespace in middle
                    ConsumeBadUrlRemnants();
                    token.Type = CssTokenType.BadUrl;
                    token.Value = _sb.ToString();
                    return;
                }
                if (c == '"' || c == '\'' || c == '(' || IsNonPrintable(c))
                {
                    // Bad URL
                    ConsumeBadUrlRemnants();
                    token.Type = CssTokenType.BadUrl;
                    token.Value = _sb.ToString();
                    return;
                }
                if (c == '\\')
                {
                    if (StartsValidEscape(_pos))
                    {
                        Advance(); // skip backslash
                        _sb.Append(ConsumeEscapedCodePoint());
                    }
                    else
                    {
                        // Bad URL
                        ConsumeBadUrlRemnants();
                        token.Type = CssTokenType.BadUrl;
                        token.Value = _sb.ToString();
                        return;
                    }
                }
                else
                {
                    _sb.Append(c);
                    Advance();
                }
            }

            // EOF
            token.Type = CssTokenType.Url;
            token.Value = _sb.ToString();
        }

        private void ConsumeBadUrlRemnants()
        {
            while (_pos < _input.Length)
            {
                char c = Peek();
                if (c == ')')
                {
                    Advance();
                    return;
                }
                if (StartsValidEscape(_pos))
                {
                    Advance();
                    ConsumeEscapedCodePoint();
                }
                else
                {
                    Advance();
                }
            }
        }

        #endregion

        #region §4.3.5 Consume a string token

        private void ConsumeString(char endChar, ref CssToken token)
        {
            Advance(); // skip opening quote
            _sb.Clear();

            while (_pos < _input.Length)
            {
                char c = Peek();

                if (c == endChar)
                {
                    Advance();
                    token.Type = CssTokenType.String;
                    token.Value = _sb.ToString();
                    return;
                }

                if (c == '\n')
                {
                    // Unescaped newline — bad string
                    // Don't consume the newline
                    token.Type = CssTokenType.BadString;
                    token.Value = _sb.ToString();
                    return;
                }

                if (c == '\\')
                {
                    Advance();
                    if (_pos >= _input.Length)
                    {
                        // EOF after backslash — just ignore it
                        break;
                    }
                    c = Peek();
                    if (c == '\n')
                    {
                        // Escaped newline — line continuation, consume and skip
                        Advance();
                        continue;
                    }
                    // Escaped code point
                    _sb.Append(ConsumeEscapedCodePoint());
                    continue;
                }

                _sb.Append(c);
                Advance();
            }

            // EOF — return what we have
            token.Type = CssTokenType.String;
            token.Value = _sb.ToString();
        }

        #endregion

        #region §4.3.7 Consume an escaped code point

        private char ConsumeEscapedCodePoint()
        {
            if (_pos >= _input.Length)
                return '\uFFFD'; // replacement character

            char c = Peek();

            // Hex escape
            if (IsHexDigit(c))
            {
                int value = 0;
                int count = 0;
                while (_pos < _input.Length && count < 6 && IsHexDigit(Peek()))
                {
                    value = value * 16 + HexValue(Peek());
                    Advance();
                    count++;
                }
                // Consume optional trailing whitespace
                if (_pos < _input.Length && IsWhitespace(Peek()))
                    Advance();

                if (value == 0 || value > 0x10FFFF || (value >= 0xD800 && value <= 0xDFFF))
                    return '\uFFFD';

                // If the value is a supplementary character we return the BMP replacement
                // (CSS tokenizer works at code point level but we produce chars)
                if (value > 0xFFFF)
                    return '\uFFFD'; // Simplification: BMP only for now

                return (char)value;
            }

            // Any other character — return it literally
            Advance();
            return c;
        }

        #endregion

        #region §4.3.11 Consume a name

        private string ConsumeName()
        {
            _sb.Clear();

            while (_pos < _input.Length)
            {
                char c = Peek();
                if (IsNameChar(c))
                {
                    _sb.Append(c);
                    Advance();
                }
                else if (StartsValidEscape(_pos))
                {
                    Advance(); // skip backslash
                    _sb.Append(ConsumeEscapedCodePoint());
                }
                else
                {
                    break;
                }
            }

            return _sb.ToString();
        }

        #endregion

        #region Whitespace consumption

        private void ConsumeWhitespace()
        {
            while (_pos < _input.Length && IsWhitespace(Peek()))
                Advance();
        }

        #endregion

        #region Lookahead / classification helpers

        /// <summary>
        /// §4.3.8 Check if two code points are a valid escape.
        /// position points to the backslash.
        /// </summary>
        private bool StartsValidEscape(int position)
        {
            if (position >= _input.Length || _input[position] != '\\')
                return false;
            if (position + 1 >= _input.Length)
                return false;
            char next = _input[position + 1];
            return next != '\n' && next != '\r' && next != '\f';
        }

        /// <summary>
        /// §4.3.9 Check if three code points would start an identifier.
        /// </summary>
        private bool StartsIdentifier(int position)
        {
            if (position >= _input.Length) return false;
            char c = _input[position];

            if (IsNameStartChar(c)) return true;

            if (c == '-')
            {
                if (position + 1 >= _input.Length) return false;
                char next = _input[position + 1];
                if (IsNameStartChar(next) || next == '-') return true;
                return StartsValidEscape(position + 1);
            }

            if (c == '\\')
                return StartsValidEscape(position);

            return false;
        }

        /// <summary>
        /// §4.3.10 Check if three code points would start a number.
        /// </summary>
        private bool StartsNumber(int position)
        {
            if (position >= _input.Length) return false;
            char c = _input[position];

            if (c >= '0' && c <= '9') return true;

            if (c == '.')
            {
                return position + 1 < _input.Length &&
                       _input[position + 1] >= '0' && _input[position + 1] <= '9';
            }

            if (c == '+' || c == '-')
            {
                if (position + 1 >= _input.Length) return false;
                char next = _input[position + 1];
                if (next >= '0' && next <= '9') return true;
                if (next == '.' && position + 2 < _input.Length &&
                    _input[position + 2] >= '0' && _input[position + 2] <= '9')
                    return true;
            }

            return false;
        }

        private static bool IsWhitespace(char c)
        {
            return c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f';
        }

        private static bool IsNameStartChar(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || c > '\u007F';
        }

        private static bool IsNameChar(char c)
        {
            return IsNameStartChar(c) || (c >= '0' && c <= '9') || c == '-';
        }

        private static bool IsHexDigit(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        }

        private static int HexValue(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            if (c >= 'A' && c <= 'F') return c - 'A' + 10;
            return 0;
        }

        private static bool IsNonPrintable(char c)
        {
            return (c >= '\u0000' && c <= '\u0008') || c == '\u000B' ||
                   (c >= '\u000E' && c <= '\u001F') || c == '\u007F';
        }

        #endregion
    }
}
