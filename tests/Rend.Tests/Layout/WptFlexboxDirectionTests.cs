using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexboxDirectionTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexboxDirectionTests(ITestOutputHelper output) { _output = output; }

        // row: items left to right
        [Fact] public void Row_LeftToRight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div><div id='c' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 100) < 2);
        }

        // row-reverse: items right to left
        [Fact] public void RowReverse_RightToLeft() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;width:300px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div><div id='c' style='width:50px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.X > LayoutTestHelper.FindById(r,"b")!.ContentRect.X);
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X > LayoutTestHelper.FindById(r,"c")!.ContentRect.X);
        }

        // column: items top to bottom
        [Fact] public void Column_TopToBottom() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div><div id='c' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - 30) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.Y - 60) < 2);
        }

        // column-reverse: items bottom to top
        [Fact] public void ColumnReverse_BottomToTop() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;height:200px;width:200px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div><div id='c' style='height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.Y > LayoutTestHelper.FindById(r,"b")!.ContentRect.Y);
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y > LayoutTestHelper.FindById(r,"c")!.ContentRect.Y);
        }

        // row: items fill cross axis (height)
        [Fact] public void Row_CrossFill() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row;height:80px;width:200px'><div id='t' style='width:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Height - 80) < 2);
        }

        // column: items fill cross axis (width)
        [Fact] public void Column_CrossFill() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px'><div id='t' style='height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 200) < 2);
        }

        // row auto height = tallest item
        [Fact] public void Row_AutoHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='flex' style='display:flex;width:200px'><div style='width:50px;height:40px'></div><div style='width:50px;height:80px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"flex")!.ContentRect.Height - 80) < 2);
        }

        // column auto height = sum of items
        [Fact] public void Column_AutoHeight() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='flex' style='display:flex;flex-direction:column;width:200px'><div style='height:40px'></div><div style='height:60px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"flex")!.ContentRect.Height - 100) < 2);
        }

        // row-reverse with justify-content:flex-start → items at right
        [Fact] public void RowReverse_JustifyStart() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;justify-content:flex-start;width:200px'><div id='t' style='width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 150) < 2);
        }

        // column-reverse with justify-content:flex-start → items at bottom
        [Fact] public void ColumnReverse_JustifyStart() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column-reverse;justify-content:flex-start;height:200px;width:100px'><div id='t' style='height:50px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y - 150) < 2);
        }

        // flex-flow: row wrap
        [Fact] public void FlexFlow_RowWrap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-flow:row wrap;width:100px'><div id='a' style='width:60px;height:20px'></div><div id='b' style='width:60px;height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.Y > 18);
        }

        // flex-flow: column wrap
        [Fact] public void FlexFlow_ColumnWrap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-flow:column wrap;height:100px;width:200px'><div id='a' style='width:50px;height:60px'></div><div id='b' style='width:50px;height:60px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"b")!.ContentRect.X > 48);
        }

        // flex-flow: row-reverse wrap-reverse
        [Fact(Skip="Known: row-reverse wrap-reverse interaction")] public void FlexFlow_RowReverseWrapReverse() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-flow:row-reverse wrap-reverse;width:100px;height:100px'><div id='a' style='width:60px;height:30px'></div><div id='b' style='width:60px;height:30px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"a")!.ContentRect.X > LayoutTestHelper.FindById(r,"b")!.ContentRect.X ||
                         LayoutTestHelper.FindById(r,"a")!.ContentRect.Y > LayoutTestHelper.FindById(r,"b")!.ContentRect.Y);
        }

        // order property
        [Fact] public void Order_Reorders() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:150px'><div id='a' style='order:3;width:50px;height:30px'></div><div id='b' style='order:1;width:50px;height:30px'></div><div id='c' style='order:2;width:50px;height:30px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"b")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"c")!.ContentRect.X - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"a")!.ContentRect.X - 100) < 2);
        }

        // gap in row flex
        [Fact] public void Gap_Row() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;gap:20px;width:200px'><div id='a' style='width:50px;height:30px'></div><div id='b' style='width:50px;height:30px'></div></div></body>");
            float gap = LayoutTestHelper.FindById(r,"b")!.ContentRect.X - (LayoutTestHelper.FindById(r,"a")!.ContentRect.X + 50);
            Assert.True(System.Math.Abs(gap - 20) < 2);
        }

        // gap in column flex
        [Fact] public void Gap_Column() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;gap:15px;width:100px'><div id='a' style='height:30px'></div><div id='b' style='height:30px'></div></div></body>");
            float gap = LayoutTestHelper.FindById(r,"b")!.ContentRect.Y - (LayoutTestHelper.FindById(r,"a")!.ContentRect.Y + 30);
            Assert.True(System.Math.Abs(gap - 15) < 2);
        }

        // row-reverse with gap
        [Fact] public void RowReverse_Gap() {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:row-reverse;gap:20px;width:200px'><div id='a' style='width:40px;height:30px'></div><div id='b' style='width:40px;height:30px'></div></div></body>");
            float gap = LayoutTestHelper.FindById(r,"a")!.ContentRect.X - (LayoutTestHelper.FindById(r,"b")!.ContentRect.X + 40);
            Assert.True(System.Math.Abs(gap - 20) < 2);
        }
    }
}
