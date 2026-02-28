namespace Rend.Html.Parser.Internal
{
    internal enum HtmlTokenType : byte
    {
        StartTag,
        EndTag,
        Character,
        Comment,
        Doctype,
        EndOfFile
    }
}
