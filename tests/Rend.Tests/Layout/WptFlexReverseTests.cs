using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexReverseTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexReverseTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void RowReverse_ItemsRightToLeft()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div><div id='c' style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "a")!.ContentRect.X > LayoutTestHelper.FindById(root, "b")!.ContentRect.X);
            Assert.True(LayoutTestHelper.FindById(root, "b")!.ContentRect.X > LayoutTestHelper.FindById(root, "c")!.ContentRect.X);
        }

        [Fact]
        public void RowReverse_FirstItemAtRightEdge()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;width:300px'><div id='first' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "first")!.ContentRect.X - 250) < 2);
        }

        [Fact]
        public void RowReverse_ThreeItemsExactPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;width:300px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div><div id='c' style='width:60px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 240) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 180) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 120) < 2);
        }

        [Fact]
        public void RowReverse_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;gap:20px;width:300px'><div id='a' style='width:40px;height:30px'></div><div id='b' style='width:40px;height:30px'></div></div></body>");
            float gapBetween = LayoutTestHelper.FindById(root, "a")!.ContentRect.X - (LayoutTestHelper.FindById(root, "b")!.ContentRect.X + 40);
            Assert.True(System.Math.Abs(gapBetween - 20) < 2);
        }

        [Fact]
        public void RowReverse_JustifyFlexStart_ItemsAtRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;justify-content:flex-start;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 250) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 200) < 2);
        }

        [Fact]
        public void RowReverse_JustifyFlexEnd_ItemsAtLeft()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;justify-content:flex-end;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 50) < 2);
        }

        [Fact]
        public void RowReverse_JustifyCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;justify-content:center;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
        }

        [Fact]
        public void RowReverse_JustifySpaceBetween()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;justify-content:space-between;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 250) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 0) < 2);
        }

        [Fact]
        public void ColumnReverse_ItemsBottomToTop()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;height:200px;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div><div id='c' style='height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y > LayoutTestHelper.FindById(root, "b")!.ContentRect.Y);
            Assert.True(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y > LayoutTestHelper.FindById(root, "c")!.ContentRect.Y);
        }

        [Fact]
        public void ColumnReverse_FirstItemAtBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;height:200px;width:200px'><div id='first' style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "first")!.ContentRect.Y - 160) < 2);
        }

        [Fact]
        public void ColumnReverse_ThreeItemsExactPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;height:200px;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div><div id='c' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 170) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 140) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 110) < 2);
        }

        [Fact]
        public void ColumnReverse_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;gap:10px;height:200px;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            float gapBetween = LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - (LayoutTestHelper.FindById(root, "b")!.ContentRect.Y + 30);
            Assert.True(System.Math.Abs(gapBetween - 10) < 2);
        }

        [Fact]
        public void ColumnReverse_JustifyFlexStart_ItemsAtBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;justify-content:flex-start;height:200px;width:200px'><div id='a' style='height:40px'></div><div id='b' style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 160) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 120) < 2);
        }

        [Fact]
        public void ColumnReverse_JustifyFlexEnd_ItemsAtTop()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;justify-content:flex-end;height:200px;width:200px'><div id='a' style='height:40px'></div><div id='b' style='height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 40) < 2);
        }

        [Fact]
        public void ColumnReverse_JustifyCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;justify-content:center;height:200px;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 70) < 2);
        }

        [Fact]
        public void RowReverse_FlexGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;width:300px'><div id='a' style='flex-grow:1;height:30px'></div><div id='b' style='flex-grow:2;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
            Assert.True(LayoutTestHelper.FindById(root, "a")!.ContentRect.X > LayoutTestHelper.FindById(root, "b")!.ContentRect.X);
        }

        [Fact]
        public void ColumnReverse_FlexGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;height:200px;width:200px'><div id='a' style='flex-grow:1'></div><div id='b' style='flex-grow:3'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Height - 150) < 2);
            Assert.True(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y > LayoutTestHelper.FindById(root, "b")!.ContentRect.Y);
        }

        [Fact]
        public void RowReverse_AlignItemsCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;align-items:center;height:100px;width:300px'><div id='t' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 35) < 2);
        }

        [Fact]
        public void RowReverse_AlignItemsFlexEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;align-items:flex-end;height:100px;width:300px'><div id='t' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 70) < 2);
        }

        [Fact]
        public void ColumnReverse_AlignItemsCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;align-items:center;height:200px;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 60) < 2);
        }

        [Fact]
        public void ColumnReverse_AlignItemsFlexEnd()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;align-items:flex-end;height:200px;width:200px'><div id='t' style='width:80px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 120) < 2);
        }

        [Fact]
        public void RowReverse_OrderProperty()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;width:300px'><div id='a' style='order:3;width:50px;height:30px'></div><div id='b' style='order:1;width:50px;height:30px'></div><div id='c' style='order:2;width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "b")!.ContentRect.X > LayoutTestHelper.FindById(root, "c")!.ContentRect.X);
            Assert.True(LayoutTestHelper.FindById(root, "c")!.ContentRect.X > LayoutTestHelper.FindById(root, "a")!.ContentRect.X);
        }

        [Fact(Skip = "Known: row-reverse wrap interaction")]
        public void RowReverse_WrapCreatesNewLine()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;flex-wrap:wrap;width:100px'><div id='a' style='width:60px;height:20px'></div><div id='b' style='width:60px;height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y > 18);
        }

        [Fact(Skip = "Known: row-reverse wrap interaction")]
        public void RowReverse_WrapItemPositions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;flex-wrap:wrap;width:150px'><div id='a' style='width:80px;height:20px'></div><div id='b' style='width:80px;height:20px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 70) < 2);
            Assert.True(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y >= 20);
        }

        [Fact]
        public void ColumnReverse_JustifySpaceBetween()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;justify-content:space-between;height:200px;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 170) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 0) < 2);
        }

        [Fact]
        public void RowReverse_JustifySpaceAround()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;justify-content:space-around;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            float spacePerItem = 200f / 2;
            float halfSpace = spacePerItem / 2;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - (300 - halfSpace - 50)) < 2);
        }

        [Fact]
        public void RowReverse_SingleItemFlexGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;width:300px'><div id='t' style='flex-grow:1;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.X - 0) < 2);
        }

        [Fact]
        public void ColumnReverse_SingleItemFlexGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;height:200px;width:200px'><div id='t' style='flex-grow:1'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Y - 0) < 2);
        }

        [Fact]
        public void RowReverse_AlignItemsStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;align-items:stretch;height:100px;width:300px'><div id='t' style='width:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void ColumnReverse_AlignItemsStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;align-items:stretch;height:200px;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "t")!.ContentRect.Width - 200) < 2);
        }

        [Fact]
        public void RowReverse_WithGapAndJustifyCenter()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;justify-content:center;gap:20px;width:300px'><div id='a' style='width:40px;height:30px'></div><div id='b' style='width:40px;height:30px'></div></div></body>");
            float centerX = (LayoutTestHelper.FindById(root, "b")!.ContentRect.X + LayoutTestHelper.FindById(root, "a")!.ContentRect.X + 40) / 2;
            Assert.True(System.Math.Abs(centerX - 150) < 2);
        }

        [Fact]
        public void ColumnReverse_WithGapAndFlexGrow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;gap:10px;height:200px;width:200px'><div id='a' style='flex-grow:1'></div><div id='b' style='flex-grow:1'></div></div></body>");
            float expectedHeight = (200 - 10) / 2f;
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - expectedHeight) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Height - expectedHeight) < 2);
        }

        [Fact]
        public void RowReverse_AutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='flex' style='display:flex;flex-direction:row-reverse;width:300px'><div style='width:50px;height:40px'></div><div style='width:50px;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "flex")!.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void ColumnReverse_AutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='flex' style='display:flex;flex-direction:column-reverse;width:200px'><div style='height:40px'></div><div style='height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "flex")!.ContentRect.Height - 100) < 2);
        }
    }
}
