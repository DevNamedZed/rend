using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS Flexbox align-content property with multi-line containers.
    /// </summary>
    public class WptFlexAlignContentTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexAlignContentTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §8.4] align-content: flex-start (default for single line)
        [Fact] public void AlignContent_FlexStart() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:flex-start;width:100px;height:200px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y < 2);
        }

        // [CSS-FLEXBOX §8.4] align-content: flex-end
        [Fact] public void AlignContent_FlexEnd() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:flex-end;width:100px;height:200px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            var b = LayoutTestHelper.FindById(r,"b")!;
            // Last item should be near bottom: Y ≈ 200 - 30 = 170
            Assert.True(b.ContentRect.Y + b.ContentRect.Height >= 198);
        }

        // [CSS-FLEXBOX §8.4] align-content: center
        [Fact] public void AlignContent_Center() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:center;width:100px;height:200px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            var a = LayoutTestHelper.FindById(r,"a")!;
            // Two lines of 30px = 60px. Free = 140. Center offset = 70.
            Assert.True(a.ContentRect.Y >= 68 && a.ContentRect.Y <= 72);
        }

        // [CSS-FLEXBOX §8.4] align-content: space-between
        [Fact] public void AlignContent_SpaceBetween() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:space-between;width:100px;height:200px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y < 2);
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y + 30 >= 198);
        }

        // [CSS-FLEXBOX §8.4] align-content: space-around
        [Fact] public void AlignContent_SpaceAround() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:space-around;width:100px;height:200px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            // Free=140, 4 half-gaps of 35: a at 35, b at 135
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y >= 33);
        }

        // [CSS-FLEXBOX §8.4] align-content: space-evenly
        [Fact] public void AlignContent_SpaceEvenly() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:space-evenly;width:100px;height:210px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            // Free=150, 3 gaps of 50: a at 50, b at 130
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y >= 48);
        }

        // [CSS-FLEXBOX §8.4] align-content: stretch (default)
        [Fact] public void AlignContent_Stretch() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-wrap:wrap;align-content:stretch;width:100px;height:200px'><div id='a' style='width:60px'></div><div id='b' style='width:60px'></div></div></body>");
            // Stretch: each line gets extra cross space
            var a = LayoutTestHelper.FindById(r,"a")!;
            var b = LayoutTestHelper.FindById(r,"b")!;
            _output.WriteLine($"a.h={a.ContentRect.Height} b.h={b.ContentRect.Height}");
            // Each line stretched to 100px (200/2)
            Assert.True(a.ContentRect.Height >= 99);
        }

        // [CSS-FLEXBOX §8.4] single-line ignores align-content
        [Fact] public void AlignContent_SingleLine_Ignored() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;align-content:flex-end;height:200px;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>");
            // Single line → align-content has no effect, items at top (align-items default stretch)
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y < 2);
        }

        // [CSS-FLEXBOX §9] row-gap with flex-wrap
        [Fact] public void FlexWrap_RowGap() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;flex-wrap:wrap;row-gap:20px;width:100px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            float gap = LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - (LayoutTestHelper.FindById(r,"a")!.ContentRect.Y + LayoutTestHelper.FindById(r,"a")!.ContentRect.Height);
            Assert.True(System.Math.Abs(gap - 20) < 2);
        }

        // [CSS-FLEXBOX §9] column-gap in flex
        [Fact] public void Flex_ColumnGap() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='display:flex;column-gap:15px;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            float gap = LayoutTestHelper.FindById(r,"b")!.ContentRect.X - (LayoutTestHelper.FindById(r,"a")!.ContentRect.X + LayoutTestHelper.FindById(r,"a")!.ContentRect.Width);
            Assert.True(System.Math.Abs(gap - 15) < 2);
        }
    }
}
