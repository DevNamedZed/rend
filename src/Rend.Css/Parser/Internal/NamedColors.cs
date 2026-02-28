using Rend.Core.Values;

namespace Rend.Css.Parser.Internal
{
    internal static class NamedColors
    {
        // Sorted alphabetically (lowercase) for binary search
        private static readonly string[] _names = new string[]
        {
            "aliceblue",
            "antiquewhite",
            "aqua",
            "aquamarine",
            "azure",
            "beige",
            "bisque",
            "black",
            "blanchedalmond",
            "blue",
            "blueviolet",
            "brown",
            "burlywood",
            "cadetblue",
            "chartreuse",
            "chocolate",
            "coral",
            "cornflowerblue",
            "cornsilk",
            "crimson",
            "cyan",
            "darkblue",
            "darkcyan",
            "darkgoldenrod",
            "darkgray",
            "darkgreen",
            "darkgrey",
            "darkkhaki",
            "darkmagenta",
            "darkolivegreen",
            "darkorange",
            "darkorchid",
            "darkred",
            "darksalmon",
            "darkseagreen",
            "darkslateblue",
            "darkslategray",
            "darkslategrey",
            "darkturquoise",
            "darkviolet",
            "deeppink",
            "deepskyblue",
            "dimgray",
            "dimgrey",
            "dodgerblue",
            "firebrick",
            "floralwhite",
            "forestgreen",
            "fuchsia",
            "gainsboro",
            "ghostwhite",
            "gold",
            "goldenrod",
            "gray",
            "green",
            "greenyellow",
            "grey",
            "honeydew",
            "hotpink",
            "indianred",
            "indigo",
            "ivory",
            "khaki",
            "lavender",
            "lavenderblush",
            "lawngreen",
            "lemonchiffon",
            "lightblue",
            "lightcoral",
            "lightcyan",
            "lightgoldenrodyellow",
            "lightgray",
            "lightgreen",
            "lightgrey",
            "lightpink",
            "lightsalmon",
            "lightseagreen",
            "lightskyblue",
            "lightslategray",
            "lightslategrey",
            "lightsteelblue",
            "lightyellow",
            "lime",
            "limegreen",
            "linen",
            "magenta",
            "maroon",
            "mediumaquamarine",
            "mediumblue",
            "mediumorchid",
            "mediumpurple",
            "mediumseagreen",
            "mediumslateblue",
            "mediumspringgreen",
            "mediumturquoise",
            "mediumvioletred",
            "midnightblue",
            "mintcream",
            "mistyrose",
            "moccasin",
            "navajowhite",
            "navy",
            "oldlace",
            "olive",
            "olivedrab",
            "orange",
            "orangered",
            "orchid",
            "palegoldenrod",
            "palegreen",
            "paleturquoise",
            "palevioletred",
            "papayawhip",
            "peachpuff",
            "peru",
            "pink",
            "plum",
            "powderblue",
            "purple",
            "rebeccapurple",
            "red",
            "rosybrown",
            "royalblue",
            "saddlebrown",
            "salmon",
            "sandybrown",
            "seagreen",
            "seashell",
            "sienna",
            "silver",
            "skyblue",
            "slateblue",
            "slategray",
            "slategrey",
            "snow",
            "springgreen",
            "steelblue",
            "tan",
            "teal",
            "thistle",
            "tomato",
            "turquoise",
            "violet",
            "wheat",
            "white",
            "whitesmoke",
            "yellow",
            "yellowgreen",
        };

        private static readonly CssColor[] _colors = new CssColor[]
        {
            new CssColor(240, 248, 255), // aliceblue
            new CssColor(250, 235, 215), // antiquewhite
            new CssColor(0, 255, 255),   // aqua
            new CssColor(127, 255, 212), // aquamarine
            new CssColor(240, 255, 255), // azure
            new CssColor(245, 245, 220), // beige
            new CssColor(255, 228, 196), // bisque
            new CssColor(0, 0, 0),       // black
            new CssColor(255, 235, 205), // blanchedalmond
            new CssColor(0, 0, 255),     // blue
            new CssColor(138, 43, 226),  // blueviolet
            new CssColor(165, 42, 42),   // brown
            new CssColor(222, 184, 135), // burlywood
            new CssColor(95, 158, 160),  // cadetblue
            new CssColor(127, 255, 0),   // chartreuse
            new CssColor(210, 105, 30),  // chocolate
            new CssColor(255, 127, 80),  // coral
            new CssColor(100, 149, 237), // cornflowerblue
            new CssColor(255, 248, 220), // cornsilk
            new CssColor(220, 20, 60),   // crimson
            new CssColor(0, 255, 255),   // cyan
            new CssColor(0, 0, 139),     // darkblue
            new CssColor(0, 139, 139),   // darkcyan
            new CssColor(184, 134, 11),  // darkgoldenrod
            new CssColor(169, 169, 169), // darkgray
            new CssColor(0, 100, 0),     // darkgreen
            new CssColor(169, 169, 169), // darkgrey
            new CssColor(189, 183, 107), // darkkhaki
            new CssColor(139, 0, 139),   // darkmagenta
            new CssColor(85, 107, 47),   // darkolivegreen
            new CssColor(255, 140, 0),   // darkorange
            new CssColor(153, 50, 204),  // darkorchid
            new CssColor(139, 0, 0),     // darkred
            new CssColor(233, 150, 122), // darksalmon
            new CssColor(143, 188, 143), // darkseagreen
            new CssColor(72, 61, 139),   // darkslateblue
            new CssColor(47, 79, 79),    // darkslategray
            new CssColor(47, 79, 79),    // darkslategrey
            new CssColor(0, 206, 209),   // darkturquoise
            new CssColor(148, 0, 211),   // darkviolet
            new CssColor(255, 20, 147),  // deeppink
            new CssColor(0, 191, 255),   // deepskyblue
            new CssColor(105, 105, 105), // dimgray
            new CssColor(105, 105, 105), // dimgrey
            new CssColor(30, 144, 255),  // dodgerblue
            new CssColor(178, 34, 34),   // firebrick
            new CssColor(255, 250, 240), // floralwhite
            new CssColor(34, 139, 34),   // forestgreen
            new CssColor(255, 0, 255),   // fuchsia
            new CssColor(220, 220, 220), // gainsboro
            new CssColor(248, 248, 255), // ghostwhite
            new CssColor(255, 215, 0),   // gold
            new CssColor(218, 165, 32),  // goldenrod
            new CssColor(128, 128, 128), // gray
            new CssColor(0, 128, 0),     // green
            new CssColor(173, 255, 47),  // greenyellow
            new CssColor(128, 128, 128), // grey
            new CssColor(240, 255, 240), // honeydew
            new CssColor(255, 105, 180), // hotpink
            new CssColor(205, 92, 92),   // indianred
            new CssColor(75, 0, 130),    // indigo
            new CssColor(255, 255, 240), // ivory
            new CssColor(240, 230, 140), // khaki
            new CssColor(230, 230, 250), // lavender
            new CssColor(255, 240, 245), // lavenderblush
            new CssColor(124, 252, 0),   // lawngreen
            new CssColor(255, 250, 205), // lemonchiffon
            new CssColor(173, 216, 230), // lightblue
            new CssColor(240, 128, 128), // lightcoral
            new CssColor(224, 255, 255), // lightcyan
            new CssColor(250, 250, 210), // lightgoldenrodyellow
            new CssColor(211, 211, 211), // lightgray
            new CssColor(144, 238, 144), // lightgreen
            new CssColor(211, 211, 211), // lightgrey
            new CssColor(255, 182, 193), // lightpink
            new CssColor(255, 160, 122), // lightsalmon
            new CssColor(32, 178, 170),  // lightseagreen
            new CssColor(135, 206, 250), // lightskyblue
            new CssColor(119, 136, 153), // lightslategray
            new CssColor(119, 136, 153), // lightslategrey
            new CssColor(176, 196, 222), // lightsteelblue
            new CssColor(255, 255, 224), // lightyellow
            new CssColor(0, 255, 0),     // lime
            new CssColor(50, 205, 50),   // limegreen
            new CssColor(250, 240, 230), // linen
            new CssColor(255, 0, 255),   // magenta
            new CssColor(128, 0, 0),     // maroon
            new CssColor(102, 205, 170), // mediumaquamarine
            new CssColor(0, 0, 205),     // mediumblue
            new CssColor(186, 85, 211),  // mediumorchid
            new CssColor(147, 112, 219), // mediumpurple
            new CssColor(60, 179, 113),  // mediumseagreen
            new CssColor(123, 104, 238), // mediumslateblue
            new CssColor(0, 250, 154),   // mediumspringgreen
            new CssColor(72, 209, 204),  // mediumturquoise
            new CssColor(199, 21, 133),  // mediumvioletred
            new CssColor(25, 25, 112),   // midnightblue
            new CssColor(245, 255, 250), // mintcream
            new CssColor(255, 228, 225), // mistyrose
            new CssColor(255, 228, 181), // moccasin
            new CssColor(255, 222, 173), // navajowhite
            new CssColor(0, 0, 128),     // navy
            new CssColor(253, 245, 230), // oldlace
            new CssColor(128, 128, 0),   // olive
            new CssColor(107, 142, 35),  // olivedrab
            new CssColor(255, 165, 0),   // orange
            new CssColor(255, 69, 0),    // orangered
            new CssColor(218, 112, 214), // orchid
            new CssColor(238, 232, 170), // palegoldenrod
            new CssColor(152, 251, 152), // palegreen
            new CssColor(175, 238, 238), // paleturquoise
            new CssColor(219, 112, 147), // palevioletred
            new CssColor(255, 239, 213), // papayawhip
            new CssColor(255, 218, 185), // peachpuff
            new CssColor(205, 133, 63),  // peru
            new CssColor(255, 192, 203), // pink
            new CssColor(221, 160, 221), // plum
            new CssColor(176, 224, 230), // powderblue
            new CssColor(128, 0, 128),   // purple
            new CssColor(102, 51, 153),  // rebeccapurple
            new CssColor(255, 0, 0),     // red
            new CssColor(188, 143, 143), // rosybrown
            new CssColor(65, 105, 225),  // royalblue
            new CssColor(139, 69, 19),   // saddlebrown
            new CssColor(250, 128, 114), // salmon
            new CssColor(244, 164, 96),  // sandybrown
            new CssColor(46, 139, 87),   // seagreen
            new CssColor(255, 245, 238), // seashell
            new CssColor(160, 82, 45),   // sienna
            new CssColor(192, 192, 192), // silver
            new CssColor(135, 206, 235), // skyblue
            new CssColor(106, 90, 205),  // slateblue
            new CssColor(112, 128, 144), // slategray
            new CssColor(112, 128, 144), // slategrey
            new CssColor(255, 250, 250), // snow
            new CssColor(0, 255, 127),   // springgreen
            new CssColor(70, 130, 180),  // steelblue
            new CssColor(210, 180, 140), // tan
            new CssColor(0, 128, 128),   // teal
            new CssColor(216, 191, 216), // thistle
            new CssColor(255, 99, 71),   // tomato
            new CssColor(64, 224, 208),  // turquoise
            new CssColor(238, 130, 238), // violet
            new CssColor(245, 222, 179), // wheat
            new CssColor(255, 255, 255), // white
            new CssColor(245, 245, 245), // whitesmoke
            new CssColor(255, 255, 0),   // yellow
            new CssColor(154, 205, 50),  // yellowgreen
        };

        public static bool TryLookup(string name, out CssColor color)
        {
            // Binary search (case-insensitive by pre-lowering input)
            int lo = 0, hi = _names.Length - 1;
            while (lo <= hi)
            {
                int mid = lo + (hi - lo) / 2;
                int cmp = string.Compare(_names[mid], name, System.StringComparison.OrdinalIgnoreCase);
                if (cmp == 0)
                {
                    color = _colors[mid];
                    return true;
                }
                if (cmp < 0) lo = mid + 1;
                else hi = mid - 1;
            }
            color = default;
            return false;
        }
    }
}
