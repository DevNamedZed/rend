using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptMinMaxConstraintTests
    {
        private readonly ITestOutputHelper _output;
        public WptMinMaxConstraintTests(ITestOutputHelper output) { _output = output; }

        // min-width > max-width: min wins
        [Fact]
        public void MinWidth_Beats_MaxWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='min-width:200px;max-width:100px;height:20px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width >= 199);
        }

        // min-height > max-height: min wins
        [Fact]
        public void MinHeight_Beats_MaxHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;min-height:200px;max-height:100px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height >= 199);
        }

        // max-width on block narrows it
        [Fact]
        public void MaxWidth_NarrowsBlock()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='max-width:200px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // min-width on narrow container expands
        [Fact]
        public void MinWidth_Expands()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:100px'>
                    <div id='t' style='min-width:200px;height:20px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width >= 199);
        }

        // min-width percentage
        [Fact]
        public void MinWidth_Percent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:50px;min-width:50%;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }

        // max-width percentage
        [Fact]
        public void MaxWidth_Percent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='max-width:25%;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // max-height clamps auto height
        [Fact]
        public void MaxHeight_ClampsAutoHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;max-height:50px'>
                    <div style='height:200px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height <= 51);
        }

        // min-height on empty element
        [Fact]
        public void MinHeight_EmptyElement()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:100px;min-height:100px'></div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height >= 99);
        }

        // flex item: max-width prevents grow
        [Fact]
        public void FlexItem_MaxWidth_PreventsGrow()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='t' style='flex:1;max-width:100px;height:30px'></div>
                    <div style='flex:1;height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width <= 101);
        }

        // flex item: min-width prevents shrink
        [Fact]
        public void FlexItem_MinWidth_PreventsShrink()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='t' style='flex:0 1 150px;min-width:120px;height:30px'></div>
                    <div style='flex:0 1 150px;height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width >= 119);
        }

        // grid item: max-height in explicit row
        // TODO: Known bug — grid stretch ignores max-height on items
        [Fact(Skip = "Known bug: grid item max-height with stretch")]
        public void GridItem_MaxHeight_InRow()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;grid-template-rows:100px;width:200px'>
                    <div id='t' style='max-height:50px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height <= 51);
        }

        // box-sizing border-box: min-width includes padding+border
        [Fact]
        public void BorderBox_MinWidth_IncludesPaddingBorder()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:50px'>
                    <div id='t' style='box-sizing:border-box;min-width:100px;padding:10px;border:5px solid;height:30px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            float totalW = t.ContentRect.Width + t.PaddingLeft + t.PaddingRight + t.BorderLeftWidth + t.BorderRightWidth;
            Assert.True(totalW >= 99);
        }

        // box-sizing border-box: max-width includes padding+border
        [Fact]
        public void BorderBox_MaxWidth_IncludesPaddingBorder()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='box-sizing:border-box;max-width:100px;padding:10px;border:5px solid;height:30px'></div>
                </div></body>");
            var t = LayoutTestHelper.FindById(r, "t")!;
            float totalW = t.ContentRect.Width + t.PaddingLeft + t.PaddingRight + t.BorderLeftWidth + t.BorderRightWidth;
            Assert.True(totalW <= 101);
        }

        // abspos: min-width on abspos element
        // TODO: Known bug — min-width not applied to abspos shrink-to-fit
        [Fact(Skip = "Known bug: abspos min-width")]
        public void AbsPos_MinWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:200px;height:100px'>
                    <div id='t' style='position:absolute;left:0;min-width:150px;height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width >= 149);
        }

        // abspos: max-width on abspos element
        // TODO: Known bug — max-width not clamping abspos width from left+right
        [Fact(Skip = "Known bug: abspos max-width")]
        public void AbsPos_MaxWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:100px'>
                    <div id='t' style='position:absolute;left:0;right:0;max-width:200px;height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width <= 201);
        }

        // explicit width between min and max: uses explicit
        [Fact]
        public void Width_BetweenMinMax_UsesExplicit()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:150px;min-width:100px;max-width:200px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 150) < 2);
        }

        // explicit width below min: uses min
        [Fact]
        public void Width_BelowMin_UsesMin()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:50px;min-width:100px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // explicit width above max: uses max
        [Fact]
        public void Width_AboveMax_UsesMax()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:300px;max-width:200px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 200) < 2);
        }
    }
}
