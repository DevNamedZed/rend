using System;
using System.Collections.Generic;
using System.Globalization;
using Rend.Core.Values;
using Rend.Css.Parser.Internal;

namespace Rend.Css.Resolution.Internal
{
    /// <summary>
    /// [CSS-VALUES-5 §8] Resolves the attr() function by substituting an element's attribute
    /// value, cast to the declared type, into a CSS value tree. Runs at computed-value time
    /// (the element is available in <see cref="StyleResolver"/>). Supports the typed form
    /// <c>attr(name type(&lt;length&gt;) [, fallback])</c> and the untyped string form
    /// <c>attr(name [, fallback])</c>.
    /// </summary>
    internal static class AttrResolver
    {
        public static CssValue Substitute(CssValue value, Func<string, string?> getAttr)
        {
            switch (value)
            {
                case CssFunctionValue fn:
                    if (string.Equals(fn.Name, "attr", StringComparison.OrdinalIgnoreCase))
                    {
                        return ResolveAttr(fn, getAttr);
                    }
                    return SubstituteInArgs(fn, getAttr);

                case CssListValue list:
                    var items = new List<CssValue>(list.Values.Count);
                    bool listChanged = false;
                    for (int i = 0; i < list.Values.Count; i++)
                    {
                        var resolved = Substitute(list.Values[i], getAttr);
                        items.Add(resolved);
                        if (!ReferenceEquals(resolved, list.Values[i])) { listChanged = true; }
                    }
                    return listChanged ? new CssListValue(items, list.Separator) : list;

                default:
                    return value;
            }
        }

        private static CssValue SubstituteInArgs(CssFunctionValue fn, Func<string, string?> getAttr)
        {
            var args = new List<CssValue>(fn.Arguments.Count);
            bool changed = false;
            for (int i = 0; i < fn.Arguments.Count; i++)
            {
                var resolved = Substitute(fn.Arguments[i], getAttr);
                args.Add(resolved);
                if (!ReferenceEquals(resolved, fn.Arguments[i])) { changed = true; }
            }
            return changed ? new CssFunctionValue(fn.Name, args) : fn;
        }

        private static CssValue ResolveAttr(CssFunctionValue fn, Func<string, string?> getAttr)
        {
            var args = fn.Arguments;
            if (args.Count == 0 || !(args[0] is CssKeywordValue nameKeyword))
            {
                return GuaranteedInvalidValue.Instance;
            }

            // [CSS-VALUES-5 §8] Optional type via type(<…>), else untyped (string). The fallback
            // (if any) is the next argument.
            string? typeName = null;
            int fallbackIndex = 1;
            if (args.Count > 1 && args[1] is CssFunctionValue typeFn
                && string.Equals(typeFn.Name, "type", StringComparison.OrdinalIgnoreCase)
                && typeFn.Arguments.Count > 0 && typeFn.Arguments[0] is CssKeywordValue typeKeyword)
            {
                typeName = typeKeyword.Keyword.Trim('<', '>').ToLowerInvariant();
                fallbackIndex = 2;
            }

            string? attribute = getAttr(nameKeyword.Keyword);
            if (attribute != null && TryCast(attribute.Trim(), typeName, out var casted))
            {
                return casted;
            }

            if (args.Count > fallbackIndex)
            {
                return args[fallbackIndex];
            }
            return GuaranteedInvalidValue.Instance;
        }

        private static bool TryCast(string text, string? type, out CssValue result)
        {
            result = GuaranteedInvalidValue.Instance;
            switch (type)
            {
                case null:
                case "string":
                    result = new CssStringValue(text);
                    return true;

                case "color":
                    if (TryParseColor(text, out var color))
                    {
                        result = new CssColorValue(color);
                        return true;
                    }
                    return false;

                case "number":
                case "integer":
                    if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                    {
                        result = new CssNumberValue(number);
                        return true;
                    }
                    return false;

                case "percentage":
                    if (text.EndsWith("%", StringComparison.Ordinal)
                        && float.TryParse(text.Substring(0, text.Length - 1),
                            NumberStyles.Float, CultureInfo.InvariantCulture, out var percent))
                    {
                        result = new CssPercentageValue(percent);
                        return true;
                    }
                    return false;

                case "length":
                case "length-percentage":
                case "angle":
                case "time":
                case "frequency":
                    return TryParseDimension(text, out result);

                default:
                    result = new CssStringValue(text);
                    return true;
            }
        }

        private static bool TryParseDimension(string text, out CssValue result)
        {
            result = GuaranteedInvalidValue.Instance;
            int index = 0;
            int numberStart = index;
            if (index < text.Length && (text[index] == '+' || text[index] == '-')) { index++; }
            while (index < text.Length && (char.IsDigit(text[index]) || text[index] == '.')) { index++; }
            if (index == numberStart || (index == numberStart + 1 && !char.IsDigit(text[numberStart])))
            {
                return false;
            }
            if (!float.TryParse(text.Substring(numberStart, index - numberStart),
                NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                return false;
            }

            string unit = text.Substring(index).Trim();
            if (unit.Length == 0)
            {
                // A typed length attribute must carry a unit (CSS-VALUES-5 §8); bare numbers are invalid.
                return false;
            }
            result = new CssDimensionValue(value, unit);
            return true;
        }

        private static bool TryParseColor(string text, out CssColor color)
        {
            if (text.StartsWith("#", StringComparison.Ordinal))
            {
                return CssColorParser.TryParseHex(text, out color);
            }
            return CssColorParser.TryParseNamed(text, out color);
        }
    }
}
