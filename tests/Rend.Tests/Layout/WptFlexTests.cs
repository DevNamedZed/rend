using Rend.Css;
using Rend.Style;
using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Flex_InlineFlex_ShrinkToFit()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 400px;'>
                    <div id='iflex' style='display: inline-flex;'>
                        <div style='width: 50px; height: 30px;'></div>
                        <div style='width: 50px; height: 30px;'></div>
                    </div>
                </div></body>");
            var iflex = LayoutTestHelper.FindById(root, "iflex");
            Assert.NotNull(iflex);
            _output.WriteLine($"iflex: {iflex!.ContentRect.Width}x{iflex.ContentRect.Height}");
            Assert.True(System.Math.Abs(iflex.ContentRect.Width - 100) < 2,
                $"inline-flex shrinks to content (got {iflex.ContentRect.Width})");
        }

        [Fact]
        public void Flex_AlignSelf_FlexEnd()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; height: 100px; width: 200px;'>
                    <div id='item' style='align-self: flex-end; width: 50px; height: 30px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.Y={item!.ContentRect.Y}");
            // flex-end: item at bottom of 100px container
            float expectedY = 100 - 30; // container bottom minus item height
            Assert.True(item.ContentRect.Y >= expectedY - 2,
                $"flex-end Y (got {item.ContentRect.Y}, expected ~{expectedY})");
        }

        [Fact]
        public void Flex_FlexGrow_Zero_NoGrow()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 300px;'>
                    <div id='item' style='flex-grow: 0; width: 100px; height: 30px;'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 100) < 2,
                $"flex-grow:0 keeps width (got {item.ContentRect.Width})");
        }

        [Fact]
        public void Flex_CrossAxis_AutoHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='flex' style='display: flex; width: 200px;'>
                    <div style='width: 50px; height: 80px;'></div>
                    <div style='width: 50px; height: 40px;'></div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            Assert.NotNull(flex);
            _output.WriteLine($"flex.h={flex!.ContentRect.Height}");
            // Auto height = tallest item = 80px
            Assert.True(System.Math.Abs(flex.ContentRect.Height - 80) < 2,
                $"Auto height = tallest item (got {flex.ContentRect.Height})");
        }

        [Fact]
        public void Flex_Column_AutoWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 300px;'>
                    <div id='flex' style='display: flex; flex-direction: column;'>
                        <div style='width: 100px; height: 30px;'></div>
                        <div style='width: 150px; height: 30px;'></div>
                    </div>
                </div></body>");
            var flex = LayoutTestHelper.FindById(root, "flex");
            Assert.NotNull(flex);
            _output.WriteLine($"flex.w={flex!.ContentRect.Width}");
            // Column flex auto width fills container
            Assert.True(flex.ContentRect.Width >= 299,
                $"Column flex fills width (got {flex.ContentRect.Width})");
        }

        [Fact]
        public void Flex_Nested_Row_In_Column()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; flex-direction: column; width: 200px;'>
                    <div id='inner' style='display: flex; flex-direction: row;'>
                        <div id='a' style='width: 60px; height: 30px;'></div>
                        <div id='b' style='width: 60px; height: 30px;'></div>
                    </div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(a);
            Assert.NotNull(b);
            Assert.True(b!.ContentRect.X > a!.ContentRect.X, "Row items horizontal in column parent");
        }

        [Fact]
        public void Flex_MinHeight_Auto_Column()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; flex-direction: column; height: 50px; width: 100px;'>
                    <div id='item' style='flex-shrink: 1; min-height: 0;'>
                        <div style='height: 200px;'></div>
                    </div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.h={item!.ContentRect.Height}");
            Assert.True(item.ContentRect.Height <= 51,
                $"min-height:0 allows shrink (got {item.ContentRect.Height})");
        }

        [Fact]
        public void Flex_AbsPos_Child_NotFlexItem()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; position: relative; width: 200px;'>
                    <div style='width: 50px; height: 30px;'></div>
                    <div id='abs' style='position: absolute; top: 0; right: 0; width: 30px; height: 30px;'></div>
                    <div id='normal' style='width: 50px; height: 30px;'></div>
                </div></body>");
            var normal = LayoutTestHelper.FindById(root, "normal");
            Assert.NotNull(normal);
            _output.WriteLine($"normal.X={normal!.ContentRect.X}");
            // Abspos child doesn't participate in flex layout — normal is 2nd flex item at X=50
            Assert.True(System.Math.Abs(normal.ContentRect.X - 50) < 2,
                $"Abspos doesn't affect flex (normal.X={normal.ContentRect.X})");
        }

        [Fact]
        public void Flex_Order_VisualReorder()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display: flex; width: 200px;'>
                    <div id='a' style='order: 3; width: 50px; height: 30px;'></div>
                    <div id='b' style='order: 1; width: 50px; height: 30px;'></div>
                    <div id='c' style='order: 2; width: 50px; height: 30px;'></div>
                </div></body>");
            var a = LayoutTestHelper.FindById(root, "a");
            var b = LayoutTestHelper.FindById(root, "b");
            var c = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(a);
            Assert.NotNull(b);
            Assert.NotNull(c);
            // Visual order: b(1) c(2) a(3)
            Assert.True(b!.ContentRect.X < c!.ContentRect.X, "B before C");
            Assert.True(c.ContentRect.X < a!.ContentRect.X, "C before A");
        }
    }
}
