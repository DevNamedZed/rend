using System;
using System.Collections.Generic;
using Rend.Css.Properties.Internal;

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
            // If the value contains var(), we can't expand the shorthand at parse time
            // because we don't know the resolved value yet. Pass it through to all
            // relevant longhand properties and let var() substitution happen later.
            if (ContainsVar(value))
            {
                return TryExpandVarShorthand(property, value, important, output);
            }

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
                case "border-spacing": return ExpandTwoValue(value, important, output, "border-spacing-h", "border-spacing-v");
                case "place-content": return ExpandTwoValue(value, important, output, "align-content", "justify-content");
                case "place-items": return ExpandTwoValue(value, important, output, "align-items", "justify-items");
                case "place-self": return ExpandTwoValue(value, important, output, "align-self", "justify-self");

                case "outline": return ExpandOutline(value, important, output);

                case "text-decoration": return ExpandTextDecoration(value, important, output);

                case "columns": return ExpandColumns(value, important, output);
                case "column-rule": return ExpandColumnRule(value, important, output);

                case "inset": return ExpandBox(value, important, output, "top", "right", "bottom", "left");

                case "grid-row": return ExpandGridLine(value, important, output, "grid-row-start", "grid-row-end");
                case "grid-column": return ExpandGridLine(value, important, output, "grid-column-start", "grid-column-end");
                case "grid-area": return ExpandGridArea(value, important, output);

                case "border-image": return ExpandBorderImage(value, important, output);

                // Logical properties (block = top/bottom, inline = left/right in LTR horizontal)
                case "margin-block": return ExpandTwoValue(value, important, output, "margin-top", "margin-bottom");
                case "margin-inline": return ExpandTwoValue(value, important, output, "margin-left", "margin-right");
                case "padding-block": return ExpandTwoValue(value, important, output, "padding-top", "padding-bottom");
                case "padding-inline": return ExpandTwoValue(value, important, output, "padding-left", "padding-right");
                case "inset-block": return ExpandTwoValue(value, important, output, "top", "bottom");
                case "inset-inline": return ExpandTwoValue(value, important, output, "left", "right");

                case "border-block-width": return ExpandTwoValue(value, important, output, "border-top-width", "border-bottom-width");
                case "border-block-style": return ExpandTwoValue(value, important, output, "border-top-style", "border-bottom-style");
                case "border-block-color": return ExpandTwoValue(value, important, output, "border-top-color", "border-bottom-color");
                case "border-inline-width": return ExpandTwoValue(value, important, output, "border-left-width", "border-right-width");
                case "border-inline-style": return ExpandTwoValue(value, important, output, "border-left-style", "border-right-style");
                case "border-inline-color": return ExpandTwoValue(value, important, output, "border-left-color", "border-right-color");

                case "border-block": return ExpandLogicalBorderPair(value, important, output, "top", "bottom");
                case "border-inline": return ExpandLogicalBorderPair(value, important, output, "left", "right");
                case "border-block-start": return ExpandBorderSide(value, important, output, "top");
                case "border-block-end": return ExpandBorderSide(value, important, output, "bottom");
                case "border-inline-start": return ExpandBorderSide(value, important, output, "left");
                case "border-inline-end": return ExpandBorderSide(value, important, output, "right");

                // Single-side logical property aliases
                case "margin-block-start": return Alias(value, important, output, "margin-top");
                case "margin-block-end": return Alias(value, important, output, "margin-bottom");
                case "margin-inline-start": return Alias(value, important, output, "margin-left");
                case "margin-inline-end": return Alias(value, important, output, "margin-right");
                case "padding-block-start": return Alias(value, important, output, "padding-top");
                case "padding-block-end": return Alias(value, important, output, "padding-bottom");
                case "padding-inline-start": return Alias(value, important, output, "padding-left");
                case "padding-inline-end": return Alias(value, important, output, "padding-right");
                case "inset-block-start": return Alias(value, important, output, "top");
                case "inset-block-end": return Alias(value, important, output, "bottom");
                case "inset-inline-start": return Alias(value, important, output, "left");
                case "inset-inline-end": return Alias(value, important, output, "right");
                case "border-block-start-width": return Alias(value, important, output, "border-top-width");
                case "border-block-end-width": return Alias(value, important, output, "border-bottom-width");
                case "border-inline-start-width": return Alias(value, important, output, "border-left-width");
                case "border-inline-end-width": return Alias(value, important, output, "border-right-width");
                case "border-block-start-style": return Alias(value, important, output, "border-top-style");
                case "border-block-end-style": return Alias(value, important, output, "border-bottom-style");
                case "border-inline-start-style": return Alias(value, important, output, "border-left-style");
                case "border-inline-end-style": return Alias(value, important, output, "border-right-style");
                case "border-block-start-color": return Alias(value, important, output, "border-top-color");
                case "border-block-end-color": return Alias(value, important, output, "border-bottom-color");
                case "border-inline-start-color": return Alias(value, important, output, "border-left-color");
                case "border-inline-end-color": return Alias(value, important, output, "border-right-color");

                case "mask": return ExpandMask(value, important, output);

                // Container shorthand: container-name / container-type
                case "container": return ExpandContainer(value, important, output);

                // Compatibility aliases
                case "word-wrap": return Alias(value, important, output, "overflow-wrap");

                case "all": return ExpandAll(value, important, output);

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

        /// <summary>
        /// Simple alias: maps a logical property name to a physical one.
        /// </summary>
        private static bool Alias(CssValue value, bool important, List<CssDeclaration> output, string target)
        {
            output.Add(new CssDeclaration(target, value, important));
            return true;
        }

        /// <summary>
        /// Expand the CSS `all` shorthand: resets all properties except direction and unicode-bidi.
        /// </summary>
        private static bool ExpandContainer(CssValue value, bool important, List<CssDeclaration> output)
        {
            // container: <name> / <type>  or  container: <type>
            var text = value.ToString().Trim();
            int slashIdx = text.IndexOf('/');
            if (slashIdx >= 0)
            {
                var namePart = text.Substring(0, slashIdx).Trim();
                var typePart = text.Substring(slashIdx + 1).Trim();
                output.Add(new CssDeclaration("container-name", new CssKeywordValue(namePart), important));
                output.Add(new CssDeclaration("container-type", new CssKeywordValue(typePart), important));
            }
            else
            {
                // Single value = container-type
                output.Add(new CssDeclaration("container-type", value, important));
            }
            return true;
        }

        private static bool ExpandAll(CssValue value, bool important, List<CssDeclaration> output)
        {
            foreach (var prop in PropertyRegistry.GetAll())
            {
                if (prop.Id == PropertyId.Direction || prop.Id == PropertyId.UnicodeBidi)
                    continue;
                output.Add(new CssDeclaration(prop.Name, value, important));
            }
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

        private static bool ExpandLogicalBorderPair(CssValue value, bool important, List<CssDeclaration> output, string side1, string side2)
        {
            ClassifyBorderParts(value, out var width, out var style, out var color);
            output.Add(new CssDeclaration($"border-{side1}-width", width, important));
            output.Add(new CssDeclaration($"border-{side1}-style", style, important));
            output.Add(new CssDeclaration($"border-{side1}-color", color, important));
            output.Add(new CssDeclaration($"border-{side2}-width", width, important));
            output.Add(new CssDeclaration($"border-{side2}-style", style, important));
            output.Add(new CssDeclaration($"border-{side2}-color", color, important));
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
            var parts = GetListValues(value);

            CssValue bgColor = new CssKeywordValue("transparent");
            CssValue bgImage = new CssKeywordValue("none");
            CssValue bgRepeat = new CssKeywordValue("repeat");
            CssValue bgSize = new CssKeywordValue("auto");
            CssValue bgClip = new CssKeywordValue("border-box");
            CssValue bgOrigin = new CssKeywordValue("padding-box");
            var positionParts = new List<CssValue>();
            bool sizeNext = false;
            var sizeParts = new List<CssValue>();

            for (int i = 0; i < parts.Count; i++)
            {
                var p = parts[i];

                // After a "/" separator, next values are background-size
                if (p is CssKeywordValue slash && slash.Keyword == "/")
                {
                    sizeNext = true;
                    continue;
                }

                if (sizeNext)
                {
                    sizeParts.Add(p);
                    // Size takes at most 2 values
                    if (sizeParts.Count >= 2 || (p is CssKeywordValue sk && (sk.Keyword == "cover" || sk.Keyword == "contain")))
                        sizeNext = false;
                    continue;
                }

                if (p is CssUrlValue || p is CssFunctionValue fn &&
                    (fn.Name == "linear-gradient" || fn.Name == "radial-gradient" ||
                     fn.Name == "conic-gradient" || fn.Name == "-webkit-linear-gradient" ||
                     fn.Name == "-webkit-radial-gradient"))
                {
                    bgImage = p;
                }
                else if (p is CssColorValue)
                {
                    bgColor = p;
                }
                else if (p is CssKeywordValue kw)
                {
                    string k = kw.Keyword;
                    if (k == "transparent" || k == "currentcolor" || k == "inherit")
                        bgColor = p;
                    else if (k == "none")
                        bgImage = p;
                    else if (k == "repeat" || k == "no-repeat" || k == "repeat-x" || k == "repeat-y" ||
                             k == "space" || k == "round")
                        bgRepeat = p;
                    else if (k == "border-box" || k == "padding-box" || k == "content-box")
                    {
                        // First box value is origin, second is clip
                        if (bgOrigin is CssKeywordValue origKw && origKw.Keyword == "padding-box")
                            bgOrigin = p;
                        else
                            bgClip = p;
                    }
                    else if (k == "cover" || k == "contain")
                        bgSize = p;
                    else if (k == "left" || k == "right" || k == "top" || k == "bottom" || k == "center")
                        positionParts.Add(p);
                }
                else if (p is CssPercentageValue || p is CssDimensionValue)
                {
                    positionParts.Add(p);
                }
                else if (p is CssNumberValue num && num.Value == 0)
                {
                    positionParts.Add(p);
                }
            }

            // Build background-position from collected parts
            CssValue bgPosition;
            if (positionParts.Count == 0)
                bgPosition = new CssKeywordValue("0% 0%");
            else if (positionParts.Count == 1)
                bgPosition = positionParts[0];
            else
                bgPosition = new CssListValue(positionParts);

            // Build background-size from collected parts
            if (sizeParts.Count == 1)
                bgSize = sizeParts[0];
            else if (sizeParts.Count >= 2)
                bgSize = new CssListValue(sizeParts);

            output.Add(new CssDeclaration("background-color", bgColor, important));
            output.Add(new CssDeclaration("background-image", bgImage, important));
            output.Add(new CssDeclaration("background-repeat", bgRepeat, important));
            output.Add(new CssDeclaration("background-position", bgPosition, important));
            output.Add(new CssDeclaration("background-size", bgSize, important));
            output.Add(new CssDeclaration("background-clip", bgClip, important));
            output.Add(new CssDeclaration("background-origin", bgOrigin, important));
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

        #region Multi-Column

        /// <summary>
        /// Expand 'columns' shorthand → column-width, column-count.
        /// Format: [column-width] || [column-count]
        /// </summary>
        private static bool ExpandColumns(CssValue value, bool important, List<CssDeclaration> output)
        {
            var parts = GetListValues(value);
            CssValue? width = null;
            CssValue? count = null;

            foreach (var part in parts)
            {
                if (part is CssKeywordValue kw && kw.Keyword == "auto")
                    continue; // auto is the default for both

                if (part is CssDimensionValue || (part is CssNumberValue n && n.Value == 0))
                {
                    width = part;
                }
                else if (part is CssNumberValue num && num.Value >= 1)
                {
                    count = part;
                }
            }

            var auto = new CssKeywordValue("auto");
            output.Add(new CssDeclaration("column-width", width ?? auto, important));
            output.Add(new CssDeclaration("column-count", count ?? auto, important));
            return true;
        }

        /// <summary>
        /// Expand 'column-rule' shorthand → column-rule-width, column-rule-style, column-rule-color.
        /// Same format as border shorthand.
        /// </summary>
        private static bool ExpandColumnRule(CssValue value, bool important, List<CssDeclaration> output)
        {
            var parts = GetListValues(value);
            CssValue? width = null;
            CssValue? style = null;
            CssValue? color = null;

            foreach (var part in parts)
            {
                if (part is CssKeywordValue kw)
                {
                    if (IsBorderStyleKeyword(kw.Keyword))
                        style = part;
                    else if (IsBorderWidthKeyword(kw.Keyword))
                        width = part;
                    else
                        color = part;
                }
                else if (part is CssColorValue || part is CssFunctionValue)
                {
                    color = part;
                }
                else if (part is CssDimensionValue || part is CssNumberValue)
                {
                    width = part;
                }
            }

            output.Add(new CssDeclaration("column-rule-width", width ?? new CssKeywordValue("medium"), important));
            output.Add(new CssDeclaration("column-rule-style", style ?? new CssKeywordValue("none"), important));
            output.Add(new CssDeclaration("column-rule-color", color ?? new CssKeywordValue("currentColor"), important));
            return true;
        }

        #endregion

        #region Grid

        /// <summary>
        /// Expands grid-row / grid-column shorthands.
        /// Format: &lt;start&gt; / &lt;end&gt; or just &lt;start&gt; (end defaults to auto).
        /// </summary>
        private static bool ExpandGridLine(CssValue value, bool important, List<CssDeclaration> output,
            string startProp, string endProp)
        {
            var parts = GetListValues(value);
            int slashIdx = -1;
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i] is CssKeywordValue kw && kw.Keyword == "/")
                {
                    slashIdx = i;
                    break;
                }
            }

            if (slashIdx >= 0 && slashIdx + 1 < parts.Count)
            {
                // Collect values before and after slash
                var startParts = parts.GetRange(0, slashIdx);
                var endParts = parts.GetRange(slashIdx + 1, parts.Count - slashIdx - 1);
                output.Add(new CssDeclaration(startProp,
                    startParts.Count == 1 ? startParts[0] : new CssListValue(startParts), important));
                output.Add(new CssDeclaration(endProp,
                    endParts.Count == 1 ? endParts[0] : new CssListValue(endParts), important));
            }
            else
            {
                output.Add(new CssDeclaration(startProp, value, important));
                output.Add(new CssDeclaration(endProp, new CssKeywordValue("auto"), important));
            }

            return true;
        }

        /// <summary>
        /// Expands grid-area: row-start / column-start / row-end / column-end.
        /// Missing values default to auto.
        /// </summary>
        private static bool ExpandGridArea(CssValue value, bool important, List<CssDeclaration> output)
        {
            var parts = GetListValues(value);

            // Split on "/" separators
            var segments = new List<CssValue>[4];
            for (int i = 0; i < 4; i++) segments[i] = new List<CssValue>();
            int segIdx = 0;
            for (int i = 0; i < parts.Count && segIdx < 4; i++)
            {
                if (parts[i] is CssKeywordValue kw && kw.Keyword == "/")
                {
                    segIdx++;
                    continue;
                }
                segments[segIdx].Add(parts[i]);
            }

            string[] props = { "grid-row-start", "grid-column-start", "grid-row-end", "grid-column-end" };
            var autoVal = new CssKeywordValue("auto");
            for (int i = 0; i < 4; i++)
            {
                CssValue val = segments[i].Count == 1 ? segments[i][0]
                    : segments[i].Count > 1 ? new CssListValue(segments[i])
                    : (CssValue)autoVal;
                output.Add(new CssDeclaration(props[i], val, important));
            }
            return true;
        }

        #endregion

        #region Border Image

        /// <summary>
        /// Expands border-image shorthand.
        /// Format: source || slice [/ width [/ outset]] || repeat
        /// Simplified: we parse source (url/gradient), slice numbers, repeat keywords.
        /// </summary>
        private static bool ExpandBorderImage(CssValue value, bool important, List<CssDeclaration> output)
        {
            var parts = GetListValues(value);
            CssValue source = new CssKeywordValue("none");
            CssValue slice = new CssNumberValue(100); // default: 100%
            CssValue repeat = new CssKeywordValue("stretch");

            for (int i = 0; i < parts.Count; i++)
            {
                var p = parts[i];
                if (p is CssUrlValue || (p is CssFunctionValue fn && (fn.Name == "linear-gradient" || fn.Name == "radial-gradient")))
                {
                    source = p;
                }
                else if (p is CssKeywordValue kw)
                {
                    if (kw.Keyword == "stretch" || kw.Keyword == "repeat" || kw.Keyword == "round" || kw.Keyword == "space")
                        repeat = p;
                    else if (kw.Keyword == "fill")
                        continue; // skip fill keyword for now
                    else if (kw.Keyword == "none")
                        source = p;
                }
                else if (p is CssNumberValue || p is CssPercentageValue || p is CssDimensionValue)
                {
                    slice = p;
                }
            }

            output.Add(new CssDeclaration("border-image-source", source, important));
            output.Add(new CssDeclaration("border-image-slice", slice, important));
            output.Add(new CssDeclaration("border-image-width", new CssNumberValue(1), important));
            output.Add(new CssDeclaration("border-image-outset", new CssNumberValue(0), important));
            output.Add(new CssDeclaration("border-image-repeat", repeat, important));

            return true;
        }

        #endregion

        #region Mask

        /// <summary>
        /// Expand 'mask' shorthand into longhand properties.
        /// Simplified: treat the first url/gradient as mask-image, keywords as mask-repeat/mask-mode.
        /// </summary>
        private static bool ExpandMask(CssValue value, bool important, List<CssDeclaration> output)
        {
            // For a simple single-layer mask, the most common pattern is:
            // mask: url(...) no-repeat / contain
            // mask: linear-gradient(...)
            // We'll extract what we can and set the longhands.

            var parts = GetListValues(value);
            CssValue? imageVal = null;
            CssValue? repeatVal = null;
            CssValue? modeVal = null;

            for (int i = 0; i < parts.Count; i++)
            {
                var p = parts[i];
                if (p is CssFunctionValue || p is CssUrlValue)
                {
                    imageVal = p;
                }
                else if (p is CssKeywordValue kw)
                {
                    string k = kw.Keyword;
                    if (k == "no-repeat" || k == "repeat" || k == "repeat-x" || k == "repeat-y" || k == "space" || k == "round")
                        repeatVal = p;
                    else if (k == "alpha" || k == "luminance" || k == "match-source")
                        modeVal = p;
                    else if (k == "none")
                        imageVal = p;
                }
            }

            if (imageVal != null)
                output.Add(new CssDeclaration("mask-image", imageVal, important));
            if (repeatVal != null)
                output.Add(new CssDeclaration("mask-repeat", repeatVal, important));
            if (modeVal != null)
                output.Add(new CssDeclaration("mask-mode", modeVal, important));

            return true;
        }

        #endregion

        #region Var() Passthrough

        /// <summary>
        /// Returns true if the value is or contains a var() function.
        /// </summary>
        private static bool ContainsVar(CssValue value)
        {
            if (value is CssFunctionValue fn)
            {
                if (fn.Name == "var") return true;
                for (int i = 0; i < fn.Arguments.Count; i++)
                    if (ContainsVar(fn.Arguments[i])) return true;
                return false;
            }
            if (value is CssListValue list)
            {
                for (int i = 0; i < list.Values.Count; i++)
                    if (ContainsVar(list.Values[i]))
                        return true;
            }
            return false;
        }

        /// <summary>
        /// Shorthand longhand mappings for var() passthrough. When a shorthand value
        /// contains var(), we pass the whole value to each longhand so that var()
        /// substitution during style resolution will produce the correct value.
        /// </summary>
        private static readonly Dictionary<string, string[]> ShorthandLonghands = new()
        {
            ["margin"] = new[] { "margin-top", "margin-right", "margin-bottom", "margin-left" },
            ["padding"] = new[] { "padding-top", "padding-right", "padding-bottom", "padding-left" },
            ["border"] = new[] { "border-top-width", "border-right-width", "border-bottom-width", "border-left-width",
                                 "border-top-style", "border-right-style", "border-bottom-style", "border-left-style",
                                 "border-top-color", "border-right-color", "border-bottom-color", "border-left-color" },
            ["border-width"] = new[] { "border-top-width", "border-right-width", "border-bottom-width", "border-left-width" },
            ["border-style"] = new[] { "border-top-style", "border-right-style", "border-bottom-style", "border-left-style" },
            ["border-color"] = new[] { "border-top-color", "border-right-color", "border-bottom-color", "border-left-color" },
            ["border-radius"] = new[] { "border-top-left-radius", "border-top-right-radius", "border-bottom-right-radius", "border-bottom-left-radius" },
            ["background"] = new[] { "background-color" },
            ["font"] = new[] { "font-size", "font-family" },
            ["flex"] = new[] { "flex-grow", "flex-shrink", "flex-basis" },
            ["outline"] = new[] { "outline-width", "outline-style", "outline-color" },
            ["gap"] = new[] { "row-gap", "column-gap" },
            ["inset"] = new[] { "top", "right", "bottom", "left" },
            ["overflow"] = new[] { "overflow-x", "overflow-y" },
            ["text-decoration"] = new[] { "text-decoration-line", "text-decoration-color", "text-decoration-style" },
            ["border-top"] = new[] { "border-top-width", "border-top-style", "border-top-color" },
            ["border-right"] = new[] { "border-right-width", "border-right-style", "border-right-color" },
            ["border-bottom"] = new[] { "border-bottom-width", "border-bottom-style", "border-bottom-color" },
            ["border-left"] = new[] { "border-left-width", "border-left-style", "border-left-color" },
        };

        private static bool TryExpandVarShorthand(string property, CssValue value, bool important, List<CssDeclaration> output)
        {
            if (ShorthandLonghands.TryGetValue(property, out var longhands))
            {
                foreach (var lh in longhands)
                    output.Add(new CssDeclaration(lh, value, important));
                return true;
            }

            // Not a known shorthand — return false to let it be treated as a longhand
            return false;
        }

        #endregion
    }
}
