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

        /// <summary>Sets the current opacity level for subsequent drawing operations.</summary>
        /// <param name="opacity">The opacity value from 0 (fully transparent) to 1 (fully opaque).</param>
        void SetOpacity(float opacity);

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
    }
}
