using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests mirroring WPT css-display patterns.
    /// </summary>
    public class WptDisplayConformanceTests
    {
        private readonly ITestOutputHelper _output;
        public WptDisplayConformanceTests(ITestOutputHelper output) { _output = output; }

        // display:none removes element and descendants
        [Fact]
        public void None_RemovesFromTree()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:none'><div id='hidden' style='width:100px;height:100px'></div></div>
                <div id='visible' style='height:20px'></div></body>");
            Assert.Null(LayoutTestHelper.FindById(r, "hidden"));
            Assert.True(LayoutTestHelper.FindById(r, "visible")!.ContentRect.Y < 2);
        }

        // display:none takes no space
        [Fact]
        public void None_NoSpace()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:30px'></div>
                    <div style='display:none;height:200px'></div>
                    <div id='after' style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "after")!.ContentRect.Y - 30) < 2);
        }

        // display:contents children visible, wrapper invisible
        [Fact]
        public void Contents_ChildrenVisible()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='display:contents'>
                        <div id='child' style='width:100px;height:50px'></div>
                    </div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "child")!.ContentRect.Width >= 99);
        }

        // display:contents border/padding not rendered
        [Fact]
        public void Contents_NoBorder()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:contents;border:10px solid red'>
                    <div id='child' style='width:50px;height:50px'></div>
                </div></body>");
            Assert.Equal(0, LayoutTestHelper.FindById(r, "child")!.BorderTopWidth);
        }

        // display:contents inherits styles to children
        [Fact]
        public void Contents_InheritsStyle()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='color:red'>
                    <div style='display:contents;color:blue'>
                        <div id='child' style='width:50px;height:20px'></div>
                    </div>
                </div></body>");
            var s = (LayoutTestHelper.FindById(r, "child")!.StyledNode as StyledElement)!;
            Assert.Equal(0, s.Style.Color.R);
            Assert.True(s.Style.Color.B > 200);
        }

        // display:contents in flex — children become flex items
        [Fact]
        public void Contents_InFlex()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div style='display:contents'>
                        <div id='a' style='width:50px;height:30px'></div>
                        <div id='b' style='width:50px;height:30px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 50) < 2);
        }

        // display:contents in grid — children become grid items
        [Fact]
        public void Contents_InGrid()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:200px'>
                    <div style='display:contents'>
                        <div id='a' style='height:20px'></div>
                        <div id='b' style='height:20px'></div>
                    </div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "b")!.ContentRect.X >= 99);
        }

        // display:block fills container width
        [Fact]
        public void Block_FillsWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='display:block;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 300) < 2);
        }

        // display:inline-block shrinks to content
        [Fact]
        public void InlineBlock_Shrinks()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <span id='t' style='display:inline-block'>
                        <div style='width:80px;height:20px'></div>
                    </span>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 80) < 2);
        }

        // display:flex creates flex container
        [Fact]
        public void Flex_Horizontal()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 50) < 2);
        }

        // display:inline-flex shrinks to content
        [Fact]
        public void InlineFlex_Shrinks()
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

        // display:grid creates grid container
        [Fact]
        public void Grid_2Columns()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:200px'>
                    <div id='a' style='height:20px'></div>
                    <div id='b' style='height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.X - 100) < 2);
        }

        // display:inline-grid shrinks to content
        [Fact]
        public void InlineGrid_Shrinks()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:inline-grid;grid-template-columns:50px 50px'>
                        <div style='height:20px'></div>
                        <div style='height:20px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width - 100) < 2);
        }

        // display:flow-root establishes BFC
        [Fact]
        public void FlowRoot_ContainsFloats()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:flow-root;width:200px'>
                    <div style='float:left;width:80px;height:60px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height >= 59);
        }

        // display:flow-root avoids sibling floats
        [Fact]
        public void FlowRoot_AvoidsFloat()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div style='float:left;width:100px;height:50px'></div>
                    <div id='t' style='display:flow-root'>content</div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.X >= 99);
        }

        // display:table creates table
        [Fact]
        public void Table_Display()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:table;width:200px'>
                    <div style='display:table-row'>
                        <div style='display:table-cell;height:30px'>A</div>
                    </div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Width >= 199);
        }

        // display:list-item has marker
        [Fact]
        public void ListItem_Display()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='display:list-item;margin-left:40px;height:20px'>Item</div></body>");
            Assert.Equal(CssDisplay.ListItem, ((LayoutTestHelper.FindById(r, "t")!.StyledNode as StyledElement)!).Style.Display);
        }
    }
}
