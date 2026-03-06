using System.IO;
using Rend.Core.Values;
using Rend.Text;
using Rend.Fonts;

namespace Rend.Rendering
{
    /// <summary>
    /// The central rendering abstraction. Implementations translate drawing commands
    /// into a specific output format (PDF, image, etc.).
    /// </summary>
    public interface IRenderTarget
    {
        /// <summary>Begins a new page with the specified dimensions.</summary>
        /// <param name="width">The page width in pixels.</param>
        /// <param name="height">The page height in pixels.</param>
        void BeginPage(float width, float height);

        /// <summary>Ends the current page.</summary>
        void EndPage();

        /// <summary>Saves the current graphics state onto the state stack.</summary>
        void Save();

        /// <summary>Restores the most recently saved graphics state.</summary>
        void Restore();

        /// <summary>Sets the current transformation matrix.</summary>
        /// <param name="transform">The 3x2 affine transformation matrix.</param>
        void SetTransform(Matrix3x2 transform);

        /// <summary>Concatenates a transformation matrix with the current transform.</summary>
        /// <param name="transform">The 3x2 affine transformation matrix to concatenate.</param>
        void ConcatTransform(Matrix3x2 transform);

        /// <summary>Sets the current opacity level for subsequent drawing operations.</summary>
        /// <param name="opacity">The opacity value from 0 (fully transparent) to 1 (fully opaque).</param>
        void SetOpacity(float opacity);

        /// <summary>
        /// Applies a CSS filter effect list as a compositing layer.
        /// Creates a SaveLayer with the combined image/color filters.
        /// Must be balanced by a subsequent Restore() call.
        /// </summary>
        /// <param name="effects">The filter effects to apply.</param>
        void ApplyFilter(CssFilterEffect[] effects);

        /// <summary>
        /// Begins a mask compositing layer. All subsequent drawing operations
        /// are captured into an offscreen buffer until <see cref="EndMask"/> is called.
        /// </summary>
        void BeginMask();

        /// <summary>
        /// Ends the mask compositing layer by applying a gradient mask to the
        /// captured content. The gradient's alpha channel is used to mask the content.
        /// </summary>
        /// <param name="gradient">The gradient to use as a mask.</param>
        /// <param name="bounds">The bounds of the masked region.</param>
        void EndMask(GradientInfo gradient, RectF bounds);

        /// <summary>Sets the current blend mode for subsequent drawing operations.</summary>
        /// <param name="blendMode">The CSS blend mode value.</param>
        void SetBlendMode(Css.CssMixBlendMode blendMode);

        /// <summary>Sets the image rendering quality hint for subsequent DrawImage calls.</summary>
        /// <param name="rendering">The CSS image-rendering value.</param>
        void SetImageRendering(Css.CssImageRendering rendering);

        /// <summary>Sets a Gaussian blur mask for subsequent fill operations (used for box-shadow).</summary>
        /// <param name="sigma">The blur sigma in pixels. 0 clears the blur.</param>
        /// <param name="inner">If true, use inner blur style (blur only inside the shape boundary).</param>
        void SetMaskBlur(float sigma, bool inner = false);

        /// <summary>Pushes a rectangular clipping region onto the clip stack.</summary>
        /// <param name="rect">The clipping rectangle.</param>
        void PushClipRect(RectF rect);

        /// <summary>Pushes a path-based clipping region onto the clip stack.</summary>
        /// <param name="path">The clipping path.</param>
        void PushClipPath(PathData path);

        /// <summary>Pops the most recent clipping region from the clip stack.</summary>
        void PopClip();

        /// <summary>Fills a rectangle with the specified brush.</summary>
        /// <param name="rect">The rectangle to fill.</param>
        /// <param name="brush">The fill brush descriptor.</param>
        void FillRect(RectF rect, BrushInfo brush);

        /// <summary>Strokes the outline of a rectangle with the specified pen.</summary>
        /// <param name="rect">The rectangle to stroke.</param>
        /// <param name="pen">The stroke pen descriptor.</param>
        void StrokeRect(RectF rect, PenInfo pen);

        /// <summary>Fills a path with the specified brush.</summary>
        /// <param name="path">The path to fill.</param>
        /// <param name="brush">The fill brush descriptor.</param>
        void FillPath(PathData path, BrushInfo brush);

        /// <summary>Strokes a path with the specified pen.</summary>
        /// <param name="path">The path to stroke.</param>
        /// <param name="pen">The stroke pen descriptor.</param>
        void StrokePath(PathData path, PenInfo pen);

        /// <summary>Draws an image into the specified destination rectangle.</summary>
        /// <param name="image">The image data to draw.</param>
        /// <param name="destRect">The destination rectangle.</param>
        void DrawImage(ImageData image, RectF destRect);

        /// <summary>Measures the advance width of a text string without drawing it.</summary>
        /// <param name="text">The text to measure.</param>
        /// <param name="style">The text style to apply.</param>
        /// <returns>The advance width in pixels, or -1 if measurement is not supported.</returns>
        float MeasureText(string text, TextStyle style);

        /// <summary>Draws a text string at the specified position using the given style.</summary>
        /// <param name="text">The text to draw.</param>
        /// <param name="x">The X coordinate of the text origin.</param>
        /// <param name="y">The Y coordinate of the text origin.</param>
        /// <param name="style">The text style to apply.</param>
        void DrawText(string text, float x, float y, TextStyle style);

        /// <summary>Draws pre-shaped glyphs at the specified position.</summary>
        /// <param name="run">The shaped text run containing positioned glyphs.</param>
        /// <param name="x">The X coordinate of the text origin.</param>
        /// <param name="y">The Y coordinate of the text origin.</param>
        /// <param name="color">The text color.</param>
        /// <param name="font">The font descriptor.</param>
        void DrawGlyphs(ShapedTextRun run, float x, float y, CssColor color, FontDescriptor font);

        /// <summary>Finalizes rendering and writes the output to the provided stream.</summary>
        /// <param name="output">The stream to write the final output to.</param>
        void Finish(Stream output);

        /// <summary>
        /// Adds a hyperlink annotation at the specified rectangle.
        /// Called by the painter when an &lt;a&gt; element is encountered.
        /// </summary>
        /// <param name="rect">The clickable region in layout coordinates.</param>
        /// <param name="uri">The link target URI string.</param>
        void AddLink(RectF rect, string uri);

        /// <summary>
        /// Adds a bookmark (outline) entry at the current page position.
        /// Called by the painter when a heading element (h1-h6) is encountered.
        /// </summary>
        /// <param name="title">The bookmark display text.</param>
        /// <param name="level">The heading level (1-6).</param>
        /// <param name="yPosition">The Y position in layout coordinates.</param>
        void AddBookmark(string title, int level, float yPosition);
    }
}
