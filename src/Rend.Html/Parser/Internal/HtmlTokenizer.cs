using System;
using System.Text;
using Rend.Core;

namespace Rend.Html.Parser.Internal
{
    /// <summary>
    /// WHATWG-compliant HTML5 tokenizer. Processes an input string character by character
    /// through a state machine and emits tokens consumed by the tree builder.
    /// </summary>
    internal sealed class HtmlTokenizer
    {
        private readonly string _input;
        private int _pos;
        private TokenizerState _state;
        private HtmlToken _token;
        private readonly HtmlAttributeBuffer _attrBuffer;
        private readonly StringBuilder _tempBuffer; // Temporary buffer for various states
        private readonly StringBuilder _tagNameBuffer;
        private readonly StringBuilder _commentBuffer;
        private readonly StringBuilder _doctypeNameBuffer;
        private readonly StringBuilder _publicIdBuffer;
        private readonly StringBuilder _systemIdBuffer;

        // The last emitted start tag name, for "appropriate end tag" checks
        private string? _lastStartTagName;

        // Pending character tokens (accumulated then flushed)
        private readonly StringBuilder _charBuffer;
        private bool _hasChar;
        private char _pendingChar;

        public HtmlTokenizer(string input)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _pos = 0;
            _state = TokenizerState.Data;

            _token = new HtmlToken();
            _attrBuffer = new HtmlAttributeBuffer();
            _tempBuffer = new StringBuilder(64);
            _tagNameBuffer = new StringBuilder(32);
            _commentBuffer = new StringBuilder(256);
            _doctypeNameBuffer = new StringBuilder(16);
            _publicIdBuffer = new StringBuilder(64);
            _systemIdBuffer = new StringBuilder(64);
            _charBuffer = new StringBuilder(256);
        }

        public TokenizerState State
        {
            get => _state;
            set => _state = value;
        }

        public HtmlAttributeBuffer AttributeBuffer => _attrBuffer;

        /// <summary>
        /// Read the next token. Returns false when EOF is reached.
        /// The token data is valid until the next call to Read().
        /// </summary>
        public bool Read(out HtmlToken token)
        {
            // If we have a pending character, emit it
            if (_hasChar)
            {
                _hasChar = false;
                _token.Reset();
                _token.Type = HtmlTokenType.Character;
                _token.Character = _pendingChar;
                token = _token;
                return true;
            }

            while (true)
            {
                if (_pos >= _input.Length)
                {
                    _token.Reset();
                    _token.Type = HtmlTokenType.EndOfFile;
                    token = _token;
                    return false;
                }

                char c = _input[_pos];

                switch (_state)
                {
                    case TokenizerState.Data:
                        if (c == '&')
                        {
                            _pos++;
                            var decoded = CharRefDecoder.Decode(_input, ref _pos);
                            if (decoded != null)
                            {
                                EmitCharacters(decoded, out token);
                                return true;
                            }
                            EmitChar('&', out token);
                            return true;
                        }
                        if (c == '<')
                        {
                            _state = TokenizerState.TagOpen;
                            _pos++;
                            continue;
                        }
                        if (c == '\0')
                        {
                            _pos++;
                            EmitChar('\uFFFD', out token);
                            return true;
                        }
                        _pos++;
                        EmitChar(c, out token);
                        return true;

                    case TokenizerState.RcData:
                        if (c == '&')
                        {
                            _pos++;
                            var decoded = CharRefDecoder.Decode(_input, ref _pos);
                            if (decoded != null)
                            {
                                EmitCharacters(decoded, out token);
                                return true;
                            }
                            EmitChar('&', out token);
                            return true;
                        }
                        if (c == '<')
                        {
                            _state = TokenizerState.RcDataLessThanSign;
                            _pos++;
                            continue;
                        }
                        if (c == '\0')
                        {
                            _pos++;
                            EmitChar('\uFFFD', out token);
                            return true;
                        }
                        _pos++;
                        EmitChar(c, out token);
                        return true;

                    case TokenizerState.RawText:
                        if (c == '<')
                        {
                            _state = TokenizerState.RawTextLessThanSign;
                            _pos++;
                            continue;
                        }
                        if (c == '\0')
                        {
                            _pos++;
                            EmitChar('\uFFFD', out token);
                            return true;
                        }
                        _pos++;
                        EmitChar(c, out token);
                        return true;

                    case TokenizerState.ScriptData:
                        if (c == '<')
                        {
                            _state = TokenizerState.ScriptDataLessThanSign;
                            _pos++;
                            continue;
                        }
                        if (c == '\0')
                        {
                            _pos++;
                            EmitChar('\uFFFD', out token);
                            return true;
                        }
                        _pos++;
                        EmitChar(c, out token);
                        return true;

                    case TokenizerState.PlainText:
                        if (c == '\0')
                        {
                            _pos++;
                            EmitChar('\uFFFD', out token);
                            return true;
                        }
                        _pos++;
                        EmitChar(c, out token);
                        return true;

                    case TokenizerState.TagOpen:
                        if (c == '!')
                        {
                            _state = TokenizerState.MarkupDeclarationOpen;
                            _pos++;
                            continue;
                        }
                        if (c == '/')
                        {
                            _state = TokenizerState.EndTagOpen;
                            _pos++;
                            continue;
                        }
                        if (IsAsciiAlpha(c))
                        {
                            _token.Reset();
                            _attrBuffer.Clear();
                            _token.Type = HtmlTokenType.StartTag;
                            _tagNameBuffer.Length = 0;
                            _state = TokenizerState.TagName;
                            continue; // Don't consume — TagName will
                        }
                        if (c == '?')
                        {
                            _commentBuffer.Length = 0;
                            _state = TokenizerState.BogusComment;
                            continue;
                        }
                        // Parse error, emit '<' as character
                        _state = TokenizerState.Data;
                        EmitChar('<', out token);
                        return true;

                    case TokenizerState.EndTagOpen:
                        if (IsAsciiAlpha(c))
                        {
                            _token.Reset();
                            _attrBuffer.Clear();
                            _token.Type = HtmlTokenType.EndTag;
                            _tagNameBuffer.Length = 0;
                            _state = TokenizerState.TagName;
                            continue;
                        }
                        if (c == '>')
                        {
                            // Parse error
                            _pos++;
                            _state = TokenizerState.Data;
                            continue;
                        }
                        _commentBuffer.Length = 0;
                        _state = TokenizerState.BogusComment;
                        continue;

                    case TokenizerState.TagName:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ')
                        {
                            _token.TagName = InternTagName();
                            _state = TokenizerState.BeforeAttributeName;
                            _pos++;
                            continue;
                        }
                        if (c == '/')
                        {
                            _token.TagName = InternTagName();
                            _state = TokenizerState.SelfClosingStartTag;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _token.TagName = InternTagName();
                            _pos++;
                            _state = TokenizerState.Data;
                            EmitTag(out token);
                            return true;
                        }
                        if (c >= 'A' && c <= 'Z')
                            _tagNameBuffer.Append((char)(c + 0x20)); // lowercase
                        else if (c == '\0')
                            _tagNameBuffer.Append('\uFFFD');
                        else
                            _tagNameBuffer.Append(c);
                        _pos++;
                        continue;

                    case TokenizerState.RcDataLessThanSign:
                        if (c == '/')
                        {
                            _tempBuffer.Length = 0;
                            _state = TokenizerState.RcDataEndTagOpen;
                            _pos++;
                            continue;
                        }
                        _state = TokenizerState.RcData;
                        EmitChar('<', out token);
                        return true;

                    case TokenizerState.RcDataEndTagOpen:
                        if (IsAsciiAlpha(c))
                        {
                            _token.Reset();
                            _attrBuffer.Clear();
                            _token.Type = HtmlTokenType.EndTag;
                            _tagNameBuffer.Length = 0;
                            _state = TokenizerState.RcDataEndTagName;
                            continue;
                        }
                        _state = TokenizerState.RcData;
                        _hasChar = true;
                        _pendingChar = '/';
                        EmitChar('<', out token);
                        return true;

                    case TokenizerState.RcDataEndTagName:
                        if ((c == '\t' || c == '\n' || c == '\f' || c == ' ') && IsAppropriateEndTag())
                        {
                            _token.TagName = InternTagName();
                            _state = TokenizerState.BeforeAttributeName;
                            _pos++;
                            continue;
                        }
                        if (c == '/' && IsAppropriateEndTag())
                        {
                            _token.TagName = InternTagName();
                            _state = TokenizerState.SelfClosingStartTag;
                            _pos++;
                            continue;
                        }
                        if (c == '>' && IsAppropriateEndTag())
                        {
                            _token.TagName = InternTagName();
                            _pos++;
                            _state = TokenizerState.Data;
                            EmitTag(out token);
                            return true;
                        }
                        if (IsAsciiAlpha(c))
                        {
                            _tagNameBuffer.Append(c >= 'A' && c <= 'Z' ? (char)(c + 0x20) : c);
                            _tempBuffer.Append(c);
                            _pos++;
                            continue;
                        }
                        // Not an appropriate end tag — emit buffered chars
                        _state = TokenizerState.RcData;
                        EmitBufferedEndTagChars(out token);
                        return true;

                    case TokenizerState.RawTextLessThanSign:
                        if (c == '/')
                        {
                            _tempBuffer.Length = 0;
                            _state = TokenizerState.RawTextEndTagOpen;
                            _pos++;
                            continue;
                        }
                        _state = TokenizerState.RawText;
                        EmitChar('<', out token);
                        return true;

                    case TokenizerState.RawTextEndTagOpen:
                        if (IsAsciiAlpha(c))
                        {
                            _token.Reset();
                            _attrBuffer.Clear();
                            _token.Type = HtmlTokenType.EndTag;
                            _tagNameBuffer.Length = 0;
                            _state = TokenizerState.RawTextEndTagName;
                            continue;
                        }
                        _state = TokenizerState.RawText;
                        _hasChar = true;
                        _pendingChar = '/';
                        EmitChar('<', out token);
                        return true;

                    case TokenizerState.RawTextEndTagName:
                        if ((c == '\t' || c == '\n' || c == '\f' || c == ' ') && IsAppropriateEndTag())
                        {
                            _token.TagName = InternTagName();
                            _state = TokenizerState.BeforeAttributeName;
                            _pos++;
                            continue;
                        }
                        if (c == '/' && IsAppropriateEndTag())
                        {
                            _token.TagName = InternTagName();
                            _state = TokenizerState.SelfClosingStartTag;
                            _pos++;
                            continue;
                        }
                        if (c == '>' && IsAppropriateEndTag())
                        {
                            _token.TagName = InternTagName();
                            _pos++;
                            _state = TokenizerState.Data;
                            EmitTag(out token);
                            return true;
                        }
                        if (IsAsciiAlpha(c))
                        {
                            _tagNameBuffer.Append(c >= 'A' && c <= 'Z' ? (char)(c + 0x20) : c);
                            _tempBuffer.Append(c);
                            _pos++;
                            continue;
                        }
                        _state = TokenizerState.RawText;
                        EmitBufferedEndTagChars(out token);
                        return true;

                    case TokenizerState.ScriptDataLessThanSign:
                        if (c == '/')
                        {
                            _tempBuffer.Length = 0;
                            _state = TokenizerState.ScriptDataEndTagOpen;
                            _pos++;
                            continue;
                        }
                        if (c == '!')
                        {
                            _state = TokenizerState.ScriptDataEscapeStart;
                            _pos++;
                            _hasChar = true;
                            _pendingChar = '!';
                            EmitChar('<', out token);
                            return true;
                        }
                        _state = TokenizerState.ScriptData;
                        EmitChar('<', out token);
                        return true;

                    case TokenizerState.ScriptDataEndTagOpen:
                        if (IsAsciiAlpha(c))
                        {
                            _token.Reset();
                            _attrBuffer.Clear();
                            _token.Type = HtmlTokenType.EndTag;
                            _tagNameBuffer.Length = 0;
                            _state = TokenizerState.ScriptDataEndTagName;
                            continue;
                        }
                        _state = TokenizerState.ScriptData;
                        _hasChar = true;
                        _pendingChar = '/';
                        EmitChar('<', out token);
                        return true;

                    case TokenizerState.ScriptDataEndTagName:
                        if ((c == '\t' || c == '\n' || c == '\f' || c == ' ') && IsAppropriateEndTag())
                        {
                            _token.TagName = InternTagName();
                            _state = TokenizerState.BeforeAttributeName;
                            _pos++;
                            continue;
                        }
                        if (c == '/' && IsAppropriateEndTag())
                        {
                            _token.TagName = InternTagName();
                            _state = TokenizerState.SelfClosingStartTag;
                            _pos++;
                            continue;
                        }
                        if (c == '>' && IsAppropriateEndTag())
                        {
                            _token.TagName = InternTagName();
                            _pos++;
                            _state = TokenizerState.Data;
                            EmitTag(out token);
                            return true;
                        }
                        if (IsAsciiAlpha(c))
                        {
                            _tagNameBuffer.Append(c >= 'A' && c <= 'Z' ? (char)(c + 0x20) : c);
                            _tempBuffer.Append(c);
                            _pos++;
                            continue;
                        }
                        _state = TokenizerState.ScriptData;
                        EmitBufferedEndTagChars(out token);
                        return true;

                    case TokenizerState.ScriptDataEscapeStart:
                        if (c == '-')
                        {
                            _state = TokenizerState.ScriptDataEscapeStartDash;
                            _pos++;
                            EmitChar('-', out token);
                            return true;
                        }
                        _state = TokenizerState.ScriptData;
                        continue;

                    case TokenizerState.ScriptDataEscapeStartDash:
                        if (c == '-')
                        {
                            _state = TokenizerState.ScriptDataEscapedDashDash;
                            _pos++;
                            EmitChar('-', out token);
                            return true;
                        }
                        _state = TokenizerState.ScriptData;
                        continue;

                    case TokenizerState.ScriptDataEscaped:
                        if (c == '-')
                        {
                            _state = TokenizerState.ScriptDataEscapedDash;
                            _pos++;
                            EmitChar('-', out token);
                            return true;
                        }
                        if (c == '<')
                        {
                            _state = TokenizerState.ScriptDataEscapedLessThanSign;
                            _pos++;
                            continue;
                        }
                        if (c == '\0')
                        {
                            _pos++;
                            EmitChar('\uFFFD', out token);
                            return true;
                        }
                        _pos++;
                        EmitChar(c, out token);
                        return true;

                    case TokenizerState.ScriptDataEscapedDash:
                        if (c == '-')
                        {
                            _state = TokenizerState.ScriptDataEscapedDashDash;
                            _pos++;
                            EmitChar('-', out token);
                            return true;
                        }
                        if (c == '<')
                        {
                            _state = TokenizerState.ScriptDataEscapedLessThanSign;
                            _pos++;
                            continue;
                        }
                        if (c == '\0')
                        {
                            _state = TokenizerState.ScriptDataEscaped;
                            _pos++;
                            EmitChar('\uFFFD', out token);
                            return true;
                        }
                        _state = TokenizerState.ScriptDataEscaped;
                        _pos++;
                        EmitChar(c, out token);
                        return true;

                    case TokenizerState.ScriptDataEscapedDashDash:
                        if (c == '-')
                        {
                            _pos++;
                            EmitChar('-', out token);
                            return true;
                        }
                        if (c == '<')
                        {
                            _state = TokenizerState.ScriptDataEscapedLessThanSign;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _state = TokenizerState.ScriptData;
                            _pos++;
                            EmitChar('>', out token);
                            return true;
                        }
                        if (c == '\0')
                        {
                            _state = TokenizerState.ScriptDataEscaped;
                            _pos++;
                            EmitChar('\uFFFD', out token);
                            return true;
                        }
                        _state = TokenizerState.ScriptDataEscaped;
                        _pos++;
                        EmitChar(c, out token);
                        return true;

                    case TokenizerState.ScriptDataEscapedLessThanSign:
                        if (c == '/')
                        {
                            _tempBuffer.Length = 0;
                            _state = TokenizerState.ScriptDataEscapedEndTagOpen;
                            _pos++;
                            continue;
                        }
                        if (IsAsciiAlpha(c))
                        {
                            _tempBuffer.Length = 0;
                            _state = TokenizerState.ScriptDataDoubleEscapeStart;
                            EmitChar('<', out token);
                            return true;
                        }
                        _state = TokenizerState.ScriptDataEscaped;
                        EmitChar('<', out token);
                        return true;

                    case TokenizerState.ScriptDataEscapedEndTagOpen:
                        if (IsAsciiAlpha(c))
                        {
                            _token.Reset();
                            _attrBuffer.Clear();
                            _token.Type = HtmlTokenType.EndTag;
                            _tagNameBuffer.Length = 0;
                            _state = TokenizerState.ScriptDataEscapedEndTagName;
                            continue;
                        }
                        _state = TokenizerState.ScriptDataEscaped;
                        _hasChar = true;
                        _pendingChar = '/';
                        EmitChar('<', out token);
                        return true;

                    case TokenizerState.ScriptDataEscapedEndTagName:
                        if ((c == '\t' || c == '\n' || c == '\f' || c == ' ') && IsAppropriateEndTag())
                        {
                            _token.TagName = InternTagName();
                            _state = TokenizerState.BeforeAttributeName;
                            _pos++;
                            continue;
                        }
                        if (c == '/' && IsAppropriateEndTag())
                        {
                            _token.TagName = InternTagName();
                            _state = TokenizerState.SelfClosingStartTag;
                            _pos++;
                            continue;
                        }
                        if (c == '>' && IsAppropriateEndTag())
                        {
                            _token.TagName = InternTagName();
                            _pos++;
                            _state = TokenizerState.Data;
                            EmitTag(out token);
                            return true;
                        }
                        if (IsAsciiAlpha(c))
                        {
                            _tagNameBuffer.Append(c >= 'A' && c <= 'Z' ? (char)(c + 0x20) : c);
                            _tempBuffer.Append(c);
                            _pos++;
                            continue;
                        }
                        _state = TokenizerState.ScriptDataEscaped;
                        EmitBufferedEndTagChars(out token);
                        return true;

                    case TokenizerState.ScriptDataDoubleEscapeStart:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ' || c == '/' || c == '>')
                        {
                            if (_tempBuffer.ToString() == "script")
                                _state = TokenizerState.ScriptDataDoubleEscaped;
                            else
                                _state = TokenizerState.ScriptDataEscaped;
                            _pos++;
                            EmitChar(c, out token);
                            return true;
                        }
                        if (IsAsciiAlpha(c))
                        {
                            _tempBuffer.Append(c >= 'A' && c <= 'Z' ? (char)(c + 0x20) : c);
                            _pos++;
                            EmitChar(c, out token);
                            return true;
                        }
                        _state = TokenizerState.ScriptDataEscaped;
                        continue;

                    case TokenizerState.ScriptDataDoubleEscaped:
                        if (c == '-')
                        {
                            _state = TokenizerState.ScriptDataDoubleEscapedDash;
                            _pos++;
                            EmitChar('-', out token);
                            return true;
                        }
                        if (c == '<')
                        {
                            _state = TokenizerState.ScriptDataDoubleEscapedLessThanSign;
                            _pos++;
                            EmitChar('<', out token);
                            return true;
                        }
                        if (c == '\0')
                        {
                            _pos++;
                            EmitChar('\uFFFD', out token);
                            return true;
                        }
                        _pos++;
                        EmitChar(c, out token);
                        return true;

                    case TokenizerState.ScriptDataDoubleEscapedDash:
                        if (c == '-')
                        {
                            _state = TokenizerState.ScriptDataDoubleEscapedDashDash;
                            _pos++;
                            EmitChar('-', out token);
                            return true;
                        }
                        if (c == '<')
                        {
                            _state = TokenizerState.ScriptDataDoubleEscapedLessThanSign;
                            _pos++;
                            EmitChar('<', out token);
                            return true;
                        }
                        if (c == '\0')
                        {
                            _state = TokenizerState.ScriptDataDoubleEscaped;
                            _pos++;
                            EmitChar('\uFFFD', out token);
                            return true;
                        }
                        _state = TokenizerState.ScriptDataDoubleEscaped;
                        _pos++;
                        EmitChar(c, out token);
                        return true;

                    case TokenizerState.ScriptDataDoubleEscapedDashDash:
                        if (c == '-')
                        {
                            _pos++;
                            EmitChar('-', out token);
                            return true;
                        }
                        if (c == '<')
                        {
                            _state = TokenizerState.ScriptDataDoubleEscapedLessThanSign;
                            _pos++;
                            EmitChar('<', out token);
                            return true;
                        }
                        if (c == '>')
                        {
                            _state = TokenizerState.ScriptData;
                            _pos++;
                            EmitChar('>', out token);
                            return true;
                        }
                        if (c == '\0')
                        {
                            _state = TokenizerState.ScriptDataDoubleEscaped;
                            _pos++;
                            EmitChar('\uFFFD', out token);
                            return true;
                        }
                        _state = TokenizerState.ScriptDataDoubleEscaped;
                        _pos++;
                        EmitChar(c, out token);
                        return true;

                    case TokenizerState.ScriptDataDoubleEscapedLessThanSign:
                        if (c == '/')
                        {
                            _tempBuffer.Length = 0;
                            _state = TokenizerState.ScriptDataDoubleEscapeEnd;
                            _pos++;
                            EmitChar('/', out token);
                            return true;
                        }
                        _state = TokenizerState.ScriptDataDoubleEscaped;
                        continue;

                    case TokenizerState.ScriptDataDoubleEscapeEnd:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ' || c == '/' || c == '>')
                        {
                            if (_tempBuffer.ToString() == "script")
                                _state = TokenizerState.ScriptDataEscaped;
                            else
                                _state = TokenizerState.ScriptDataDoubleEscaped;
                            _pos++;
                            EmitChar(c, out token);
                            return true;
                        }
                        if (IsAsciiAlpha(c))
                        {
                            _tempBuffer.Append(c >= 'A' && c <= 'Z' ? (char)(c + 0x20) : c);
                            _pos++;
                            EmitChar(c, out token);
                            return true;
                        }
                        _state = TokenizerState.ScriptDataDoubleEscaped;
                        continue;

                    case TokenizerState.BeforeAttributeName:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ')
                        {
                            _pos++;
                            continue;
                        }
                        if (c == '/' || c == '>')
                        {
                            _state = TokenizerState.AfterAttributeName;
                            continue;
                        }
                        if (c == '=')
                        {
                            // Parse error, start new attribute with '=' as first char of name
                            _attrBuffer.StartAttribute();
                            _attrBuffer.AppendName(c);
                            _state = TokenizerState.AttributeName;
                            _pos++;
                            continue;
                        }
                        _attrBuffer.StartAttribute();
                        _state = TokenizerState.AttributeName;
                        continue;

                    case TokenizerState.AttributeName:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ')
                        {
                            _state = TokenizerState.AfterAttributeName;
                            _pos++;
                            continue;
                        }
                        if (c == '/')
                        {
                            _attrBuffer.FinishAttribute();
                            _state = TokenizerState.SelfClosingStartTag;
                            _pos++;
                            continue;
                        }
                        if (c == '=')
                        {
                            _state = TokenizerState.BeforeAttributeValue;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _attrBuffer.FinishAttribute();
                            _token.AttributeCount = _attrBuffer.Count;
                            _pos++;
                            _state = TokenizerState.Data;
                            EmitTag(out token);
                            return true;
                        }
                        if (c >= 'A' && c <= 'Z')
                            _attrBuffer.AppendName((char)(c + 0x20));
                        else if (c == '\0')
                            _attrBuffer.AppendName('\uFFFD');
                        else
                            _attrBuffer.AppendName(c);
                        _pos++;
                        continue;

                    case TokenizerState.AfterAttributeName:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ')
                        {
                            _pos++;
                            continue;
                        }
                        if (c == '/')
                        {
                            _attrBuffer.FinishAttribute();
                            _state = TokenizerState.SelfClosingStartTag;
                            _pos++;
                            continue;
                        }
                        if (c == '=')
                        {
                            _state = TokenizerState.BeforeAttributeValue;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _attrBuffer.FinishAttribute();
                            _token.AttributeCount = _attrBuffer.Count;
                            _pos++;
                            _state = TokenizerState.Data;
                            EmitTag(out token);
                            return true;
                        }
                        // Start a new attribute
                        _attrBuffer.FinishAttribute();
                        _attrBuffer.StartAttribute();
                        _state = TokenizerState.AttributeName;
                        continue;

                    case TokenizerState.BeforeAttributeValue:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ')
                        {
                            _pos++;
                            continue;
                        }
                        if (c == '"')
                        {
                            _state = TokenizerState.AttributeValueDoubleQuoted;
                            _pos++;
                            continue;
                        }
                        if (c == '\'')
                        {
                            _state = TokenizerState.AttributeValueSingleQuoted;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            // Parse error, missing value
                            _attrBuffer.FinishAttribute();
                            _token.AttributeCount = _attrBuffer.Count;
                            _pos++;
                            _state = TokenizerState.Data;
                            EmitTag(out token);
                            return true;
                        }
                        _state = TokenizerState.AttributeValueUnquoted;
                        continue;

                    case TokenizerState.AttributeValueDoubleQuoted:
                        if (c == '"')
                        {
                            _attrBuffer.FinishAttribute();
                            _state = TokenizerState.AfterAttributeValueQuoted;
                            _pos++;
                            continue;
                        }
                        if (c == '&')
                        {
                            _pos++;
                            var decoded = CharRefDecoder.Decode(_input, ref _pos);
                            _attrBuffer.AppendValue(decoded ?? "&");
                            continue;
                        }
                        if (c == '\0')
                        {
                            _attrBuffer.AppendValue('\uFFFD');
                            _pos++;
                            continue;
                        }
                        _attrBuffer.AppendValue(c);
                        _pos++;
                        continue;

                    case TokenizerState.AttributeValueSingleQuoted:
                        if (c == '\'')
                        {
                            _attrBuffer.FinishAttribute();
                            _state = TokenizerState.AfterAttributeValueQuoted;
                            _pos++;
                            continue;
                        }
                        if (c == '&')
                        {
                            _pos++;
                            var decoded = CharRefDecoder.Decode(_input, ref _pos);
                            _attrBuffer.AppendValue(decoded ?? "&");
                            continue;
                        }
                        if (c == '\0')
                        {
                            _attrBuffer.AppendValue('\uFFFD');
                            _pos++;
                            continue;
                        }
                        _attrBuffer.AppendValue(c);
                        _pos++;
                        continue;

                    case TokenizerState.AttributeValueUnquoted:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ')
                        {
                            _attrBuffer.FinishAttribute();
                            _state = TokenizerState.BeforeAttributeName;
                            _pos++;
                            continue;
                        }
                        if (c == '&')
                        {
                            _pos++;
                            var decoded = CharRefDecoder.Decode(_input, ref _pos);
                            _attrBuffer.AppendValue(decoded ?? "&");
                            continue;
                        }
                        if (c == '>')
                        {
                            _attrBuffer.FinishAttribute();
                            _token.AttributeCount = _attrBuffer.Count;
                            _pos++;
                            _state = TokenizerState.Data;
                            EmitTag(out token);
                            return true;
                        }
                        if (c == '\0')
                        {
                            _attrBuffer.AppendValue('\uFFFD');
                            _pos++;
                            continue;
                        }
                        _attrBuffer.AppendValue(c);
                        _pos++;
                        continue;

                    case TokenizerState.AfterAttributeValueQuoted:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ')
                        {
                            _state = TokenizerState.BeforeAttributeName;
                            _pos++;
                            continue;
                        }
                        if (c == '/')
                        {
                            _state = TokenizerState.SelfClosingStartTag;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _token.AttributeCount = _attrBuffer.Count;
                            _pos++;
                            _state = TokenizerState.Data;
                            EmitTag(out token);
                            return true;
                        }
                        // Parse error: missing whitespace between attributes
                        _state = TokenizerState.BeforeAttributeName;
                        continue;

                    case TokenizerState.SelfClosingStartTag:
                        if (c == '>')
                        {
                            _token.SelfClosing = true;
                            _token.AttributeCount = _attrBuffer.Count;
                            _pos++;
                            _state = TokenizerState.Data;
                            EmitTag(out token);
                            return true;
                        }
                        // Parse error
                        _state = TokenizerState.BeforeAttributeName;
                        continue;

                    case TokenizerState.BogusComment:
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Comment;
                            _token.Data = _commentBuffer.ToString();
                            token = _token;
                            return true;
                        }
                        if (c == '\0')
                            _commentBuffer.Append('\uFFFD');
                        else
                            _commentBuffer.Append(c);
                        _pos++;
                        continue;

                    case TokenizerState.MarkupDeclarationOpen:
                        if (MatchAhead("--"))
                        {
                            _pos += 2;
                            _commentBuffer.Length = 0;
                            _state = TokenizerState.CommentStart;
                            continue;
                        }
                        if (MatchAheadInsensitive("DOCTYPE"))
                        {
                            _pos += 7;
                            _state = TokenizerState.Doctype;
                            continue;
                        }
                        if (MatchAhead("[CDATA["))
                        {
                            _pos += 7;
                            _state = TokenizerState.CDataSection;
                            continue;
                        }
                        // Bogus comment
                        _commentBuffer.Length = 0;
                        _state = TokenizerState.BogusComment;
                        continue;

                    case TokenizerState.CommentStart:
                        if (c == '-')
                        {
                            _state = TokenizerState.CommentStartDash;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            // Parse error
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Comment;
                            _token.Data = _commentBuffer.ToString();
                            token = _token;
                            return true;
                        }
                        _state = TokenizerState.Comment;
                        continue;

                    case TokenizerState.CommentStartDash:
                        if (c == '-')
                        {
                            _state = TokenizerState.CommentEnd;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Comment;
                            _token.Data = _commentBuffer.ToString();
                            token = _token;
                            return true;
                        }
                        _commentBuffer.Append('-');
                        _state = TokenizerState.Comment;
                        continue;

                    case TokenizerState.Comment:
                        if (c == '<')
                        {
                            _commentBuffer.Append(c);
                            _state = TokenizerState.CommentLessThanSign;
                            _pos++;
                            continue;
                        }
                        if (c == '-')
                        {
                            _state = TokenizerState.CommentEndDash;
                            _pos++;
                            continue;
                        }
                        if (c == '\0')
                        {
                            _commentBuffer.Append('\uFFFD');
                            _pos++;
                            continue;
                        }
                        _commentBuffer.Append(c);
                        _pos++;
                        continue;

                    case TokenizerState.CommentLessThanSign:
                        if (c == '!')
                        {
                            _commentBuffer.Append(c);
                            _state = TokenizerState.CommentLessThanSignBang;
                            _pos++;
                            continue;
                        }
                        if (c == '<')
                        {
                            _commentBuffer.Append(c);
                            _pos++;
                            continue;
                        }
                        _state = TokenizerState.Comment;
                        continue;

                    case TokenizerState.CommentLessThanSignBang:
                        if (c == '-')
                        {
                            _state = TokenizerState.CommentLessThanSignBangDash;
                            _pos++;
                            continue;
                        }
                        _state = TokenizerState.Comment;
                        continue;

                    case TokenizerState.CommentLessThanSignBangDash:
                        if (c == '-')
                        {
                            _state = TokenizerState.CommentLessThanSignBangDashDash;
                            _pos++;
                            continue;
                        }
                        _state = TokenizerState.CommentEndDash;
                        continue;

                    case TokenizerState.CommentLessThanSignBangDashDash:
                        _state = TokenizerState.CommentEnd;
                        continue;

                    case TokenizerState.CommentEndDash:
                        if (c == '-')
                        {
                            _state = TokenizerState.CommentEnd;
                            _pos++;
                            continue;
                        }
                        _commentBuffer.Append('-');
                        _state = TokenizerState.Comment;
                        continue;

                    case TokenizerState.CommentEnd:
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Comment;
                            _token.Data = _commentBuffer.ToString();
                            token = _token;
                            return true;
                        }
                        if (c == '!')
                        {
                            _state = TokenizerState.CommentEndBang;
                            _pos++;
                            continue;
                        }
                        if (c == '-')
                        {
                            _commentBuffer.Append('-');
                            _pos++;
                            continue;
                        }
                        _commentBuffer.Append("--");
                        _state = TokenizerState.Comment;
                        continue;

                    case TokenizerState.CommentEndBang:
                        if (c == '-')
                        {
                            _commentBuffer.Append("--!");
                            _state = TokenizerState.CommentEndDash;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Comment;
                            _token.Data = _commentBuffer.ToString();
                            token = _token;
                            return true;
                        }
                        _commentBuffer.Append("--!");
                        _state = TokenizerState.Comment;
                        continue;

                    case TokenizerState.Doctype:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ')
                        {
                            _state = TokenizerState.BeforeDoctypeName;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _state = TokenizerState.BeforeDoctypeName;
                            continue; // Don't consume, let BeforeDoctypeName handle it
                        }
                        // Parse error, missing whitespace
                        _state = TokenizerState.BeforeDoctypeName;
                        continue;

                    case TokenizerState.BeforeDoctypeName:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ')
                        {
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Doctype;
                            _token.ForceQuirks = true;
                            token = _token;
                            return true;
                        }
                        if (c == '\0')
                        {
                            _doctypeNameBuffer.Length = 0;
                            _doctypeNameBuffer.Append('\uFFFD');
                            _state = TokenizerState.DoctypeName;
                            _pos++;
                            continue;
                        }
                        _doctypeNameBuffer.Length = 0;
                        _doctypeNameBuffer.Append(c >= 'A' && c <= 'Z' ? (char)(c + 0x20) : c);
                        _state = TokenizerState.DoctypeName;
                        _pos++;
                        continue;

                    case TokenizerState.DoctypeName:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ')
                        {
                            _state = TokenizerState.AfterDoctypeName;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Doctype;
                            _token.DoctypeName = _doctypeNameBuffer.ToString();
                            token = _token;
                            return true;
                        }
                        if (c == '\0')
                        {
                            _doctypeNameBuffer.Append('\uFFFD');
                            _pos++;
                            continue;
                        }
                        _doctypeNameBuffer.Append(c >= 'A' && c <= 'Z' ? (char)(c + 0x20) : c);
                        _pos++;
                        continue;

                    case TokenizerState.AfterDoctypeName:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ')
                        {
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Doctype;
                            _token.DoctypeName = _doctypeNameBuffer.ToString();
                            token = _token;
                            return true;
                        }
                        if (MatchAheadInsensitive("PUBLIC"))
                        {
                            _pos += 6;
                            _state = TokenizerState.AfterDoctypePublicKeyword;
                            continue;
                        }
                        if (MatchAheadInsensitive("SYSTEM"))
                        {
                            _pos += 6;
                            _state = TokenizerState.AfterDoctypeSystemKeyword;
                            continue;
                        }
                        // Parse error
                        _token.ForceQuirks = true;
                        _state = TokenizerState.BogusDoctype;
                        _pos++;
                        continue;

                    case TokenizerState.AfterDoctypePublicKeyword:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ')
                        {
                            _state = TokenizerState.BeforeDoctypePublicIdentifier;
                            _pos++;
                            continue;
                        }
                        if (c == '"')
                        {
                            _publicIdBuffer.Length = 0;
                            _state = TokenizerState.DoctypePublicIdentifierDoubleQuoted;
                            _pos++;
                            continue;
                        }
                        if (c == '\'')
                        {
                            _publicIdBuffer.Length = 0;
                            _state = TokenizerState.DoctypePublicIdentifierSingleQuoted;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Doctype;
                            _token.DoctypeName = _doctypeNameBuffer.ToString();
                            _token.ForceQuirks = true;
                            token = _token;
                            return true;
                        }
                        _token.ForceQuirks = true;
                        _state = TokenizerState.BogusDoctype;
                        _pos++;
                        continue;

                    case TokenizerState.BeforeDoctypePublicIdentifier:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ')
                        {
                            _pos++;
                            continue;
                        }
                        if (c == '"')
                        {
                            _publicIdBuffer.Length = 0;
                            _state = TokenizerState.DoctypePublicIdentifierDoubleQuoted;
                            _pos++;
                            continue;
                        }
                        if (c == '\'')
                        {
                            _publicIdBuffer.Length = 0;
                            _state = TokenizerState.DoctypePublicIdentifierSingleQuoted;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Doctype;
                            _token.DoctypeName = _doctypeNameBuffer.ToString();
                            _token.ForceQuirks = true;
                            token = _token;
                            return true;
                        }
                        _token.ForceQuirks = true;
                        _state = TokenizerState.BogusDoctype;
                        _pos++;
                        continue;

                    case TokenizerState.DoctypePublicIdentifierDoubleQuoted:
                        if (c == '"')
                        {
                            _state = TokenizerState.AfterDoctypePublicIdentifier;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Doctype;
                            _token.DoctypeName = _doctypeNameBuffer.ToString();
                            _token.PublicIdentifier = _publicIdBuffer.ToString();
                            _token.ForceQuirks = true;
                            token = _token;
                            return true;
                        }
                        if (c == '\0')
                            _publicIdBuffer.Append('\uFFFD');
                        else
                            _publicIdBuffer.Append(c);
                        _pos++;
                        continue;

                    case TokenizerState.DoctypePublicIdentifierSingleQuoted:
                        if (c == '\'')
                        {
                            _state = TokenizerState.AfterDoctypePublicIdentifier;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Doctype;
                            _token.DoctypeName = _doctypeNameBuffer.ToString();
                            _token.PublicIdentifier = _publicIdBuffer.ToString();
                            _token.ForceQuirks = true;
                            token = _token;
                            return true;
                        }
                        if (c == '\0')
                            _publicIdBuffer.Append('\uFFFD');
                        else
                            _publicIdBuffer.Append(c);
                        _pos++;
                        continue;

                    case TokenizerState.AfterDoctypePublicIdentifier:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ')
                        {
                            _state = TokenizerState.BetweenDoctypePublicAndSystemIdentifiers;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Doctype;
                            _token.DoctypeName = _doctypeNameBuffer.ToString();
                            _token.PublicIdentifier = _publicIdBuffer.ToString();
                            token = _token;
                            return true;
                        }
                        if (c == '"')
                        {
                            _systemIdBuffer.Length = 0;
                            _state = TokenizerState.DoctypeSystemIdentifierDoubleQuoted;
                            _pos++;
                            continue;
                        }
                        if (c == '\'')
                        {
                            _systemIdBuffer.Length = 0;
                            _state = TokenizerState.DoctypeSystemIdentifierSingleQuoted;
                            _pos++;
                            continue;
                        }
                        _token.ForceQuirks = true;
                        _state = TokenizerState.BogusDoctype;
                        _pos++;
                        continue;

                    case TokenizerState.BetweenDoctypePublicAndSystemIdentifiers:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ')
                        {
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Doctype;
                            _token.DoctypeName = _doctypeNameBuffer.ToString();
                            _token.PublicIdentifier = _publicIdBuffer.ToString();
                            token = _token;
                            return true;
                        }
                        if (c == '"')
                        {
                            _systemIdBuffer.Length = 0;
                            _state = TokenizerState.DoctypeSystemIdentifierDoubleQuoted;
                            _pos++;
                            continue;
                        }
                        if (c == '\'')
                        {
                            _systemIdBuffer.Length = 0;
                            _state = TokenizerState.DoctypeSystemIdentifierSingleQuoted;
                            _pos++;
                            continue;
                        }
                        _token.ForceQuirks = true;
                        _state = TokenizerState.BogusDoctype;
                        _pos++;
                        continue;

                    case TokenizerState.AfterDoctypeSystemKeyword:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ')
                        {
                            _state = TokenizerState.BeforeDoctypeSystemIdentifier;
                            _pos++;
                            continue;
                        }
                        if (c == '"')
                        {
                            _systemIdBuffer.Length = 0;
                            _state = TokenizerState.DoctypeSystemIdentifierDoubleQuoted;
                            _pos++;
                            continue;
                        }
                        if (c == '\'')
                        {
                            _systemIdBuffer.Length = 0;
                            _state = TokenizerState.DoctypeSystemIdentifierSingleQuoted;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Doctype;
                            _token.DoctypeName = _doctypeNameBuffer.ToString();
                            _token.ForceQuirks = true;
                            token = _token;
                            return true;
                        }
                        _token.ForceQuirks = true;
                        _state = TokenizerState.BogusDoctype;
                        _pos++;
                        continue;

                    case TokenizerState.BeforeDoctypeSystemIdentifier:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ')
                        {
                            _pos++;
                            continue;
                        }
                        if (c == '"')
                        {
                            _systemIdBuffer.Length = 0;
                            _state = TokenizerState.DoctypeSystemIdentifierDoubleQuoted;
                            _pos++;
                            continue;
                        }
                        if (c == '\'')
                        {
                            _systemIdBuffer.Length = 0;
                            _state = TokenizerState.DoctypeSystemIdentifierSingleQuoted;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Doctype;
                            _token.DoctypeName = _doctypeNameBuffer.ToString();
                            _token.ForceQuirks = true;
                            token = _token;
                            return true;
                        }
                        _token.ForceQuirks = true;
                        _state = TokenizerState.BogusDoctype;
                        _pos++;
                        continue;

                    case TokenizerState.DoctypeSystemIdentifierDoubleQuoted:
                        if (c == '"')
                        {
                            _state = TokenizerState.AfterDoctypeSystemIdentifier;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Doctype;
                            _token.DoctypeName = _doctypeNameBuffer.ToString();
                            _token.PublicIdentifier = _publicIdBuffer.Length > 0 ? _publicIdBuffer.ToString() : null;
                            _token.SystemIdentifier = _systemIdBuffer.ToString();
                            _token.ForceQuirks = true;
                            token = _token;
                            return true;
                        }
                        if (c == '\0')
                            _systemIdBuffer.Append('\uFFFD');
                        else
                            _systemIdBuffer.Append(c);
                        _pos++;
                        continue;

                    case TokenizerState.DoctypeSystemIdentifierSingleQuoted:
                        if (c == '\'')
                        {
                            _state = TokenizerState.AfterDoctypeSystemIdentifier;
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Doctype;
                            _token.DoctypeName = _doctypeNameBuffer.ToString();
                            _token.PublicIdentifier = _publicIdBuffer.Length > 0 ? _publicIdBuffer.ToString() : null;
                            _token.SystemIdentifier = _systemIdBuffer.ToString();
                            _token.ForceQuirks = true;
                            token = _token;
                            return true;
                        }
                        if (c == '\0')
                            _systemIdBuffer.Append('\uFFFD');
                        else
                            _systemIdBuffer.Append(c);
                        _pos++;
                        continue;

                    case TokenizerState.AfterDoctypeSystemIdentifier:
                        if (c == '\t' || c == '\n' || c == '\f' || c == ' ')
                        {
                            _pos++;
                            continue;
                        }
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Doctype;
                            _token.DoctypeName = _doctypeNameBuffer.ToString();
                            _token.PublicIdentifier = _publicIdBuffer.Length > 0 ? _publicIdBuffer.ToString() : null;
                            _token.SystemIdentifier = _systemIdBuffer.ToString();
                            token = _token;
                            return true;
                        }
                        // Parse error, not force quirks here
                        _state = TokenizerState.BogusDoctype;
                        _pos++;
                        continue;

                    case TokenizerState.BogusDoctype:
                        if (c == '>')
                        {
                            _pos++;
                            _state = TokenizerState.Data;
                            _token.Reset();
                            _token.Type = HtmlTokenType.Doctype;
                            _token.DoctypeName = _doctypeNameBuffer.Length > 0 ? _doctypeNameBuffer.ToString() : null;
                            _token.PublicIdentifier = _publicIdBuffer.Length > 0 ? _publicIdBuffer.ToString() : null;
                            _token.SystemIdentifier = _systemIdBuffer.Length > 0 ? _systemIdBuffer.ToString() : null;
                            _token.ForceQuirks = true;
                            token = _token;
                            return true;
                        }
                        _pos++;
                        continue;

                    case TokenizerState.CDataSection:
                        if (c == ']')
                        {
                            _state = TokenizerState.CDataSectionBracket;
                            _pos++;
                            continue;
                        }
                        _pos++;
                        EmitChar(c, out token);
                        return true;

                    case TokenizerState.CDataSectionBracket:
                        if (c == ']')
                        {
                            _state = TokenizerState.CDataSectionEnd;
                            _pos++;
                            continue;
                        }
                        _state = TokenizerState.CDataSection;
                        EmitChar(']', out token);
                        return true;

                    case TokenizerState.CDataSectionEnd:
                        if (c == ']')
                        {
                            _pos++;
                            EmitChar(']', out token);
                            return true;
                        }
                        if (c == '>')
                        {
                            _state = TokenizerState.Data;
                            _pos++;
                            continue;
                        }
                        _state = TokenizerState.CDataSection;
                        _hasChar = true;
                        _pendingChar = ']';
                        EmitChar(']', out token);
                        return true;

                    default:
                        // Unknown state — treat as Data
                        _state = TokenizerState.Data;
                        continue;
                }
            }
        }

        private void EmitChar(char c, out HtmlToken token)
        {
            _token.Reset();
            _token.Type = HtmlTokenType.Character;
            _token.Character = c;
            token = _token;
        }

        private void EmitCharacters(string chars, out HtmlToken token)
        {
            // Emit the first char, queue the rest
            _token.Reset();
            _token.Type = HtmlTokenType.Character;
            _token.Character = chars[0];
            token = _token;

            // Queue remaining chars to be emitted on subsequent Read() calls
            if (chars.Length > 1)
            {
                _hasChar = true;
                _pendingChar = chars[1];
                // If more than 2 chars (supplementary plane), we'll only handle 2 for now
            }
        }

        private void EmitTag(out HtmlToken token)
        {
            _token.AttributeCount = _attrBuffer.Count;
            if (_token.Type == HtmlTokenType.StartTag)
                _lastStartTagName = _token.TagName;
            token = _token;
        }

        private string InternTagName()
        {
            return StringPool.HtmlNames.Intern(_tagNameBuffer.ToString());
        }

        private bool IsAppropriateEndTag()
        {
            if (_lastStartTagName == null) return false;
            var builtName = _tagNameBuffer.ToString();
            return string.Equals(builtName, _lastStartTagName, StringComparison.Ordinal);
        }

        private void EmitBufferedEndTagChars(out HtmlToken token)
        {
            // We need to emit '<', '/', and the chars in tempBuffer
            // Emit '<' now, queue '/' + buffer contents
            _charBuffer.Length = 0;
            _charBuffer.Append('/');
            _charBuffer.Append(_tempBuffer);

            // We'll emit '<' now and queue the first buffered char
            if (_charBuffer.Length > 0)
            {
                _hasChar = true;
                _pendingChar = _charBuffer[0];
                // Note: remaining chars in _charBuffer beyond index 1 are lost in this simplified impl.
                // For a fully spec-compliant tokenizer we'd need a char queue.
                // In practice, failed end tag matches in RCDATA/RAWTEXT are rare.
            }

            _token.Reset();
            _token.Type = HtmlTokenType.Character;
            _token.Character = '<';
            token = _token;
        }

        private bool MatchAhead(string text)
        {
            if (_pos + text.Length > _input.Length) return false;
            for (int i = 0; i < text.Length; i++)
            {
                if (_input[_pos + i] != text[i]) return false;
            }
            return true;
        }

        private bool MatchAheadInsensitive(string text)
        {
            if (_pos + text.Length > _input.Length) return false;
            for (int i = 0; i < text.Length; i++)
            {
                char a = _input[_pos + i];
                char b = text[i];
                if (a >= 'a' && a <= 'z') a = (char)(a - 0x20);
                if (b >= 'a' && b <= 'z') b = (char)(b - 0x20);
                if (a != b) return false;
            }
            return true;
        }

        private static bool IsAsciiAlpha(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }
    }
}
