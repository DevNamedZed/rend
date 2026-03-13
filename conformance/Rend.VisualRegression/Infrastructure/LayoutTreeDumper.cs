using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace Rend.VisualRegression.Infrastructure
{
    /// <summary>
    /// Extracts Chrome's layout tree via CDP (Chrome DevTools Protocol) using
    /// Puppeteer's page.EvaluateFunctionAsync(). Walks the DOM and captures
    /// bounding rects and computed styles for every element.
    /// </summary>
    public static class LayoutTreeDumper
    {
        /// <summary>
        /// JavaScript function injected into Chrome to walk the DOM tree and
        /// extract layout information for every element. Returns a JSON-serializable
        /// object representing the full layout tree.
        /// </summary>
        private const string DumpLayoutJs = @"() => {
            function walkNode(el) {
                if (el.nodeType !== 1) return null; // Element nodes only

                const rect = el.getBoundingClientRect();
                const cs = getComputedStyle(el);

                // Get direct text content (not from children)
                let textContent = '';
                for (const child of el.childNodes) {
                    if (child.nodeType === 3) { // Text node
                        const t = child.textContent.trim();
                        if (t) textContent += (textContent ? ' ' : '') + t;
                    }
                }
                if (textContent.length > 80) {
                    textContent = textContent.substring(0, 77) + '...';
                }

                const node = {
                    tag: el.tagName.toLowerCase(),
                    id: el.id || '',
                    classes: el.className || '',
                    x: Math.round(rect.x * 100) / 100,
                    y: Math.round(rect.y * 100) / 100,
                    width: Math.round(rect.width * 100) / 100,
                    height: Math.round(rect.height * 100) / 100,
                    display: cs.display,
                    position: cs.position,
                    boxSizing: cs.boxSizing,
                    marginTop: cs.marginTop,
                    marginRight: cs.marginRight,
                    marginBottom: cs.marginBottom,
                    marginLeft: cs.marginLeft,
                    paddingTop: cs.paddingTop,
                    paddingRight: cs.paddingRight,
                    paddingBottom: cs.paddingBottom,
                    paddingLeft: cs.paddingLeft,
                    borderTopWidth: cs.borderTopWidth,
                    borderRightWidth: cs.borderRightWidth,
                    borderBottomWidth: cs.borderBottomWidth,
                    borderLeftWidth: cs.borderLeftWidth,
                    fontSize: cs.fontSize,
                    lineHeight: cs.lineHeight,
                    color: cs.color,
                    backgroundColor: cs.backgroundColor,
                    fontFamily: cs.fontFamily,
                    textContent: textContent,
                    children: []
                };

                for (const child of el.children) {
                    const childNode = walkNode(child);
                    if (childNode) {
                        node.children.push(childNode);
                    }
                }

                return node;
            }

            // Start from <html> to capture full document tree
            const root = document.documentElement;
            return walkNode(root);
        }";

        /// <summary>
        /// Dumps Chrome's layout tree for the currently loaded page.
        /// Call this after page.SetContentAsync() and before screenshot.
        /// </summary>
        public static async Task<LayoutNode?> DumpAsync(IPage page)
        {
            var json = await page.EvaluateFunctionAsync<JsonElement>(DumpLayoutJs);
            return DeserializeNode(json);
        }

        private static LayoutNode? DeserializeNode(JsonElement el)
        {
            if (el.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var node = new LayoutNode
            {
                Tag = GetString(el, "tag"),
                Id = GetString(el, "id"),
                Classes = GetString(el, "classes"),
                X = GetFloat(el, "x"),
                Y = GetFloat(el, "y"),
                Width = GetFloat(el, "width"),
                Height = GetFloat(el, "height"),
                Display = GetString(el, "display"),
                Position = GetString(el, "position"),
                BoxSizing = GetString(el, "boxSizing"),
                MarginTop = GetString(el, "marginTop"),
                MarginRight = GetString(el, "marginRight"),
                MarginBottom = GetString(el, "marginBottom"),
                MarginLeft = GetString(el, "marginLeft"),
                PaddingTop = GetString(el, "paddingTop"),
                PaddingRight = GetString(el, "paddingRight"),
                PaddingBottom = GetString(el, "paddingBottom"),
                PaddingLeft = GetString(el, "paddingLeft"),
                BorderTopWidth = GetString(el, "borderTopWidth"),
                BorderRightWidth = GetString(el, "borderRightWidth"),
                BorderBottomWidth = GetString(el, "borderBottomWidth"),
                BorderLeftWidth = GetString(el, "borderLeftWidth"),
                FontSize = GetString(el, "fontSize"),
                LineHeight = GetString(el, "lineHeight"),
                Color = GetString(el, "color"),
                BackgroundColor = GetString(el, "backgroundColor"),
                FontFamily = GetString(el, "fontFamily"),
                TextContent = GetString(el, "textContent"),
            };

            if (el.TryGetProperty("children", out var children) && children.ValueKind == JsonValueKind.Array)
            {
                foreach (var child in children.EnumerateArray())
                {
                    var childNode = DeserializeNode(child);
                    if (childNode != null)
                    {
                        node.Children.Add(childNode);
                    }
                }
            }

            return node;
        }

        private static string GetString(JsonElement el, string prop)
        {
            if (el.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String)
            {
                return val.GetString() ?? "";
            }
            return "";
        }

        private static float GetFloat(JsonElement el, string prop)
        {
            if (el.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.Number)
            {
                return (float)val.GetDouble();
            }
            return 0f;
        }
    }
}
