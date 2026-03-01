using System;
using System.Collections.Generic;
using System.Text;
using Rend.Core;

namespace Rend.Html.Parser.Internal
{
    /// <summary>
    /// WHATWG tree construction algorithm. Consumes tokens from the tokenizer
    /// and builds a DOM tree. Handles insertion modes, implicit tag opening/closing,
    /// active formatting elements, and foster parenting.
    /// </summary>
    internal sealed class HtmlTreeBuilder
    {
        private readonly Document _document;
        private readonly HtmlTokenizer _tokenizer;
        private readonly List<Element> _openElements;
        private readonly ActiveFormattingElements _activeFormattingElements;
        private InsertionMode _insertionMode;
        private InsertionMode _originalInsertionMode;
        private Element? _headElement;
        private Element? _formElement;
        private bool _fosterParenting;
#pragma warning disable CS0414 // assigned but never read — tracks state for frameset mode
        private bool _framesetOk;
#pragma warning restore CS0414
        private readonly StringBuilder _pendingTableText;
        private bool _pendingTableTextHasNonWhitespace;
        private bool _skipNextNewline;

        // Pre-interned tag names used in tree construction
        private static readonly StringPool P = StringPool.HtmlNames;
        private static readonly string _html = P.Intern("html");
        private static readonly string _head = P.Intern("head");
        private static readonly string _body = P.Intern("body");
        private static readonly string _p = P.Intern("p");
        private static readonly string _li = P.Intern("li");
        private static readonly string _dd = P.Intern("dd");
        private static readonly string _dt = P.Intern("dt");
        private static readonly string _a = P.Intern("a");
        private static readonly string _table = P.Intern("table");
        private static readonly string _tbody = P.Intern("tbody");
        private static readonly string _thead = P.Intern("thead");
        private static readonly string _tfoot = P.Intern("tfoot");
        private static readonly string _tr = P.Intern("tr");
        private static readonly string _td = P.Intern("td");
        private static readonly string _th = P.Intern("th");
        private static readonly string _caption = P.Intern("caption");
        private static readonly string _colgroup = P.Intern("colgroup");
        private static readonly string _col = P.Intern("col");
        private static readonly string _select = P.Intern("select");
        private static readonly string _option = P.Intern("option");
        private static readonly string _optgroup = P.Intern("optgroup");
        private static readonly string _template = P.Intern("template");
        private static readonly string _script = P.Intern("script");
        private static readonly string _style = P.Intern("style");
        private static readonly string _title = P.Intern("title");
        private static readonly string _textarea = P.Intern("textarea");
        private static readonly string _pre = P.Intern("pre");
        private static readonly string _listing = P.Intern("listing");
        private static readonly string _form = P.Intern("form");
        private static readonly string _button = P.Intern("button");
        private static readonly string _marquee = P.Intern("marquee");
        private static readonly string _object = P.Intern("object");
        private static readonly string _applet = P.Intern("applet");
        private static readonly string _nobr = P.Intern("nobr");
        private static readonly string _br = P.Intern("br");
        private static readonly string _hr = P.Intern("hr");
        private static readonly string _input = P.Intern("input");
        private static readonly string _img = P.Intern("img");
        private static readonly string _image = P.Intern("image");
        private static readonly string _meta = P.Intern("meta");
        private static readonly string _link = P.Intern("link");
        private static readonly string _base = P.Intern("base");
        private static readonly string _noscript = P.Intern("noscript");
        private static readonly string _frameset = P.Intern("frameset");
        private static readonly string _frame = P.Intern("frame");
        private static readonly string _noframes = P.Intern("noframes");
        private static readonly string _plaintext = P.Intern("plaintext");
        private static readonly string _xmp = P.Intern("xmp");
        private static readonly string _iframe = P.Intern("iframe");
        private static readonly string _noembed = P.Intern("noembed");
        private static readonly string _wbr = P.Intern("wbr");
        private static readonly string _embed = P.Intern("embed");
        private static readonly string _param = P.Intern("param");
        private static readonly string _source = P.Intern("source");
        private static readonly string _track = P.Intern("track");
        private static readonly string _area = P.Intern("area");
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
        private static readonly string _header = P.Intern("header");
        private static readonly string _hgroup = P.Intern("hgroup");
        private static readonly string _main = P.Intern("main");
        private static readonly string _menu = P.Intern("menu");
        private static readonly string _nav = P.Intern("nav");
        private static readonly string _ol = P.Intern("ol");
        private static readonly string _section = P.Intern("section");
        private static readonly string _summary = P.Intern("summary");
        private static readonly string _ul = P.Intern("ul");

        internal HtmlTreeBuilder(Document document, HtmlTokenizer tokenizer)
        {
            _document = document;
            _tokenizer = tokenizer;
            _openElements = new List<Element>(64);
            _activeFormattingElements = new ActiveFormattingElements();
            _insertionMode = InsertionMode.Initial;
            _originalInsertionMode = InsertionMode.Initial;
            _framesetOk = true;
            _fosterParenting = false;
            _pendingTableText = new StringBuilder();
        }

        public Document Run()
        {
            while (_tokenizer.Read(out var token))
            {
                ProcessToken(ref token);
            }
            // Process EOF
            var eof = new HtmlToken { Type = HtmlTokenType.EndOfFile };
            ProcessToken(ref eof);
            return _document;
        }

        private void ProcessToken(ref HtmlToken token)
        {
            switch (_insertionMode)
            {
                case InsertionMode.Initial:
                    HandleInitial(ref token);
                    break;
                case InsertionMode.BeforeHtml:
                    HandleBeforeHtml(ref token);
                    break;
                case InsertionMode.BeforeHead:
                    HandleBeforeHead(ref token);
                    break;
                case InsertionMode.InHead:
                    HandleInHead(ref token);
                    break;
                case InsertionMode.AfterHead:
                    HandleAfterHead(ref token);
                    break;
                case InsertionMode.InBody:
                    HandleInBody(ref token);
                    break;
                case InsertionMode.Text:
                    HandleText(ref token);
                    break;
                case InsertionMode.InTable:
                    HandleInTable(ref token);
                    break;
                case InsertionMode.InTableBody:
                    HandleInTableBody(ref token);
                    break;
                case InsertionMode.InRow:
                    HandleInRow(ref token);
                    break;
                case InsertionMode.InCell:
                    HandleInCell(ref token);
                    break;
                case InsertionMode.InCaption:
                    HandleInCaption(ref token);
                    break;
                case InsertionMode.InColumnGroup:
                    HandleInColumnGroup(ref token);
                    break;
                case InsertionMode.InSelect:
                    HandleInSelect(ref token);
                    break;
                case InsertionMode.InSelectInTable:
                    HandleInSelectInTable(ref token);
                    break;
                case InsertionMode.AfterBody:
                    HandleAfterBody(ref token);
                    break;
                case InsertionMode.AfterAfterBody:
                    HandleAfterAfterBody(ref token);
                    break;
                case InsertionMode.InTableText:
                    HandleInTableText(ref token);
                    break;
                default:
                    HandleInBody(ref token);
                    break;
            }
        }

        // ──────────────────────────────────────────────────────
        //  Insertion Mode: Initial
        // ──────────────────────────────────────────────────────
        private void HandleInitial(ref HtmlToken token)
        {
            if (token.Type == HtmlTokenType.Character && IsWhitespace(token.Character))
                return; // Ignore whitespace

            if (token.Type == HtmlTokenType.Comment)
            {
                _document.AppendChild(_document.CreateComment(token.Data ?? string.Empty));
                return;
            }

            if (token.Type == HtmlTokenType.Doctype)
            {
                var dt = _document.CreateDocumentType(
                    token.DoctypeName ?? "html",
                    token.PublicIdentifier ?? string.Empty,
                    token.SystemIdentifier ?? string.Empty);
                _document.AppendChild(dt);
                _insertionMode = InsertionMode.BeforeHtml;
                return;
            }

            // Anything else — no doctype, switch to BeforeHtml and reprocess
            _insertionMode = InsertionMode.BeforeHtml;
            ProcessToken(ref token);
        }

        // ──────────────────────────────────────────────────────
        //  Insertion Mode: Before HTML
        // ──────────────────────────────────────────────────────
        private void HandleBeforeHtml(ref HtmlToken token)
        {
            if (token.Type == HtmlTokenType.Character && IsWhitespace(token.Character))
                return;

            if (token.Type == HtmlTokenType.Comment)
            {
                _document.AppendChild(_document.CreateComment(token.Data ?? string.Empty));
                return;
            }

            if (token.Type == HtmlTokenType.Doctype)
                return; // Ignore

            if (token.Type == HtmlTokenType.StartTag && ReferenceEquals(token.TagName, _html))
            {
                var el = CreateElement(token.TagName!);
                CopyAttributes(el);
                _document.AppendChild(el);
                _openElements.Add(el);
                _insertionMode = InsertionMode.BeforeHead;
                return;
            }

            if (token.Type == HtmlTokenType.EndTag)
            {
                var tag = token.TagName;
                if (!ReferenceEquals(tag, _head) && !ReferenceEquals(tag, _body) &&
                    !ReferenceEquals(tag, _html) && !ReferenceEquals(tag, _br))
                    return; // Ignore
                // Fall through to "anything else"
            }

            // Anything else: create html element, reprocess
            var htmlEl = CreateElement(_html);
            _document.AppendChild(htmlEl);
            _openElements.Add(htmlEl);
            _insertionMode = InsertionMode.BeforeHead;
            ProcessToken(ref token);
        }

        // ──────────────────────────────────────────────────────
        //  Insertion Mode: Before Head
        // ──────────────────────────────────────────────────────
        private void HandleBeforeHead(ref HtmlToken token)
        {
            if (token.Type == HtmlTokenType.Character && IsWhitespace(token.Character))
                return;

            if (token.Type == HtmlTokenType.Comment)
            {
                InsertComment(token.Data ?? string.Empty);
                return;
            }

            if (token.Type == HtmlTokenType.Doctype)
                return;

            if (token.Type == HtmlTokenType.StartTag && ReferenceEquals(token.TagName, _head))
            {
                var el = InsertElement(token.TagName!);
                CopyAttributes(el);
                _headElement = el;
                _insertionMode = InsertionMode.InHead;
                return;
            }

            if (token.Type == HtmlTokenType.EndTag)
            {
                var tag = token.TagName;
                if (!ReferenceEquals(tag, _head) && !ReferenceEquals(tag, _body) &&
                    !ReferenceEquals(tag, _html) && !ReferenceEquals(tag, _br))
                    return;
            }

            // Anything else: insert head, reprocess
            var headEl = InsertElement(_head);
            _headElement = headEl;
            _insertionMode = InsertionMode.InHead;
            ProcessToken(ref token);
        }

        // ──────────────────────────────────────────────────────
        //  Insertion Mode: In Head
        // ──────────────────────────────────────────────────────
        private void HandleInHead(ref HtmlToken token)
        {
            if (token.Type == HtmlTokenType.Character && IsWhitespace(token.Character))
            {
                InsertCharacter(token.Character);
                return;
            }

            if (token.Type == HtmlTokenType.Comment)
            {
                InsertComment(token.Data ?? string.Empty);
                return;
            }

            if (token.Type == HtmlTokenType.Doctype)
                return;

            if (token.Type == HtmlTokenType.StartTag)
            {
                var tag = token.TagName;

                if (ReferenceEquals(tag, _base) || ReferenceEquals(tag, _link) ||
                    ReferenceEquals(tag, _meta))
                {
                    var el = InsertElement(tag!);
                    CopyAttributes(el);
                    PopOpenElement(); // Self-closing
                    return;
                }

                if (ReferenceEquals(tag, _title))
                {
                    var el = InsertElement(tag!);
                    CopyAttributes(el);
                    _tokenizer.State = TokenizerState.RcData;
                    _originalInsertionMode = _insertionMode;
                    _insertionMode = InsertionMode.Text;
                    return;
                }

                if (ReferenceEquals(tag, _style) || ReferenceEquals(tag, _noframes) ||
                    ReferenceEquals(tag, _noscript))
                {
                    var el = InsertElement(tag!);
                    CopyAttributes(el);
                    _tokenizer.State = TokenizerState.RawText;
                    _originalInsertionMode = _insertionMode;
                    _insertionMode = InsertionMode.Text;
                    return;
                }

                if (ReferenceEquals(tag, _script))
                {
                    var el = InsertElement(tag!);
                    CopyAttributes(el);
                    _tokenizer.State = TokenizerState.ScriptData;
                    _originalInsertionMode = _insertionMode;
                    _insertionMode = InsertionMode.Text;
                    return;
                }

                if (ReferenceEquals(tag, _head))
                    return; // Ignore duplicate head

                if (ReferenceEquals(tag, _template))
                {
                    var el = InsertElement(tag!);
                    CopyAttributes(el);
                    return;
                }
            }

            if (token.Type == HtmlTokenType.EndTag)
            {
                if (ReferenceEquals(token.TagName, _head))
                {
                    PopOpenElement();
                    _insertionMode = InsertionMode.AfterHead;
                    return;
                }

                if (ReferenceEquals(token.TagName, _template))
                {
                    if (!HasInScope(_template))
                        return;
                    PopUntil(_template);
                    return;
                }

                if (!ReferenceEquals(token.TagName, _body) && !ReferenceEquals(token.TagName, _html) &&
                    !ReferenceEquals(token.TagName, _br))
                    return; // Ignore
            }

            // Anything else: pop head, switch to AfterHead, reprocess
            PopOpenElement();
            _insertionMode = InsertionMode.AfterHead;
            ProcessToken(ref token);
        }

        // ──────────────────────────────────────────────────────
        //  Insertion Mode: After Head
        // ──────────────────────────────────────────────────────
        private void HandleAfterHead(ref HtmlToken token)
        {
            if (token.Type == HtmlTokenType.Character && IsWhitespace(token.Character))
            {
                InsertCharacter(token.Character);
                return;
            }

            if (token.Type == HtmlTokenType.Comment)
            {
                InsertComment(token.Data ?? string.Empty);
                return;
            }

            if (token.Type == HtmlTokenType.Doctype)
                return;

            if (token.Type == HtmlTokenType.StartTag)
            {
                if (ReferenceEquals(token.TagName, _body))
                {
                    var el = InsertElement(_body);
                    CopyAttributes(el);
                    _framesetOk = false;
                    _insertionMode = InsertionMode.InBody;
                    return;
                }

                if (ReferenceEquals(token.TagName, _frameset))
                {
                    var el = InsertElement(_frameset);
                    CopyAttributes(el);
                    _insertionMode = InsertionMode.InFrameset;
                    return;
                }

                if (ReferenceEquals(token.TagName, _base) || ReferenceEquals(token.TagName, _link) ||
                    ReferenceEquals(token.TagName, _meta) || ReferenceEquals(token.TagName, _noframes) ||
                    ReferenceEquals(token.TagName, _script) || ReferenceEquals(token.TagName, _style) ||
                    ReferenceEquals(token.TagName, _template) || ReferenceEquals(token.TagName, _title))
                {
                    // Push head back on, process in InHead, pop it
                    if (_headElement != null)
                        _openElements.Add(_headElement);
                    HandleInHead(ref token);
                    _openElements.Remove(_headElement!);
                    return;
                }

                if (ReferenceEquals(token.TagName, _head))
                    return; // Ignore
            }

            if (token.Type == HtmlTokenType.EndTag)
            {
                if (!ReferenceEquals(token.TagName, _body) && !ReferenceEquals(token.TagName, _html) &&
                    !ReferenceEquals(token.TagName, _br) && !ReferenceEquals(token.TagName, _template))
                    return; // Ignore
            }

            // Anything else: insert body, reprocess in InBody
            InsertElement(_body);
            _insertionMode = InsertionMode.InBody;
            ProcessToken(ref token);
        }

        // ──────────────────────────────────────────────────────
        //  Insertion Mode: In Body
        // ──────────────────────────────────────────────────────
        private void HandleInBody(ref HtmlToken token)
        {
            if (token.Type == HtmlTokenType.Character)
            {
                if (token.Character == '\0')
                    return; // Ignore null

                // Skip leading newline after <pre>/<listing>/<textarea> per WHATWG spec
                if (_skipNextNewline)
                {
                    _skipNextNewline = false;
                    if (token.Character == '\n')
                        return;
                }

                _activeFormattingElements.Reconstruct(this);
                InsertCharacter(token.Character);

                if (!IsWhitespace(token.Character))
                    _framesetOk = false;
                return;
            }

            if (token.Type == HtmlTokenType.Comment)
            {
                InsertComment(token.Data ?? string.Empty);
                return;
            }

            if (token.Type == HtmlTokenType.Doctype)
                return;

            if (token.Type == HtmlTokenType.EndOfFile)
            {
                // Stop parsing
                return;
            }

            if (token.Type == HtmlTokenType.StartTag)
            {
                HandleInBodyStartTag(ref token);
                return;
            }

            if (token.Type == HtmlTokenType.EndTag)
            {
                HandleInBodyEndTag(ref token);
                return;
            }
        }

        private void HandleInBodyStartTag(ref HtmlToken token)
        {
            var tag = token.TagName!;

            if (ReferenceEquals(tag, _html))
            {
                // Merge attributes onto existing html element
                if (_openElements.Count > 0)
                    MergeAttributes(_openElements[0]);
                return;
            }

            if (ReferenceEquals(tag, _body))
            {
                // Merge attributes onto existing body element
                if (_openElements.Count >= 2 && ReferenceEquals(_openElements[1].TagName, _body))
                {
                    _framesetOk = false;
                    MergeAttributes(_openElements[1]);
                }
                return;
            }

            // Block-level elements that close <p>
            if (ElementCategories.ClosesImpliedParagraph(tag))
            {
                if (HasInButtonScope(_p))
                    ClosePElement();

                var el = InsertElement(tag);
                CopyAttributes(el);

                if (ReferenceEquals(tag, _pre) || ReferenceEquals(tag, _listing))
                {
                    _framesetOk = false;
                    _skipNextNewline = true;
                }

                if (ReferenceEquals(tag, _form))
                {
                    if (_formElement == null)
                        _formElement = el;
                }

                return;
            }

            // Heading elements
            if (ElementCategories.IsHeadingElement(tag))
            {
                if (HasInButtonScope(_p))
                    ClosePElement();

                // Close any open heading
                if (_openElements.Count > 0 &&
                    ElementCategories.IsHeadingElement(CurrentNode.TagName))
                    PopOpenElement();

                var el = InsertElement(tag);
                CopyAttributes(el);
                return;
            }

            // List items
            if (ReferenceEquals(tag, _li))
            {
                _framesetOk = false;

                for (int i = _openElements.Count - 1; i >= 0; i--)
                {
                    var node = _openElements[i];
                    if (ReferenceEquals(node.TagName, _li))
                    {
                        GenerateImpliedEndTags(_li);
                        PopUntilInclusive(_li);
                        break;
                    }
                    if (ElementCategories.IsSpecialElement(node.TagName) &&
                        !ReferenceEquals(node.TagName, _address) &&
                        !ReferenceEquals(node.TagName, _div) &&
                        !ReferenceEquals(node.TagName, _p))
                        break;
                }

                if (HasInButtonScope(_p))
                    ClosePElement();

                var el = InsertElement(tag);
                CopyAttributes(el);
                return;
            }

            if (ReferenceEquals(tag, _dd) || ReferenceEquals(tag, _dt))
            {
                _framesetOk = false;

                for (int i = _openElements.Count - 1; i >= 0; i--)
                {
                    var node = _openElements[i];
                    if (ReferenceEquals(node.TagName, _dd) || ReferenceEquals(node.TagName, _dt))
                    {
                        GenerateImpliedEndTags(node.TagName);
                        PopUntilInclusive(node.TagName);
                        break;
                    }
                    if (ElementCategories.IsSpecialElement(node.TagName) &&
                        !ReferenceEquals(node.TagName, _address) &&
                        !ReferenceEquals(node.TagName, _div) &&
                        !ReferenceEquals(node.TagName, _p))
                        break;
                }

                if (HasInButtonScope(_p))
                    ClosePElement();

                var el = InsertElement(tag);
                CopyAttributes(el);
                return;
            }

            if (ReferenceEquals(tag, _plaintext))
            {
                if (HasInButtonScope(_p))
                    ClosePElement();
                InsertElement(tag);
                _tokenizer.State = TokenizerState.PlainText;
                return;
            }

            if (ReferenceEquals(tag, _button))
            {
                if (HasInScope(_button))
                {
                    GenerateImpliedEndTags();
                    PopUntilInclusive(_button);
                }
                _activeFormattingElements.Reconstruct(this);
                var el = InsertElement(tag);
                CopyAttributes(el);
                _framesetOk = false;
                return;
            }

            // Formatting elements (a, b, i, em, strong, etc.)
            if (ReferenceEquals(tag, _a))
            {
                // Close any existing <a> in the active formatting elements
                int existingA = _activeFormattingElements.FindLastWithTag(_a);
                if (existingA >= 0)
                {
                    var existing = _activeFormattingElements[existingA]!;
                    AdoptionAgency(_a);
                    _activeFormattingElements.Remove(existing);
                    _openElements.Remove(existing);
                }

                _activeFormattingElements.Reconstruct(this);
                var el = InsertElement(tag);
                CopyAttributes(el);
                _activeFormattingElements.Push(el);
                return;
            }

            if (ElementCategories.IsFormattingElement(tag) && !ReferenceEquals(tag, _a))
            {
                _activeFormattingElements.Reconstruct(this);
                var el = InsertElement(tag);
                CopyAttributes(el);
                _activeFormattingElements.Push(el);
                return;
            }

            if (ReferenceEquals(tag, _nobr))
            {
                _activeFormattingElements.Reconstruct(this);
                if (HasInScope(_nobr))
                    AdoptionAgency(_nobr);
                _activeFormattingElements.Reconstruct(this);
                var el = InsertElement(tag);
                CopyAttributes(el);
                _activeFormattingElements.Push(el);
                return;
            }

            // Table
            if (ReferenceEquals(tag, _table))
            {
                if (HasInButtonScope(_p))
                    ClosePElement();
                var el = InsertElement(tag);
                CopyAttributes(el);
                _framesetOk = false;
                _insertionMode = InsertionMode.InTable;
                return;
            }

            // Void elements
            if (ReferenceEquals(tag, _area) || ReferenceEquals(tag, _br) ||
                ReferenceEquals(tag, _embed) || ReferenceEquals(tag, _img) ||
                ReferenceEquals(tag, _input) || ReferenceEquals(tag, _param) ||
                ReferenceEquals(tag, _source) || ReferenceEquals(tag, _track) ||
                ReferenceEquals(tag, _wbr))
            {
                _activeFormattingElements.Reconstruct(this);
                var el = InsertElement(tag);
                CopyAttributes(el);
                PopOpenElement();
                if (!ReferenceEquals(tag, _input))
                    _framesetOk = false;
                return;
            }

            if (ReferenceEquals(tag, _hr))
            {
                if (HasInButtonScope(_p))
                    ClosePElement();
                var el = InsertElement(tag);
                CopyAttributes(el);
                PopOpenElement();
                _framesetOk = false;
                return;
            }

            if (ReferenceEquals(tag, _image))
            {
                // Parse error: change tag name to img and reprocess
                token.TagName = _img;
                HandleInBodyStartTag(ref token);
                return;
            }

            if (ReferenceEquals(tag, _textarea))
            {
                var el = InsertElement(tag);
                CopyAttributes(el);
                _skipNextNewline = true;
                _tokenizer.State = TokenizerState.RcData;
                _originalInsertionMode = _insertionMode;
                _framesetOk = false;
                _insertionMode = InsertionMode.Text;
                return;
            }

            if (ReferenceEquals(tag, _xmp))
            {
                if (HasInButtonScope(_p))
                    ClosePElement();
                _activeFormattingElements.Reconstruct(this);
                _framesetOk = false;
                var el = InsertElement(tag);
                CopyAttributes(el);
                _tokenizer.State = TokenizerState.RawText;
                _originalInsertionMode = _insertionMode;
                _insertionMode = InsertionMode.Text;
                return;
            }

            if (ReferenceEquals(tag, _iframe))
            {
                _framesetOk = false;
                var el = InsertElement(tag);
                CopyAttributes(el);
                _tokenizer.State = TokenizerState.RawText;
                _originalInsertionMode = _insertionMode;
                _insertionMode = InsertionMode.Text;
                return;
            }

            if (ReferenceEquals(tag, _noembed) || ReferenceEquals(tag, _noframes))
            {
                var el = InsertElement(tag);
                CopyAttributes(el);
                _tokenizer.State = TokenizerState.RawText;
                _originalInsertionMode = _insertionMode;
                _insertionMode = InsertionMode.Text;
                return;
            }

            if (ReferenceEquals(tag, _select))
            {
                _activeFormattingElements.Reconstruct(this);
                var el = InsertElement(tag);
                CopyAttributes(el);
                _framesetOk = false;

                if (_insertionMode == InsertionMode.InTable ||
                    _insertionMode == InsertionMode.InCaption ||
                    _insertionMode == InsertionMode.InTableBody ||
                    _insertionMode == InsertionMode.InRow ||
                    _insertionMode == InsertionMode.InCell)
                    _insertionMode = InsertionMode.InSelectInTable;
                else
                    _insertionMode = InsertionMode.InSelect;
                return;
            }

            if (ReferenceEquals(tag, _optgroup) || ReferenceEquals(tag, _option))
            {
                if (ReferenceEquals(CurrentNode.TagName, _option))
                    PopOpenElement();
                _activeFormattingElements.Reconstruct(this);
                var el = InsertElement(tag);
                CopyAttributes(el);
                return;
            }

            if (ReferenceEquals(tag, _marquee) || ReferenceEquals(tag, _object) ||
                ReferenceEquals(tag, _applet))
            {
                _activeFormattingElements.Reconstruct(this);
                var el = InsertElement(tag);
                CopyAttributes(el);
                _activeFormattingElements.InsertMarker();
                _framesetOk = false;
                return;
            }

            // Script, style, etc. in body
            if (ReferenceEquals(tag, _script))
            {
                HandleInHead(ref token);
                return;
            }

            if (ReferenceEquals(tag, _style))
            {
                HandleInHead(ref token);
                return;
            }

            // Head-only elements that appear in body
            if (ReferenceEquals(tag, _base) || ReferenceEquals(tag, _link) ||
                ReferenceEquals(tag, _meta) || ReferenceEquals(tag, _title))
            {
                HandleInHead(ref token);
                return;
            }

            // Any other start tag: reconstruct formatting, insert element
            _activeFormattingElements.Reconstruct(this);
            var genericEl = InsertElement(tag);
            CopyAttributes(genericEl);
        }

        private void HandleInBodyEndTag(ref HtmlToken token)
        {
            var tag = token.TagName!;

            if (ReferenceEquals(tag, _body))
            {
                if (!HasInScope(_body))
                    return;
                _insertionMode = InsertionMode.AfterBody;
                return;
            }

            if (ReferenceEquals(tag, _html))
            {
                if (!HasInScope(_body))
                    return;
                _insertionMode = InsertionMode.AfterBody;
                ProcessToken(ref token);
                return;
            }

            // Block-level closing tags
            if (ReferenceEquals(tag, _address) || ReferenceEquals(tag, _article) ||
                ReferenceEquals(tag, _aside) || ReferenceEquals(tag, _blockquote) ||
                ReferenceEquals(tag, _center) || ReferenceEquals(tag, _details) ||
                ReferenceEquals(tag, _dialog) || ReferenceEquals(tag, _dir) ||
                ReferenceEquals(tag, _div) || ReferenceEquals(tag, _dl) ||
                ReferenceEquals(tag, _fieldset) || ReferenceEquals(tag, _figcaption) ||
                ReferenceEquals(tag, _figure) || ReferenceEquals(tag, _footer) ||
                ReferenceEquals(tag, _header) || ReferenceEquals(tag, _hgroup) ||
                ReferenceEquals(tag, _listing) || ReferenceEquals(tag, _main) ||
                ReferenceEquals(tag, _menu) || ReferenceEquals(tag, _nav) ||
                ReferenceEquals(tag, _ol) || ReferenceEquals(tag, _pre) ||
                ReferenceEquals(tag, _section) || ReferenceEquals(tag, _summary) ||
                ReferenceEquals(tag, _ul))
            {
                if (!HasInScope(tag))
                    return;
                GenerateImpliedEndTags();
                PopUntilInclusive(tag);
                return;
            }

            if (ReferenceEquals(tag, _form))
            {
                var node = _formElement;
                _formElement = null;
                if (node == null || !HasInScope(node.TagName))
                    return;
                GenerateImpliedEndTags();
                _openElements.Remove(node);
                return;
            }

            if (ReferenceEquals(tag, _p))
            {
                if (!HasInButtonScope(_p))
                {
                    // Parse error: insert <p>, then close it
                    InsertElement(_p);
                }
                ClosePElement();
                return;
            }

            if (ReferenceEquals(tag, _li))
            {
                if (!HasInListItemScope(_li))
                    return;
                GenerateImpliedEndTags(_li);
                PopUntilInclusive(_li);
                return;
            }

            if (ReferenceEquals(tag, _dd) || ReferenceEquals(tag, _dt))
            {
                if (!HasInScope(tag))
                    return;
                GenerateImpliedEndTags(tag);
                PopUntilInclusive(tag);
                return;
            }

            if (ElementCategories.IsHeadingElement(tag))
            {
                if (!HasHeadingInScope())
                    return;
                GenerateImpliedEndTags();
                // Pop until a heading element
                while (_openElements.Count > 0)
                {
                    var popped = PopOpenElement();
                    if (ElementCategories.IsHeadingElement(popped.TagName))
                        break;
                }
                return;
            }

            if (ReferenceEquals(tag, _applet) || ReferenceEquals(tag, _marquee) ||
                ReferenceEquals(tag, _object))
            {
                if (!HasInScope(tag))
                    return;
                GenerateImpliedEndTags();
                PopUntilInclusive(tag);
                _activeFormattingElements.ClearToLastMarker();
                return;
            }

            if (ReferenceEquals(tag, _br))
            {
                // Parse error: treat as <br> start tag
                _activeFormattingElements.Reconstruct(this);
                InsertElement(_br);
                PopOpenElement();
                _framesetOk = false;
                return;
            }

            // Formatting elements — adoption agency
            if (ElementCategories.IsFormattingElement(tag))
            {
                AdoptionAgency(tag);
                return;
            }

            // Any other end tag
            AnyOtherEndTag(tag);
        }

        private void AnyOtherEndTag(string tag)
        {
            for (int i = _openElements.Count - 1; i >= 0; i--)
            {
                var node = _openElements[i];
                if (ReferenceEquals(node.TagName, tag))
                {
                    GenerateImpliedEndTags(tag);
                    // Pop up to and including node
                    while (_openElements.Count > i)
                        PopOpenElement();
                    return;
                }
                if (ElementCategories.IsSpecialElement(node.TagName))
                    return; // Ignore
            }
        }

        // ──────────────────────────────────────────────────────
        //  Insertion Mode: Text
        // ──────────────────────────────────────────────────────
        private void HandleText(ref HtmlToken token)
        {
            if (token.Type == HtmlTokenType.Character)
            {
                // Skip leading newline after <textarea> per WHATWG spec
                if (_skipNextNewline)
                {
                    _skipNextNewline = false;
                    if (token.Character == '\n')
                        return;
                }

                InsertCharacter(token.Character);
                return;
            }

            if (token.Type == HtmlTokenType.EndOfFile)
            {
                PopOpenElement();
                _insertionMode = _originalInsertionMode;
                ProcessToken(ref token);
                return;
            }

            if (token.Type == HtmlTokenType.EndTag)
            {
                PopOpenElement();
                _insertionMode = _originalInsertionMode;
                return;
            }
        }

        // ──────────────────────────────────────────────────────
        //  Insertion Mode: In Table
        // ──────────────────────────────────────────────────────
        private void HandleInTable(ref HtmlToken token)
        {
            if (token.Type == HtmlTokenType.Character)
            {
                // Table text handling — collect characters
                _pendingTableText.Length = 0;
                _pendingTableTextHasNonWhitespace = !IsWhitespace(token.Character);
                _pendingTableText.Append(token.Character);
                _originalInsertionMode = _insertionMode;
                _insertionMode = InsertionMode.InTableText;
                return;
            }

            if (token.Type == HtmlTokenType.Comment)
            {
                InsertComment(token.Data ?? string.Empty);
                return;
            }

            if (token.Type == HtmlTokenType.Doctype)
                return;

            if (token.Type == HtmlTokenType.StartTag)
            {
                var tag = token.TagName!;

                if (ReferenceEquals(tag, _caption))
                {
                    ClearStackToTableContext();
                    _activeFormattingElements.InsertMarker();
                    var el = InsertElement(tag);
                    CopyAttributes(el);
                    _insertionMode = InsertionMode.InCaption;
                    return;
                }

                if (ReferenceEquals(tag, _colgroup))
                {
                    ClearStackToTableContext();
                    var el = InsertElement(tag);
                    CopyAttributes(el);
                    _insertionMode = InsertionMode.InColumnGroup;
                    return;
                }

                if (ReferenceEquals(tag, _col))
                {
                    ClearStackToTableContext();
                    InsertElement(_colgroup);
                    _insertionMode = InsertionMode.InColumnGroup;
                    ProcessToken(ref token);
                    return;
                }

                if (ReferenceEquals(tag, _tbody) || ReferenceEquals(tag, _thead) ||
                    ReferenceEquals(tag, _tfoot))
                {
                    ClearStackToTableContext();
                    var el = InsertElement(tag);
                    CopyAttributes(el);
                    _insertionMode = InsertionMode.InTableBody;
                    return;
                }

                if (ReferenceEquals(tag, _td) || ReferenceEquals(tag, _th) ||
                    ReferenceEquals(tag, _tr))
                {
                    ClearStackToTableContext();
                    InsertElement(_tbody);
                    _insertionMode = InsertionMode.InTableBody;
                    ProcessToken(ref token);
                    return;
                }

                if (ReferenceEquals(tag, _table))
                {
                    // Close current table, reprocess
                    if (!HasInTableScope(_table))
                        return;
                    PopUntilInclusive(_table);
                    ResetInsertionMode();
                    ProcessToken(ref token);
                    return;
                }

                if (ReferenceEquals(tag, _style) || ReferenceEquals(tag, _script) ||
                    ReferenceEquals(tag, _template))
                {
                    HandleInHead(ref token);
                    return;
                }

                if (ReferenceEquals(tag, _form))
                {
                    if (_formElement != null)
                        return;
                    var el = InsertElement(tag);
                    CopyAttributes(el);
                    _formElement = el;
                    PopOpenElement();
                    return;
                }

                if (ReferenceEquals(tag, _input))
                {
                    // Only hidden inputs handled specially
                    var el = InsertElement(tag);
                    CopyAttributes(el);
                    PopOpenElement();
                    return;
                }
            }

            if (token.Type == HtmlTokenType.EndTag)
            {
                if (ReferenceEquals(token.TagName, _table))
                {
                    if (!HasInTableScope(_table))
                        return;
                    PopUntilInclusive(_table);
                    ResetInsertionMode();
                    return;
                }

                if (ReferenceEquals(token.TagName, _body) || ReferenceEquals(token.TagName, _caption) ||
                    ReferenceEquals(token.TagName, _col) || ReferenceEquals(token.TagName, _colgroup) ||
                    ReferenceEquals(token.TagName, _html) || ReferenceEquals(token.TagName, _tbody) ||
                    ReferenceEquals(token.TagName, _td) || ReferenceEquals(token.TagName, _tfoot) ||
                    ReferenceEquals(token.TagName, _th) || ReferenceEquals(token.TagName, _thead) ||
                    ReferenceEquals(token.TagName, _tr))
                    return; // Ignore

                if (ReferenceEquals(token.TagName, _template))
                {
                    HandleInHead(ref token);
                    return;
                }
            }

            if (token.Type == HtmlTokenType.EndOfFile)
            {
                HandleInBody(ref token);
                return;
            }

            // Foster parenting: insert into body instead of table
            _fosterParenting = true;
            HandleInBody(ref token);
            _fosterParenting = false;
        }

        // ──────────────────────────────────────────────────────
        //  Insertion Mode: In Table Text
        // ──────────────────────────────────────────────────────
        private void HandleInTableText(ref HtmlToken token)
        {
            if (token.Type == HtmlTokenType.Character)
            {
                if (token.Character == '\0')
                    return;
                if (!IsWhitespace(token.Character))
                    _pendingTableTextHasNonWhitespace = true;
                _pendingTableText.Append(token.Character);
                return;
            }

            // Flush pending table text
            if (_pendingTableTextHasNonWhitespace)
            {
                // Foster parent the text
                _fosterParenting = true;
                for (int i = 0; i < _pendingTableText.Length; i++)
                    InsertCharacter(_pendingTableText[i]);
                _fosterParenting = false;
            }
            else
            {
                for (int i = 0; i < _pendingTableText.Length; i++)
                    InsertCharacter(_pendingTableText[i]);
            }

            _insertionMode = _originalInsertionMode;
            ProcessToken(ref token);
        }

        // ──────────────────────────────────────────────────────
        //  Insertion Mode: In Table Body
        // ──────────────────────────────────────────────────────
        private void HandleInTableBody(ref HtmlToken token)
        {
            if (token.Type == HtmlTokenType.StartTag)
            {
                if (ReferenceEquals(token.TagName, _tr))
                {
                    ClearStackToTableBodyContext();
                    var el = InsertElement(_tr);
                    CopyAttributes(el);
                    _insertionMode = InsertionMode.InRow;
                    return;
                }

                if (ReferenceEquals(token.TagName, _td) || ReferenceEquals(token.TagName, _th))
                {
                    ClearStackToTableBodyContext();
                    InsertElement(_tr);
                    _insertionMode = InsertionMode.InRow;
                    ProcessToken(ref token);
                    return;
                }

                if (ReferenceEquals(token.TagName, _caption) || ReferenceEquals(token.TagName, _col) ||
                    ReferenceEquals(token.TagName, _colgroup) || ReferenceEquals(token.TagName, _tbody) ||
                    ReferenceEquals(token.TagName, _thead) || ReferenceEquals(token.TagName, _tfoot))
                {
                    if (!HasInTableScope(_tbody) && !HasInTableScope(_thead) && !HasInTableScope(_tfoot))
                        return;
                    ClearStackToTableBodyContext();
                    PopOpenElement();
                    _insertionMode = InsertionMode.InTable;
                    ProcessToken(ref token);
                    return;
                }
            }

            if (token.Type == HtmlTokenType.EndTag)
            {
                if (ReferenceEquals(token.TagName, _tbody) || ReferenceEquals(token.TagName, _thead) ||
                    ReferenceEquals(token.TagName, _tfoot))
                {
                    if (!HasInTableScope(token.TagName!))
                        return;
                    ClearStackToTableBodyContext();
                    PopOpenElement();
                    _insertionMode = InsertionMode.InTable;
                    return;
                }

                if (ReferenceEquals(token.TagName, _table))
                {
                    if (!HasInTableScope(_tbody) && !HasInTableScope(_thead) && !HasInTableScope(_tfoot))
                        return;
                    ClearStackToTableBodyContext();
                    PopOpenElement();
                    _insertionMode = InsertionMode.InTable;
                    ProcessToken(ref token);
                    return;
                }

                if (ReferenceEquals(token.TagName, _body) || ReferenceEquals(token.TagName, _caption) ||
                    ReferenceEquals(token.TagName, _col) || ReferenceEquals(token.TagName, _colgroup) ||
                    ReferenceEquals(token.TagName, _html) || ReferenceEquals(token.TagName, _td) ||
                    ReferenceEquals(token.TagName, _th) || ReferenceEquals(token.TagName, _tr))
                    return; // Ignore
            }

            HandleInTable(ref token);
        }

        // ──────────────────────────────────────────────────────
        //  Insertion Mode: In Row
        // ──────────────────────────────────────────────────────
        private void HandleInRow(ref HtmlToken token)
        {
            if (token.Type == HtmlTokenType.StartTag)
            {
                if (ReferenceEquals(token.TagName, _td) || ReferenceEquals(token.TagName, _th))
                {
                    ClearStackToTableRowContext();
                    var el = InsertElement(token.TagName!);
                    CopyAttributes(el);
                    _insertionMode = InsertionMode.InCell;
                    _activeFormattingElements.InsertMarker();
                    return;
                }

                if (ReferenceEquals(token.TagName, _caption) || ReferenceEquals(token.TagName, _col) ||
                    ReferenceEquals(token.TagName, _colgroup) || ReferenceEquals(token.TagName, _tbody) ||
                    ReferenceEquals(token.TagName, _thead) || ReferenceEquals(token.TagName, _tfoot) ||
                    ReferenceEquals(token.TagName, _tr))
                {
                    if (!HasInTableScope(_tr))
                        return;
                    ClearStackToTableRowContext();
                    PopOpenElement(); // Pop tr
                    _insertionMode = InsertionMode.InTableBody;
                    ProcessToken(ref token);
                    return;
                }
            }

            if (token.Type == HtmlTokenType.EndTag)
            {
                if (ReferenceEquals(token.TagName, _tr))
                {
                    if (!HasInTableScope(_tr))
                        return;
                    ClearStackToTableRowContext();
                    PopOpenElement();
                    _insertionMode = InsertionMode.InTableBody;
                    return;
                }

                if (ReferenceEquals(token.TagName, _table))
                {
                    if (!HasInTableScope(_tr))
                        return;
                    ClearStackToTableRowContext();
                    PopOpenElement();
                    _insertionMode = InsertionMode.InTableBody;
                    ProcessToken(ref token);
                    return;
                }

                if (ReferenceEquals(token.TagName, _tbody) || ReferenceEquals(token.TagName, _thead) ||
                    ReferenceEquals(token.TagName, _tfoot))
                {
                    if (!HasInTableScope(token.TagName!))
                        return;
                    if (!HasInTableScope(_tr))
                        return;
                    ClearStackToTableRowContext();
                    PopOpenElement();
                    _insertionMode = InsertionMode.InTableBody;
                    ProcessToken(ref token);
                    return;
                }

                if (ReferenceEquals(token.TagName, _body) || ReferenceEquals(token.TagName, _caption) ||
                    ReferenceEquals(token.TagName, _col) || ReferenceEquals(token.TagName, _colgroup) ||
                    ReferenceEquals(token.TagName, _html) || ReferenceEquals(token.TagName, _td) ||
                    ReferenceEquals(token.TagName, _th))
                    return; // Ignore
            }

            HandleInTable(ref token);
        }

        // ──────────────────────────────────────────────────────
        //  Insertion Mode: In Cell
        // ──────────────────────────────────────────────────────
        private void HandleInCell(ref HtmlToken token)
        {
            if (token.Type == HtmlTokenType.EndTag)
            {
                if (ReferenceEquals(token.TagName, _td) || ReferenceEquals(token.TagName, _th))
                {
                    if (!HasInTableScope(token.TagName!))
                        return;
                    GenerateImpliedEndTags();
                    PopUntilInclusive(token.TagName!);
                    _activeFormattingElements.ClearToLastMarker();
                    _insertionMode = InsertionMode.InRow;
                    return;
                }

                if (ReferenceEquals(token.TagName, _body) || ReferenceEquals(token.TagName, _caption) ||
                    ReferenceEquals(token.TagName, _col) || ReferenceEquals(token.TagName, _colgroup) ||
                    ReferenceEquals(token.TagName, _html))
                    return; // Ignore

                if (ReferenceEquals(token.TagName, _table) || ReferenceEquals(token.TagName, _tbody) ||
                    ReferenceEquals(token.TagName, _thead) || ReferenceEquals(token.TagName, _tfoot) ||
                    ReferenceEquals(token.TagName, _tr))
                {
                    if (!HasInTableScope(token.TagName!))
                        return;
                    CloseCell();
                    ProcessToken(ref token);
                    return;
                }
            }

            if (token.Type == HtmlTokenType.StartTag)
            {
                if (ReferenceEquals(token.TagName, _caption) || ReferenceEquals(token.TagName, _col) ||
                    ReferenceEquals(token.TagName, _colgroup) || ReferenceEquals(token.TagName, _tbody) ||
                    ReferenceEquals(token.TagName, _thead) || ReferenceEquals(token.TagName, _tfoot) ||
                    ReferenceEquals(token.TagName, _td) || ReferenceEquals(token.TagName, _th) ||
                    ReferenceEquals(token.TagName, _tr))
                {
                    if (!HasInTableScope(_td) && !HasInTableScope(_th))
                        return;
                    CloseCell();
                    ProcessToken(ref token);
                    return;
                }
            }

            HandleInBody(ref token);
        }

        // ──────────────────────────────────────────────────────
        //  Insertion Mode: In Caption
        // ──────────────────────────────────────────────────────
        private void HandleInCaption(ref HtmlToken token)
        {
            if (token.Type == HtmlTokenType.EndTag && ReferenceEquals(token.TagName, _caption))
            {
                if (!HasInTableScope(_caption))
                    return;
                GenerateImpliedEndTags();
                PopUntilInclusive(_caption);
                _activeFormattingElements.ClearToLastMarker();
                _insertionMode = InsertionMode.InTable;
                return;
            }

            if (token.Type == HtmlTokenType.StartTag &&
                (ReferenceEquals(token.TagName, _caption) || ReferenceEquals(token.TagName, _col) ||
                 ReferenceEquals(token.TagName, _colgroup) || ReferenceEquals(token.TagName, _tbody) ||
                 ReferenceEquals(token.TagName, _thead) || ReferenceEquals(token.TagName, _tfoot) ||
                 ReferenceEquals(token.TagName, _td) || ReferenceEquals(token.TagName, _th) ||
                 ReferenceEquals(token.TagName, _tr)))
            {
                if (!HasInTableScope(_caption))
                    return;
                GenerateImpliedEndTags();
                PopUntilInclusive(_caption);
                _activeFormattingElements.ClearToLastMarker();
                _insertionMode = InsertionMode.InTable;
                ProcessToken(ref token);
                return;
            }

            if (token.Type == HtmlTokenType.EndTag && ReferenceEquals(token.TagName, _table))
            {
                if (!HasInTableScope(_caption))
                    return;
                GenerateImpliedEndTags();
                PopUntilInclusive(_caption);
                _activeFormattingElements.ClearToLastMarker();
                _insertionMode = InsertionMode.InTable;
                ProcessToken(ref token);
                return;
            }

            if (token.Type == HtmlTokenType.EndTag &&
                (ReferenceEquals(token.TagName, _body) || ReferenceEquals(token.TagName, _col) ||
                 ReferenceEquals(token.TagName, _colgroup) || ReferenceEquals(token.TagName, _html) ||
                 ReferenceEquals(token.TagName, _tbody) || ReferenceEquals(token.TagName, _td) ||
                 ReferenceEquals(token.TagName, _tfoot) || ReferenceEquals(token.TagName, _th) ||
                 ReferenceEquals(token.TagName, _thead) || ReferenceEquals(token.TagName, _tr)))
                return; // Ignore

            HandleInBody(ref token);
        }

        // ──────────────────────────────────────────────────────
        //  Insertion Mode: In Column Group
        // ──────────────────────────────────────────────────────
        private void HandleInColumnGroup(ref HtmlToken token)
        {
            if (token.Type == HtmlTokenType.Character && IsWhitespace(token.Character))
            {
                InsertCharacter(token.Character);
                return;
            }

            if (token.Type == HtmlTokenType.Comment)
            {
                InsertComment(token.Data ?? string.Empty);
                return;
            }

            if (token.Type == HtmlTokenType.Doctype)
                return;

            if (token.Type == HtmlTokenType.StartTag && ReferenceEquals(token.TagName, _col))
            {
                var el = InsertElement(_col);
                CopyAttributes(el);
                PopOpenElement();
                return;
            }

            if (token.Type == HtmlTokenType.EndTag && ReferenceEquals(token.TagName, _colgroup))
            {
                if (!ReferenceEquals(CurrentNode.TagName, _colgroup))
                    return;
                PopOpenElement();
                _insertionMode = InsertionMode.InTable;
                return;
            }

            if (token.Type == HtmlTokenType.EndTag && ReferenceEquals(token.TagName, _col))
                return; // Ignore

            if (token.Type == HtmlTokenType.EndOfFile)
            {
                HandleInBody(ref token);
                return;
            }

            // Anything else: pop colgroup, reprocess
            if (!ReferenceEquals(CurrentNode.TagName, _colgroup))
                return;
            PopOpenElement();
            _insertionMode = InsertionMode.InTable;
            ProcessToken(ref token);
        }

        // ──────────────────────────────────────────────────────
        //  Insertion Mode: In Select
        // ──────────────────────────────────────────────────────
        private void HandleInSelect(ref HtmlToken token)
        {
            if (token.Type == HtmlTokenType.Character)
            {
                if (token.Character == '\0')
                    return;
                InsertCharacter(token.Character);
                return;
            }

            if (token.Type == HtmlTokenType.Comment)
            {
                InsertComment(token.Data ?? string.Empty);
                return;
            }

            if (token.Type == HtmlTokenType.Doctype)
                return;

            if (token.Type == HtmlTokenType.StartTag)
            {
                if (ReferenceEquals(token.TagName, _option))
                {
                    if (ReferenceEquals(CurrentNode.TagName, _option))
                        PopOpenElement();
                    var el = InsertElement(_option);
                    CopyAttributes(el);
                    return;
                }

                if (ReferenceEquals(token.TagName, _optgroup))
                {
                    if (ReferenceEquals(CurrentNode.TagName, _option))
                        PopOpenElement();
                    if (ReferenceEquals(CurrentNode.TagName, _optgroup))
                        PopOpenElement();
                    var el = InsertElement(_optgroup);
                    CopyAttributes(el);
                    return;
                }

                if (ReferenceEquals(token.TagName, _select))
                {
                    if (!HasInSelectScope(_select))
                        return;
                    PopUntilInclusive(_select);
                    ResetInsertionMode();
                    return;
                }

                if (ReferenceEquals(token.TagName, _input) || ReferenceEquals(token.TagName, _textarea))
                {
                    if (!HasInSelectScope(_select))
                        return;
                    PopUntilInclusive(_select);
                    ResetInsertionMode();
                    ProcessToken(ref token);
                    return;
                }

                if (ReferenceEquals(token.TagName, _script) || ReferenceEquals(token.TagName, _template))
                {
                    HandleInHead(ref token);
                    return;
                }
            }

            if (token.Type == HtmlTokenType.EndTag)
            {
                if (ReferenceEquals(token.TagName, _optgroup))
                {
                    if (ReferenceEquals(CurrentNode.TagName, _option) &&
                        _openElements.Count >= 2 &&
                        ReferenceEquals(_openElements[_openElements.Count - 2].TagName, _optgroup))
                        PopOpenElement();
                    if (ReferenceEquals(CurrentNode.TagName, _optgroup))
                        PopOpenElement();
                    return;
                }

                if (ReferenceEquals(token.TagName, _option))
                {
                    if (ReferenceEquals(CurrentNode.TagName, _option))
                        PopOpenElement();
                    return;
                }

                if (ReferenceEquals(token.TagName, _select))
                {
                    if (!HasInSelectScope(_select))
                        return;
                    PopUntilInclusive(_select);
                    ResetInsertionMode();
                    return;
                }

                if (ReferenceEquals(token.TagName, _template))
                {
                    HandleInHead(ref token);
                    return;
                }
            }

            if (token.Type == HtmlTokenType.EndOfFile)
            {
                HandleInBody(ref token);
                return;
            }
        }

        // ──────────────────────────────────────────────────────
        //  Insertion Mode: In Select In Table
        // ──────────────────────────────────────────────────────
        private void HandleInSelectInTable(ref HtmlToken token)
        {
            if (token.Type == HtmlTokenType.StartTag &&
                (ReferenceEquals(token.TagName, _caption) || ReferenceEquals(token.TagName, _table) ||
                 ReferenceEquals(token.TagName, _tbody) || ReferenceEquals(token.TagName, _thead) ||
                 ReferenceEquals(token.TagName, _tfoot) || ReferenceEquals(token.TagName, _tr) ||
                 ReferenceEquals(token.TagName, _td) || ReferenceEquals(token.TagName, _th)))
            {
                PopUntilInclusive(_select);
                ResetInsertionMode();
                ProcessToken(ref token);
                return;
            }

            if (token.Type == HtmlTokenType.EndTag &&
                (ReferenceEquals(token.TagName, _caption) || ReferenceEquals(token.TagName, _table) ||
                 ReferenceEquals(token.TagName, _tbody) || ReferenceEquals(token.TagName, _thead) ||
                 ReferenceEquals(token.TagName, _tfoot) || ReferenceEquals(token.TagName, _tr) ||
                 ReferenceEquals(token.TagName, _td) || ReferenceEquals(token.TagName, _th)))
            {
                if (!HasInTableScope(token.TagName!))
                    return;
                PopUntilInclusive(_select);
                ResetInsertionMode();
                ProcessToken(ref token);
                return;
            }

            HandleInSelect(ref token);
        }

        // ──────────────────────────────────────────────────────
        //  Insertion Mode: After Body
        // ──────────────────────────────────────────────────────
        private void HandleAfterBody(ref HtmlToken token)
        {
            if (token.Type == HtmlTokenType.Character && IsWhitespace(token.Character))
            {
                HandleInBody(ref token);
                return;
            }

            if (token.Type == HtmlTokenType.Comment)
            {
                // Append to html element
                if (_openElements.Count > 0)
                    _openElements[0].AppendChild(_document.CreateComment(token.Data ?? string.Empty));
                return;
            }

            if (token.Type == HtmlTokenType.Doctype)
                return;

            if (token.Type == HtmlTokenType.StartTag && ReferenceEquals(token.TagName, _html))
            {
                HandleInBody(ref token);
                return;
            }

            if (token.Type == HtmlTokenType.EndTag && ReferenceEquals(token.TagName, _html))
            {
                _insertionMode = InsertionMode.AfterAfterBody;
                return;
            }

            if (token.Type == HtmlTokenType.EndOfFile)
                return; // Stop

            // Anything else: parse error, reprocess in InBody
            _insertionMode = InsertionMode.InBody;
            ProcessToken(ref token);
        }

        // ──────────────────────────────────────────────────────
        //  Insertion Mode: After After Body
        // ──────────────────────────────────────────────────────
        private void HandleAfterAfterBody(ref HtmlToken token)
        {
            if (token.Type == HtmlTokenType.Comment)
            {
                _document.AppendChild(_document.CreateComment(token.Data ?? string.Empty));
                return;
            }

            if (token.Type == HtmlTokenType.Doctype)
                return;

            if (token.Type == HtmlTokenType.Character && IsWhitespace(token.Character))
            {
                HandleInBody(ref token);
                return;
            }

            if (token.Type == HtmlTokenType.StartTag && ReferenceEquals(token.TagName, _html))
            {
                HandleInBody(ref token);
                return;
            }

            if (token.Type == HtmlTokenType.EndOfFile)
                return;

            _insertionMode = InsertionMode.InBody;
            ProcessToken(ref token);
        }

        // ──────────────────────────────────────────────────────
        //  Adoption Agency Algorithm (simplified)
        // ──────────────────────────────────────────────────────
        private void AdoptionAgency(string tag)
        {
            // Simplified adoption agency: just find and close the formatting element
            {
                // Find the formatting element in the AFE list
                int afeIndex = _activeFormattingElements.FindLastWithTag(tag);
                if (afeIndex < 0)
                {
                    AnyOtherEndTag(tag);
                    return;
                }

                var formattingElement = _activeFormattingElements[afeIndex]!;

                // Check if it's in the open elements stack
                int stackIndex = -1;
                for (int i = _openElements.Count - 1; i >= 0; i--)
                {
                    if (ReferenceEquals(_openElements[i], formattingElement))
                    {
                        stackIndex = i;
                        break;
                    }
                }

                if (stackIndex < 0)
                {
                    // Not on stack — remove from AFE list
                    _activeFormattingElements.RemoveAt(afeIndex);
                    return;
                }

                if (!HasInScope(tag))
                    return;

                // Find the furthest block
                int furthestBlockIndex = -1;
                for (int i = stackIndex + 1; i < _openElements.Count; i++)
                {
                    if (ElementCategories.IsSpecialElement(_openElements[i].TagName))
                    {
                        furthestBlockIndex = i;
                        break;
                    }
                }

                if (furthestBlockIndex < 0)
                {
                    // No furthest block — pop until formatting element is popped
                    while (_openElements.Count > stackIndex)
                        PopOpenElement();
                    _activeFormattingElements.Remove(formattingElement);
                    return;
                }

                // Simplified: pop elements and recreate formatting
                var furthestBlock = _openElements[furthestBlockIndex];
                var commonAncestor = _openElements[stackIndex - 1];

                // Remove everything between formatting element and furthest block
                // Move furthest block's children under a new formatting element clone
                var newElement = CreateElement(tag);
                // Copy attributes from the original formatting element
                var attrs = formattingElement.Attributes;
                for (int i = 0; i < attrs.Count; i++)
                    newElement.SetAttribute(attrs[i].Name, attrs[i].Value);

                // Move all children of furthest block into new element
                while (furthestBlock.FirstChild != null)
                    newElement.AppendChild(furthestBlock.FirstChild);

                furthestBlock.AppendChild(newElement);

                _activeFormattingElements.Replace(formattingElement, newElement);

                // Remove elements between formatting element and furthest block from stack
                for (int i = furthestBlockIndex - 1; i >= stackIndex; i--)
                    _openElements.RemoveAt(i);

                // Insert new element after furthest block in the stack
                int newFbIndex = _openElements.IndexOf(furthestBlock);
                _openElements.Insert(newFbIndex + 1, newElement);

            }
        }

        // ──────────────────────────────────────────────────────
        //  Helper methods
        // ──────────────────────────────────────────────────────

        private Element CurrentNode => _openElements[_openElements.Count - 1];

        private Element CreateElement(string tagName)
        {
            return new Element(tagName, _document);
        }

        /// <summary>
        /// Create an element and insert it at the current insertion point.
        /// Used by ActiveFormattingElements.Reconstruct.
        /// </summary>
        internal Element CreateAndInsertElement(string tagName, Element templateElement)
        {
            var el = CreateElement(tagName);
            // Copy attributes from template
            var attrs = templateElement.Attributes;
            for (int i = 0; i < attrs.Count; i++)
                el.SetAttribute(attrs[i].Name, attrs[i].Value);
            InsertNode(el);
            _openElements.Add(el);
            return el;
        }

        private Element InsertElement(string tagName)
        {
            var el = CreateElement(tagName);
            InsertNode(el);
            _openElements.Add(el);
            return el;
        }

        private void CopyAttributes(Element element)
        {
            _tokenizer.AttributeBuffer.CopyTo(element);
        }

        private void MergeAttributes(Element target)
        {
            var buffer = _tokenizer.AttributeBuffer;
            for (int i = 0; i < buffer.Count; i++)
            {
                var attr = buffer.Items[i];
                if (!target.HasAttribute(attr.Name))
                    target.SetAttribute(attr.Name, attr.Value);
            }
        }

        private void InsertNode(Node node)
        {
            if (_fosterParenting)
            {
                FosterParent(node);
                return;
            }

            var target = _openElements.Count > 0 ? (Node)CurrentNode : (Node)_document;
            target.AppendChild(node);
        }

        private void InsertCharacter(char c)
        {
            Node target;
            if (_fosterParenting)
            {
                // Foster parent target
                target = GetFosterParentTarget();
            }
            else
            {
                target = _openElements.Count > 0 ? (Node)CurrentNode : (Node)_document;
            }

            // If the last child is a text node, append to it
            if (target.LastChild is TextNode existingText)
            {
                existingText.Data += c;
                return;
            }

            var textNode = _document.CreateTextNode(c.ToString());
            target.AppendChild(textNode);
        }

        private void InsertComment(string data)
        {
            var comment = _document.CreateComment(data);
            InsertNode(comment);
        }

        private void FosterParent(Node node)
        {
            var target = GetFosterParentTarget();
            target.AppendChild(node);
        }

        private Node GetFosterParentTarget()
        {
            // Find the last table element in the stack
            for (int i = _openElements.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(_openElements[i].TagName, _table))
                {
                    var parent = _openElements[i].Parent;
                    if (parent != null)
                        return parent;
                    // No parent: use element before table in stack
                    if (i > 0)
                        return _openElements[i - 1];
                }
            }
            return _openElements.Count > 0 ? (Node)_openElements[0] : (Node)_document;
        }

        private Element PopOpenElement()
        {
            var el = _openElements[_openElements.Count - 1];
            _openElements.RemoveAt(_openElements.Count - 1);
            return el;
        }

        private void PopUntil(string tagName)
        {
            while (_openElements.Count > 0)
            {
                if (ReferenceEquals(CurrentNode.TagName, tagName))
                    return;
                PopOpenElement();
            }
        }

        private void PopUntilInclusive(string tagName)
        {
            while (_openElements.Count > 0)
            {
                var popped = PopOpenElement();
                if (ReferenceEquals(popped.TagName, tagName))
                    return;
            }
        }

        private void ClosePElement()
        {
            GenerateImpliedEndTags(_p);
            PopUntilInclusive(_p);
        }

        private void CloseCell()
        {
            GenerateImpliedEndTags();
            // Pop until td or th
            while (_openElements.Count > 0)
            {
                var popped = PopOpenElement();
                if (ReferenceEquals(popped.TagName, _td) || ReferenceEquals(popped.TagName, _th))
                    break;
            }
            _activeFormattingElements.ClearToLastMarker();
            _insertionMode = InsertionMode.InRow;
        }

        private void GenerateImpliedEndTags(string? except = null)
        {
            while (_openElements.Count > 0)
            {
                var tag = CurrentNode.TagName;
                if (except != null && ReferenceEquals(tag, except))
                    break;
                if (!ElementCategories.HasImpliedEndTag(tag))
                    break;
                PopOpenElement();
            }
        }

        internal bool IsInOpenElements(Element element)
        {
            for (int i = 0; i < _openElements.Count; i++)
            {
                if (ReferenceEquals(_openElements[i], element))
                    return true;
            }
            return false;
        }

        // ──────────────────────────────────────────────────────
        //  Scope checks
        // ──────────────────────────────────────────────────────

        private bool HasInScope(string tag)
        {
            for (int i = _openElements.Count - 1; i >= 0; i--)
            {
                var el = _openElements[i];
                if (ReferenceEquals(el.TagName, tag)) return true;
                if (ElementCategories.IsScopeMarker(el.TagName)) return false;
            }
            return false;
        }

        private bool HasInButtonScope(string tag)
        {
            for (int i = _openElements.Count - 1; i >= 0; i--)
            {
                var el = _openElements[i];
                if (ReferenceEquals(el.TagName, tag)) return true;
                if (ElementCategories.IsButtonScopeMarker(el.TagName)) return false;
            }
            return false;
        }

        private bool HasInListItemScope(string tag)
        {
            for (int i = _openElements.Count - 1; i >= 0; i--)
            {
                var el = _openElements[i];
                if (ReferenceEquals(el.TagName, tag)) return true;
                if (ElementCategories.IsListItemScopeMarker(el.TagName)) return false;
            }
            return false;
        }

        private bool HasInTableScope(string tag)
        {
            for (int i = _openElements.Count - 1; i >= 0; i--)
            {
                var el = _openElements[i];
                if (ReferenceEquals(el.TagName, tag)) return true;
                if (ElementCategories.IsTableScopeMarker(el.TagName)) return false;
            }
            return false;
        }

        private bool HasInSelectScope(string tag)
        {
            for (int i = _openElements.Count - 1; i >= 0; i--)
            {
                var el = _openElements[i];
                if (ReferenceEquals(el.TagName, tag)) return true;
                if (ElementCategories.IsSelectScopeMarker(el.TagName)) return false;
            }
            return false;
        }

        private bool HasHeadingInScope()
        {
            for (int i = _openElements.Count - 1; i >= 0; i--)
            {
                var el = _openElements[i];
                if (ElementCategories.IsHeadingElement(el.TagName)) return true;
                if (ElementCategories.IsScopeMarker(el.TagName)) return false;
            }
            return false;
        }

        // ──────────────────────────────────────────────────────
        //  Stack clearing helpers
        // ──────────────────────────────────────────────────────

        private void ClearStackToTableContext()
        {
            while (_openElements.Count > 0)
            {
                var tag = CurrentNode.TagName;
                if (ReferenceEquals(tag, _table) || ReferenceEquals(tag, _template) ||
                    ReferenceEquals(tag, _html))
                    return;
                PopOpenElement();
            }
        }

        private void ClearStackToTableBodyContext()
        {
            while (_openElements.Count > 0)
            {
                var tag = CurrentNode.TagName;
                if (ReferenceEquals(tag, _tbody) || ReferenceEquals(tag, _thead) ||
                    ReferenceEquals(tag, _tfoot) || ReferenceEquals(tag, _template) ||
                    ReferenceEquals(tag, _html))
                    return;
                PopOpenElement();
            }
        }

        private void ClearStackToTableRowContext()
        {
            while (_openElements.Count > 0)
            {
                var tag = CurrentNode.TagName;
                if (ReferenceEquals(tag, _tr) || ReferenceEquals(tag, _template) ||
                    ReferenceEquals(tag, _html))
                    return;
                PopOpenElement();
            }
        }

        private void ResetInsertionMode()
        {
            for (int i = _openElements.Count - 1; i >= 0; i--)
            {
                var node = _openElements[i];
                bool last = (i == 0);
                var tag = node.TagName;

                if (ReferenceEquals(tag, _select))
                {
                    _insertionMode = InsertionMode.InSelect;
                    return;
                }
                if (ReferenceEquals(tag, _td) || ReferenceEquals(tag, _th))
                {
                    if (!last) { _insertionMode = InsertionMode.InCell; return; }
                }
                if (ReferenceEquals(tag, _tr))
                {
                    _insertionMode = InsertionMode.InRow;
                    return;
                }
                if (ReferenceEquals(tag, _tbody) || ReferenceEquals(tag, _thead) ||
                    ReferenceEquals(tag, _tfoot))
                {
                    _insertionMode = InsertionMode.InTableBody;
                    return;
                }
                if (ReferenceEquals(tag, _caption))
                {
                    _insertionMode = InsertionMode.InCaption;
                    return;
                }
                if (ReferenceEquals(tag, _colgroup))
                {
                    _insertionMode = InsertionMode.InColumnGroup;
                    return;
                }
                if (ReferenceEquals(tag, _table))
                {
                    _insertionMode = InsertionMode.InTable;
                    return;
                }
                if (ReferenceEquals(tag, _template))
                {
                    _insertionMode = InsertionMode.InBody; // Simplified
                    return;
                }
                if (ReferenceEquals(tag, _head))
                {
                    if (!last) { _insertionMode = InsertionMode.InHead; return; }
                }
                if (ReferenceEquals(tag, _body))
                {
                    _insertionMode = InsertionMode.InBody;
                    return;
                }
                if (ReferenceEquals(tag, _frameset))
                {
                    _insertionMode = InsertionMode.InFrameset;
                    return;
                }
                if (ReferenceEquals(tag, _html))
                {
                    if (_headElement == null)
                        _insertionMode = InsertionMode.BeforeHead;
                    else
                        _insertionMode = InsertionMode.AfterHead;
                    return;
                }
                if (last)
                {
                    _insertionMode = InsertionMode.InBody;
                    return;
                }
            }
        }

        private static bool IsWhitespace(char c)
        {
            return c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f';
        }
    }
}
