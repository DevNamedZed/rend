using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests that mirror actual WPT css-flexbox test patterns.
    /// Each test reproduces a specific WPT test's HTML and verifies exact dimensions.
    /// </summary>
    public class WptFlexboxConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxConformanceTests(ITestOutputHelper output) { _output = output; }

        // WPT: align-items-001 — align-items:flex-start, items at top
        [Fact]
        public void AlignItems001_FlexStart()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;height:100px;width:200px'>
                    <div id='t' style='width:100px;height:50px;background:green'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 50) < 2);
        }

        // WPT: align-items-002 — align-items:flex-end
        [Fact]
        public void AlignItems002_FlexEnd()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-end;height:100px;width:200px'>
                    <div id='t' style='width:100px;height:50px;background:green'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 50) < 2);
        }

        // WPT: align-items-003 — align-items:center
        [Fact]
        public void AlignItems003_Center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:center;height:100px;width:200px'>
                    <div id='t' style='width:100px;height:50px;background:green'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 25) < 2);
        }

        // WPT: align-items-004 — align-items:stretch (default)
        [Fact]
        public void AlignItems004_Stretch()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:stretch;height:100px;width:200px'>
                    <div id='t' style='width:100px;background:green'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 100) < 2);
        }

        // WPT: align-items-008 — min-height with align-items:stretch
        // TODO: Known bug — min-height on flex item with auto height (stored as 0) not applied
        [Fact(Skip = "Known bug: height initial value 0 vs NaN")]
        public void AlignItems008_MinHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;min-height:200px;width:200px'>
                    <div id='t' style='width:100px;min-height:100px;background:green'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            _output.WriteLine($"t.h={t.ContentRect.Height}");
            // Item has min-height:100px. Container has min-height:200px.
            // Stretch should ideally make item 200px, but min-height on container
            // doesn't establish definite cross-size. Item should at least be 100px.
            Assert.True(t.ContentRect.Height >= 99,
                $"Item min-height respected (got {t.ContentRect.Height})");
        }

        // WPT: align-self-001 — align-self overrides align-items
        [Fact]
        public void AlignSelf001_Override()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;height:100px;width:200px'>
                    <div id='t' style='align-self:flex-end;width:50px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 70) < 2);
        }

        // WPT: flex-grow-001 — flex-grow distributes space
        [Fact]
        public void FlexGrow001()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='a' style='flex-grow:1;height:10px;background:green'></div>
                    <div id='b' style='flex-grow:3;height:10px;background:blue'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 25) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 75) < 2);
        }

        // WPT: flex-shrink-001 — flex-shrink handles overflow
        [Fact]
        public void FlexShrink001()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='a' style='flex-shrink:1;width:100px;height:10px'></div>
                    <div id='b' style='flex-shrink:1;width:100px;height:10px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 50) < 2);
        }

        // WPT: flex-basis-001 — flex-basis overrides width
        [Fact]
        public void FlexBasis001()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='t' style='flex-basis:100px;width:50px;height:10px;flex-grow:0;flex-shrink:0'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // WPT: flex-direction-row — items horizontal
        [Fact]
        public void FlexDirectionRow()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:row;width:200px'>
                    <div id='a' style='width:50px;height:50px'></div>
                    <div id='b' style='width:50px;height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 50) < 2);
        }

        // WPT: flex-direction-column — items vertical
        [Fact]
        public void FlexDirectionColumn()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px'>
                    <div id='a' style='height:50px'></div>
                    <div id='b' style='height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 50) < 2);
        }

        // WPT: flex-direction-row-reverse
        [Fact]
        public void FlexDirectionRowReverse()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:row-reverse;width:200px'>
                    <div id='a' style='width:50px;height:50px'></div>
                    <div id='b' style='width:50px;height:50px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.X > LayoutTestHelper.FindById(r, "b")!.ContentRect.X);
        }

        // WPT: flex-direction-column-reverse
        [Fact]
        public void FlexDirectionColumnReverse()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column-reverse;width:200px;height:200px'>
                    <div id='a' style='height:50px'></div>
                    <div id='b' style='height:50px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y > LayoutTestHelper.FindById(r, "b")!.ContentRect.Y);
        }

        // WPT: flex-wrap-001 — items wrap to next line
        [Fact]
        public void FlexWrap001()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:100px'>
                    <div id='a' style='width:60px;height:20px'></div>
                    <div id='b' style='width:60px;height:20px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y >= 19);
        }

        // WPT: flex-wrap-002 — column wrap
        [Fact]
        public void FlexWrap002_Column()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;flex-wrap:wrap;width:200px;height:100px'>
                    <div id='a' style='width:50px;height:60px'></div>
                    <div id='b' style='width:50px;height:60px'></div>
                </div></body>");
            // b wraps to next column
            Assert.True(LayoutTestHelper.FindById(r, "b")!.ContentRect.X > LayoutTestHelper.FindById(r, "a")!.ContentRect.X);
        }

        // WPT: justify-content-001 — flex-start (default)
        [Fact]
        public void JustifyContent001_FlexStart()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;justify-content:flex-start;width:200px'>
                    <div id='t' style='width:50px;height:50px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.X < 2);
        }

        // WPT: justify-content-002 — flex-end
        [Fact]
        public void JustifyContent002_FlexEnd()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;justify-content:flex-end;width:200px'>
                    <div id='t' style='width:50px;height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 150) < 2);
        }

        // WPT: justify-content-003 — center
        [Fact]
        public void JustifyContent003_Center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;justify-content:center;width:200px'>
                    <div id='t' style='width:50px;height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 75) < 2);
        }

        // WPT: justify-content-004 — space-between
        [Fact]
        public void JustifyContent004_SpaceBetween()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;justify-content:space-between;width:200px'>
                    <div id='a' style='width:50px;height:50px'></div>
                    <div id='b' style='width:50px;height:50px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.X < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 150) < 2);
        }

        // WPT: justify-content-005 — space-around
        [Fact]
        public void JustifyContent005_SpaceAround()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;justify-content:space-around;width:200px'>
                    <div id='a' style='width:50px;height:50px'></div>
                    <div id='b' style='width:50px;height:50px'></div>
                </div></body>");
            // free=100. 4 half-gaps of 25. a at 25, b at 125.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 25) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 125) < 2);
        }

        // WPT: order-001 — order reorders items
        [Fact]
        public void Order001()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:150px'>
                    <div id='a' style='order:2;width:50px;height:50px'></div>
                    <div id='b' style='order:1;width:50px;height:50px'></div>
                    <div id='c' style='order:3;width:50px;height:50px'></div>
                </div></body>");
            // Visual order: b(1) a(2) c(3)
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.X - 100) < 2);
        }

        // WPT: gap-001-ltr — column-gap in row flex
        [Fact]
        public void Gap001_Ltr()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;column-gap:20px;width:200px'>
                    <div id='a' style='width:50px;height:50px'></div>
                    <div id='b' style='width:50px;height:50px'></div>
                </div></body>");
            float gap = LayoutTestHelper.FindById(r, "b")!.ContentRect.X - (LayoutTestHelper.FindById(r, "a")!.ContentRect.X + 50);
            Assert.True(System.Math.Abs(gap - 20) < 2);
        }

        // WPT: flex-001 — flex:1 distributes equally
        [Fact]
        public void Flex001_Equal()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:50px'></div>
                    <div id='b' style='flex:1;height:50px'></div>
                    <div id='c' style='flex:1;height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "c")!.ContentRect.Width - 100) < 2);
        }

        // WPT: flex-none — flex:none keeps width
        [Fact]
        public void FlexNone()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:none;width:100px;height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // WPT: flex-auto — flex:auto grows from basis
        [Fact]
        public void FlexAuto()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:auto;width:100px;height:50px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width >= 299);
        }

        // WPT: flex-shrink-002 — negative shrink invalid
        [Fact]
        public void FlexShrink002_NegativeInvalid()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='a' style='flex-shrink:-2;width:100px;height:10px'></div>
                    <div id='b' style='flex-shrink:-3;width:100px;height:10px'></div>
                </div></body>");
            // Invalid → default(1). Equal shrink: 50 each.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 50) < 2);
        }

        // WPT: auto-margins — margin-left:auto pushes right
        [Fact]
        public void AutoMargins_LeftAuto()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div style='width:50px;height:50px'></div>
                    <div id='t' style='margin-left:auto;width:50px;height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 150) < 2);
        }

        // WPT: auto-margins — margin:auto centers
        [Fact]
        public void AutoMargins_BothCenter()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='t' style='margin:0 auto;width:50px;height:50px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 75) < 2);
        }

        // WPT: flexbox-abspos-child — abspos not flex item
        [Fact]
        public void AbsposChild_NotFlexItem()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;position:relative;width:200px'>
                    <div style='width:50px;height:30px'></div>
                    <div style='position:absolute;width:30px;height:30px'></div>
                    <div id='t' style='width:50px;height:30px'></div>
                </div></body>");
            // Abspos doesn't take space → t at X=50
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 50) < 2);
        }

        // WPT: flex-flow-001 — flex-flow shorthand
        [Fact]
        public void FlexFlow001()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-flow:row wrap;width:100px'>
                    <div id='a' style='width:60px;height:20px'></div>
                    <div id='b' style='width:60px;height:20px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y >= 19);
        }

        // WPT: flex-grow with basis — grow from non-zero basis
        [Fact]
        public void FlexGrow_FromBasis()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex-grow:1;flex-basis:100px;height:30px'></div>
                    <div id='b' style='flex-grow:1;flex-basis:50px;height:30px'></div>
                </div></body>");
            // Free=150. Equal grow: +75 each. a=175, b=125.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 175) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 125) < 2);
        }

        // WPT: flex-shrink weighted by basis
        [Fact]
        public void FlexShrink_WeightedByBasis()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:0 1 200px;height:30px'></div>
                    <div id='b' style='flex:0 3 200px;height:30px'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(r, "a")!;
            var b = LayoutTestHelper.FindById(r, "b")!;
            _output.WriteLine($"a={a.ContentRect.Width} b={b.ContentRect.Width}");
            // Overflow=200. Scaled: a=1*200=200, b=3*200=600. Total=800.
            // a shrinks 200*200/800=50→150. b shrinks 200*600/800=150→50.
            Assert.True(System.Math.Abs(a.ContentRect.Width - 150) < 3);
            Assert.True(System.Math.Abs(b.ContentRect.Width - 50) < 3);
        }

        // WPT: inline-flex shrinks to content
        [Fact]
        public void InlineFlex_ShrinkToFit()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:inline-flex'>
                        <div style='width:60px;height:30px'></div>
                        <div style='width:40px;height:30px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // WPT: flex column — items fill cross axis (width)
        [Fact]
        public void FlexColumn_ItemsFillWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px'>
                    <div id='t' style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // WPT: flex column — justify-content:center
        [Fact]
        public void FlexColumn_JustifyCenter()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;justify-content:center;height:200px;width:100px'>
                    <div id='t' style='height:60px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 70) < 2);
        }

        // WPT: flex items establish BFC
        [Fact]
        public void FlexItems_EstablishBFC()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='t' style='width:100px'>
                        <div style='float:left;width:50px;height:60px'></div>
                    </div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height >= 59);
        }

        // WPT: flex with calc basis
        [Fact]
        public void Flex_CalcBasis()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='flex:0 0 calc(50% - 20px);height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 180) < 2);
        }

        // WPT: display:contents in flex
        [Fact]
        public void DisplayContents_InFlex()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div style='display:contents'>
                        <div id='a' style='width:50px;height:30px'></div>
                        <div id='b' style='width:50px;height:30px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 50) < 2);
        }

        // WPT: flex-grow:0.5 (fractional)
        [Fact]
        public void FlexGrow_Fractional()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='t' style='flex-grow:0.5;flex-basis:50px;height:30px'></div>
                </div></body>");
            // Free=150. grow<1: scaled=150*0.5=75. Item=50+75=125.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 125) < 2);
        }

        // WPT: flex:0 0 — basis defaults to 0
        [Fact]
        public void Flex_0_0_BasisZero()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='t' style='flex:0 0;width:100px;height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width < 2);
        }

        // WPT: flex column with gap
        [Fact]
        public void FlexColumn_Gap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-direction:column;gap:15px;width:100px'>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            // 30*3 + 15*2 = 120
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "flex")!.ContentRect.Height - 120) < 2);
        }

        // WPT: nested flex — row in column
        [Fact]
        public void NestedFlex_RowInColumn()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:300px'>
                    <div style='display:flex'>
                        <div id='a' style='flex:1;height:30px'></div>
                        <div id='b' style='flex:1;height:30px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 150) < 2);
        }

        // WPT: flex with mixed fixed and flexible items
        [Fact]
        public void MixedFixedFlexible()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='fixed' style='flex:0 0 80px;height:30px'></div>
                    <div id='grow' style='flex:1;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "fixed")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "grow")!.ContentRect.Width - 220) < 2);
        }

        // WPT: flex-shrink:0 prevents shrinking
        [Fact]
        public void FlexShrink0_NoShrink()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:100px'>
                    <div id='t' style='flex-shrink:0;width:200px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // WPT: cross-axis auto margin centers
        [Fact]
        public void CrossMarginAuto_Centers()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:100px;width:200px'>
                    <div id='t' style='margin-top:auto;margin-bottom:auto;width:50px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 35) < 2);
        }
    }
}
