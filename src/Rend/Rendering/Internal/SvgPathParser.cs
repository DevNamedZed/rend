using System;

namespace Rend.Rendering.Internal
{
    /// <summary>
    /// Parses SVG path data (the 'd' attribute) into PathData segments.
    /// Supports all SVG path commands: M, L, H, V, C, S, Q, T, A, Z.
    /// </summary>
    internal static class SvgPathParser
    {
        public static PathData Parse(string d)
        {
            var path = new PathData();
            if (string.IsNullOrEmpty(d)) return path;

            int i = 0;
            int len = d.Length;
            float cx = 0, cy = 0;   // current point
            float sx = 0, sy = 0;   // start of current subpath
            float lcx = 0, lcy = 0; // last control point (for S/T smooth curves)
            char lastCmd = ' ';

            while (i < len)
            {
                SkipWhitespaceAndCommas(d, ref i, len);
                if (i >= len) break;

                char cmd = d[i];
                if (IsCommand(cmd))
                {
                    i++;
                }
                else
                {
                    // Implicit repeat of last command (M→L, m→l)
                    cmd = lastCmd;
                    if (cmd == 'M') cmd = 'L';
                    else if (cmd == 'm') cmd = 'l';
                }

                switch (cmd)
                {
                    case 'M':
                    {
                        float x = ReadNumber(d, ref i, len);
                        float y = ReadNumber(d, ref i, len);
                        path.MoveTo(x, y);
                        cx = x; cy = y;
                        sx = x; sy = y;
                        lastCmd = 'M';
                        break;
                    }
                    case 'm':
                    {
                        float dx = ReadNumber(d, ref i, len);
                        float dy = ReadNumber(d, ref i, len);
                        cx += dx; cy += dy;
                        path.MoveTo(cx, cy);
                        sx = cx; sy = cy;
                        lastCmd = 'm';
                        break;
                    }
                    case 'L':
                    {
                        float x = ReadNumber(d, ref i, len);
                        float y = ReadNumber(d, ref i, len);
                        path.LineTo(x, y);
                        cx = x; cy = y;
                        lastCmd = 'L';
                        break;
                    }
                    case 'l':
                    {
                        float dx = ReadNumber(d, ref i, len);
                        float dy = ReadNumber(d, ref i, len);
                        cx += dx; cy += dy;
                        path.LineTo(cx, cy);
                        lastCmd = 'l';
                        break;
                    }
                    case 'H':
                    {
                        float x = ReadNumber(d, ref i, len);
                        cx = x;
                        path.LineTo(cx, cy);
                        lastCmd = 'H';
                        break;
                    }
                    case 'h':
                    {
                        float dx = ReadNumber(d, ref i, len);
                        cx += dx;
                        path.LineTo(cx, cy);
                        lastCmd = 'h';
                        break;
                    }
                    case 'V':
                    {
                        float y = ReadNumber(d, ref i, len);
                        cy = y;
                        path.LineTo(cx, cy);
                        lastCmd = 'V';
                        break;
                    }
                    case 'v':
                    {
                        float dy = ReadNumber(d, ref i, len);
                        cy += dy;
                        path.LineTo(cx, cy);
                        lastCmd = 'v';
                        break;
                    }
                    case 'C':
                    {
                        float x1 = ReadNumber(d, ref i, len);
                        float y1 = ReadNumber(d, ref i, len);
                        float x2 = ReadNumber(d, ref i, len);
                        float y2 = ReadNumber(d, ref i, len);
                        float x = ReadNumber(d, ref i, len);
                        float y = ReadNumber(d, ref i, len);
                        path.CubicBezierTo(x1, y1, x2, y2, x, y);
                        lcx = x2; lcy = y2;
                        cx = x; cy = y;
                        lastCmd = 'C';
                        break;
                    }
                    case 'c':
                    {
                        float x1 = cx + ReadNumber(d, ref i, len);
                        float y1 = cy + ReadNumber(d, ref i, len);
                        float x2 = cx + ReadNumber(d, ref i, len);
                        float y2 = cy + ReadNumber(d, ref i, len);
                        float x = cx + ReadNumber(d, ref i, len);
                        float y = cy + ReadNumber(d, ref i, len);
                        path.CubicBezierTo(x1, y1, x2, y2, x, y);
                        lcx = x2; lcy = y2;
                        cx = x; cy = y;
                        lastCmd = 'c';
                        break;
                    }
                    case 'S':
                    {
                        // Smooth cubic: reflect previous control point
                        float rx = 2 * cx - lcx;
                        float ry = 2 * cy - lcy;
                        if (lastCmd != 'C' && lastCmd != 'c' && lastCmd != 'S' && lastCmd != 's')
                        { rx = cx; ry = cy; }

                        float x2 = ReadNumber(d, ref i, len);
                        float y2 = ReadNumber(d, ref i, len);
                        float x = ReadNumber(d, ref i, len);
                        float y = ReadNumber(d, ref i, len);
                        path.CubicBezierTo(rx, ry, x2, y2, x, y);
                        lcx = x2; lcy = y2;
                        cx = x; cy = y;
                        lastCmd = 'S';
                        break;
                    }
                    case 's':
                    {
                        float rx = 2 * cx - lcx;
                        float ry = 2 * cy - lcy;
                        if (lastCmd != 'C' && lastCmd != 'c' && lastCmd != 'S' && lastCmd != 's')
                        { rx = cx; ry = cy; }

                        float x2 = cx + ReadNumber(d, ref i, len);
                        float y2 = cy + ReadNumber(d, ref i, len);
                        float x = cx + ReadNumber(d, ref i, len);
                        float y = cy + ReadNumber(d, ref i, len);
                        path.CubicBezierTo(rx, ry, x2, y2, x, y);
                        lcx = x2; lcy = y2;
                        cx = x; cy = y;
                        lastCmd = 's';
                        break;
                    }
                    case 'Q':
                    {
                        float x1 = ReadNumber(d, ref i, len);
                        float y1 = ReadNumber(d, ref i, len);
                        float x = ReadNumber(d, ref i, len);
                        float y = ReadNumber(d, ref i, len);
                        path.QuadraticBezierTo(x1, y1, x, y);
                        lcx = x1; lcy = y1;
                        cx = x; cy = y;
                        lastCmd = 'Q';
                        break;
                    }
                    case 'q':
                    {
                        float x1 = cx + ReadNumber(d, ref i, len);
                        float y1 = cy + ReadNumber(d, ref i, len);
                        float x = cx + ReadNumber(d, ref i, len);
                        float y = cy + ReadNumber(d, ref i, len);
                        path.QuadraticBezierTo(x1, y1, x, y);
                        lcx = x1; lcy = y1;
                        cx = x; cy = y;
                        lastCmd = 'q';
                        break;
                    }
                    case 'T':
                    {
                        // Smooth quadratic: reflect previous control point
                        float rx = 2 * cx - lcx;
                        float ry = 2 * cy - lcy;
                        if (lastCmd != 'Q' && lastCmd != 'q' && lastCmd != 'T' && lastCmd != 't')
                        { rx = cx; ry = cy; }

                        float x = ReadNumber(d, ref i, len);
                        float y = ReadNumber(d, ref i, len);
                        path.QuadraticBezierTo(rx, ry, x, y);
                        lcx = rx; lcy = ry;
                        cx = x; cy = y;
                        lastCmd = 'T';
                        break;
                    }
                    case 't':
                    {
                        float rx = 2 * cx - lcx;
                        float ry = 2 * cy - lcy;
                        if (lastCmd != 'Q' && lastCmd != 'q' && lastCmd != 'T' && lastCmd != 't')
                        { rx = cx; ry = cy; }

                        float x = cx + ReadNumber(d, ref i, len);
                        float y = cy + ReadNumber(d, ref i, len);
                        path.QuadraticBezierTo(rx, ry, x, y);
                        lcx = rx; lcy = ry;
                        cx = x; cy = y;
                        lastCmd = 't';
                        break;
                    }
                    case 'A':
                    case 'a':
                    {
                        float rx = ReadNumber(d, ref i, len);
                        float ry = ReadNumber(d, ref i, len);
                        float rotation = ReadNumber(d, ref i, len);
                        float largeArc = ReadNumber(d, ref i, len);
                        float sweep = ReadNumber(d, ref i, len);
                        float x, y;
                        if (cmd == 'A')
                        {
                            x = ReadNumber(d, ref i, len);
                            y = ReadNumber(d, ref i, len);
                        }
                        else
                        {
                            x = cx + ReadNumber(d, ref i, len);
                            y = cy + ReadNumber(d, ref i, len);
                        }
                        ArcToBezier(path, cx, cy, rx, ry, rotation, largeArc != 0, sweep != 0, x, y);
                        cx = x; cy = y;
                        lastCmd = cmd;
                        break;
                    }
                    case 'Z':
                    case 'z':
                    {
                        path.Close();
                        cx = sx; cy = sy;
                        lastCmd = cmd;
                        break;
                    }
                }
            }

            return path;
        }

        private static bool IsCommand(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }

        private static void SkipWhitespaceAndCommas(string s, ref int i, int len)
        {
            while (i < len && (s[i] == ' ' || s[i] == '\t' || s[i] == '\n' || s[i] == '\r' || s[i] == ','))
                i++;
        }

        private static float ReadNumber(string s, ref int i, int len)
        {
            SkipWhitespaceAndCommas(s, ref i, len);
            if (i >= len) return 0;

            int start = i;

            // Sign
            if (i < len && (s[i] == '-' || s[i] == '+'))
                i++;

            // Integer part
            while (i < len && s[i] >= '0' && s[i] <= '9')
                i++;

            // Decimal part
            if (i < len && s[i] == '.')
            {
                i++;
                while (i < len && s[i] >= '0' && s[i] <= '9')
                    i++;
            }

            // Exponent
            if (i < len && (s[i] == 'e' || s[i] == 'E'))
            {
                i++;
                if (i < len && (s[i] == '-' || s[i] == '+'))
                    i++;
                while (i < len && s[i] >= '0' && s[i] <= '9')
                    i++;
            }

            if (i == start) return 0;

#if NETSTANDARD2_0
            return float.Parse(s.Substring(start, i - start),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture);
#else
            return float.Parse(s.AsSpan(start, i - start),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture);
#endif
        }

        /// <summary>
        /// Convert an SVG arc to cubic Bezier curves using endpoint parameterization.
        /// Based on the SVG spec arc implementation notes (§F.6).
        /// </summary>
        private static void ArcToBezier(PathData path, float x1, float y1,
            float rxIn, float ryIn, float angleDeg,
            bool largeArc, bool sweep, float x2, float y2)
        {
            // Degenerate cases
            if (Math.Abs(x1 - x2) < 1e-6f && Math.Abs(y1 - y2) < 1e-6f)
                return;

            float rx = Math.Abs(rxIn);
            float ry = Math.Abs(ryIn);

            if (rx < 1e-6f || ry < 1e-6f)
            {
                path.LineTo(x2, y2);
                return;
            }

            float angle = angleDeg * (float)(Math.PI / 180.0);
            float cosA = (float)Math.Cos(angle);
            float sinA = (float)Math.Sin(angle);

            // Step 1: Compute (x1', y1') — midpoint in rotated frame
            float dx2 = (x1 - x2) / 2f;
            float dy2 = (y1 - y2) / 2f;
            float x1p = cosA * dx2 + sinA * dy2;
            float y1p = -sinA * dx2 + cosA * dy2;

            // Step 2: Correct radii
            float x1pSq = x1p * x1p;
            float y1pSq = y1p * y1p;
            float rxSq = rx * rx;
            float rySq = ry * ry;
            float lambda = x1pSq / rxSq + y1pSq / rySq;
            if (lambda > 1f)
            {
                float sqrtLambda = (float)Math.Sqrt(lambda);
                rx *= sqrtLambda;
                ry *= sqrtLambda;
                rxSq = rx * rx;
                rySq = ry * ry;
            }

            // Step 3: Compute center point in rotated frame
            float num = rxSq * rySq - rxSq * y1pSq - rySq * x1pSq;
            float den = rxSq * y1pSq + rySq * x1pSq;
            float sq = (den > 0) ? (float)Math.Sqrt(Math.Max(0, num / den)) : 0f;
            if (largeArc == sweep) sq = -sq;

            float cxp = sq * rx * y1p / ry;
            float cyp = -sq * ry * x1p / rx;

            // Step 4: Compute center point in original frame
            float cxr = cosA * cxp - sinA * cyp + (x1 + x2) / 2f;
            float cyr = sinA * cxp + cosA * cyp + (y1 + y2) / 2f;

            // Step 5: Compute start and sweep angles
            float ux = (x1p - cxp) / rx;
            float uy = (y1p - cyp) / ry;
            float vx = (-x1p - cxp) / rx;
            float vy = (-y1p - cyp) / ry;

            float startAngle = AngleBetween(1, 0, ux, uy);
            float sweepAngle = AngleBetween(ux, uy, vx, vy);

            if (!sweep && sweepAngle > 0) sweepAngle -= 2f * (float)Math.PI;
            else if (sweep && sweepAngle < 0) sweepAngle += 2f * (float)Math.PI;

            // Split into segments of at most π/2
            int segments = (int)Math.Ceiling(Math.Abs(sweepAngle) / (Math.PI / 2.0));
            if (segments < 1) segments = 1;

            float segAngle = sweepAngle / segments;
            float alpha = (float)(4.0 / 3.0 * Math.Tan(segAngle / 4.0));

            float currentAngle = startAngle;
            for (int seg = 0; seg < segments; seg++)
            {
                float cos1 = (float)Math.Cos(currentAngle);
                float sin1 = (float)Math.Sin(currentAngle);
                float nextAngle = currentAngle + segAngle;
                float cos2 = (float)Math.Cos(nextAngle);
                float sin2 = (float)Math.Sin(nextAngle);

                // Control points in unit circle
                float cpx1 = cos1 - alpha * sin1;
                float cpy1 = sin1 + alpha * cos1;
                float cpx2 = cos2 + alpha * sin2;
                float cpy2 = sin2 - alpha * cos2;

                // Transform back to original coordinate system
                float bx1 = cosA * rx * cpx1 - sinA * ry * cpy1 + cxr;
                float by1 = sinA * rx * cpx1 + cosA * ry * cpy1 + cyr;
                float bx2 = cosA * rx * cpx2 - sinA * ry * cpy2 + cxr;
                float by2 = sinA * rx * cpx2 + cosA * ry * cpy2 + cyr;
                float bx = cosA * rx * cos2 - sinA * ry * sin2 + cxr;
                float by = sinA * rx * cos2 + cosA * ry * sin2 + cyr;

                path.CubicBezierTo(bx1, by1, bx2, by2, bx, by);
                currentAngle = nextAngle;
            }
        }

        private static float AngleBetween(float ux, float uy, float vx, float vy)
        {
            float dot = ux * vx + uy * vy;
            float len = (float)(Math.Sqrt(ux * ux + uy * uy) * Math.Sqrt(vx * vx + vy * vy));
            float cos = len > 0 ? Math.Max(-1f, Math.Min(1f, dot / len)) : 0f;
            float angle = (float)Math.Acos(cos);
            if (ux * vy - uy * vx < 0) angle = -angle;
            return angle;
        }
    }
}
