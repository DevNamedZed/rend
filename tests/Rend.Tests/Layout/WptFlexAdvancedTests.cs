using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Advanced flexbox tests covering percentage sizing, nested flex,
    /// flex-basis resolution, cross-axis behavior, and edge cases.
    /// </summary>
    public class WptFlexAdvancedTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexAdvancedTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §9.2] flex-basis: 0 with flex-grow distributes all space
        [Fact] public void FlexBasis0_GrowDistributesAll() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:300px'><div id='a' style='flex:1 0 0px;height:30px'></div><div id='b' style='flex:1 0 0px;height:30px'></div><div id='c' style='flex:1 0 0px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.2] flex-basis: auto uses width property
        [Fact] public void FlexBasisAuto_UsesWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:0 0 auto;width:120px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 120) < 2);
        }

        // [CSS-FLEXBOX §9.2] flex-basis percentage
        [Fact] public void FlexBasis_Percent() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex-basis:50%;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §4] abspos children are not flex items
        [Fact] public void AbsPos_NotFlexItem() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;position:relative;width:200px'><div style='width:50px;height:30px'></div><div style='position:absolute;width:30px;height:30px'></div><div id='t' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 50) < 2);
        }

        // [CSS-FLEXBOX §8.1] auto margins absorb free space
        [Fact] public void AutoMargin_Left_PushesRight() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div style='width:50px;height:30px'></div><div id='t' style='margin-left:auto;width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X >= 149);
        }

        // [CSS-FLEXBOX §8.1] auto margins center item
        [Fact] public void AutoMargin_Both_Centers() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='margin:0 auto;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 75) < 2);
        }

        // [CSS-FLEXBOX §8.3] align-items: stretch is default
        [Fact] public void AlignItems_Stretch_FillsCross() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='width:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 99);
        }

        // [CSS-FLEXBOX §8.3] align-items: flex-start
        [Fact] public void AlignItems_FlexStart() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;align-items:flex-start;height:100px;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 2);
        }

        // [CSS-FLEXBOX §8.3] align-items: center
        [Fact] public void AlignItems_Center_Vertically() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;align-items:center;height:100px;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(t.ContentRect.Y >= 34 && t.ContentRect.Y <= 36);
        }

        // [CSS-FLEXBOX §8.4] justify-content: space-between
        [Fact] public void JustifyContent_SpaceBetween() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;justify-content:space-between;width:200px'><div id='a' style='width:30px;height:30px'></div><div id='b' style='width:30px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.X < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 170) < 2);
        }

        // [CSS-FLEXBOX §8.4] justify-content: space-around
        [Fact] public void JustifyContent_SpaceAround() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;justify-content:space-around;width:200px'><div id='a' style='width:40px;height:30px'></div><div id='b' style='width:40px;height:30px'></div></div></body>");
            var a = LayoutTestHelper.FindById(r,"a")!;
            var b = LayoutTestHelper.FindById(r,"b")!;
            // free=120, 4 half-gaps of 30: a at 30, b at 130
            Assert.True(a.ContentRect.X >= 29 && a.ContentRect.X <= 31);
        }

        // [CSS-FLEXBOX §8.4] justify-content: space-evenly
        [Fact] public void JustifyContent_SpaceEvenly() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;justify-content:space-evenly;width:200px'><div id='a' style='width:40px;height:30px'></div><div id='b' style='width:40px;height:30px'></div></div></body>");
            var a = LayoutTestHelper.FindById(r,"a")!;
            // free=120, 3 gaps of 40: a at 40
            Assert.True(System.Math.Abs(a.ContentRect.X - 40) < 2);
        }

        // [CSS-FLEXBOX §5.1] flex-direction: row-reverse
        [Fact] public void FlexDirection_RowReverse() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.X > LayoutTestHelper.FindById(r,"b")!.ContentRect.X);
        }

        // [CSS-FLEXBOX §5.1] flex-direction: column-reverse
        [Fact] public void FlexDirection_ColumnReverse() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;width:200px;height:200px'><div id='a' style='height:50px'></div><div id='b' style='height:50px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y > LayoutTestHelper.FindById(r,"b")!.ContentRect.Y);
        }

        // [CSS-FLEXBOX §5.2] flex-wrap: wrap
        [Fact] public void FlexWrap_Wrap() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-wrap:wrap;width:100px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y > LayoutTestHelper.FindById(r,"a")!.ContentRect.Y);
        }

        // [CSS-FLEXBOX §5.2] flex-wrap: wrap-reverse
        [Fact] public void FlexWrap_WrapReverse() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-wrap:wrap-reverse;width:100px;height:100px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y > LayoutTestHelper.FindById(r,"b")!.ContentRect.Y);
        }

        // [CSS-FLEXBOX §7.1] flex shorthand: flex: 0 0 → basis 0
        [Fact] public void Flex_Shorthand_00_BasisZero() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex:0 0;width:100px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width < 1);
        }

        // [CSS-FLEXBOX §7.1] flex shorthand: flex: 1 → grows, basis 0
        [Fact] public void Flex_Shorthand_1_Grows() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-FLEXBOX §7.1] flex: none = 0 0 auto
        [Fact] public void Flex_None_KeepsWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex:none;width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        // [CSS-FLEXBOX §7.1] flex: auto = 1 1 auto
        [Fact] public void Flex_Auto_GrowsFromWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:300px'><div id='t' style='flex:auto;width:100px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width >= 299);
        }

        // [CSS-FLEXBOX §9.5] gap in flex
        [Fact] public void Flex_Gap_RowGap() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-wrap:wrap;width:100px;row-gap:10px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            float gap = LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - (LayoutTestHelper.FindById(r,"a")!.ContentRect.Y + LayoutTestHelper.FindById(r,"a")!.ContentRect.Height);
            Assert.True(System.Math.Abs(gap - 10) < 2);
        }

        // [CSS-FLEXBOX §4] display:contents in flex → children become flex items
        [Fact] public void DisplayContents_InFlex() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div style='display:contents'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X > LayoutTestHelper.FindById(r,"a")!.ContentRect.X);
        }

        // [CSS-FLEXBOX §9] nested flex: row inside column
        [Fact] public void Nested_RowInColumn() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div style='display:flex'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X > LayoutTestHelper.FindById(r,"a")!.ContentRect.X);
        }

        // [CSS-FLEXBOX §9] nested flex: column inside row
        [Fact] public void Nested_ColumnInRow() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div style='display:flex;flex-direction:column'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y > LayoutTestHelper.FindById(r,"a")!.ContentRect.Y);
        }

        // [CSS-FLEXBOX §4] flex items establish BFC
        [Fact] public void FlexItem_EstablishesBFC() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='width:100px'><div style='float:left;width:50px;height:60px'></div></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(t.ContentRect.Height >= 59);
        }

        // [CSS-FLEXBOX §7.2] negative flex-grow invalid
        [Fact] public void FlexGrow_Negative_Invalid() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:200px'><div id='t' style='flex-grow:-1;width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width <= 51);
        }

        // [CSS-FLEXBOX §7.3] negative flex-shrink invalid
        [Fact] public void FlexShrink_Negative_Invalid() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;width:100px'><div id='a' style='flex-shrink:-1;width:80px;height:30px'></div><div id='b' style='width:80px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Width < 80);
        }

        // [CSS-FLEXBOX §9.4] single-line stretch fills container cross (auto height)
        [Fact] public void SingleLine_CrossSize_EqualsContainer() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;height:100px;width:200px'><div id='t' style='width:50px'></div></div></body>");
            // default stretch + auto height → item fills container cross
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height >= 99);
        }

        // [CSS-FLEXBOX §9] inline-flex shrinks to content
        [Fact] public void InlineFlex_ShrinkToFit() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:400px'><div id='t' style='display:inline-flex'><div style='width:50px;height:30px'></div><div style='width:50px;height:30px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 100) < 2);
        }
    }
}
