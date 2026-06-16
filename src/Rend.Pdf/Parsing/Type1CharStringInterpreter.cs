#nullable enable
using System;
using System.Collections.Generic;

namespace Rend.Pdf.Parsing
{
    /// <summary>
    /// Interprets a decrypted Type1 charstring into an absolute-coordinate
    /// <see cref="GlyphOutline"/>. Handles the full Type1 operator set: sidebearing
    /// (hsbw/sbw), moveto/lineto/curveto families, subroutines (callsubr — no bias in
    /// Type1), div, the flex and hint-replacement OtherSubrs protocols, and seac
    /// (accented-character composition). Hints are intentionally dropped — they do not
    /// affect the outline shape.
    /// [SPEC] Adobe Type 1 Font Format (1990) §6 (charstrings), §8 (OtherSubrs).
    /// </summary>
    internal sealed class Type1CharStringInterpreter
    {
        private readonly IReadOnlyList<byte[]> _subrs;
        private readonly Func<int, byte[]?> _standardEncodedCharString;

        private readonly List<double> _operands = new List<double>();
        private readonly List<double> _psStack = new List<double>();
        private GlyphOutline _outline = new GlyphOutline();
        private GlyphContour? _contour;
        private double _currentX;
        private double _currentY;
        private bool _finished;
        private bool _inFlex;
        private readonly List<double> _flexX = new List<double>();
        private readonly List<double> _flexY = new List<double>();

        /// <param name="subrs">The font's local Subrs (Type1 indexes them directly, no bias).</param>
        /// <param name="standardEncodedCharString">
        /// Resolves a StandardEncoding code to that glyph's raw decrypted Type1 charstring,
        /// used by <c>seac</c>. May return null when unavailable.
        /// </param>
        public Type1CharStringInterpreter(IReadOnlyList<byte[]> subrs,
            Func<int, byte[]?> standardEncodedCharString)
        {
            _subrs = subrs;
            _standardEncodedCharString = standardEncodedCharString;
        }

        public GlyphOutline Interpret(byte[] charString)
        {
            _operands.Clear();
            _psStack.Clear();
            _outline = new GlyphOutline();
            _contour = null;
            _currentX = 0;
            _currentY = 0;
            _finished = false;
            _inFlex = false;
            _flexX.Clear();
            _flexY.Clear();

            Execute(charString);
            return _outline;
        }

        private void Execute(byte[] code)
        {
            int pos = 0;
            while (pos < code.Length && !_finished)
            {
                int b = code[pos];
                if (b >= 32 || b == 28)
                {
                    pos += ReadNumber(code, pos);
                    continue;
                }

                pos++;
                if (b == 12)
                {
                    if (pos >= code.Length)
                    {
                        break;
                    }
                    int escaped = code[pos];
                    pos++;
                    if (HandleEscapedOperator(escaped))
                    {
                        return;
                    }
                }
                else if (HandleOperator(b))
                {
                    return;
                }
            }
        }

        private int ReadNumber(byte[] code, int pos)
        {
            int b = code[pos];
            if (b == 255)
            {
                int value = (code[pos + 1] << 24) | (code[pos + 2] << 16) |
                            (code[pos + 3] << 8) | code[pos + 4];
                _operands.Add(value);
                return 5;
            }
            if (b >= 32 && b <= 246)
            {
                _operands.Add(b - 139);
                return 1;
            }
            if (b >= 247 && b <= 250)
            {
                _operands.Add((b - 247) * 256 + code[pos + 1] + 108);
                return 2;
            }
            // 251..254
            _operands.Add(-(b - 251) * 256 - code[pos + 1] - 108);
            return 2;
        }

        // Returns true if execution of the whole glyph should stop (return to top).
        private bool HandleOperator(int op)
        {
            switch (op)
            {
                case 1: // hstem
                case 3: // vstem
                    _operands.Clear();
                    return false;
                case 4: // vmoveto
                    MoveBy(0, Arg(0));
                    return false;
                case 5: // rlineto
                    LineBy(Arg(0), Arg(1));
                    return false;
                case 6: // hlineto
                    LineBy(Arg(0), 0);
                    return false;
                case 7: // vlineto
                    LineBy(0, Arg(0));
                    return false;
                case 8: // rrcurveto
                    CurveBy(Arg(0), Arg(1), Arg(2), Arg(3), Arg(4), Arg(5));
                    return false;
                case 9: // closepath
                    _operands.Clear();
                    return false;
                case 10: // callsubr (no bias)
                    CallSubr();
                    return false;
                case 11: // return
                    return false;
                case 13: // hsbw
                    SetSidebearingWidth(Arg(0), 0, Arg(1));
                    return false;
                case 14: // endchar
                    _finished = true;
                    return true;
                case 21: // rmoveto
                    MoveBy(Arg(0), Arg(1));
                    return false;
                case 22: // hmoveto
                    MoveBy(Arg(0), 0);
                    return false;
                case 30: // vhcurveto
                    CurveAbsolute(_currentX, _currentY + Arg(0),
                        _currentX + Arg(1), _currentY + Arg(0) + Arg(2),
                        _currentX + Arg(1) + Arg(3), _currentY + Arg(0) + Arg(2));
                    return false;
                case 31: // hvcurveto
                    CurveAbsolute(_currentX + Arg(0), _currentY,
                        _currentX + Arg(0) + Arg(1), _currentY + Arg(2),
                        _currentX + Arg(0) + Arg(1), _currentY + Arg(2) + Arg(3));
                    return false;
                default:
                    _operands.Clear();
                    return false;
            }
        }

        private bool HandleEscapedOperator(int op)
        {
            switch (op)
            {
                case 0: // dotsection
                case 1: // vstem3
                case 2: // hstem3
                    _operands.Clear();
                    return false;
                case 6: // seac
                    ComposeAccentedCharacter(Arg(0), Arg(1), Arg(2), (int)Arg(3), (int)Arg(4));
                    return true;
                case 7: // sbw
                    SetSidebearingWidth(Arg(0), Arg(1), Arg(2));
                    return false;
                case 12: // div
                    ApplyDivide();
                    return false;
                case 16: // callothersubr
                    CallOtherSubr();
                    return false;
                case 17: // pop
                    _operands.Add(_psStack.Count > 0 ? PopPs() : 0);
                    return false;
                case 33: // setcurrentpoint
                    _currentX = Arg(0);
                    _currentY = Arg(1);
                    _operands.Clear();
                    return false;
                default:
                    _operands.Clear();
                    return false;
            }
        }

        private double Arg(int index)
        {
            return index < _operands.Count ? _operands[index] : 0;
        }

        private double PopPs()
        {
            double value = _psStack[_psStack.Count - 1];
            _psStack.RemoveAt(_psStack.Count - 1);
            return value;
        }

        private void SetSidebearingWidth(double sidebearingX, double sidebearingY, double width)
        {
            _currentX = sidebearingX;
            _currentY = sidebearingY;
            _outline.AdvanceWidth = (float)width;
            _operands.Clear();
        }

        private void MoveBy(double deltaX, double deltaY)
        {
            _currentX += deltaX;
            _currentY += deltaY;
            if (_inFlex)
            {
                _flexX.Add(_currentX);
                _flexY.Add(_currentY);
            }
            else
            {
                _contour = new GlyphContour((float)_currentX, (float)_currentY);
                _outline.Contours.Add(_contour);
            }
            _operands.Clear();
        }

        private void LineBy(double deltaX, double deltaY)
        {
            _currentX += deltaX;
            _currentY += deltaY;
            _contour?.Segments.Add(GlyphPathSegment.Line((float)_currentX, (float)_currentY));
            _operands.Clear();
        }

        private void CurveBy(double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
        {
            double control1X = _currentX + dx1;
            double control1Y = _currentY + dy1;
            double control2X = control1X + dx2;
            double control2Y = control1Y + dy2;
            double endX = control2X + dx3;
            double endY = control2Y + dy3;
            CurveAbsolute(control1X, control1Y, control2X, control2Y, endX, endY);
        }

        private void CurveAbsolute(double control1X, double control1Y,
            double control2X, double control2Y, double endX, double endY)
        {
            _contour?.Segments.Add(GlyphPathSegment.Cubic(
                (float)control1X, (float)control1Y, (float)control2X, (float)control2Y,
                (float)endX, (float)endY));
            _currentX = endX;
            _currentY = endY;
            _operands.Clear();
        }

        private void CallSubr()
        {
            if (_operands.Count == 0)
            {
                return;
            }
            int index = (int)_operands[_operands.Count - 1];
            _operands.RemoveAt(_operands.Count - 1);
            if (index >= 0 && index < _subrs.Count)
            {
                Execute(_subrs[index]);
            }
        }

        private void ApplyDivide()
        {
            if (_operands.Count < 2)
            {
                return;
            }
            double divisor = _operands[_operands.Count - 1];
            double dividend = _operands[_operands.Count - 2];
            _operands.RemoveAt(_operands.Count - 1);
            _operands.RemoveAt(_operands.Count - 1);
            _operands.Add(divisor != 0 ? dividend / divisor : 0);
        }

        private void CallOtherSubr()
        {
            if (_operands.Count < 2)
            {
                _operands.Clear();
                return;
            }
            int otherSubr = (int)_operands[_operands.Count - 1];
            int argCount = (int)_operands[_operands.Count - 2];
            _operands.RemoveAt(_operands.Count - 1);
            _operands.RemoveAt(_operands.Count - 1);

            var args = new List<double>();
            for (int i = 0; i < argCount && _operands.Count > 0; i++)
            {
                args.Insert(0, _operands[_operands.Count - 1]);
                _operands.RemoveAt(_operands.Count - 1);
            }

            switch (otherSubr)
            {
                case 1: // flex begin
                    _inFlex = true;
                    _flexX.Clear();
                    _flexY.Clear();
                    break;
                case 2: // flex add point — the point was recorded by the preceding rmoveto
                    break;
                case 0: // flex end
                    EndFlex(args);
                    break;
                case 3: // hint replacement — return the subr number for the following pop/callsubr
                    _psStack.Add(args.Count > 0 ? args[0] : 3);
                    break;
                default:
                    for (int i = args.Count - 1; i >= 0; i--)
                    {
                        _psStack.Add(args[i]);
                    }
                    break;
            }
        }

        private void EndFlex(List<double> args)
        {
            _inFlex = false;
            if (_flexX.Count >= 7 && _contour != null)
            {
                _contour.Segments.Add(GlyphPathSegment.Cubic(
                    (float)_flexX[1], (float)_flexY[1], (float)_flexX[2], (float)_flexY[2],
                    (float)_flexX[3], (float)_flexY[3]));
                _contour.Segments.Add(GlyphPathSegment.Cubic(
                    (float)_flexX[4], (float)_flexY[4], (float)_flexX[5], (float)_flexY[5],
                    (float)_flexX[6], (float)_flexY[6]));
                _currentX = _flexX[6];
                _currentY = _flexY[6];
            }
            // OtherSubr 0 returns the final (x, y) for the trailing "pop pop setcurrentpoint".
            _psStack.Add(_currentY);
            _psStack.Add(_currentX);
        }

        private void ComposeAccentedCharacter(double accentSidebearing, double accentX,
            double accentY, int baseCode, int accentCode)
        {
            _operands.Clear();
            float baseWidth = _outline.AdvanceWidth;

            byte[]? baseCharString = _standardEncodedCharString(baseCode);
            if (baseCharString != null)
            {
                AppendComponent(baseCharString, 0, 0);
            }

            byte[]? accentCharString = _standardEncodedCharString(accentCode);
            if (accentCharString != null)
            {
                AppendComponent(accentCharString, accentX - accentSidebearing, accentY);
            }

            _outline.AdvanceWidth = baseWidth;
            _finished = true;
        }

        private void AppendComponent(byte[] charString, double offsetX, double offsetY)
        {
            var component = new Type1CharStringInterpreter(_subrs, _standardEncodedCharString)
                .Interpret(charString);
            foreach (GlyphContour source in component.Contours)
            {
                var moved = new GlyphContour(
                    (float)(source.StartX + offsetX), (float)(source.StartY + offsetY));
                foreach (GlyphPathSegment segment in source.Segments)
                {
                    moved.Segments.Add(TranslateSegment(segment, offsetX, offsetY));
                }
                _outline.Contours.Add(moved);
            }
        }

        private static GlyphPathSegment TranslateSegment(GlyphPathSegment segment,
            double offsetX, double offsetY)
        {
            if (segment.Type == GlyphPathSegmentType.Line)
            {
                return GlyphPathSegment.Line(
                    (float)(segment.EndX + offsetX), (float)(segment.EndY + offsetY));
            }
            return GlyphPathSegment.Cubic(
                (float)(segment.Control1X + offsetX), (float)(segment.Control1Y + offsetY),
                (float)(segment.Control2X + offsetX), (float)(segment.Control2Y + offsetY),
                (float)(segment.EndX + offsetX), (float)(segment.EndY + offsetY));
        }
    }
}
