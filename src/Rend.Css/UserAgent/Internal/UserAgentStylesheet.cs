namespace Rend.Css.UserAgent.Internal
{
    /// <summary>
    /// Default browser stylesheet for HTML elements.
    /// Based on the WHATWG rendering spec / Chromium defaults.
    /// </summary>
    internal static class UserAgentStylesheet
    {
        private static Stylesheet? _cached;
        private static readonly object _lock = new object();

        /// <summary>
        /// Get the default user-agent stylesheet (cached after first call).
        /// </summary>
        public static Stylesheet Get()
        {
            if (_cached != null) return _cached;
            lock (_lock)
            {
                if (_cached != null) return _cached;
                _cached = CssParser.Parse(Css);
                return _cached;
            }
        }

        private const string Css = @"
/* Block-level elements */
html, address, blockquote, body, dd, div, dl, dt,
fieldset, form, frame, frameset, h1, h2, h3, h4, h5, h6,
hr, noframes, ol, p, ul, center, dir, menu, pre,
article, aside, details, dialog, figcaption, figure,
footer, header, hgroup, main, nav, section, summary {
    display: block;
}

/* Headings */
h1 { font-size: 2em; font-weight: bold; margin-top: 0.67em; margin-bottom: 0.67em; }
h2 { font-size: 1.5em; font-weight: bold; margin-top: 0.83em; margin-bottom: 0.83em; }
h3 { font-size: 1.17em; font-weight: bold; margin-top: 1em; margin-bottom: 1em; }
h4 { font-weight: bold; margin-top: 1.33em; margin-bottom: 1.33em; }
h5 { font-size: 0.83em; font-weight: bold; margin-top: 1.67em; margin-bottom: 1.67em; }
h6 { font-size: 0.67em; font-weight: bold; margin-top: 2.33em; margin-bottom: 2.33em; }

/* Paragraphs */
p { margin-top: 1em; margin-bottom: 1em; }

/* Lists */
ul, menu, dir { margin-top: 1em; margin-bottom: 1em; padding-left: 40px; }
ol { margin-top: 1em; margin-bottom: 1em; padding-left: 40px; }
ul { list-style-type: disc; }
ol { list-style-type: decimal; }
li { display: list-item; }

/* Definition lists */
dl { margin-top: 1em; margin-bottom: 1em; }
dd { margin-left: 40px; }

/* Blockquote */
blockquote { margin-top: 1em; margin-bottom: 1em; margin-left: 40px; margin-right: 40px; }

/* Table */
table { display: table; border-collapse: separate; border-spacing: 2px; }
thead { display: table-header-group; vertical-align: middle; }
tbody { display: table-row-group; vertical-align: middle; }
tfoot { display: table-footer-group; vertical-align: middle; }
tr { display: table-row; vertical-align: inherit; }
td, th { display: table-cell; vertical-align: inherit; padding: 1px; }
th { font-weight: bold; text-align: center; }
caption { display: table-caption; text-align: center; }
col { display: table-column; }
colgroup { display: table-column-group; }

/* Inline elements */
b, strong { font-weight: bold; }
i, em, cite, var, dfn { font-style: italic; }
u, ins { text-decoration: underline; }
s, strike, del { text-decoration: line-through; }
small { font-size: 0.83em; }
sub { vertical-align: sub; font-size: 0.83em; }
sup { vertical-align: super; font-size: 0.83em; }
code, kbd, samp, tt { font-family: monospace; }
pre { white-space: pre; margin-top: 1em; margin-bottom: 1em; font-family: monospace; }
mark { background-color: yellow; color: black; }
abbr[title] { text-decoration: underline; }

/* Links */
a { color: #0000EE; text-decoration: underline; }

/* Form elements */
input, textarea, select, button {
    display: inline-block;
    font-family: inherit;
    font-size: inherit;
}

/* Hidden */
[hidden], template, head, style, script, link, meta, title,
noscript, area, param, source, track, base {
    display: none;
}

/* HR */
hr {
    display: block;
    margin-top: 0.5em;
    margin-bottom: 0.5em;
    border-top-style: solid;
    border-top-width: 1px;
    border-top-color: #808080;
}

/* Image */
img { display: inline-block; }

/* Body */
body { margin: 8px; }

/* HTML */
html { display: block; }

/* Fieldset */
fieldset {
    margin-left: 2px;
    margin-right: 2px;
    padding-top: 0.35em;
    padding-bottom: 0.625em;
    padding-left: 0.75em;
    padding-right: 0.75em;
    border: 2px groove;
}

legend { padding-left: 2px; padding-right: 2px; }

/* Print defaults: prevent awkward page breaks */
@media print {
    img, svg { page-break-inside: avoid; }
    h1, h2, h3, h4, h5, h6 { page-break-after: avoid; }
    table, figure, blockquote { page-break-inside: avoid; }
    p { orphans: 2; widows: 2; }
}
";
    }
}
