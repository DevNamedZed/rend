using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexItemAspectRatioTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexItemAspectRatioTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void FlexItem_AspectRatio_2_1_WithWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:400px'>
                    <div id='t' style='width:100px;aspect-ratio:2/1'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"Expected width ~100, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2,
                $"Expected height ~50, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_1_1_Square()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:400px'>
                    <div id='t' style='width:80px;aspect-ratio:1/1'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2,
                $"Expected height ~80, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_1_2_Portrait()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:400px'>
                    <div id='t' style='width:60px;aspect-ratio:1/2'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 120) < 2,
                $"Expected height ~120, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_16_9()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:400px'>
                    <div id='t' style='width:320px;aspect-ratio:16/9'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 180) < 2,
                $"Expected height ~180, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_4_3()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:400px'>
                    <div id='t' style='width:120px;aspect-ratio:4/3'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 90) < 2,
                $"Expected height ~90, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_3_2()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:400px'>
                    <div id='t' style='width:150px;aspect-ratio:3/2'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Expected height ~100, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_FromHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:400px'>
                    <div id='t' style='height:100px;aspect-ratio:2/1'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"Expected width ~200, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Expected height ~100, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_InFlexRow()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;flex-direction:row;align-items:flex-start;width:400px'>
                    <div id='a' style='width:100px;aspect-ratio:2/1'></div>
                    <div id='b' style='width:100px;height:80px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a: w={itemA!.ContentRect.Width} h={itemA.ContentRect.Height}");
            _output.WriteLine($"b: w={itemB!.ContentRect.Width} h={itemB.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 50) < 2,
                $"Expected item A height ~50, got {itemA.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2,
                $"Expected item B at x~100, got {itemB.ContentRect.X}");
        }

        [Fact]
        public void FlexItem_AspectRatio_InFlexColumn()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;flex-direction:column;align-items:flex-start;width:400px'>
                    <div id='t' style='width:200px;aspect-ratio:2/1'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"Expected width ~200, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Expected height ~100, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_WithFlexGrow()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:300px'>
                    <div style='width:100px;height:50px'></div>
                    <div id='t' style='flex-grow:1;aspect-ratio:2/1'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"Expected width ~200, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Expected height ~100, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_WithFlexShrink()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:200px'>
                    <div id='t' style='width:300px;flex-shrink:1;aspect-ratio:2/1'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(target.ContentRect.Width <= 202,
                $"Expected width <=200, got {target.ContentRect.Width}");
            float expectedHeight = target.ContentRect.Width / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.Height - expectedHeight) < 2,
                $"Expected height ~{expectedHeight}, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_WithMaxHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:400px'>
                    <div id='t' style='width:200px;aspect-ratio:1/1;max-height:100px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(target.ContentRect.Height <= 101,
                $"Expected height <=100, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_WithMinHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:400px'>
                    <div id='t' style='width:100px;aspect-ratio:2/1;min-height:80px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(target.ContentRect.Height >= 79,
                $"Expected height >=80, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_WithMaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:400px'>
                    <div id='t' style='flex-grow:1;aspect-ratio:2/1;max-width:200px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(target.ContentRect.Width <= 202,
                $"Expected width <=200, got {target.ContentRect.Width}");
            float expectedHeight = target.ContentRect.Width / 2f;
            Assert.True(System.Math.Abs(target.ContentRect.Height - expectedHeight) < 2,
                $"Expected height ~{expectedHeight}, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_SingleNumber()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:400px'>
                    <div id='t' style='width:100px;aspect-ratio:2'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2,
                $"Expected height ~50, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_PercentageWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:400px'>
                    <div id='t' style='width:50%;aspect-ratio:2/1'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"Expected width ~200, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Expected height ~100, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_WithBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:400px'>
                    <div id='t' style='box-sizing:border-box;width:200px;aspect-ratio:2/1;padding:10px;border:5px solid black'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            float borderBoxWidth = target!.BorderRect.Width;
            float borderBoxHeight = target.BorderRect.Height;
            _output.WriteLine($"border-box: {borderBoxWidth}x{borderBoxHeight}, content: {target.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(System.Math.Abs(borderBoxWidth - 200) < 2,
                $"Expected border-box width ~200, got {borderBoxWidth}");
            Assert.True(System.Math.Abs(borderBoxHeight - 100) < 2,
                $"Expected border-box height ~100, got {borderBoxHeight}");
        }

        [Fact]
        public void FlexItem_AspectRatio_WithPadding()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:400px'>
                    <div id='t' style='width:200px;aspect-ratio:2/1;padding:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"content: {target!.ContentRect.Width}x{target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"Expected content width ~200, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Expected content height ~100, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_OnFloat()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='float:left;width:100px;aspect-ratio:2/1'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"Expected width ~100, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2,
                $"Expected height ~50, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_OnAbspos()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:400px;height:400px'>
                    <div id='t' style='position:absolute;width:200px;aspect-ratio:2/1'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"Expected width ~200, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Expected height ~100, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_OnInlineBlock()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:inline-block;width:100px;aspect-ratio:1/1'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"Expected width ~100, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Expected height ~100, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_Auto_NoEffect()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:400px'>
                    <div id='t' style='width:100px;height:80px;aspect-ratio:auto'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 2,
                $"Expected width ~100, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2,
                $"Expected height ~80, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_InGrid()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:grid;grid-template-columns:200px;width:200px'>
                    <div id='t' style='aspect-ratio:2/1'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"Expected width ~200, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Expected height ~100, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_FlexColumn_HeightDeterminesWidth()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;flex-direction:column;align-items:flex-start;width:400px;height:200px'>
                    <div id='t' style='height:100px;aspect-ratio:2/1'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"width={target!.ContentRect.Width} height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Expected height ~100, got {target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"Expected width ~200, got {target.ContentRect.Width}");
        }

        [Fact]
        public void FlexItem_AspectRatio_FlexGrow_DistributesEvenly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:400px'>
                    <div id='a' style='flex-grow:1;aspect-ratio:1/1'></div>
                    <div id='b' style='flex-grow:1;aspect-ratio:1/1'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a: {itemA!.ContentRect.Width}x{itemA.ContentRect.Height}");
            _output.WriteLine($"b: {itemB!.ContentRect.Width}x{itemB.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"Expected item A width ~200, got {itemA.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 200) < 2,
                $"Expected item A height ~200, got {itemA.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_WithBorder_ContentBox()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:400px'>
                    <div id='t' style='width:200px;aspect-ratio:2/1;border:10px solid black'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(target);
            _output.WriteLine($"content: {target!.ContentRect.Width}x{target.ContentRect.Height}");
            _output.WriteLine($"border: {target.BorderRect.Width}x{target.BorderRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 2,
                $"Expected content width ~200, got {target.ContentRect.Width}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2,
                $"Expected content height ~100, got {target.ContentRect.Height}");
        }

        [Fact]
        public void FlexItem_AspectRatio_MultipleItems_DifferentRatios()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:400px'>
                    <div id='a' style='width:100px;aspect-ratio:1/1'></div>
                    <div id='b' style='width:100px;aspect-ratio:2/1'></div>
                    <div id='c' style='width:100px;aspect-ratio:1/2'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"a: {itemA!.ContentRect.Width}x{itemA.ContentRect.Height}");
            _output.WriteLine($"b: {itemB!.ContentRect.Width}x{itemB.ContentRect.Height}");
            _output.WriteLine($"c: {itemC!.ContentRect.Width}x{itemC.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 100) < 2,
                $"Expected A height ~100, got {itemA.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 50) < 2,
                $"Expected B height ~50, got {itemB.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemC.ContentRect.Height - 200) < 2,
                $"Expected C height ~200, got {itemC.ContentRect.Height}");
        }
    }
}
