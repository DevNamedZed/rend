using System;
using System.Collections.Generic;
using System.Text;

namespace Rend.Css.Parser.Internal
{
    /// <summary>
    /// Core CSS parser. Tokenizes the input, then parses the token stream into a Stylesheet AST.
    /// Implements error recovery: on invalid input, skip to the next valid boundary (';', '}', matching bracket).
    /// </summary>
    internal sealed class CssParserCore
    {
        private readonly CssTokenizer _tokenizer;
        private CssToken _current;
        private CssToken _next;
        private bool _hasNext;

        public CssParserCore(string input)
        {
            _tokenizer = new CssTokenizer(input);
            _current = new CssToken();
            _next = new CssToken();
            _hasNext = false;
            Advance(); // prime the first token
        }

        /// <summary>Parse the entire input into a Stylesheet.</summary>
        public Stylesheet ParseStylesheet()
        {
            var rules = new List<CssRule>();
            SkipWhitespaceAndCDx();

            while (_current.Type != CssTokenType.EOF)
            {
                var rule = ParseRule();
                if (rule != null)
                    rules.Add(rule);
                SkipWhitespaceAndCDx();
            }

            return new Stylesheet(rules);
        }

        private CssRule? ParseRule()
        {
            if (_current.Type == CssTokenType.AtKeyword)
                return ParseAtRule();

            return ParseStyleRule();
        }

        #region Style Rules

        private StyleRule? ParseStyleRule()
        {
            // Consume selector tokens until '{'
            var selectorBuilder = new StringBuilder();
            int braceDepth = 0;

            while (_current.Type != CssTokenType.EOF)
            {
                if (_current.Type == CssTokenType.LeftBrace && braceDepth == 0)
                    break;

                if (_current.Type == CssTokenType.LeftBrace) braceDepth++;
                else if (_current.Type == CssTokenType.RightBrace) braceDepth--;

                AppendTokenText(selectorBuilder);
                Advance();
            }

            var selectorText = selectorBuilder.ToString().Trim();
            if (string.IsNullOrEmpty(selectorText) || _current.Type != CssTokenType.LeftBrace)
            {
                // Skip to next rule boundary
                SkipToRuleBoundary();
                return null;
            }

            Advance(); // skip '{'

            var declarations = ParseDeclarationBlock();

            if (_current.Type == CssTokenType.RightBrace)
                Advance();

            return new StyleRule(selectorText, declarations);
        }

        #endregion

        #region At-Rules

        private CssRule? ParseAtRule()
        {
            var keyword = _current.Value.ToLowerInvariant();
            Advance(); // skip @keyword

            switch (keyword)
            {
                case "media": return ParseMediaRule();
                case "font-face": return ParseFontFaceRule();
                case "import": return ParseImportRule();
                case "page": return ParsePageRule();
                default:
                    // Unknown at-rule — skip it
                    SkipAtRule();
                    return null;
            }
        }

        private MediaRule ParseMediaRule()
        {
            SkipWhitespace();

            // Consume media query text until '{'
            var mediaBuilder = new StringBuilder();
            while (_current.Type != CssTokenType.EOF && _current.Type != CssTokenType.LeftBrace)
            {
                AppendTokenText(mediaBuilder);
                Advance();
            }

            var mediaText = mediaBuilder.ToString().Trim();

            if (_current.Type != CssTokenType.LeftBrace)
                return new MediaRule(mediaText, new List<CssRule>());

            Advance(); // skip '{'

            // Parse nested rules
            var rules = new List<CssRule>();
            SkipWhitespaceAndCDx();

            while (_current.Type != CssTokenType.EOF && _current.Type != CssTokenType.RightBrace)
            {
                var rule = ParseRule();
                if (rule != null)
                    rules.Add(rule);
                SkipWhitespaceAndCDx();
            }

            if (_current.Type == CssTokenType.RightBrace)
                Advance();

            return new MediaRule(mediaText, rules);
        }

        private FontFaceRule ParseFontFaceRule()
        {
            SkipWhitespace();

            if (_current.Type != CssTokenType.LeftBrace)
            {
                SkipAtRule();
                return new FontFaceRule(new List<CssDeclaration>());
            }

            Advance(); // skip '{'
            var declarations = ParseDeclarationBlock();

            if (_current.Type == CssTokenType.RightBrace)
                Advance();

            return new FontFaceRule(declarations);
        }

        private ImportRule? ParseImportRule()
        {
            SkipWhitespace();

            string? url = null;

            // @import url("...") or @import "..."
            if (_current.Type == CssTokenType.String)
            {
                url = _current.Value;
                Advance();
            }
            else if (_current.Type == CssTokenType.Url)
            {
                url = _current.Value;
                Advance();
            }
            else if (_current.Type == CssTokenType.Function &&
                     _current.Value.Equals("url", StringComparison.OrdinalIgnoreCase))
            {
                Advance(); // skip function token
                SkipWhitespace();
                if (_current.Type == CssTokenType.String)
                {
                    url = _current.Value;
                    Advance();
                }
                SkipWhitespace();
                if (_current.Type == CssTokenType.RightParen)
                    Advance();
            }

            if (url == null)
            {
                SkipToSemicolon();
                return null;
            }

            SkipWhitespace();

            // Optional media query
            var mediaBuilder = new StringBuilder();
            while (_current.Type != CssTokenType.EOF && _current.Type != CssTokenType.Semicolon)
            {
                AppendTokenText(mediaBuilder);
                Advance();
            }

            var mediaText = mediaBuilder.ToString().Trim();

            if (_current.Type == CssTokenType.Semicolon)
                Advance();

            return new ImportRule(url, string.IsNullOrEmpty(mediaText) ? null : mediaText);
        }

        private PageRule ParsePageRule()
        {
            SkipWhitespace();

            // Optional page selector
            string? pageSelector = null;
            if (_current.Type == CssTokenType.Colon || _current.Type == CssTokenType.Ident)
            {
                var sb = new StringBuilder();
                while (_current.Type != CssTokenType.EOF &&
                       _current.Type != CssTokenType.LeftBrace)
                {
                    AppendTokenText(sb);
                    Advance();
                }
                pageSelector = sb.ToString().Trim();
                if (string.IsNullOrEmpty(pageSelector)) pageSelector = null;
            }

            if (_current.Type != CssTokenType.LeftBrace)
            {
                SkipAtRule();
                return new PageRule(pageSelector, new List<CssDeclaration>());
            }

            Advance(); // skip '{'
            var declarations = ParseDeclarationBlock();

            if (_current.Type == CssTokenType.RightBrace)
                Advance();

            return new PageRule(pageSelector, declarations);
        }

        #endregion

        #region Declaration Parsing

        /// <summary>
        /// Parse declarations inside a { } block. Stops at '}' or EOF.
        /// </summary>
        private List<CssDeclaration> ParseDeclarationBlock()
        {
            var declarations = new List<CssDeclaration>();
            SkipWhitespace();

            while (_current.Type != CssTokenType.EOF && _current.Type != CssTokenType.RightBrace)
            {
                if (_current.Type == CssTokenType.Semicolon)
                {
                    Advance();
                    SkipWhitespace();
                    continue;
                }

                var decl = ParseDeclaration();
                if (decl != null)
                {
                    // Try shorthand expansion
                    if (!CssShorthandExpander.TryExpand(decl.Property, decl.Value, decl.Important, declarations))
                        declarations.Add(decl);
                }

                SkipWhitespace();
            }

            return declarations;
        }

        private CssDeclaration? ParseDeclaration()
        {
            // property-name : value [!important] ;
            if (_current.Type != CssTokenType.Ident)
            {
                SkipToDeclarationBoundary();
                return null;
            }

            var property = _current.Value.ToLowerInvariant();
            Advance();
            SkipWhitespace();

            if (_current.Type != CssTokenType.Colon)
            {
                SkipToDeclarationBoundary();
                return null;
            }
            Advance(); // skip ':'
            SkipWhitespace();

            // Collect value tokens until ';', '}', or EOF
            var valueTokens = new List<CssToken>();
            bool important = false;
            int parenDepth = 0;

            while (_current.Type != CssTokenType.EOF)
            {
                if (parenDepth == 0 &&
                    (_current.Type == CssTokenType.Semicolon || _current.Type == CssTokenType.RightBrace))
                    break;

                if (_current.Type == CssTokenType.LeftParen || _current.Type == CssTokenType.Function)
                    parenDepth++;
                else if (_current.Type == CssTokenType.RightParen)
                    parenDepth = Math.Max(0, parenDepth - 1);

                valueTokens.Add(_current);
                Advance();
            }

            // Check for !important
            // Look backwards through value tokens for "!" "important"
            int vtCount = valueTokens.Count;
            if (vtCount >= 2)
            {
                // Strip trailing whitespace
                int end = vtCount - 1;
                while (end >= 0 && valueTokens[end].Type == CssTokenType.Whitespace) end--;

                if (end >= 1 && valueTokens[end].Type == CssTokenType.Ident &&
                    valueTokens[end].Value.Equals("important", StringComparison.OrdinalIgnoreCase))
                {
                    int bangPos = end - 1;
                    while (bangPos >= 0 && valueTokens[bangPos].Type == CssTokenType.Whitespace) bangPos--;

                    if (bangPos >= 0 && valueTokens[bangPos].Type == CssTokenType.Delim &&
                        valueTokens[bangPos].Value == "!")
                    {
                        important = true;
                        vtCount = bangPos;
                        // Trim trailing whitespace before !
                        while (vtCount > 0 && valueTokens[vtCount - 1].Type == CssTokenType.Whitespace)
                            vtCount--;
                    }
                }
            }

            if (vtCount == 0)
            {
                if (_current.Type == CssTokenType.Semicolon) Advance();
                return null;
            }

            // Parse value tokens
            var tokenArray = new CssToken[vtCount];
            for (int i = 0; i < vtCount; i++)
                tokenArray[i] = valueTokens[i];

            var parser = new CssValueParser(tokenArray, vtCount);
            var value = parser.Parse();

            if (_current.Type == CssTokenType.Semicolon)
                Advance();

            return new CssDeclaration(property, value, important);
        }

        #endregion

        #region Token management

        private void Advance()
        {
            if (_hasNext)
            {
                _current = _next;
                _hasNext = false;
            }
            else
            {
                _tokenizer.Read(ref _current);
            }
        }

        private void SkipWhitespace()
        {
            while (_current.Type == CssTokenType.Whitespace)
                Advance();
        }

        private void SkipWhitespaceAndCDx()
        {
            while (_current.Type == CssTokenType.Whitespace ||
                   _current.Type == CssTokenType.CDO ||
                   _current.Type == CssTokenType.CDC)
                Advance();
        }

        private void SkipToRuleBoundary()
        {
            int braceDepth = 0;
            while (_current.Type != CssTokenType.EOF)
            {
                if (_current.Type == CssTokenType.LeftBrace) braceDepth++;
                else if (_current.Type == CssTokenType.RightBrace)
                {
                    if (braceDepth <= 0)
                    {
                        Advance();
                        return;
                    }
                    braceDepth--;
                }
                Advance();
            }
        }

        private void SkipToDeclarationBoundary()
        {
            while (_current.Type != CssTokenType.EOF &&
                   _current.Type != CssTokenType.Semicolon &&
                   _current.Type != CssTokenType.RightBrace)
                Advance();

            if (_current.Type == CssTokenType.Semicolon)
                Advance();
        }

        private void SkipToSemicolon()
        {
            while (_current.Type != CssTokenType.EOF && _current.Type != CssTokenType.Semicolon)
                Advance();
            if (_current.Type == CssTokenType.Semicolon)
                Advance();
        }

        private void SkipAtRule()
        {
            // Skip until ';' or a { } block
            while (_current.Type != CssTokenType.EOF)
            {
                if (_current.Type == CssTokenType.Semicolon)
                {
                    Advance();
                    return;
                }
                if (_current.Type == CssTokenType.LeftBrace)
                {
                    SkipBlock();
                    return;
                }
                Advance();
            }
        }

        private void SkipBlock()
        {
            int depth = 1;
            Advance(); // skip '{'
            while (_current.Type != CssTokenType.EOF && depth > 0)
            {
                if (_current.Type == CssTokenType.LeftBrace) depth++;
                else if (_current.Type == CssTokenType.RightBrace) depth--;
                Advance();
            }
        }

        /// <summary>
        /// Append the textual representation of the current token to a builder.
        /// Used for reconstructing selector text and media queries.
        /// </summary>
        private void AppendTokenText(StringBuilder sb)
        {
            switch (_current.Type)
            {
                case CssTokenType.Ident:
                    sb.Append(_current.Value);
                    break;
                case CssTokenType.Function:
                    sb.Append(_current.Value).Append('(');
                    break;
                case CssTokenType.AtKeyword:
                    sb.Append('@').Append(_current.Value);
                    break;
                case CssTokenType.Hash:
                    sb.Append('#').Append(_current.Value);
                    break;
                case CssTokenType.String:
                    sb.Append('"').Append(_current.Value).Append('"');
                    break;
                case CssTokenType.Url:
                    sb.Append("url(").Append(_current.Value).Append(')');
                    break;
                case CssTokenType.Number:
                    sb.Append(_current.NumericValue.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case CssTokenType.Percentage:
                    sb.Append(_current.NumericValue.ToString("G", System.Globalization.CultureInfo.InvariantCulture)).Append('%');
                    break;
                case CssTokenType.Dimension:
                    sb.Append(_current.NumericValue.ToString("G", System.Globalization.CultureInfo.InvariantCulture)).Append(_current.Unit);
                    break;
                case CssTokenType.Whitespace:
                    sb.Append(' ');
                    break;
                case CssTokenType.Delim:
                    sb.Append(_current.Value);
                    break;
                case CssTokenType.Colon: sb.Append(':'); break;
                case CssTokenType.Semicolon: sb.Append(';'); break;
                case CssTokenType.Comma: sb.Append(','); break;
                case CssTokenType.LeftParen: sb.Append('('); break;
                case CssTokenType.RightParen: sb.Append(')'); break;
                case CssTokenType.LeftBracket: sb.Append('['); break;
                case CssTokenType.RightBracket: sb.Append(']'); break;
                case CssTokenType.LeftBrace: sb.Append('{'); break;
                case CssTokenType.RightBrace: sb.Append('}'); break;
                case CssTokenType.CDO: sb.Append("<!--"); break;
                case CssTokenType.CDC: sb.Append("-->"); break;
            }
        }

        #endregion
    }
}
