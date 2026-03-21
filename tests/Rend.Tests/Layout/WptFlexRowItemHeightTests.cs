using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for flex item HEIGHT behavior in row direction.
    /// Covers stretch, explicit height, alignment, percentages, min/max,
    /// box-sizing, auto margins, and interaction with padding/border.
    /// </summary>
    public class WptFlexRowItemHeightTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexRowItemHeightTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Stretch_Fills_100px_Container()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:100px;width:200px'>
                    <div id='t' style='width:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void Stretch_Fills_200px_Container()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:200px'>
                    <div id='t' style='width:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 200) < 2);
        }

        [Fact]
        public void Stretch_Fills_150px_Container()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:150px;width:200px'>
                    <div id='t' style='width:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 150) < 2);
        }

        [Fact]
        public void Explicit_Height_40_Overrides_Stretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:40px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 40) < 2);
        }

        [Fact]
        public void Explicit_Height_80_Overrides_Stretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:200px'>
                    <div id='t' style='width:50px;height:80px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void AlignItems_FlexStart_Preserves_Height_30()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height} Y={box.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 30) < 2);
            Assert.True(box.ContentRect.Y < 2);
        }

        [Fact]
        public void AlignItems_Center_Height_30_Centered()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:center;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height} Y={box.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 30) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Y - 35) < 2);
        }

        [Fact]
        public void AlignItems_FlexEnd_Height_30_AtBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-end;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height} Y={box.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 30) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Y - 70) < 2);
        }

        [Fact]
        public void AlignSelf_Center_Overrides_Stretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:100px;width:200px'>
                    <div id='t' style='align-self:center;width:50px;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height} Y={box.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 30) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Y - 35) < 2);
        }

        [Fact]
        public void AlignSelf_FlexEnd_Overrides_Stretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:100px;width:200px'>
                    <div id='t' style='align-self:flex-end;width:50px;height:30px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height} Y={box.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 30) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Y - 70) < 2);
        }

        [Fact]
        public void AlignSelf_Stretch_Fills_Container()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;height:100px;width:200px'>
                    <div id='t' style='align-self:stretch;width:50px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void Auto_Height_From_Content()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;height:200px;width:200px'>
                    <div id='t' style='width:50px'><div style='height:60px'></div></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2);
        }

        [Fact]
        public void Height_50_Percent_Of_200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:200px'>
                    <div id='t' style='width:50px;height:50%'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void Height_100_Percent_Fills_Container()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:200px'>
                    <div id='t' style='width:50px;height:100%'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 200) < 2);
        }

        [Fact]
        public void MinHeight_100_Enforced()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;height:200px;width:200px'>
                    <div id='t' style='width:50px;height:50px;min-height:100px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void MaxHeight_60_Clamps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:200px'>
                    <div id='t' style='width:50px;max-height:60px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height}");
            Assert.True(box.ContentRect.Height <= 61);
        }

        [Fact]
        public void Height_With_Padding_ContentBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;height:200px;width:200px'>
                    <div id='t' style='width:50px;height:80px;padding:10px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentHeight={box.ContentRect.Height} PaddingTop={box.PaddingTop} PaddingBottom={box.PaddingBottom}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2);
            float borderBoxHeight = box.ContentRect.Height + box.PaddingTop + box.PaddingBottom;
            Assert.True(System.Math.Abs(borderBoxHeight - 100) < 2);
        }

        [Fact]
        public void Height_BorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;height:200px;width:200px'>
                    <div id='t' style='width:50px;height:80px;padding:10px;box-sizing:border-box'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentHeight={box.ContentRect.Height} BorderBoxHeight={box.BorderRect.Height}");
            Assert.True(System.Math.Abs(box.BorderRect.Height - 80) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2);
        }

        [Fact]
        public void Height_Zero_With_FlexStart()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:0'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height}");
            Assert.True(box.ContentRect.Height < 2);
        }

        [Fact]
        public void MarginTop_Auto_Pushes_Down()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:30px;margin-top:auto'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={box.ContentRect.Y} Height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 70) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 30) < 2);
        }

        [Fact]
        public void MarginBottom_Auto_Pushes_Up()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:30px;margin-bottom:auto'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={box.ContentRect.Y} Height={box.ContentRect.Height}");
            Assert.True(box.ContentRect.Y < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 30) < 2);
        }

        [Fact]
        public void MarginTopBottom_Auto_Centers_Vertically()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:100px;width:200px'>
                    <div id='t' style='width:50px;height:30px;margin-top:auto;margin-bottom:auto'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Y={box.ContentRect.Y} Height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Y - 35) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 30) < 2);
        }

        [Fact]
        public void Stretch_With_Padding_Reduces_ContentHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:100px;width:200px'>
                    <div id='t' style='width:50px;padding:10px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentHeight={box.ContentRect.Height} PaddingTop={box.PaddingTop} PaddingBottom={box.PaddingBottom}");
            float totalHeight = box.ContentRect.Height + box.PaddingTop + box.PaddingBottom;
            Assert.True(System.Math.Abs(totalHeight - 100) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void Stretch_With_Border_Reduces_ContentHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:100px;width:200px'>
                    <div id='t' style='width:50px;border:5px solid black'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentHeight={box.ContentRect.Height} BorderTop={box.BorderTopWidth} BorderBottom={box.BorderBottomWidth}");
            float totalHeight = box.ContentRect.Height + box.BorderTopWidth + box.BorderBottomWidth;
            Assert.True(System.Math.Abs(totalHeight - 100) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 90) < 2);
        }

        [Fact]
        public void Stretch_With_Padding_And_Border()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:120px;width:200px'>
                    <div id='t' style='width:50px;padding:10px;border:5px solid black'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentHeight={box.ContentRect.Height} BorderBoxHeight={box.BorderRect.Height}");
            Assert.True(System.Math.Abs(box.BorderRect.Height - 120) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 90) < 2);
        }

        [Fact]
        public void MaxHeight_Clamps_Stretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:200px'>
                    <div id='t' style='width:50px;max-height:80px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height}");
            Assert.True(box.ContentRect.Height <= 81);
        }

        [Fact]
        public void MinHeight_Overrides_Explicit_Height()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;height:200px;width:200px'>
                    <div id='t' style='width:50px;height:30px;min-height:80px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"Height={box.ContentRect.Height}");
            Assert.True(System.Math.Abs(box.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void Height_BorderBox_With_Border()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;height:200px;width:200px'>
                    <div id='t' style='width:50px;height:100px;border:5px solid black;box-sizing:border-box'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentHeight={box.ContentRect.Height} BorderBoxHeight={box.BorderRect.Height}");
            Assert.True(System.Math.Abs(box.BorderRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 90) < 2);
        }

        [Fact]
        public void Height_BorderBox_With_Padding_And_Border()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;height:200px;width:200px'>
                    <div id='t' style='width:50px;height:100px;padding:10px;border:5px solid black;box-sizing:border-box'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentHeight={box.ContentRect.Height} BorderBoxHeight={box.BorderRect.Height}");
            Assert.True(System.Math.Abs(box.BorderRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 70) < 2);
        }
    }
}
