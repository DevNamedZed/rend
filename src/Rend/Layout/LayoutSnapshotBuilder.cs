using System;
using Rend.Css;
using Rend.Style;

namespace Rend.Layout
{
    /// <summary>
    /// Converts the internal layout tree (LayoutBox) into a public LayoutSnapshot
    /// for diagnostic comparison with browser layout trees.
    /// </summary>
    internal static class LayoutSnapshotBuilder
    {
        public static LayoutSnapshot Build(LayoutBox box)
        {
            var snapshot = new LayoutSnapshot();

            // Extract element info from styled node
            if (box.StyledNode is StyledElement element)
            {
                snapshot.Tag = element.TagName.ToLowerInvariant();
                snapshot.Id = element.GetAttribute("id") ?? "";
                snapshot.Classes = element.GetAttribute("class") ?? "";
            }
            else if (box is LayoutText textBox)
            {
                snapshot.Tag = "#text";
                var text = textBox.Text?.Trim() ?? "";
                if (text.Length > 80)
                {
                    text = text.Substring(0, 77) + "...";
                }
                snapshot.TextContent = text;
            }
            else
            {
                snapshot.Tag = box.BoxType.ToString().ToLowerInvariant();
            }

            // Border rect = getBoundingClientRect equivalent
            var borderRect = box.BorderRect;
            snapshot.X = Round(borderRect.X);
            snapshot.Y = Round(borderRect.Y);
            snapshot.Width = Round(borderRect.Width);
            snapshot.Height = Round(borderRect.Height);

            // Content rect
            snapshot.ContentX = Round(box.ContentRect.X);
            snapshot.ContentY = Round(box.ContentRect.Y);
            snapshot.ContentWidth = Round(box.ContentRect.Width);
            snapshot.ContentHeight = Round(box.ContentRect.Height);

            // Box model values (as px strings, matching Chrome's getComputedStyle format)
            snapshot.MarginTop = ToPx(box.MarginTop);
            snapshot.MarginRight = ToPx(box.MarginRight);
            snapshot.MarginBottom = ToPx(box.MarginBottom);
            snapshot.MarginLeft = ToPx(box.MarginLeft);
            snapshot.PaddingTop = ToPx(box.PaddingTop);
            snapshot.PaddingRight = ToPx(box.PaddingRight);
            snapshot.PaddingBottom = ToPx(box.PaddingBottom);
            snapshot.PaddingLeft = ToPx(box.PaddingLeft);
            snapshot.BorderTopWidth = ToPx(box.BorderTopWidth);
            snapshot.BorderRightWidth = ToPx(box.BorderRightWidth);
            snapshot.BorderBottomWidth = ToPx(box.BorderBottomWidth);
            snapshot.BorderLeftWidth = ToPx(box.BorderLeftWidth);
            snapshot.BoxType = box.BoxType.ToString();

            // Computed style properties
            if (box.StyledNode != null)
            {
                var style = box.StyledNode.Style;
                snapshot.Display = style.Display.ToString().ToLowerInvariant().Replace("_", "-");
                snapshot.Position = style.Position.ToString().ToLowerInvariant();
                snapshot.BoxSizing = style.BoxSizing == CssBoxSizing.BorderBox ? "border-box" : "content-box";
                snapshot.FontSize = ToPx(style.FontSize);
                snapshot.LineHeight = FormatLineHeight(style);
                snapshot.Color = FormatColor(style.Color);
                snapshot.BackgroundColor = FormatColor(style.BackgroundColor);
                snapshot.FontFamily = style.FontFamilies != null && style.FontFamilies.Length > 0
                    ? string.Join(", ", style.FontFamilies)
                    : "";
            }

            // Get text content from inline line boxes
            if (string.IsNullOrEmpty(snapshot.TextContent) && box.LineBoxes != null)
            {
                var textParts = new System.Text.StringBuilder();
                foreach (var lineBox in box.LineBoxes)
                {
                    foreach (var frag in lineBox.Fragments)
                    {
                        if (frag.Text != null)
                        {
                            var t = frag.Text.Trim();
                            if (t.Length > 0)
                            {
                                if (textParts.Length > 0)
                                {
                                    textParts.Append(' ');
                                }
                                textParts.Append(t);
                            }
                        }
                    }
                }
                var fullText = textParts.ToString();
                if (fullText.Length > 80)
                {
                    fullText = fullText.Substring(0, 77) + "...";
                }
                snapshot.TextContent = fullText;
            }

            // Recurse into children
            foreach (var child in box.Children)
            {
                snapshot.Children.Add(Build(child));
            }

            return snapshot;
        }

        private static float Round(float v)
        {
            return (float)Math.Round(v * 100f) / 100f;
        }

        private static string ToPx(float v)
        {
            // Match Chrome's format: "0px", "10px", "1.5px"
            if (v == 0f)
            {
                return "0px";
            }
            var rounded = Math.Round(v, 2);
            if (rounded == Math.Floor(rounded))
            {
                return $"{(int)rounded}px";
            }
            return $"{rounded}px";
        }

        private static string FormatLineHeight(ComputedStyle style)
        {
            var lh = style.LineHeight;
            if (float.IsNaN(lh))
            {
                return "normal";
            }
            if (lh < 0)
            {
                // Unitless multiplier: resolve to px
                var resolved = style.FontSize * (-lh);
                return ToPx(resolved);
            }
            return ToPx(lh);
        }

        private static string FormatColor(Core.Values.CssColor color)
        {
            if (color.A == 255)
            {
                return $"rgb({color.R}, {color.G}, {color.B})";
            }
            var alpha = Math.Round(color.A / 255.0, 3);
            return $"rgba({color.R}, {color.G}, {color.B}, {alpha})";
        }
    }
}
