using System;
using System.Globalization;
using Rend.Core.Values;
using Rend.Html;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Renders MathML elements by traversing the DOM subtree and drawing
    /// fractions, radicals, scripts, operators, and text using IRenderTarget calls.
    /// </summary>
    internal static class MathmlRenderer
    {
        private const float BaseFontSize = 16f;
        private const float ScriptScale = 0.7f;
        private const float FractionBarThickness = 1f;
        private const float FractionGap = 2f;
        private const float RadicalGap = 2f;
        private const float ScriptShift = 4f;
        private const float OperatorPadding = 3f;

        private static readonly CssColor TextColor = CssColor.Black;

        /// <summary>
        /// Render a &lt;math&gt; element into the given target at the specified content rect.
        /// </summary>
        public static void Render(Element mathElement, IRenderTarget target, RectF contentRect)
        {
            target.Save();
            target.PushClipRect(contentRect);

            float fontSize = BaseFontSize;
            string? mathsize = mathElement.GetAttribute("mathsize");
            if (mathsize != null)
                fontSize = ParseLength(mathsize, BaseFontSize);

            // Measure the math content to center it in the content rect
            var size = Measure(mathElement, fontSize);
            float x = contentRect.X + 2f; // small left padding
            float y = contentRect.Y + (contentRect.Height - size.Height) / 2f + size.Ascent;

            // Render the math tree
            RenderElement(mathElement, target, ref x, y, fontSize);

            target.PopClip();
            target.Restore();
        }

        /// <summary>
        /// Measure the intrinsic size of a math element tree.
        /// </summary>
        public static MathSize MeasureElement(Element element, float fontSize)
        {
            return Measure(element, fontSize);
        }

        private static void RenderElement(Element element, IRenderTarget target,
            ref float x, float baseline, float fontSize)
        {
            string tag = element.TagName;

            switch (tag)
            {
                case "math":
                case "mrow":
                case "mstyle":
                case "merror":
                case "mpadded":
                    RenderChildren(element, target, ref x, baseline, fontSize);
                    break;

                case "mi":
                    RenderToken(element, target, ref x, baseline, fontSize, italic: true);
                    break;

                case "mn":
                case "mtext":
                    RenderToken(element, target, ref x, baseline, fontSize, italic: false);
                    break;

                case "mo":
                    RenderOperator(element, target, ref x, baseline, fontSize);
                    break;

                case "mfrac":
                    RenderFraction(element, target, ref x, baseline, fontSize);
                    break;

                case "msqrt":
                    RenderSquareRoot(element, target, ref x, baseline, fontSize);
                    break;

                case "mroot":
                    RenderRoot(element, target, ref x, baseline, fontSize);
                    break;

                case "msub":
                    RenderSubscript(element, target, ref x, baseline, fontSize);
                    break;

                case "msup":
                    RenderSuperscript(element, target, ref x, baseline, fontSize);
                    break;

                case "msubsup":
                    RenderSubSup(element, target, ref x, baseline, fontSize);
                    break;

                case "mover":
                    RenderOverscript(element, target, ref x, baseline, fontSize);
                    break;

                case "munder":
                    RenderUnderscript(element, target, ref x, baseline, fontSize);
                    break;

                case "munderover":
                    RenderUnderOverscript(element, target, ref x, baseline, fontSize);
                    break;

                case "mspace":
                    float width = ParseLength(element.GetAttribute("width"), 0f);
                    x += width;
                    break;

                case "mphantom":
                    // Invisible — just advance x by the measured width
                    var phantomSize = Measure(element, fontSize);
                    x += phantomSize.Width;
                    break;

                default:
                    // Unknown element — render text content
                    RenderChildren(element, target, ref x, baseline, fontSize);
                    break;
            }
        }

        private static void RenderChildren(Element element, IRenderTarget target,
            ref float x, float baseline, float fontSize)
        {
            var child = element.FirstChild;
            while (child != null)
            {
                if (child is Element childEl)
                {
                    RenderElement(childEl, target, ref x, baseline, fontSize);
                }
                else if (child is TextNode textNode)
                {
                    string text = textNode.Data?.Trim() ?? "";
                    if (text.Length > 0)
                    {
                        DrawMathText(target, text, x, baseline, fontSize, false);
                        x += EstimateTextWidth(text, fontSize);
                    }
                }
                child = child.NextSibling;
            }
        }

        private static void RenderToken(Element element, IRenderTarget target,
            ref float x, float baseline, float fontSize, bool italic)
        {
            string text = GetTextContent(element).Trim();
            if (text.Length == 0) return;

            // Check mathvariant attribute
            string? variant = element.GetAttribute("mathvariant");
            if (variant == "normal") italic = false;
            else if (variant == "italic") italic = true;
            else if (variant == "bold-italic") italic = true;

            // For <mi> with single character, render italic by default
            if (element.TagName == "mi" && text.Length == 1 && variant == null)
                italic = true;
            else if (element.TagName == "mi" && text.Length > 1 && variant == null)
                italic = false; // Multi-character identifiers: normal (like "sin", "cos")

            DrawMathText(target, text, x, baseline, fontSize, italic);
            x += EstimateTextWidth(text, fontSize);
        }

        private static void RenderOperator(Element element, IRenderTarget target,
            ref float x, float baseline, float fontSize)
        {
            string text = GetTextContent(element).Trim();
            if (text.Length == 0) return;

            // Add padding around operators
            x += OperatorPadding;
            DrawMathText(target, text, x, baseline, fontSize, false);
            x += EstimateTextWidth(text, fontSize);
            x += OperatorPadding;
        }

        private static void RenderFraction(Element element, IRenderTarget target,
            ref float x, float baseline, float fontSize)
        {
            // First child = numerator, second child = denominator
            Element? numerator = GetChildElement(element, 0);
            Element? denominator = GetChildElement(element, 1);
            if (numerator == null) return;

            float scriptSize = fontSize * ScriptScale;

            var numSize = Measure(numerator, scriptSize);
            var denSize = denominator != null ? Measure(denominator, scriptSize) : new MathSize(0, 0, 0);

            float fracWidth = Math.Max(numSize.Width, denSize.Width) + 4f;
            float barY = baseline - fontSize * 0.3f; // Position bar at math axis

            // Draw numerator centered above bar
            float numX = x + (fracWidth - numSize.Width) / 2f;
            float numBaseline = barY - FractionGap - numSize.Descent;
            RenderElement(numerator, target, ref numX, numBaseline, scriptSize);

            // Draw fraction bar
            var barRect = new RectF(x, barY - FractionBarThickness / 2f, fracWidth, FractionBarThickness);
            target.FillRect(barRect, BrushInfo.Solid(TextColor));

            // Draw denominator centered below bar
            if (denominator != null)
            {
                float denX = x + (fracWidth - denSize.Width) / 2f;
                float denBaseline = barY + FractionGap + FractionBarThickness + denSize.Ascent;
                RenderElement(denominator, target, ref denX, denBaseline, scriptSize);
            }

            x += fracWidth;
        }

        private static void RenderSquareRoot(Element element, IRenderTarget target,
            ref float x, float baseline, float fontSize)
        {
            // Measure content
            var contentSize = MeasureChildren(element, fontSize);
            float radicalWidth = fontSize * 0.6f;
            float totalHeight = contentSize.Height + RadicalGap;

            float top = baseline - contentSize.Ascent - RadicalGap;

            // Draw radical sign (√)
            var radPath = new PathData();
            float rx = x;
            float tick = radicalWidth * 0.3f;
            radPath.MoveTo(rx, baseline - fontSize * 0.15f);
            radPath.LineTo(rx + tick, baseline);
            radPath.LineTo(rx + radicalWidth, top);
            target.StrokePath(radPath, new PenInfo(TextColor, 1f));

            // Draw top bar (vinculum)
            var barPath = new PathData();
            barPath.MoveTo(rx + radicalWidth, top);
            barPath.LineTo(rx + radicalWidth + contentSize.Width + 2f, top);
            target.StrokePath(barPath, new PenInfo(TextColor, 1f));

            // Render content after radical
            float contentX = x + radicalWidth + 1f;
            RenderChildElements(element, target, ref contentX, baseline, fontSize);

            x += radicalWidth + contentSize.Width + 3f;
        }

        private static void RenderRoot(Element element, IRenderTarget target,
            ref float x, float baseline, float fontSize)
        {
            // First child = base, second child = index
            Element? baseEl = GetChildElement(element, 0);
            Element? indexEl = GetChildElement(element, 1);
            if (baseEl == null) return;

            float indexSize = fontSize * ScriptScale * ScriptScale;
            float radicalWidth = fontSize * 0.6f;

            // Draw index (small, upper-left of radical)
            if (indexEl != null)
            {
                var idxSize = Measure(indexEl, indexSize);
                float idxX = x;
                float idxBaseline = baseline - fontSize * 0.6f;
                RenderElement(indexEl, target, ref idxX, idxBaseline, indexSize);
            }

            // Draw radical and content (same as sqrt)
            var contentSize = Measure(baseEl, fontSize);
            float top = baseline - contentSize.Ascent - RadicalGap;

            var radPath = new PathData();
            float rx = x;
            float tick = radicalWidth * 0.3f;
            radPath.MoveTo(rx, baseline - fontSize * 0.15f);
            radPath.LineTo(rx + tick, baseline);
            radPath.LineTo(rx + radicalWidth, top);
            target.StrokePath(radPath, new PenInfo(TextColor, 1f));

            var barPath = new PathData();
            barPath.MoveTo(rx + radicalWidth, top);
            barPath.LineTo(rx + radicalWidth + contentSize.Width + 2f, top);
            target.StrokePath(barPath, new PenInfo(TextColor, 1f));

            float contentX = x + radicalWidth + 1f;
            RenderElement(baseEl, target, ref contentX, baseline, fontSize);

            x += radicalWidth + contentSize.Width + 3f;
        }

        private static void RenderSubscript(Element element, IRenderTarget target,
            ref float x, float baseline, float fontSize)
        {
            Element? baseEl = GetChildElement(element, 0);
            Element? subEl = GetChildElement(element, 1);
            if (baseEl == null) return;

            // Render base at normal size
            RenderElement(baseEl, target, ref x, baseline, fontSize);

            // Render subscript smaller and lower
            if (subEl != null)
            {
                float scriptFontSize = fontSize * ScriptScale;
                float subBaseline = baseline + ScriptShift;
                RenderElement(subEl, target, ref x, subBaseline, scriptFontSize);
            }
        }

        private static void RenderSuperscript(Element element, IRenderTarget target,
            ref float x, float baseline, float fontSize)
        {
            Element? baseEl = GetChildElement(element, 0);
            Element? supEl = GetChildElement(element, 1);
            if (baseEl == null) return;

            // Render base at normal size
            RenderElement(baseEl, target, ref x, baseline, fontSize);

            // Render superscript smaller and higher
            if (supEl != null)
            {
                float scriptFontSize = fontSize * ScriptScale;
                float supBaseline = baseline - fontSize * 0.4f;
                RenderElement(supEl, target, ref x, supBaseline, scriptFontSize);
            }
        }

        private static void RenderSubSup(Element element, IRenderTarget target,
            ref float x, float baseline, float fontSize)
        {
            Element? baseEl = GetChildElement(element, 0);
            Element? subEl = GetChildElement(element, 1);
            Element? supEl = GetChildElement(element, 2);
            if (baseEl == null) return;

            // Render base
            RenderElement(baseEl, target, ref x, baseline, fontSize);

            float scriptFontSize = fontSize * ScriptScale;
            float savedX = x;

            // Render subscript
            if (subEl != null)
            {
                float subX = savedX;
                float subBaseline = baseline + ScriptShift;
                RenderElement(subEl, target, ref subX, subBaseline, scriptFontSize);
                x = Math.Max(x, subX);
            }

            // Render superscript
            if (supEl != null)
            {
                float supX = savedX;
                float supBaseline = baseline - fontSize * 0.4f;
                RenderElement(supEl, target, ref supX, supBaseline, scriptFontSize);
                x = Math.Max(x, supX);
            }
        }

        private static void RenderOverscript(Element element, IRenderTarget target,
            ref float x, float baseline, float fontSize)
        {
            Element? baseEl = GetChildElement(element, 0);
            Element? overEl = GetChildElement(element, 1);
            if (baseEl == null) return;

            float scriptFontSize = fontSize * ScriptScale;
            var baseSize = Measure(baseEl, fontSize);
            var overSize = overEl != null ? Measure(overEl, scriptFontSize) : new MathSize(0, 0, 0);

            float totalWidth = Math.Max(baseSize.Width, overSize.Width);

            // Render overscript centered above base
            if (overEl != null)
            {
                float overX = x + (totalWidth - overSize.Width) / 2f;
                float overBaseline = baseline - baseSize.Ascent - 2f - overSize.Descent;
                RenderElement(overEl, target, ref overX, overBaseline, scriptFontSize);
            }

            // Render base centered
            float baseX = x + (totalWidth - baseSize.Width) / 2f;
            RenderElement(baseEl, target, ref baseX, baseline, fontSize);

            x += totalWidth;
        }

        private static void RenderUnderscript(Element element, IRenderTarget target,
            ref float x, float baseline, float fontSize)
        {
            Element? baseEl = GetChildElement(element, 0);
            Element? underEl = GetChildElement(element, 1);
            if (baseEl == null) return;

            float scriptFontSize = fontSize * ScriptScale;
            var baseSize = Measure(baseEl, fontSize);
            var underSize = underEl != null ? Measure(underEl, scriptFontSize) : new MathSize(0, 0, 0);

            float totalWidth = Math.Max(baseSize.Width, underSize.Width);

            // Render base centered
            float baseX = x + (totalWidth - baseSize.Width) / 2f;
            RenderElement(baseEl, target, ref baseX, baseline, fontSize);

            // Render underscript centered below base
            if (underEl != null)
            {
                float underX = x + (totalWidth - underSize.Width) / 2f;
                float underBaseline = baseline + baseSize.Descent + 2f + underSize.Ascent;
                RenderElement(underEl, target, ref underX, underBaseline, scriptFontSize);
            }

            x += totalWidth;
        }

        private static void RenderUnderOverscript(Element element, IRenderTarget target,
            ref float x, float baseline, float fontSize)
        {
            Element? baseEl = GetChildElement(element, 0);
            Element? underEl = GetChildElement(element, 1);
            Element? overEl = GetChildElement(element, 2);
            if (baseEl == null) return;

            float scriptFontSize = fontSize * ScriptScale;
            var baseSize = Measure(baseEl, fontSize);
            var underSize = underEl != null ? Measure(underEl, scriptFontSize) : new MathSize(0, 0, 0);
            var overSize = overEl != null ? Measure(overEl, scriptFontSize) : new MathSize(0, 0, 0);

            float totalWidth = Math.Max(baseSize.Width, Math.Max(underSize.Width, overSize.Width));

            // Render overscript
            if (overEl != null)
            {
                float overX = x + (totalWidth - overSize.Width) / 2f;
                float overBaseline = baseline - baseSize.Ascent - 2f - overSize.Descent;
                RenderElement(overEl, target, ref overX, overBaseline, scriptFontSize);
            }

            // Render base
            float baseX = x + (totalWidth - baseSize.Width) / 2f;
            RenderElement(baseEl, target, ref baseX, baseline, fontSize);

            // Render underscript
            if (underEl != null)
            {
                float underX = x + (totalWidth - underSize.Width) / 2f;
                float underBaseline = baseline + baseSize.Descent + 2f + underSize.Ascent;
                RenderElement(underEl, target, ref underX, underBaseline, scriptFontSize);
            }

            x += totalWidth;
        }

        // ----- Measurement -----

        private static MathSize Measure(Element element, float fontSize)
        {
            string tag = element.TagName;

            switch (tag)
            {
                case "math":
                case "mrow":
                case "mstyle":
                case "merror":
                case "mpadded":
                case "mphantom":
                    return MeasureChildren(element, fontSize);

                case "mi":
                case "mn":
                case "mtext":
                    return MeasureToken(element, fontSize);

                case "mo":
                    return MeasureOperator(element, fontSize);

                case "mfrac":
                    return MeasureFraction(element, fontSize);

                case "msqrt":
                    return MeasureSquareRoot(element, fontSize);

                case "mroot":
                    return MeasureRoot(element, fontSize);

                case "msub":
                    return MeasureSubscript(element, fontSize);

                case "msup":
                    return MeasureSuperscript(element, fontSize);

                case "msubsup":
                    return MeasureSubSup(element, fontSize);

                case "mover":
                    return MeasureOverscript(element, fontSize);

                case "munder":
                    return MeasureUnderscript(element, fontSize);

                case "munderover":
                    return MeasureUnderOverscript(element, fontSize);

                case "mspace":
                    float w = ParseLength(element.GetAttribute("width"), 0f);
                    float h = ParseLength(element.GetAttribute("height"), fontSize);
                    return new MathSize(w, h * 0.7f, h * 0.3f);

                default:
                    return MeasureChildren(element, fontSize);
            }
        }

        private static MathSize MeasureChildren(Element element, float fontSize)
        {
            float width = 0;
            float ascent = 0;
            float descent = 0;

            var child = element.FirstChild;
            while (child != null)
            {
                if (child is Element childEl)
                {
                    var size = Measure(childEl, fontSize);
                    width += size.Width;
                    ascent = Math.Max(ascent, size.Ascent);
                    descent = Math.Max(descent, size.Descent);
                }
                else if (child is TextNode textNode)
                {
                    string text = textNode.Data?.Trim() ?? "";
                    if (text.Length > 0)
                    {
                        width += EstimateTextWidth(text, fontSize);
                        ascent = Math.Max(ascent, fontSize * 0.75f);
                        descent = Math.Max(descent, fontSize * 0.25f);
                    }
                }
                child = child.NextSibling;
            }

            if (ascent == 0) ascent = fontSize * 0.75f;
            if (descent == 0) descent = fontSize * 0.25f;

            return new MathSize(width, ascent, descent);
        }

        private static MathSize MeasureToken(Element element, float fontSize)
        {
            string text = GetTextContent(element).Trim();
            float w = EstimateTextWidth(text, fontSize);
            return new MathSize(w, fontSize * 0.75f, fontSize * 0.25f);
        }

        private static MathSize MeasureOperator(Element element, float fontSize)
        {
            string text = GetTextContent(element).Trim();
            float w = EstimateTextWidth(text, fontSize) + OperatorPadding * 2;
            return new MathSize(w, fontSize * 0.75f, fontSize * 0.25f);
        }

        private static MathSize MeasureFraction(Element element, float fontSize)
        {
            Element? numerator = GetChildElement(element, 0);
            Element? denominator = GetChildElement(element, 1);

            float scriptSize = fontSize * ScriptScale;
            var numSize = numerator != null ? Measure(numerator, scriptSize) : new MathSize(0, 0, 0);
            var denSize = denominator != null ? Measure(denominator, scriptSize) : new MathSize(0, 0, 0);

            float width = Math.Max(numSize.Width, denSize.Width) + 4f;
            float ascent = numSize.Height + FractionGap + FractionBarThickness / 2f + fontSize * 0.3f;
            float descent = denSize.Height + FractionGap + FractionBarThickness / 2f - fontSize * 0.3f + fontSize * 0.25f;

            return new MathSize(width, ascent, descent);
        }

        private static MathSize MeasureSquareRoot(Element element, float fontSize)
        {
            var contentSize = MeasureChildren(element, fontSize);
            float radicalWidth = fontSize * 0.6f;
            float width = radicalWidth + contentSize.Width + 3f;
            float ascent = contentSize.Ascent + RadicalGap;
            return new MathSize(width, ascent, contentSize.Descent);
        }

        private static MathSize MeasureRoot(Element element, float fontSize)
        {
            Element? baseEl = GetChildElement(element, 0);
            var contentSize = baseEl != null ? Measure(baseEl, fontSize) : new MathSize(0, 0, 0);
            float radicalWidth = fontSize * 0.6f;
            float width = radicalWidth + contentSize.Width + 3f;
            float ascent = contentSize.Ascent + RadicalGap;
            return new MathSize(width, ascent, contentSize.Descent);
        }

        private static MathSize MeasureSubscript(Element element, float fontSize)
        {
            Element? baseEl = GetChildElement(element, 0);
            Element? subEl = GetChildElement(element, 1);

            var baseSize = baseEl != null ? Measure(baseEl, fontSize) : new MathSize(0, 0, 0);
            var subSize = subEl != null ? Measure(subEl, fontSize * ScriptScale) : new MathSize(0, 0, 0);

            float width = baseSize.Width + subSize.Width;
            float descent = Math.Max(baseSize.Descent, ScriptShift + subSize.Height - subSize.Ascent);

            return new MathSize(width, baseSize.Ascent, descent);
        }

        private static MathSize MeasureSuperscript(Element element, float fontSize)
        {
            Element? baseEl = GetChildElement(element, 0);
            Element? supEl = GetChildElement(element, 1);

            var baseSize = baseEl != null ? Measure(baseEl, fontSize) : new MathSize(0, 0, 0);
            var supSize = supEl != null ? Measure(supEl, fontSize * ScriptScale) : new MathSize(0, 0, 0);

            float width = baseSize.Width + supSize.Width;
            float ascent = Math.Max(baseSize.Ascent, fontSize * 0.4f + supSize.Height);

            return new MathSize(width, ascent, baseSize.Descent);
        }

        private static MathSize MeasureSubSup(Element element, float fontSize)
        {
            Element? baseEl = GetChildElement(element, 0);
            Element? subEl = GetChildElement(element, 1);
            Element? supEl = GetChildElement(element, 2);

            var baseSize = baseEl != null ? Measure(baseEl, fontSize) : new MathSize(0, 0, 0);
            float scriptFontSize = fontSize * ScriptScale;
            var subSize = subEl != null ? Measure(subEl, scriptFontSize) : new MathSize(0, 0, 0);
            var supSize = supEl != null ? Measure(supEl, scriptFontSize) : new MathSize(0, 0, 0);

            float scriptWidth = Math.Max(subSize.Width, supSize.Width);
            float width = baseSize.Width + scriptWidth;
            float ascent = Math.Max(baseSize.Ascent, fontSize * 0.4f + supSize.Height);
            float descent = Math.Max(baseSize.Descent, ScriptShift + subSize.Height - subSize.Ascent);

            return new MathSize(width, ascent, descent);
        }

        private static MathSize MeasureOverscript(Element element, float fontSize)
        {
            Element? baseEl = GetChildElement(element, 0);
            Element? overEl = GetChildElement(element, 1);

            var baseSize = baseEl != null ? Measure(baseEl, fontSize) : new MathSize(0, 0, 0);
            float scriptFontSize = fontSize * ScriptScale;
            var overSize = overEl != null ? Measure(overEl, scriptFontSize) : new MathSize(0, 0, 0);

            float width = Math.Max(baseSize.Width, overSize.Width);
            float ascent = baseSize.Ascent + 2f + overSize.Height;

            return new MathSize(width, ascent, baseSize.Descent);
        }

        private static MathSize MeasureUnderscript(Element element, float fontSize)
        {
            Element? baseEl = GetChildElement(element, 0);
            Element? underEl = GetChildElement(element, 1);

            var baseSize = baseEl != null ? Measure(baseEl, fontSize) : new MathSize(0, 0, 0);
            float scriptFontSize = fontSize * ScriptScale;
            var underSize = underEl != null ? Measure(underEl, scriptFontSize) : new MathSize(0, 0, 0);

            float width = Math.Max(baseSize.Width, underSize.Width);
            float descent = baseSize.Descent + 2f + underSize.Height;

            return new MathSize(width, baseSize.Ascent, descent);
        }

        private static MathSize MeasureUnderOverscript(Element element, float fontSize)
        {
            Element? baseEl = GetChildElement(element, 0);
            Element? underEl = GetChildElement(element, 1);
            Element? overEl = GetChildElement(element, 2);

            var baseSize = baseEl != null ? Measure(baseEl, fontSize) : new MathSize(0, 0, 0);
            float scriptFontSize = fontSize * ScriptScale;
            var underSize = underEl != null ? Measure(underEl, scriptFontSize) : new MathSize(0, 0, 0);
            var overSize = overEl != null ? Measure(overEl, scriptFontSize) : new MathSize(0, 0, 0);

            float width = Math.Max(baseSize.Width, Math.Max(underSize.Width, overSize.Width));
            float ascent = baseSize.Ascent + 2f + overSize.Height;
            float descent = baseSize.Descent + 2f + underSize.Height;

            return new MathSize(width, ascent, descent);
        }

        // ----- Helpers -----

        private static void RenderChildElements(Element element, IRenderTarget target,
            ref float x, float baseline, float fontSize)
        {
            var child = element.FirstChild;
            while (child != null)
            {
                if (child is Element childEl)
                {
                    RenderElement(childEl, target, ref x, baseline, fontSize);
                }
                else if (child is TextNode textNode)
                {
                    string text = textNode.Data?.Trim() ?? "";
                    if (text.Length > 0)
                    {
                        DrawMathText(target, text, x, baseline, fontSize, false);
                        x += EstimateTextWidth(text, fontSize);
                    }
                }
                child = child.NextSibling;
            }
        }

        private static void DrawMathText(IRenderTarget target, string text,
            float x, float baseline, float fontSize, bool italic)
        {
            string family = italic ? "serif" : "sans-serif";
            float weight = 400f;

            target.DrawText(text, x, baseline - fontSize * 0.75f, new TextStyle
            {
                Font = new Fonts.FontDescriptor(family, weight),
                FontSize = fontSize,
                Color = TextColor
            });
        }

        private static float EstimateTextWidth(string text, float fontSize)
        {
            // Approximate width: ~0.55em per character for proportional fonts
            return text.Length * fontSize * 0.55f;
        }

        private static string GetTextContent(Element element)
        {
            var child = element.FirstChild;
            while (child != null)
            {
                if (child is TextNode textNode)
                    return textNode.Data ?? "";
                child = child.NextSibling;
            }
            return "";
        }

        private static Element? GetChildElement(Element parent, int index)
        {
            int count = 0;
            var child = parent.FirstChild;
            while (child != null)
            {
                if (child is Element el)
                {
                    if (count == index) return el;
                    count++;
                }
                child = child.NextSibling;
            }
            return null;
        }

        private static float ParseLength(string? value, float defaultValue)
        {
            if (value == null) return defaultValue;
            value = value.Trim();
            if (value.Length == 0) return defaultValue;

            // Handle em units
            if (value.EndsWith("em", StringComparison.OrdinalIgnoreCase))
            {
                if (float.TryParse(value.Substring(0, value.Length - 2),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out float em))
                    return em * defaultValue;
            }

            // Handle px units
            if (value.EndsWith("px", StringComparison.OrdinalIgnoreCase))
            {
                if (float.TryParse(value.Substring(0, value.Length - 2),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out float px))
                    return px;
            }

            // Bare number
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float num))
                return num;

            return defaultValue;
        }

        /// <summary>
        /// Represents the measured size of a math layout box.
        /// </summary>
        internal struct MathSize
        {
            public float Width;
            public float Ascent;
            public float Descent;
            public float Height => Ascent + Descent;

            public MathSize(float width, float ascent, float descent)
            {
                Width = width;
                Ascent = ascent;
                Descent = descent;
            }
        }
    }
}
