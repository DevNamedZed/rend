using System;
using System.Collections.Generic;
using System.Text;
using Rend.Core.Values;

namespace Rend.Css
{
    /// <summary>
    /// Abstract base for CSS values produced by the parser.
    /// Represents the parsed form of a CSS declaration value before resolution.
    /// </summary>
    public abstract class CssValue
    {
        public abstract CssValueKind Kind { get; }
        public abstract override string ToString();
    }

    /// <summary>Kind discriminator for CssValue subclasses.</summary>
    public enum CssValueKind : byte
    {
        Keyword, Number, Dimension, Percentage, Color, String, Url, Function, List
    }

    /// <summary>A CSS keyword value (e.g. "auto", "inherit", "block").</summary>
    public sealed class CssKeywordValue : CssValue
    {
        public override CssValueKind Kind => CssValueKind.Keyword;
        public string Keyword { get; }

        public CssKeywordValue(string keyword)
        {
            Keyword = keyword ?? throw new ArgumentNullException(nameof(keyword));
        }

        public override string ToString() => Keyword;
    }

    /// <summary>A CSS numeric value without a unit (e.g. "0", "1.5").</summary>
    public sealed class CssNumberValue : CssValue
    {
        public override CssValueKind Kind => CssValueKind.Number;
        public float Value { get; }
        public bool IsInteger { get; }

        public CssNumberValue(float value, bool isInteger = false)
        {
            Value = value;
            IsInteger = isInteger;
        }

        public override string ToString() => IsInteger ? ((int)Value).ToString() : Value.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>A CSS dimension value (number + unit, e.g. "10px", "2em", "90deg").</summary>
    public sealed class CssDimensionValue : CssValue
    {
        public override CssValueKind Kind => CssValueKind.Dimension;
        public float Value { get; }
        public string Unit { get; }

        public CssDimensionValue(float value, string unit)
        {
            Value = value;
            Unit = unit ?? throw new ArgumentNullException(nameof(unit));
        }

        public override string ToString() => $"{Value.ToString("G", System.Globalization.CultureInfo.InvariantCulture)}{Unit}";
    }

    /// <summary>A CSS percentage value (e.g. "50%").</summary>
    public sealed class CssPercentageValue : CssValue
    {
        public override CssValueKind Kind => CssValueKind.Percentage;
        public float Value { get; }

        public CssPercentageValue(float value)
        {
            Value = value;
        }

        public override string ToString() => $"{Value.ToString("G", System.Globalization.CultureInfo.InvariantCulture)}%";
    }

    /// <summary>A CSS color value (resolved to RGBA).</summary>
    public sealed class CssColorValue : CssValue
    {
        public override CssValueKind Kind => CssValueKind.Color;
        public CssColor Color { get; }

        public CssColorValue(CssColor color)
        {
            Color = color;
        }

        public override string ToString() => Color.ToString();
    }

    /// <summary>A CSS string value (from quotes).</summary>
    public sealed class CssStringValue : CssValue
    {
        public override CssValueKind Kind => CssValueKind.String;
        public string Value { get; }

        public CssStringValue(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override string ToString() => $"\"{Value}\"";
    }

    /// <summary>A CSS url() value.</summary>
    public sealed class CssUrlValue : CssValue
    {
        public override CssValueKind Kind => CssValueKind.Url;
        public string Url { get; }

        public CssUrlValue(string url)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
        }

        public override string ToString() => $"url({Url})";
    }

    /// <summary>
    /// A CSS function value (e.g. "calc(100% - 20px)", "var(--color)").
    /// Arguments stored as CssValues — not evaluated in v1.
    /// </summary>
    public sealed class CssFunctionValue : CssValue
    {
        public override CssValueKind Kind => CssValueKind.Function;
        public string Name { get; }
        public IReadOnlyList<CssValue> Arguments { get; }

        public CssFunctionValue(string name, List<CssValue> arguments)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Name).Append('(');
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(Arguments[i]);
            }
            sb.Append(')');
            return sb.ToString();
        }
    }

    /// <summary>
    /// A CSS value list (space-separated or comma-separated values).
    /// </summary>
    public sealed class CssListValue : CssValue
    {
        public override CssValueKind Kind => CssValueKind.List;
        public IReadOnlyList<CssValue> Values { get; }
        public char Separator { get; }

        public CssListValue(List<CssValue> values, char separator = ' ')
        {
            Values = values ?? throw new ArgumentNullException(nameof(values));
            Separator = separator;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var sep = Separator == ',' ? ", " : " ";
            for (int i = 0; i < Values.Count; i++)
            {
                if (i > 0) sb.Append(sep);
                sb.Append(Values[i]);
            }
            return sb.ToString();
        }
    }
}
