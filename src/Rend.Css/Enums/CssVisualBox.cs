namespace Rend.Css
{
    /// <summary>
    /// [CSS-OVERFLOW-3 §3.4] The &lt;visual-box&gt; reference box for overflow-clip-margin.
    /// Determines which box edge the clip margin extends from.
    /// </summary>
    public enum CssVisualBox : byte
    {
        ContentBox,
        PaddingBox,
        BorderBox
    }
}
