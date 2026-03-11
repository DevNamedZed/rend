using System;
using System.Collections.Generic;

namespace Rend.Pdf
{
    /// <summary>
    /// Simple table builder for the low-level PDF API.
    /// Lays out text in a grid with borders and padding.
    /// </summary>
    public sealed class PdfTableBuilder
    {
        private readonly PdfFont _font;
        private readonly float _fontSize;
        private readonly List<float> _columnWidths = new List<float>();
        private readonly List<string[]> _rows = new List<string[]>();
        private float _x = 72f;
        private float _y = 72f;
        private float _availableWidth = 451.28f; // A4 - 2*72
        private float _padding = 4f;
        private float _borderWidth = 0.5f;

        /// <summary>Create a table builder with the specified font and size.</summary>
        /// <param name="font">The font to use for all cell text.</param>
        /// <param name="fontSize">Font size in points.</param>
        public PdfTableBuilder(PdfFont font, float fontSize)
        {
            _font = font ?? throw new ArgumentNullException(nameof(font));
            if (fontSize <= 0) throw new ArgumentOutOfRangeException(nameof(fontSize), "Font size must be positive.");
            _fontSize = fontSize;
        }

        /// <summary>Set the top-left position and available width for the table.</summary>
        public void SetBounds(float x, float y, float availableWidth)
        {
            _x = x;
            _y = y;
            _availableWidth = availableWidth;
        }

        /// <summary>Set cell padding in points.</summary>
        public void SetPadding(float padding) => _padding = padding;

        /// <summary>Set border line width in points (0 for no borders).</summary>
        public void SetBorder(float width) => _borderWidth = width;

        /// <summary>Add a column with a fixed width in points.</summary>
        public void AddColumn(float width) => _columnWidths.Add(width);

        /// <summary>Add a row of cell values. Must match the number of columns.</summary>
        public void AddRow(params string[] cells)
        {
            if (cells.Length != _columnWidths.Count)
                throw new ArgumentException($"Expected {_columnWidths.Count} cells, got {cells.Length}");
            _rows.Add(cells);
        }

        /// <summary>
        /// Draw the table onto the content stream. Call after adding all columns and rows.
        /// </summary>
        public void Draw(PdfContentStream cs)
        {
            int numCols = _columnWidths.Count;
            if (numCols == 0 || _rows.Count == 0) return;

            // Resolve column widths — distribute remaining space proportionally if columns
            // don't fill available width
            float totalFixed = 0;
            for (int i = 0; i < numCols; i++)
                totalFixed += _columnWidths[i];

            float scale = totalFixed > 0 && totalFixed < _availableWidth
                ? _availableWidth / totalFixed
                : 1f;

            var colW = new float[numCols];
            for (int i = 0; i < numCols; i++)
                colW[i] = _columnWidths[i] * scale;

            float rowHeight = _fontSize + _padding * 2;
            float tableTop = _y; // PDF y = top of page minus offset

            // Draw borders and text row by row
            float curY = tableTop;

            for (int r = 0; r < _rows.Count; r++)
            {
                float curX = _x;

                // Draw cell borders
                if (_borderWidth > 0)
                {
                    cs.SetLineWidth(_borderWidth);
                    for (int c = 0; c < numCols; c++)
                    {
                        cs.Rectangle(curX, curY - rowHeight, colW[c], rowHeight);
                        cs.Stroke();
                        curX += colW[c];
                    }
                }

                // Draw cell text
                cs.BeginText();
                cs.SetFont(_font, _fontSize);
                curX = _x;
                for (int c = 0; c < numCols; c++)
                {
                    float textX = curX + _padding;
                    float textY = curY - _padding - _fontSize;
                    cs.MoveTextPosition(textX, textY);
                    cs.ShowText(_font, _rows[r][c]);
                    curX += colW[c];
                }
                cs.EndText();

                curY -= rowHeight;
            }
        }
    }
}
