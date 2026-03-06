using System;
using System.Collections.Generic;
using Rend.Core.Values;

namespace Rend.Css.Parser.Internal
{
    /// <summary>
    /// Parses CSS declaration values from a token stream.
    /// Produces CssValue instances from sequences of tokens.
    /// </summary>
    internal sealed class CssValueParser
    {
        private readonly CssToken[] _tokens;
        private int _pos;
        private readonly int _count;

        public CssValueParser(CssToken[] tokens, int count)
        {
            _tokens = tokens;
            _count = count;
            _pos = 0;
        }

        /// <summary>
        /// Parse the entire token sequence into a single CssValue.
        /// Multiple space-separated values become a CssListValue.
        /// </summary>
        public CssValue Parse()
        {
            SkipWhitespace();

            var commaGroups = new List<CssValue>();
            var currentGroup = new List<CssValue>();
            bool hasComma = false;

            while (_pos < _count && _tokens[_pos].Type != CssTokenType.EOF)
            {
                var val = ParseSingleValue();
                if (val == null) break;
                currentGroup.Add(val);

                SkipWhitespace();

                // Check for comma separator
                if (_pos < _count && _tokens[_pos].Type == CssTokenType.Comma)
                {
                    hasComma = true;
                    // Flush current space-separated group
                    commaGroups.Add(GroupValues(currentGroup, ' '));
                    currentGroup.Clear();
                    _pos++;
                    SkipWhitespace();
                }
            }

            if (hasComma)
            {
                // Flush last group
                if (currentGroup.Count > 0)
                    commaGroups.Add(GroupValues(currentGroup, ' '));
                if (commaGroups.Count == 0) return new CssKeywordValue("");
                if (commaGroups.Count == 1) return commaGroups[0];
                return new CssListValue(commaGroups, ',');
            }

            // No commas — return space-separated list
            if (currentGroup.Count == 0) return new CssKeywordValue("");
            if (currentGroup.Count == 1) return currentGroup[0];
            return new CssListValue(currentGroup, ' ');
        }

        private static CssValue GroupValues(List<CssValue> values, char sep)
        {
            if (values.Count == 0) return new CssKeywordValue("");
            if (values.Count == 1) return values[0];
            return new CssListValue(new List<CssValue>(values), sep);
        }

        /// <summary>
        /// Parse a single CSS value (not a list).
        /// </summary>
        public CssValue? ParseSingleValue()
        {
            if (_pos >= _count) return null;

            ref var token = ref _tokens[_pos];

            switch (token.Type)
            {
                case CssTokenType.Number:
                    _pos++;
                    return new CssNumberValue(token.NumericValue, token.Flag);

                case CssTokenType.Percentage:
                    _pos++;
                    return new CssPercentageValue(token.NumericValue);

                case CssTokenType.Dimension:
                    _pos++;
                    return new CssDimensionValue(token.NumericValue, token.Unit ?? "");

                case CssTokenType.String:
                    _pos++;
                    return new CssStringValue(token.Value);

                case CssTokenType.Url:
                    _pos++;
                    return new CssUrlValue(token.Value);

                case CssTokenType.Hash:
                {
                    _pos++;
                    if (CssColorParser.TryParseHex(token.Value, out var color))
                        return new CssColorValue(color);
                    // Not a valid hex color — return as keyword with #
                    return new CssKeywordValue("#" + token.Value);
                }

                case CssTokenType.Function:
                    return ParseFunction();

                case CssTokenType.Ident:
                {
                    var ident = token.Value;
                    _pos++;

                    // Check for named color
                    if (CssColorParser.TryParseNamed(ident, out var namedColor))
                        return new CssColorValue(namedColor);

                    return new CssKeywordValue(ident.ToLowerInvariant());
                }

                case CssTokenType.Delim:
                {
                    // Handle '/' separator (e.g. in font shorthand, border-radius)
                    if (token.Value == "/")
                    {
                        _pos++;
                        return new CssKeywordValue("/");
                    }
                    // Unknown delimiter — skip
                    _pos++;
                    return null;
                }

                default:
                    return null;
            }
        }

        private CssValue ParseFunction()
        {
            var name = _tokens[_pos].Value;
            _pos++; // skip function token

            var args = new List<CssValue>();
            SkipWhitespace();

            // Special handling for color functions
            var lowerName = name.ToLowerInvariant();
            if (lowerName == "rgb" || lowerName == "rgba")
            {
                ParseFunctionArgs(args);
                if (CssColorParser.TryParseRgb(args, out var color))
                    return new CssColorValue(color);
                return new CssFunctionValue(lowerName, args);
            }

            if (lowerName == "hsl" || lowerName == "hsla")
            {
                ParseFunctionArgs(args);
                if (CssColorParser.TryParseHsl(args, out var color))
                    return new CssColorValue(color);
                return new CssFunctionValue(lowerName, args);
            }

            if (lowerName == "hwb")
            {
                ParseFunctionArgs(args);
                if (CssColorParser.TryParseHwb(args, out var color))
                    return new CssColorValue(color);
                return new CssFunctionValue(lowerName, args);
            }

            if (lowerName == "lab")
            {
                ParseFunctionArgs(args);
                if (CssColorParser.TryParseLab(args, out var color))
                    return new CssColorValue(color);
                return new CssFunctionValue(lowerName, args);
            }

            if (lowerName == "lch")
            {
                ParseFunctionArgs(args);
                if (CssColorParser.TryParseLch(args, out var color))
                    return new CssColorValue(color);
                return new CssFunctionValue(lowerName, args);
            }

            if (lowerName == "oklab")
            {
                ParseFunctionArgs(args);
                if (CssColorParser.TryParseOklab(args, out var color))
                    return new CssColorValue(color);
                return new CssFunctionValue(lowerName, args);
            }

            if (lowerName == "oklch")
            {
                ParseFunctionArgs(args);
                if (CssColorParser.TryParseOklch(args, out var color))
                    return new CssColorValue(color);
                return new CssFunctionValue(lowerName, args);
            }

            if (lowerName == "color-mix")
            {
                ParseFunctionArgs(args);
                if (CssColorParser.TryParseColorMix(args, out var color))
                    return new CssColorValue(color);
                return new CssFunctionValue(lowerName, args);
            }

            if (lowerName == "color")
            {
                ParseFunctionArgs(args);
                if (CssColorParser.TryParseColorFunction(args, out var color))
                    return new CssColorValue(color);
                return new CssFunctionValue(lowerName, args);
            }

            // For calc() — preserve arithmetic operators as CssKeywordValue
            if (lowerName == "calc")
            {
                ParseCalcArgs(args);
                return new CssFunctionValue(lowerName, args);
            }

            // For var() and other functions — store as CssFunctionValue
            ParseFunctionArgs(args);
            return new CssFunctionValue(lowerName, args);
        }

        private void ParseFunctionArgs(List<CssValue> args)
        {
            int depth = 1;
            SkipWhitespace();

            while (_pos < _count && depth > 0)
            {
                ref var t = ref _tokens[_pos];

                if (t.Type == CssTokenType.RightParen)
                {
                    depth--;
                    if (depth == 0)
                    {
                        _pos++;
                        return;
                    }
                    _pos++;
                    continue;
                }

                if (t.Type == CssTokenType.LeftParen || t.Type == CssTokenType.Function)
                {
                    if (t.Type == CssTokenType.Function)
                    {
                        // Nested function
                        var val = ParseFunction();
                        if (val != null) args.Add(val);
                        continue;
                    }
                    depth++;
                    _pos++;
                    continue;
                }

                if (t.Type == CssTokenType.Comma)
                {
                    _pos++;
                    SkipWhitespace();
                    continue;
                }

                if (t.Type == CssTokenType.Whitespace)
                {
                    SkipWhitespace();

                    // Check for slash separator (used in modern rgb/hsl syntax: rgb(255 0 0 / 0.5))
                    if (_pos < _count && _tokens[_pos].Type == CssTokenType.Delim && _tokens[_pos].Value == "/")
                    {
                        _pos++;
                        SkipWhitespace();
                    }
                    continue;
                }

                if (t.Type == CssTokenType.Delim && t.Value == "/")
                {
                    _pos++;
                    SkipWhitespace();
                    continue;
                }

                var val2 = ParseSingleValue();
                if (val2 != null)
                    args.Add(val2);
                else
                    _pos++; // skip unknown
            }
        }

        /// <summary>
        /// Parses calc() function arguments, preserving arithmetic operators (+, -, *, /)
        /// as CssKeywordValue nodes so they can be evaluated later.
        /// </summary>
        private void ParseCalcArgs(List<CssValue> args)
        {
            int depth = 1;
            SkipWhitespace();

            while (_pos < _count && depth > 0)
            {
                ref var t = ref _tokens[_pos];

                if (t.Type == CssTokenType.RightParen)
                {
                    depth--;
                    if (depth == 0)
                    {
                        _pos++;
                        return;
                    }
                    _pos++;
                    continue;
                }

                if (t.Type == CssTokenType.Function)
                {
                    var val = ParseFunction();
                    if (val != null) args.Add(val);
                    continue;
                }

                if (t.Type == CssTokenType.LeftParen)
                {
                    depth++;
                    _pos++;
                    continue;
                }

                if (t.Type == CssTokenType.Whitespace)
                {
                    SkipWhitespace();
                    continue;
                }

                if (t.Type == CssTokenType.Delim)
                {
                    string op = t.Value;
                    if (op == "+" || op == "-" || op == "*" || op == "/")
                    {
                        args.Add(new CssKeywordValue(op));
                        _pos++;
                        SkipWhitespace();
                        continue;
                    }
                    _pos++;
                    continue;
                }

                var val2 = ParseSingleValue();
                if (val2 != null)
                    args.Add(val2);
                else
                    _pos++;
            }
        }

        private void SkipWhitespace()
        {
            while (_pos < _count && _tokens[_pos].Type == CssTokenType.Whitespace)
                _pos++;
        }
    }
}
