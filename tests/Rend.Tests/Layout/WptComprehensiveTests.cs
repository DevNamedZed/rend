using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Comprehensive WPT-pattern layout tests covering CSS2, Flexbox, Grid, Sizing,
    /// Positioning, Overflow, Colors, Backgrounds, Tables, and Multicol.
    /// </summary>
    public class WptComprehensiveTests
    {
        private readonly ITestOutputHelper _output;
        public WptComprehensiveTests(ITestOutputHelper output) { _output = output; }

        // ======================================================================
        // CSS2 §8 Box Model
        // ======================================================================

        /// <spec>CSS2 §8.3 https://www.w3.org/TR/CSS2/box.html#margin-properties</spec>
        [Fact] public void Margin_Negative_PullsElement() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='height:50px'></div><div id='t' style='margin-top:-20px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 50);
        }

        /// <spec>CSS2 §8.3.1 https://www.w3.org/TR/CSS2/box.html#collapsing-margins</spec>
        [Fact] public void Margin_Collapse_Adjacent() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='margin-bottom:40px;height:20px'></div><div id='t' style='margin-top:30px;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 60) < 2);
        }

        /// <spec>CSS2 §8.3.1 https://www.w3.org/TR/CSS2/box.html#collapsing-margins</spec>
        [Fact] public void Margin_Collapse_ParentChild() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='p' style='width:200px;margin-top:20px'><div style='margin-top:30px;height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"p")!.ContentRect.Y >= 29);
        }

        /// <spec>CSS2 §8.3.1 https://www.w3.org/TR/CSS2/box.html#collapsing-margins</spec>
        [Fact] public void Margin_Collapse_Blocked_By_Padding() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='p' style='width:200px;margin-top:20px;padding-top:1px'><div id='c' style='margin-top:30px;height:20px'></div></div></body>");
            var p = LayoutTestHelper.FindById(r,"p")!;
            var c = LayoutTestHelper.FindById(r,"c")!;
            Assert.True(c.ContentRect.Y > p.ContentRect.Y + 25);
        }

        /// <spec>CSS2 §8.5 https://www.w3.org/TR/CSS2/box.html#border-properties</spec>
        [Fact] public void Border_Width_Medium_Default() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border: medium solid black;width:100px;height:50px'></div></body>");
            Assert.Equal(3, LayoutTestHelper.FindById(r,"t")!.BorderTopWidth);
        }

        /// <spec>CSS2 §8.5 https://www.w3.org/TR/CSS2/box.html#border-properties</spec>
        [Fact] public void Border_Style_None_ZeroWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='border: 5px none red;width:100px;height:50px'></div></body>");
            Assert.Equal(0, LayoutTestHelper.FindById(r,"t")!.BorderTopWidth);
        }

        // ======================================================================
        // CSS2 §9 Visual Formatting
        // ======================================================================

        /// <spec>CSS2 §9.2.4 https://www.w3.org/TR/CSS2/visuren.html#display-prop</spec>
        [Fact] public void Display_None_NoBox() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:none'><div id='t' style='width:100px;height:50px'></div></div><div id='v' style='height:20px'></div></body>");
            Assert.Null(LayoutTestHelper.FindById(r,"t"));
            Assert.True(LayoutTestHelper.FindById(r,"v")!.ContentRect.Y < 2);
        }

        /// <spec>CSS2 §9.4.1 https://www.w3.org/TR/CSS2/visuren.html#block-formatting</spec>
        [Fact] public void BFC_Float_Containment() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='overflow:hidden;width:200px'><div style='float:left;width:80px;height:100px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 99);
        }

        /// <spec>CSS2 §9.5 https://www.w3.org/TR/CSS2/visuren.html#floats</spec>
        [Fact] public void Float_Left_Right() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><div id='l' style='float:left;width:80px;height:40px'></div><div id='r' style='float:right;width:80px;height:40px'></div></div></body>");
            var l = LayoutTestHelper.FindById(r,"l")!;
            var ri = LayoutTestHelper.FindById(r,"r")!;
            Assert.True(ri.ContentRect.X > l.ContentRect.X + 50);
        }

        /// <spec>CSS2 §9.5.2 https://www.w3.org/TR/CSS2/visuren.html#flow-control</spec>
        [Fact] public void Clear_Both() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><div style='float:left;width:80px;height:50px'></div><div style='float:right;width:80px;height:30px'></div><div id='t' style='clear:both;height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 49);
        }

        // ======================================================================
        // CSS2 §10 Visual Formatting Details
        // ======================================================================

        /// <spec>CSS2 §10.3.3 https://www.w3.org/TR/CSS2/visudet.html#blockwidth</spec>
        [Fact] public void Block_Width_Auto_Fills() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><div id='t' style='margin:0 20px;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 260) < 2);
        }

        /// <spec>CSS2 §10.3.4 https://www.w3.org/TR/CSS2/visudet.html#abs-non-replaced-width</spec>
        [Fact] public void AbsPos_Width_LeftRight() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='position:relative;width:300px;height:100px'><div id='t' style='position:absolute;left:20px;right:30px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 250) < 2);
        }

        /// <spec>CSS2 §10.5 https://www.w3.org/TR/CSS2/visudet.html#the-height-property</spec>
        [Fact] public void Height_Percent_Definite() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:100px;height:200px'><div id='t' style='height:50%'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        /// <spec>CSS2 §10.5 https://www.w3.org/TR/CSS2/visudet.html#the-height-property</spec>
        [Fact] public void Height_Percent_Indefinite_Auto() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:100px'><div id='t' style='height:50%'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height < 1);
        }

        /// <spec>CSS2 §10.4 https://www.w3.org/TR/CSS2/visudet.html#min-max-widths</spec>
        [Fact] public void MaxWidth_Clamps() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='max-width:150px;height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 151);
        }

        /// <spec>CSS2 §10.4 https://www.w3.org/TR/CSS2/visudet.html#min-max-widths</spec>
        [Fact] public void MinWidth_Expands() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:50px'><div id='t' style='min-width:100px;height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 99);
        }

        // ======================================================================
        // CSS-SIZING §4-5
        // ======================================================================

        /// <spec>CSS-SIZING §4.1 https://drafts.csswg.org/css-sizing/#width-height-keywords</spec>
        [Fact] public void Width_FitContent() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><div id='t' style='width:fit-content'><div style='width:80px;height:10px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        /// <spec>CSS-SIZING §4.1 https://drafts.csswg.org/css-sizing/#width-height-keywords</spec>
        [Fact] public void Width_MinContent() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><div id='t' style='width:min-content'><div style='width:120px;height:10px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 120) < 2);
        }

        /// <spec>CSS-SIZING §4.1 https://drafts.csswg.org/css-sizing/#width-height-keywords</spec>
        [Fact] public void Width_MaxContent() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><div id='t' style='width:max-content'><div style='width:120px;height:10px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 120) < 2);
        }

        /// <spec>CSS-SIZING §5.1 https://drafts.csswg.org/css-sizing/#aspect-ratio</spec>
        [Fact] public void AspectRatio_2to1() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:200px;aspect-ratio:2/1'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 100) < 2);
        }

        /// <spec>CSS-SIZING §5.1 https://drafts.csswg.org/css-sizing/#aspect-ratio</spec>
        [Fact] public void AspectRatio_16to9() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:320px;aspect-ratio:16/9'></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 180) < 2);
        }

        // ======================================================================
        // CSS-DISPLAY §2-3
        // ======================================================================

        /// <spec>CSS-DISPLAY §2.1 https://drafts.csswg.org/css-display/#the-display-properties</spec>
        [Fact] public void Display_Contents_Transparent() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:contents;border:10px solid red'><div id='c' style='width:50px;height:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width >= 49);
        }

        /// <spec>CSS-DISPLAY §3 https://drafts.csswg.org/css-display/#valdef-display-flow-root</spec>
        [Fact] public void Display_FlowRoot_ContainsFloat() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='display:flow-root;width:200px'><div style='float:left;width:80px;height:60px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 59);
        }

        // ======================================================================
        // CSS-FLEXBOX §4-9
        // ======================================================================

        /// <spec>CSS-FLEXBOX §5.1 https://drafts.csswg.org/css-flexbox/#flex-direction-property</spec>
        [Fact] public void FlexDirection_Row() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X > LayoutTestHelper.FindById(r,"a")!.ContentRect.X);
        }

        /// <spec>CSS-FLEXBOX §5.1 https://drafts.csswg.org/css-flexbox/#flex-direction-property</spec>
        [Fact] public void FlexDirection_Column() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y > LayoutTestHelper.FindById(r,"a")!.ContentRect.Y);
        }

        /// <spec>CSS-FLEXBOX §7.1 https://drafts.csswg.org/css-flexbox/#flex-grow-property</spec>
        [Fact] public void FlexGrow_Distributes() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex-grow:1;height:30px'></div><div id='b' style='flex-grow:2;height:30px'></div></div></body>");
            float ratio = LayoutTestHelper.FindById(r,"b")!.ContentRect.Width / LayoutTestHelper.FindById(r,"a")!.ContentRect.Width;
            Assert.True(ratio > 1.8 && ratio < 2.2);
        }

        /// <spec>CSS-FLEXBOX §7.2 https://drafts.csswg.org/css-flexbox/#flex-shrink-property</spec>
        [Fact] public void FlexShrink_Reduces() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:100px'><div id='a' style='width:80px;flex-shrink:1;height:30px'></div><div id='b' style='width:80px;flex-shrink:1;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width < 80);
        }

        /// <spec>CSS-FLEXBOX §5.2 https://drafts.csswg.org/css-flexbox/#flex-wrap-property</spec>
        [Fact] public void FlexWrap_Wraps() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-wrap:wrap;width:100px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y > LayoutTestHelper.FindById(r,"a")!.ContentRect.Y);
        }

        /// <spec>CSS-FLEXBOX §8.2 https://drafts.csswg.org/css-flexbox/#align-items-property</spec>
        [Fact] public void AlignItems_Stretch_Default() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='width:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 99);
        }

        /// <spec>CSS-FLEXBOX §8.4 https://drafts.csswg.org/css-flexbox/#justify-content-property</spec>
        [Fact] public void JustifyContent_FlexEnd() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;justify-content:flex-end;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X >= 149);
        }

        /// <spec>CSS-FLEXBOX §5.4 https://drafts.csswg.org/css-flexbox/#order-property</spec>
        [Fact] public void Order_Reorders() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='a' style='order:2;width:50px;height:30px'></div><div id='b' style='order:1;width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X < LayoutTestHelper.FindById(r,"a")!.ContentRect.X);
        }

        /// <spec>CSS-FLEXBOX §9.5 https://drafts.csswg.org/css-flexbox/#algo-main-container</spec>
        [Fact] public void Flex_Gap() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;gap:20px;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            float gap = LayoutTestHelper.FindById(r,"b")!.ContentRect.X - (LayoutTestHelper.FindById(r,"a")!.ContentRect.X + LayoutTestHelper.FindById(r,"a")!.ContentRect.Width);
            Assert.True(System.Math.Abs(gap - 20) < 2);
        }

        // ======================================================================
        // CSS-GRID §7-8
        // ======================================================================

        /// <spec>CSS-GRID §7.2 https://drafts.csswg.org/css-grid/#track-sizing</spec>
        [Fact] public void Grid_Fr_Tracks() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 2fr;width:300px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 200) < 2);
        }

        /// <spec>CSS-GRID §8.3 https://drafts.csswg.org/css-grid/#placement</spec>
        [Fact] public void Grid_Explicit_Placement() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:100px 100px;grid-template-rows:50px 50px;width:200px'><div id='t' style='grid-column:2;grid-row:2'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(t.ContentRect.X >= 99);
            Assert.True(t.ContentRect.Y >= 49);
        }

        /// <spec>CSS-GRID §8.5 https://drafts.csswg.org/css-grid/#grid-template-areas-property</spec>
        [Fact] public void Grid_Named_Areas() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-areas:\"h h\" \"s m\";grid-template-columns:80px 1fr;grid-template-rows:40px 60px;width:200px'><div id='h' style='grid-area:h'></div><div id='s' style='grid-area:s'></div><div id='m' style='grid-area:m'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"h")!.ContentRect.Width >= 199);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"s")!.ContentRect.Width - 80) < 2);
        }

        /// <spec>CSS-GRID §10.1 https://drafts.csswg.org/css-grid/#gutters</spec>
        [Fact] public void Grid_Gap() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;gap:20px;width:220px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            float gap = LayoutTestHelper.FindById(r,"b")!.ContentRect.X - (LayoutTestHelper.FindById(r,"a")!.ContentRect.X + LayoutTestHelper.FindById(r,"a")!.ContentRect.Width);
            Assert.True(System.Math.Abs(gap - 20) < 2);
        }

        /// <spec>CSS-GRID §7.3 https://drafts.csswg.org/css-grid/#repeat-notation</spec>
        [Fact] public void Grid_Repeat() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(4,50px);width:200px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div><div id='c' style='height:20px'></div><div id='d' style='height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"d")!.ContentRect.X - 150) < 2);
        }

        /// <spec>CSS-GRID §7.2.1 https://drafts.csswg.org/css-grid/#valdef-grid-template-columns-minmax</spec>
        [Fact] public void Grid_MinMax() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:grid;grid-template-columns:minmax(50px,1fr) minmax(50px,1fr);width:200px'><div id='a' style='height:20px'></div><div id='b' style='height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width >= 99);
        }

        // ======================================================================
        // CSS-VALUES §6-8
        // ======================================================================

        /// <spec>CSS-VALUES §6.1 https://drafts.csswg.org/css-values/#em</spec>
        [Fact] public void Em_Unit() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='font-size:20px'><div id='t' style='width:5em;height:2em'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 40) < 2);
        }

        /// <spec>CSS-VALUES §6.1 https://drafts.csswg.org/css-values/#rem</spec>
        [Fact] public void Rem_Unit() {
            var r = LayoutTestHelper.Layout("<html style='font-size:20px'><body style='margin:0'><div style='font-size:10px'><div id='t' style='width:5rem;height:2rem'></div></div></body></html>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        /// <spec>CSS-VALUES §6.3 https://drafts.csswg.org/css-values/#viewport-relative-lengths</spec>
        [Fact] public void Vw_Vh_Units() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='width:50vw;height:25vh'></div></body>", 400, 300);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 75) < 2);
        }

        /// <spec>CSS-VALUES §8.1 https://drafts.csswg.org/css-values/#calc-notation</spec>
        [Fact] public void Calc_PercentPlusPx() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:calc(50% - 20px);height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 180) < 2);
        }

        /// <spec>CSS-VALUES §8.3 https://drafts.csswg.org/css-values/#funcdef-clamp</spec>
        [Fact] public void Clamp_LowerBound() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:60px'><div id='t' style='width:clamp(50px,50%,200px);height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 50) < 2);
        }

        /// <spec>CSS-VALUES §8.2 https://drafts.csswg.org/css-values/#funcdef-min</spec>
        [Fact] public void Min_Function() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:min(300px,50%);height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        /// <spec>CSS-VALUES §8.2 https://drafts.csswg.org/css-values/#funcdef-max</spec>
        [Fact] public void Max_Function() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='width:max(100px,25%);height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        // ======================================================================
        // CSS-COLOR §4
        // ======================================================================

        /// <spec>CSS-COLOR §4.1 https://drafts.csswg.org/css-color/#named-colors</spec>
        [Fact] public void Color_Named_Lime() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='color:lime;width:10px;height:10px'></div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.Equal(0, s.Style.Color.R);
            Assert.Equal(255, s.Style.Color.G);
            Assert.Equal(0, s.Style.Color.B);
        }

        /// <spec>CSS-COLOR §4.2 https://drafts.csswg.org/css-color/#hex-color</spec>
        [Fact] public void Color_Hex_Short() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='color:#f00;width:10px;height:10px'></div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.Equal(255, s.Style.Color.R);
        }

        /// <spec>CSS-COLOR §4.2 https://drafts.csswg.org/css-color/#hex-color</spec>
        [Fact] public void Color_Hex_8Digit_Alpha() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='color:#ff000080;width:10px;height:10px'></div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.Equal(255, s.Style.Color.R);
            Assert.Equal(128, s.Style.Color.A);
        }

        // ======================================================================
        // CSS-MULTICOL §3-5
        // ======================================================================

        /// <spec>CSS-MULTICOL §3.1 https://drafts.csswg.org/css-multicol/#cc</spec>
        [Fact] public void Multicol_ColumnCount() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='column-count:3;column-gap:0;width:300px'><div style='height:60px'></div><div style='height:60px'></div><div style='height:60px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height <= 61);
        }

        /// <spec>CSS-MULTICOL §5.1 https://drafts.csswg.org/css-multicol/#column-span</spec>
        [Fact] public void Multicol_Span_All() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='column-count:2;width:200px'><div style='height:30px'></div><div id='s' style='column-span:all;height:20px'></div><div style='height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"s")!.ContentRect.Width >= 199);
        }

        // ======================================================================
        // CSS-TABLE §4-5
        // ======================================================================

        /// <spec>CSS-TABLES §4.3 https://drafts.csswg.org/css-tables/#border-collapse-property</spec>
        [Fact] public void Table_BorderCollapse() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><table id='t' style='border-collapse:collapse;width:200px'><tr><td style='border:2px solid;height:30px'>A</td><td style='border:2px solid;height:30px'>B</td></tr></table></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.Equal(CssBorderCollapse.Collapse, s.Style.BorderCollapse);
        }

        /// <spec>CSS-TABLES §4.4 https://drafts.csswg.org/css-tables/#border-spacing-property</spec>
        [Fact] public void Table_BorderSpacing() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><table id='t' style='border-spacing:10px;width:200px'><tr><td style='height:30px'>A</td></tr></table></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 49);
        }

        // ======================================================================
        // CSS-BACKGROUNDS §3-4
        // ======================================================================

        /// <spec>CSS-BACKGROUNDS §3.1 https://drafts.csswg.org/css-backgrounds/#background-color</spec>
        [Fact] public void BackgroundColor_Parsed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='background-color:green;width:50px;height:50px'></div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.True(s.Style.BackgroundColor.G > 100);
        }

        // ======================================================================
        // CSS-TEXT §6-8
        // ======================================================================

        /// <spec>CSS-TEXT §6.1 https://drafts.csswg.org/css-text/#text-align-property</spec>
        [Fact] public void TextAlign_Parsed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-align:center;width:200px'>x</div></body>");
            Assert.Equal(CssTextAlign.Center, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextAlign);
        }

        /// <spec>CSS-TEXT §6.2 https://drafts.csswg.org/css-text/#text-align-last-property</spec>
        [Fact] public void TextAlignLast_Parsed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-align-last:justify;width:200px'>x</div></body>");
            Assert.Equal(CssTextAlign.Justify, ((LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!).Style.TextAlignLast);
        }

        /// <spec>CSS-TEXT §7.1 https://drafts.csswg.org/css-text/#text-indent-property</spec>
        [Fact] public void TextIndent_Parsed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='text-indent:2em;font-size:16px;width:200px'>x</div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.True(System.Math.Abs(s.Style.TextIndent - 32) < 2);
        }

        /// <spec>CSS-TEXT §8.1 https://drafts.csswg.org/css-text/#word-spacing-property</spec>
        [Fact] public void WordSpacing_Parsed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='word-spacing:5px;width:200px'>x</div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.True(System.Math.Abs(s.Style.WordSpacing - 5) < 0.5f);
        }

        /// <spec>CSS-TEXT §8.2 https://drafts.csswg.org/css-text/#letter-spacing-property</spec>
        [Fact] public void LetterSpacing_Parsed() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div id='t' style='letter-spacing:3px;width:200px'>x</div></body>");
            var s = (LayoutTestHelper.FindById(r,"t")!.StyledNode as StyledElement)!;
            Assert.True(System.Math.Abs(s.Style.LetterSpacing - 3) < 0.5f);
        }

        // ======================================================================
        // CSS-TRANSFORMS §2
        // ======================================================================

        /// <spec>CSS-TRANSFORMS §2 https://drafts.csswg.org/css-transforms/#transform-property</spec>
        [Fact] public void Transform_NoLayoutEffect() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='transform:translateX(100px);height:30px'></div><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 30) < 2);
        }
    }
}
