using Rend.Core;

namespace Rend.Html.Parser.Internal
{
    /// <summary>
    /// Lookup tables for HTML element categories used by the tree builder.
    /// All tag names are pre-interned so lookups use reference equality.
    /// </summary>
    internal static class ElementCategories
    {
        // Pre-intern all tag names we need
        private static readonly StringPool P = StringPool.HtmlNames;

        // Void elements (no closing tag)
        private static readonly string _area = P.Intern("area");
        private static readonly string _base = P.Intern("base");
        private static readonly string _br = P.Intern("br");
        private static readonly string _col = P.Intern("col");
        private static readonly string _embed = P.Intern("embed");
        private static readonly string _hr = P.Intern("hr");
        private static readonly string _img = P.Intern("img");
        private static readonly string _input = P.Intern("input");
        private static readonly string _link = P.Intern("link");
        private static readonly string _meta = P.Intern("meta");
        private static readonly string _param = P.Intern("param");
        private static readonly string _source = P.Intern("source");
        private static readonly string _track = P.Intern("track");
        private static readonly string _wbr = P.Intern("wbr");

        // Raw text elements
        private static readonly string _script = P.Intern("script");
        private static readonly string _style = P.Intern("style");

        // RCDATA elements
        private static readonly string _textarea = P.Intern("textarea");
        private static readonly string _title = P.Intern("title");

        // Formatting elements
        private static readonly string _a = P.Intern("a");
        private static readonly string _b = P.Intern("b");
        private static readonly string _big = P.Intern("big");
        private static readonly string _code = P.Intern("code");
        private static readonly string _em = P.Intern("em");
        private static readonly string _font = P.Intern("font");
        private static readonly string _i = P.Intern("i");
        private static readonly string _nobr = P.Intern("nobr");
        private static readonly string _s = P.Intern("s");
        private static readonly string _small = P.Intern("small");
        private static readonly string _strike = P.Intern("strike");
        private static readonly string _strong = P.Intern("strong");
        private static readonly string _tt = P.Intern("tt");
        private static readonly string _u = P.Intern("u");

        // Heading elements
        private static readonly string _h1 = P.Intern("h1");
        private static readonly string _h2 = P.Intern("h2");
        private static readonly string _h3 = P.Intern("h3");
        private static readonly string _h4 = P.Intern("h4");
        private static readonly string _h5 = P.Intern("h5");
        private static readonly string _h6 = P.Intern("h6");

        // Key structural elements
        internal static readonly string Html = P.Intern("html");
        internal static readonly string Head = P.Intern("head");
        internal static readonly string Body = P.Intern("body");

        // Table elements
        private static readonly string _table = P.Intern("table");
        private static readonly string _tbody = P.Intern("tbody");
        private static readonly string _thead = P.Intern("thead");
        private static readonly string _tfoot = P.Intern("tfoot");
        private static readonly string _tr = P.Intern("tr");
        private static readonly string _td = P.Intern("td");
        private static readonly string _th = P.Intern("th");
        private static readonly string _caption = P.Intern("caption");
        private static readonly string _colgroup = P.Intern("colgroup");

        // Other structural/special
        private static readonly string _address = P.Intern("address");
        private static readonly string _article = P.Intern("article");
        private static readonly string _aside = P.Intern("aside");
        private static readonly string _blockquote = P.Intern("blockquote");
        private static readonly string _center = P.Intern("center");
        private static readonly string _details = P.Intern("details");
        private static readonly string _dialog = P.Intern("dialog");
        private static readonly string _dir = P.Intern("dir");
        private static readonly string _div = P.Intern("div");
        private static readonly string _dl = P.Intern("dl");
        private static readonly string _fieldset = P.Intern("fieldset");
        private static readonly string _figcaption = P.Intern("figcaption");
        private static readonly string _figure = P.Intern("figure");
        private static readonly string _footer = P.Intern("footer");
        private static readonly string _form = P.Intern("form");
        private static readonly string _header = P.Intern("header");
        private static readonly string _hgroup = P.Intern("hgroup");
        private static readonly string _li = P.Intern("li");
        private static readonly string _listing = P.Intern("listing");
        private static readonly string _main = P.Intern("main");
        private static readonly string _menu = P.Intern("menu");
        private static readonly string _nav = P.Intern("nav");
        private static readonly string _ol = P.Intern("ol");
        private static readonly string _p = P.Intern("p");
        private static readonly string _plaintext = P.Intern("plaintext");
        private static readonly string _pre = P.Intern("pre");
        private static readonly string _section = P.Intern("section");
        private static readonly string _summary = P.Intern("summary");
        private static readonly string _ul = P.Intern("ul");

        // Other elements
        private static readonly string _dd = P.Intern("dd");
        private static readonly string _dt = P.Intern("dt");
        private static readonly string _frameset = P.Intern("frameset");
        private static readonly string _frame = P.Intern("frame");
        private static readonly string _noframes = P.Intern("noframes");
        private static readonly string _noscript = P.Intern("noscript");
        private static readonly string _optgroup = P.Intern("optgroup");
        private static readonly string _option = P.Intern("option");
        private static readonly string _select = P.Intern("select");
        private static readonly string _template = P.Intern("template");
        private static readonly string _body2 = P.Intern("body");
        private static readonly string _html2 = P.Intern("html");
        private static readonly string _marquee = P.Intern("marquee");
        private static readonly string _object = P.Intern("object");
        private static readonly string _applet = P.Intern("applet");
        private static readonly string _button = P.Intern("button");
        private static readonly string _ruby = P.Intern("ruby");
        private static readonly string _rb = P.Intern("rb");
        private static readonly string _rt = P.Intern("rt");
        private static readonly string _rtc = P.Intern("rtc");
        private static readonly string _rp = P.Intern("rp");
        private static readonly string _xmp = P.Intern("xmp");
        private static readonly string _iframe = P.Intern("iframe");
        private static readonly string _noembed = P.Intern("noembed");
        private static readonly string _span = P.Intern("span");
        private static readonly string _sub = P.Intern("sub");
        private static readonly string _sup = P.Intern("sup");
        private static readonly string _label = P.Intern("label");
        private static readonly string _output = P.Intern("output");
        private static readonly string _legend = P.Intern("legend");
        private static readonly string _datalist = P.Intern("datalist");
        private static readonly string _progress = P.Intern("progress");
        private static readonly string _meter = P.Intern("meter");

        public static bool IsVoidElement(string tag)
        {
            return ReferenceEquals(tag, _area) || ReferenceEquals(tag, _base) ||
                   ReferenceEquals(tag, _br) || ReferenceEquals(tag, _col) ||
                   ReferenceEquals(tag, _embed) || ReferenceEquals(tag, _hr) ||
                   ReferenceEquals(tag, _img) || ReferenceEquals(tag, _input) ||
                   ReferenceEquals(tag, _link) || ReferenceEquals(tag, _meta) ||
                   ReferenceEquals(tag, _param) || ReferenceEquals(tag, _source) ||
                   ReferenceEquals(tag, _track) || ReferenceEquals(tag, _wbr);
        }

        public static bool IsRawTextElement(string tag)
        {
            return ReferenceEquals(tag, _script) || ReferenceEquals(tag, _style) ||
                   ReferenceEquals(tag, _xmp) || ReferenceEquals(tag, _iframe) ||
                   ReferenceEquals(tag, _noembed) || ReferenceEquals(tag, _noframes) ||
                   ReferenceEquals(tag, _noscript);
        }

        public static bool IsRcDataElement(string tag)
        {
            return ReferenceEquals(tag, _textarea) || ReferenceEquals(tag, _title);
        }

        public static bool IsFormattingElement(string tag)
        {
            return ReferenceEquals(tag, _a) || ReferenceEquals(tag, _b) ||
                   ReferenceEquals(tag, _big) || ReferenceEquals(tag, _code) ||
                   ReferenceEquals(tag, _em) || ReferenceEquals(tag, _font) ||
                   ReferenceEquals(tag, _i) || ReferenceEquals(tag, _nobr) ||
                   ReferenceEquals(tag, _s) || ReferenceEquals(tag, _small) ||
                   ReferenceEquals(tag, _strike) || ReferenceEquals(tag, _strong) ||
                   ReferenceEquals(tag, _tt) || ReferenceEquals(tag, _u);
        }

        public static bool IsHeadingElement(string tag)
        {
            return ReferenceEquals(tag, _h1) || ReferenceEquals(tag, _h2) ||
                   ReferenceEquals(tag, _h3) || ReferenceEquals(tag, _h4) ||
                   ReferenceEquals(tag, _h5) || ReferenceEquals(tag, _h6);
        }

        /// <summary>
        /// Special elements per WHATWG spec. These have special insertion rules
        /// and are scope markers for various element scopes.
        /// </summary>
        public static bool IsSpecialElement(string tag)
        {
            return ReferenceEquals(tag, _address) || ReferenceEquals(tag, _applet) ||
                   ReferenceEquals(tag, _area) || ReferenceEquals(tag, _article) ||
                   ReferenceEquals(tag, _aside) || ReferenceEquals(tag, _base) ||
                   ReferenceEquals(tag, _blockquote) || ReferenceEquals(tag, Body) ||
                   ReferenceEquals(tag, _br) || ReferenceEquals(tag, _button) ||
                   ReferenceEquals(tag, _caption) || ReferenceEquals(tag, _center) ||
                   ReferenceEquals(tag, _col) || ReferenceEquals(tag, _colgroup) ||
                   ReferenceEquals(tag, _dd) || ReferenceEquals(tag, _details) ||
                   ReferenceEquals(tag, _dialog) || ReferenceEquals(tag, _dir) ||
                   ReferenceEquals(tag, _div) || ReferenceEquals(tag, _dl) ||
                   ReferenceEquals(tag, _dt) || ReferenceEquals(tag, _embed) ||
                   ReferenceEquals(tag, _fieldset) || ReferenceEquals(tag, _figcaption) ||
                   ReferenceEquals(tag, _figure) || ReferenceEquals(tag, _footer) ||
                   ReferenceEquals(tag, _form) || ReferenceEquals(tag, _frame) ||
                   ReferenceEquals(tag, _frameset) || IsHeadingElement(tag) ||
                   ReferenceEquals(tag, Head) || ReferenceEquals(tag, _header) ||
                   ReferenceEquals(tag, _hgroup) || ReferenceEquals(tag, _hr) ||
                   ReferenceEquals(tag, Html) || ReferenceEquals(tag, _iframe) ||
                   ReferenceEquals(tag, _img) || ReferenceEquals(tag, _input) ||
                   ReferenceEquals(tag, _li) || ReferenceEquals(tag, _link) ||
                   ReferenceEquals(tag, _listing) || ReferenceEquals(tag, _main) ||
                   ReferenceEquals(tag, _marquee) || ReferenceEquals(tag, _menu) ||
                   ReferenceEquals(tag, _meta) || ReferenceEquals(tag, _nav) ||
                   ReferenceEquals(tag, _noembed) || ReferenceEquals(tag, _noframes) ||
                   ReferenceEquals(tag, _noscript) || ReferenceEquals(tag, _object) ||
                   ReferenceEquals(tag, _ol) || ReferenceEquals(tag, _p) ||
                   ReferenceEquals(tag, _param) || ReferenceEquals(tag, _plaintext) ||
                   ReferenceEquals(tag, _pre) || ReferenceEquals(tag, _script) ||
                   ReferenceEquals(tag, _section) || ReferenceEquals(tag, _select) ||
                   ReferenceEquals(tag, _source) || ReferenceEquals(tag, _style) ||
                   ReferenceEquals(tag, _summary) || ReferenceEquals(tag, _table) ||
                   ReferenceEquals(tag, _tbody) || ReferenceEquals(tag, _td) ||
                   ReferenceEquals(tag, _template) || ReferenceEquals(tag, _textarea) ||
                   ReferenceEquals(tag, _tfoot) || ReferenceEquals(tag, _th) ||
                   ReferenceEquals(tag, _thead) || ReferenceEquals(tag, _title) ||
                   ReferenceEquals(tag, _tr) || ReferenceEquals(tag, _track) ||
                   ReferenceEquals(tag, _ul) || ReferenceEquals(tag, _wbr) ||
                   ReferenceEquals(tag, _xmp);
        }

        /// <summary>
        /// Elements that imply closing a paragraph when opened.
        /// </summary>
        public static bool ClosesImpliedParagraph(string tag)
        {
            // Note: <hr> is intentionally excluded here — it has its own handler
            // in HtmlTreeBuilder.HandleInBodyStartTag that closes <p> and pops itself
            // as a void element. Including it here would short-circuit to the generic
            // block handler which leaves hr on the open elements stack.
            return ReferenceEquals(tag, _address) || ReferenceEquals(tag, _article) ||
                   ReferenceEquals(tag, _aside) || ReferenceEquals(tag, _blockquote) ||
                   ReferenceEquals(tag, _center) || ReferenceEquals(tag, _details) ||
                   ReferenceEquals(tag, _dialog) || ReferenceEquals(tag, _dir) ||
                   ReferenceEquals(tag, _div) || ReferenceEquals(tag, _dl) ||
                   ReferenceEquals(tag, _fieldset) || ReferenceEquals(tag, _figcaption) ||
                   ReferenceEquals(tag, _figure) || ReferenceEquals(tag, _footer) ||
                   ReferenceEquals(tag, _form) || ReferenceEquals(tag, _header) ||
                   ReferenceEquals(tag, _hgroup) ||
                   IsHeadingElement(tag) || ReferenceEquals(tag, _li) ||
                   ReferenceEquals(tag, _listing) || ReferenceEquals(tag, _main) ||
                   ReferenceEquals(tag, _menu) || ReferenceEquals(tag, _nav) ||
                   ReferenceEquals(tag, _ol) || ReferenceEquals(tag, _p) ||
                   ReferenceEquals(tag, _plaintext) || ReferenceEquals(tag, _pre) ||
                   ReferenceEquals(tag, _section) || ReferenceEquals(tag, _summary) ||
                   ReferenceEquals(tag, _table) || ReferenceEquals(tag, _ul);
        }

        /// <summary>Scope markers for "in scope" checks (general scope).</summary>
        public static bool IsScopeMarker(string tag)
        {
            return ReferenceEquals(tag, _applet) || ReferenceEquals(tag, _caption) ||
                   ReferenceEquals(tag, Html) || ReferenceEquals(tag, _table) ||
                   ReferenceEquals(tag, _td) || ReferenceEquals(tag, _th) ||
                   ReferenceEquals(tag, _marquee) || ReferenceEquals(tag, _object) ||
                   ReferenceEquals(tag, _template);
        }

        /// <summary>Scope markers for "in list item scope".</summary>
        public static bool IsListItemScopeMarker(string tag)
        {
            return IsScopeMarker(tag) || ReferenceEquals(tag, _ol) || ReferenceEquals(tag, _ul);
        }

        /// <summary>Scope markers for "in button scope".</summary>
        public static bool IsButtonScopeMarker(string tag)
        {
            return IsScopeMarker(tag) || ReferenceEquals(tag, _button);
        }

        /// <summary>Scope markers for "in table scope".</summary>
        public static bool IsTableScopeMarker(string tag)
        {
            return ReferenceEquals(tag, Html) || ReferenceEquals(tag, _table) ||
                   ReferenceEquals(tag, _template);
        }

        /// <summary>Scope markers for "in select scope" — everything except optgroup and option.</summary>
        public static bool IsSelectScopeMarker(string tag)
        {
            return !(ReferenceEquals(tag, _optgroup) || ReferenceEquals(tag, _option));
        }

        /// <summary>Elements that are implied to close on end tag.</summary>
        public static bool HasImpliedEndTag(string tag)
        {
            return ReferenceEquals(tag, _dd) || ReferenceEquals(tag, _dt) ||
                   ReferenceEquals(tag, _li) || ReferenceEquals(tag, _optgroup) ||
                   ReferenceEquals(tag, _option) || ReferenceEquals(tag, _p) ||
                   ReferenceEquals(tag, _rb) || ReferenceEquals(tag, _rp) ||
                   ReferenceEquals(tag, _rt) || ReferenceEquals(tag, _rtc);
        }
    }
}
