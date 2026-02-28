namespace Rend.Html.Selectors.Internal
{
    /// <summary>
    /// Parses the An+B microsyntax used by :nth-child, :nth-of-type, etc.
    /// Examples: "odd", "even", "3", "2n+1", "-n+3", "3n"
    /// </summary>
    internal static class NthParser
    {
        /// <summary>
        /// Parse An+B syntax. Returns (a, b) coefficients.
        /// </summary>
        public static (int a, int b) Parse(string expr)
        {
            expr = expr.Trim().ToLowerInvariant();

            if (expr == "odd") return (2, 1);
            if (expr == "even") return (2, 0);

            // Try plain number
            if (int.TryParse(expr, out int plain))
                return (0, plain);

            int nIndex = expr.IndexOf('n');
            if (nIndex < 0)
                return (0, 0); // Invalid

            // Parse A (before 'n')
            int a;
            string aPart = expr.Substring(0, nIndex).Trim();
            if (aPart.Length == 0 || aPart == "+")
                a = 1;
            else if (aPart == "-")
                a = -1;
            else if (!int.TryParse(aPart, out a))
                return (0, 0);

            // Parse B (after 'n')
            int b = 0;
            string rest = expr.Substring(nIndex + 1).Trim();
            if (rest.Length > 0)
            {
                // Remove whitespace around + or -
                rest = rest.Replace(" ", "");
                if (!int.TryParse(rest, out b))
                    b = 0;
            }

            return (a, b);
        }

        /// <summary>
        /// Check if a 1-based position matches the An+B formula.
        /// </summary>
        public static bool Matches(int a, int b, int position)
        {
            if (a == 0)
                return position == b;

            // position = a*n + b, solve for n: n = (position - b) / a
            int diff = position - b;
            if (diff == 0) return true;
            if (a > 0 && diff < 0) return false;
            if (a < 0 && diff > 0) return false;
            return diff % a == 0;
        }
    }
}
