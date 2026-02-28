using System;
using System.Collections.Generic;

namespace Rend.Css.Parser.Internal
{
    /// <summary>
    /// Expands CSS shorthand properties into their longhand equivalents.
    /// Called during parsing so that the cascade only deals with longhand properties.
    /// </summary>
    internal static class CssShorthandExpander
    {
        /// <summary>
        /// If the property is a shorthand, expand it into longhand declarations.
        /// Returns true if expanded (results added to output list), false if not a shorthand.
        /// </summary>
        public static bool TryExpand(string property, CssValue value, bool important, List<CssDeclaration> output)
        {
            switch (property)
            {
                case "margin": return ExpandBox(value, important, output, "margin-top", "margin-right", "margin-bottom", "margin-left");
                case "padding": return ExpandBox(value, important, output, "padding-top", "padding-right", "padding-bottom", "padding-left");

                case "border": return ExpandBorder(value, important, output);
                case "border-top": return ExpandBorderSide(value, important, output, "top");
                case "border-right": return ExpandBorderSide(value, important, output, "right");
                case "border-bottom": return ExpandBorderSide(value, important, output, "bottom");
                case "border-left": return ExpandBorderSide(value, important, output, "left");

                case "border-width": return ExpandBox(value, important, output, "border-top-width", "border-right-width", "border-bottom-width", "border-left-width");
                case "border-style": return ExpandBox(value, important, output, "border-top-style", "border-right-style", "border-bottom-style", "border-left-style");
                case "border-color": return ExpandBox(value, important, output, "border-top-color", "border-right-color", "border-bottom-color", "border-left-color");

                case "border-radius": return ExpandBorderRadius(value, important, output);

                case "font": return ExpandFont(value, important, output);

                case "flex": return ExpandFlex(value, important, output);
                case "flex-flow": return ExpandFlexFlow(value, important, output);

                case "background": return ExpandBackground(value, important, output);

                case "list-style": return ExpandListStyle(value, important, output);

                case "overflow": return ExpandTwoValue(value, important, output, "overflow-x", "overflow-y");
                case "gap": return ExpandTwoValue(value, important, output, "row-gap", "column-gap");

                case "outline": return ExpandOutline(value, important, output);

                case "text-decoration": return ExpandTextDecoration(value, important, output);

                default: return false;
            }
        }

        #region Box Model (1-4 value pattern)

        /// <summary>
        /// Expand a 1-4 value shorthand: margin, padding, border-width, etc.
        /// 1 value: all four. 2 values: top/bottom, left/right. 3: top, left/right, bottom. 4: all separate.
        /// </summary>
        private static bool ExpandBox(CssValue value, bool important, List<CssDeclaration> output,
            string top, string right, string bottom, string left)
        {
            var parts = GetListValues(value);

            CssValue vTop, vRight, vBottom, vLeft;

            switch (parts.Count)
            {
                case 1:
                    vTop = vRight = vBottom = vLeft = parts[0];
                    break;
                case 2:
                    vTop = vBottom = parts[0];
                    vRight = vLeft = parts[1];
                    break;
                case 3:
                    vTop = parts[0];
                    vRight = vLeft = parts[1];
                    vBottom = parts[2];
                    break;
                default: // 4+
                    vTop = parts[0];
                    vRight = parts[1];
                    vBottom = parts[2];
                    vLeft = parts.Count > 3 ? parts[3] : parts[2];
                    break;
            }

            output.Add(new CssDeclaration(top, vTop, important));
            output.Add(new CssDeclaration(right, vRight, important));
            output.Add(new CssDeclaration(bottom, vBottom, important));
            output.Add(new CssDeclaration(left, vLeft, important));
            return true;
        }

        /// <summary>
        /// Expand a 1-2 value shorthand: overflow → overflow-x, overflow-y.
        /// </summary>
        private static bool ExpandTwoValue(CssValue value, bool important, List<CssDeclaration> output,
            string first, string second)
        {
            var parts = GetListValues(value);
            output.Add(new CssDeclaration(first, parts[0], important));
            output.Add(new CssDeclaration(second, parts.Count >= 2 ? parts[1] : parts[0], important));
            return true;
        }

        #endregion

        #region Border

        private static bool ExpandBorder(CssValue value, bool important, List<CssDeclaration> output)
        {
            // border: [width] [style] [color]
            ClassifyBorderParts(value, out var width, out var style, out var color);

            foreach (var side in new[] { "top", "right", "bottom", "left" })
            {
                output.Add(new CssDeclaration($"border-{side}-width", width, important));
                output.Add(new CssDeclaration($"border-{side}-style", style, important));
                output.Add(new CssDeclaration($"border-{side}-color", color, important));
            }
            return true;
        }

        private static bool ExpandBorderSide(CssValue value, bool important, List<CssDeclaration> output, string side)
        {
            ClassifyBorderParts(value, out var width, out var style, out var color);
            output.Add(new CssDeclaration($"border-{side}-width", width, important));
            output.Add(new CssDeclaration($"border-{side}-style", style, important));
            output.Add(new CssDeclaration($"border-{side}-color", color, important));
            return true;
        }

        private static void ClassifyBorderParts(CssValue value, out CssValue width, out CssValue style, out CssValue color)
        {
            width = new CssKeywordValue("medium");
            style = new CssKeywordValue("none");
            color = new CssKeywordValue("currentcolor");

            var parts = GetListValues(value);
            foreach (var part in parts)
            {
                if (part is CssDimensionValue || part is CssNumberValue n && n.Value == 0)
                {
                    width = part;
                }
                else if (part is CssKeywordValue kw)
                {
                    if (IsBorderStyleKeyword(kw.Keyword))
                        style = part;
                    else if (IsBorderWidthKeyword(kw.Keyword))
                        width = part;
                    else
                        color = part;
                }
                else if (part is CssColorValue)
                {
                    color = part;
                }
                else
                {
                    // Dimension/percentage — treat as width
                    width = part;
                }
            }
        }

        private static bool IsBorderStyleKeyword(string kw)
        {
            switch (kw)
            {
                case "none": case "hidden": case "dotted": case "dashed":
                case "solid": case "double": case "groove": case "ridge":
                case "inset": case "outset":
                    return true;
                default: return false;
            }
        }

        private static bool IsBorderWidthKeyword(string kw)
        {
            return kw == "thin" || kw == "medium" || kw == "thick";
        }

        #endregion

        #region Border Radius

        private static bool ExpandBorderRadius(CssValue value, bool important, List<CssDeclaration> output)
        {
            // border-radius: TL TR BR BL [/ TL TR BR BL]
            // For v1: ignore the '/' separator (elliptical radii), just expand the first part
            var parts = GetListValues(value);

            // Filter out '/' separator
            var horizontal = new List<CssValue>();
            foreach (var p in parts)
            {
                if (p is CssKeywordValue kw && kw.Keyword == "/") break;
                horizontal.Add(p);
            }

            CssValue tl, tr, br, bl;
            switch (horizontal.Count)
            {
                case 1:
                    tl = tr = br = bl = horizontal[0];
                    break;
                case 2:
                    tl = br = horizontal[0];
                    tr = bl = horizontal[1];
                    break;
                case 3:
                    tl = horizontal[0];
                    tr = bl = horizontal[1];
                    br = horizontal[2];
                    break;
                default:
                    tl = horizontal[0];
                    tr = horizontal.Count > 1 ? horizontal[1] : horizontal[0];
                    br = horizontal.Count > 2 ? horizontal[2] : horizontal[0];
                    bl = horizontal.Count > 3 ? horizontal[3] : horizontal.Count > 1 ? horizontal[1] : horizontal[0];
                    break;
            }

            output.Add(new CssDeclaration("border-top-left-radius", tl, important));
            output.Add(new CssDeclaration("border-top-right-radius", tr, important));
            output.Add(new CssDeclaration("border-bottom-right-radius", br, important));
            output.Add(new CssDeclaration("border-bottom-left-radius", bl, important));
            return true;
        }

        #endregion

        #region Font

        private static bool ExpandFont(CssValue value, bool important, List<CssDeclaration> output)
        {
            // font: [style] [variant] [weight] [stretch] size[/line-height] family
            // This is one of the most complex shorthands. Simplified approach:

            if (value is CssKeywordValue systemFont)
            {
                // System font keywords: caption, icon, menu, message-box, small-caption, status-bar
                // Just pass through as keyword for all sub-properties
                output.Add(new CssDeclaration("font-style", new CssKeywordValue("normal"), important));
                output.Add(new CssDeclaration("font-variant", new CssKeywordValue("normal"), important));
                output.Add(new CssDeclaration("font-weight", new CssKeywordValue("normal"), important));
                output.Add(new CssDeclaration("font-size", new CssKeywordValue("medium"), important));
                output.Add(new CssDeclaration("line-height", new CssKeywordValue("normal"), important));
                output.Add(new CssDeclaration("font-family", systemFont, important));
                return true;
            }

            var parts = GetListValues(value);
            if (parts.Count == 0) return false;

            CssValue fontStyle = new CssKeywordValue("normal");
            CssValue fontVariant = new CssKeywordValue("normal");
            CssValue fontWeight = new CssKeywordValue("normal");
            CssValue? fontSize = null;
            var lineHeight = new CssKeywordValue("normal") as CssValue;
            var familyParts = new List<CssValue>();

            int i = 0;

            // Parse optional style/variant/weight
            while (i < parts.Count)
            {
                if (parts[i] is CssKeywordValue kw)
                {
                    if (kw.Keyword == "italic" || kw.Keyword == "oblique")
                    {
                        fontStyle = kw;
                        i++;
                        continue;
                    }
                    if (kw.Keyword == "small-caps")
                    {
                        fontVariant = kw;
                        i++;
                        continue;
                    }
                    if (IsFontWeightKeyword(kw.Keyword))
                    {
                        fontWeight = kw;
                        i++;
                        continue;
                    }
                    if (kw.Keyword == "normal")
                    {
                        i++;
                        continue;
                    }
                }
                if (parts[i] is CssNumberValue nw && IsFontWeightNumber(nw.Value))
                {
                    fontWeight = parts[i];
                    i++;
                    continue;
                }
                break;
            }

            // Next must be font-size (possibly with /line-height)
            if (i < parts.Count)
            {
                fontSize = parts[i];
                i++;

                // Check for /line-height
                if (i < parts.Count && parts[i] is CssKeywordValue slash && slash.Keyword == "/")
                {
                    i++;
                    if (i < parts.Count)
                    {
                        lineHeight = parts[i];
                        i++;
                    }
                }
            }

            // Remaining = font-family
            while (i < parts.Count)
            {
                familyParts.Add(parts[i]);
                i++;
            }

            output.Add(new CssDeclaration("font-style", fontStyle, important));
            output.Add(new CssDeclaration("font-variant", fontVariant, important));
            output.Add(new CssDeclaration("font-weight", fontWeight, important));
            output.Add(new CssDeclaration("font-size", fontSize ?? new CssKeywordValue("medium"), important));
            output.Add(new CssDeclaration("line-height", lineHeight, important));

            if (familyParts.Count > 0)
                output.Add(new CssDeclaration("font-family", familyParts.Count == 1 ? familyParts[0] : new CssListValue(familyParts, ','), important));
            else
                output.Add(new CssDeclaration("font-family", new CssKeywordValue("inherit"), important));

            return true;
        }

        private static bool IsFontWeightKeyword(string kw)
        {
            return kw == "bold" || kw == "bolder" || kw == "lighter";
        }

        private static bool IsFontWeightNumber(float value)
        {
            return value >= 100 && value <= 900 && value % 100 == 0;
        }

        #endregion

        #region Flex

        private static bool ExpandFlex(CssValue value, bool important, List<CssDeclaration> output)
        {
            // flex: none | [ <flex-grow> <flex-shrink>? || <flex-basis> ]
            if (value is CssKeywordValue kw)
            {
                switch (kw.Keyword)
                {
                    case "none":
                        output.Add(new CssDeclaration("flex-grow", new CssNumberValue(0, true), important));
                        output.Add(new CssDeclaration("flex-shrink", new CssNumberValue(0, true), important));
                        output.Add(new CssDeclaration("flex-basis", new CssKeywordValue("auto"), important));
                        return true;
                    case "auto":
                        output.Add(new CssDeclaration("flex-grow", new CssNumberValue(1, true), important));
                        output.Add(new CssDeclaration("flex-shrink", new CssNumberValue(1, true), important));
                        output.Add(new CssDeclaration("flex-basis", new CssKeywordValue("auto"), important));
                        return true;
                    case "initial":
                        output.Add(new CssDeclaration("flex-grow", new CssNumberValue(0, true), important));
                        output.Add(new CssDeclaration("flex-shrink", new CssNumberValue(1, true), important));
                        output.Add(new CssDeclaration("flex-basis", new CssKeywordValue("auto"), important));
                        return true;
                }
            }

            var parts = GetListValues(value);

            var grow = new CssNumberValue(0, true) as CssValue;
            var shrink = new CssNumberValue(1, true) as CssValue;
            var basis = new CssKeywordValue("auto") as CssValue;

            int numIdx = 0;
            foreach (var p in parts)
            {
                if (p is CssNumberValue || (p is CssKeywordValue k2 && k2.Keyword == "0"))
                {
                    if (numIdx == 0) grow = p;
                    else if (numIdx == 1) shrink = p;
                    numIdx++;
                }
                else if (p is CssDimensionValue || p is CssPercentageValue ||
                         (p is CssKeywordValue kv && (kv.Keyword == "auto" || kv.Keyword == "content")))
                {
                    basis = p;
                }
            }

            // If only a single unitless number: flex-grow with flex-basis 0
            if (parts.Count == 1 && (parts[0] is CssNumberValue))
            {
                basis = new CssDimensionValue(0, "px");
            }

            output.Add(new CssDeclaration("flex-grow", grow, important));
            output.Add(new CssDeclaration("flex-shrink", shrink, important));
            output.Add(new CssDeclaration("flex-basis", basis, important));
            return true;
        }

        private static bool ExpandFlexFlow(CssValue value, bool important, List<CssDeclaration> output)
        {
            // flex-flow: <flex-direction> || <flex-wrap>
            var parts = GetListValues(value);

            CssValue direction = new CssKeywordValue("row");
            CssValue wrap = new CssKeywordValue("nowrap");

            foreach (var p in parts)
            {
                if (p is CssKeywordValue kw)
                {
                    if (IsFlexDirection(kw.Keyword))
                        direction = p;
                    else if (IsFlexWrap(kw.Keyword))
                        wrap = p;
                }
            }

            output.Add(new CssDeclaration("flex-direction", direction, important));
            output.Add(new CssDeclaration("flex-wrap", wrap, important));
            return true;
        }

        private static bool IsFlexDirection(string kw)
        {
            return kw == "row" || kw == "row-reverse" || kw == "column" || kw == "column-reverse";
        }

        private static bool IsFlexWrap(string kw)
        {
            return kw == "nowrap" || kw == "wrap" || kw == "wrap-reverse";
        }

        #endregion

        #region Background (v1: color only)

        private static bool ExpandBackground(CssValue value, bool important, List<CssDeclaration> output)
        {
            // v1: only handle background-color. Other sub-properties get defaults.
            var parts = GetListValues(value);

            CssValue bgColor = new CssKeywordValue("transparent");

            foreach (var p in parts)
            {
                if (p is CssColorValue || (p is CssKeywordValue kw &&
                    (kw.Keyword == "transparent" || kw.Keyword == "currentcolor" || kw.Keyword == "inherit")))
                {
                    bgColor = p;
                }
            }

            output.Add(new CssDeclaration("background-color", bgColor, important));
            output.Add(new CssDeclaration("background-image", new CssKeywordValue("none"), important));
            output.Add(new CssDeclaration("background-repeat", new CssKeywordValue("repeat"), important));
            output.Add(new CssDeclaration("background-position", new CssKeywordValue("0% 0%"), important));
            output.Add(new CssDeclaration("background-size", new CssKeywordValue("auto"), important));
            return true;
        }

        #endregion

        #region List Style

        private static bool ExpandListStyle(CssValue value, bool important, List<CssDeclaration> output)
        {
            // list-style: <type> || <position> || <image>
            var parts = GetListValues(value);

            CssValue type = new CssKeywordValue("disc");
            CssValue position = new CssKeywordValue("outside");
            CssValue image = new CssKeywordValue("none");

            foreach (var p in parts)
            {
                if (p is CssUrlValue)
                {
                    image = p;
                }
                else if (p is CssKeywordValue kw)
                {
                    if (kw.Keyword == "inside" || kw.Keyword == "outside")
                        position = p;
                    else if (kw.Keyword == "none")
                    {
                        // "none" could apply to type or image
                        // Per spec: if only one "none", applies to type. If two, both.
                        type = p;
                    }
                    else
                        type = p;
                }
            }

            output.Add(new CssDeclaration("list-style-type", type, important));
            output.Add(new CssDeclaration("list-style-position", position, important));
            output.Add(new CssDeclaration("list-style-image", image, important));
            return true;
        }

        #endregion

        #region Outline

        private static bool ExpandOutline(CssValue value, bool important, List<CssDeclaration> output)
        {
            // outline: [color] [style] [width]
            var parts = GetListValues(value);

            CssValue color = new CssKeywordValue("invert");
            CssValue style = new CssKeywordValue("none");
            CssValue width = new CssKeywordValue("medium");

            foreach (var p in parts)
            {
                if (p is CssDimensionValue || (p is CssNumberValue nv && nv.Value == 0))
                    width = p;
                else if (p is CssKeywordValue kw)
                {
                    if (IsBorderStyleKeyword(kw.Keyword))
                        style = p;
                    else if (IsBorderWidthKeyword(kw.Keyword))
                        width = p;
                    else
                        color = p;
                }
                else if (p is CssColorValue)
                    color = p;
            }

            output.Add(new CssDeclaration("outline-color", color, important));
            output.Add(new CssDeclaration("outline-style", style, important));
            output.Add(new CssDeclaration("outline-width", width, important));
            return true;
        }

        #endregion

        #region Text Decoration

        private static bool ExpandTextDecoration(CssValue value, bool important, List<CssDeclaration> output)
        {
            // text-decoration: <line> || <style> || <color>
            var parts = GetListValues(value);

            CssValue line = new CssKeywordValue("none");
            CssValue decoStyle = new CssKeywordValue("solid");
            CssValue color = new CssKeywordValue("currentcolor");

            foreach (var p in parts)
            {
                if (p is CssKeywordValue kw)
                {
                    if (kw.Keyword == "underline" || kw.Keyword == "overline" ||
                        kw.Keyword == "line-through" || kw.Keyword == "none")
                        line = p;
                    else if (kw.Keyword == "solid" || kw.Keyword == "double" ||
                             kw.Keyword == "dotted" || kw.Keyword == "dashed" ||
                             kw.Keyword == "wavy")
                        decoStyle = p;
                    else
                        color = p;
                }
                else if (p is CssColorValue)
                    color = p;
            }

            output.Add(new CssDeclaration("text-decoration-line", line, important));
            output.Add(new CssDeclaration("text-decoration-style", decoStyle, important));
            output.Add(new CssDeclaration("text-decoration-color", color, important));
            return true;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Get the constituent values from a CssValue.
        /// If it's a list, return the items. Otherwise return a single-item list.
        /// </summary>
        private static List<CssValue> GetListValues(CssValue value)
        {
            if (value is CssListValue list)
            {
                var result = new List<CssValue>(list.Values.Count);
                for (int i = 0; i < list.Values.Count; i++)
                    result.Add(list.Values[i]);
                return result;
            }
            return new List<CssValue> { value };
        }

        #endregion
    }
}
