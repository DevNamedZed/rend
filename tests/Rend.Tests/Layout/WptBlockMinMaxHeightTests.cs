using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBlockMinMaxHeightTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockMinMaxHeightTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void MinHeight_50_Height30_MinWins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;height:30px;min-height:50px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void MinHeight_100_Height200_NoEffect()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;height:200px;min-height:100px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 200) < 2);
        }

        [Fact]
        public void MaxHeight_80_Height200_MaxWins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;height:200px;max-height:80px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void MaxHeight_200_Height100_NoEffect()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;height:100px;max-height:200px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void MinHeight_50_AutoHeight_Content30_MinWins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;min-height:50px'><div style='height:30px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void MinHeight_100_AutoHeight_Content80_MinWins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;min-height:100px'><div style='height:80px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void MaxHeight_50_AutoHeight_Content100_MaxWins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;max-height:50px'><div style='height:100px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void MinHeight_Percentage_ResolvesAgainstParent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='height:200px'><div id='t' style='width:100px;height:30px;min-height:50%'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void MaxHeight_Percentage_ResolvesAgainstParent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='height:400px'><div id='t' style='width:100px;height:300px;max-height:50%'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 200) < 2);
        }

        [Fact]
        public void MinMax_Both_HeightInMiddle()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;height:100px;min-height:50px;max-height:150px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 100) < 2);
        }

        [Fact]
        public void MinMax_Both_HeightBelowMin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;height:30px;min-height:50px;max-height:150px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void MinMax_Both_HeightAboveMax()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;height:200px;min-height:50px;max-height:150px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 150) < 2);
        }

        [Fact]
        public void MinHeight_OnFlexContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='display:flex;width:200px;min-height:120px'><div style='height:40px;width:50px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(target.ContentRect.Height >= 119);
        }

        [Fact]
        public void MaxHeight_OnFlexContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='display:flex;width:200px;max-height:60px'><div style='height:150px;width:50px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(target.ContentRect.Height <= 61);
        }

        [Fact]
        public void MinHeight_OnGridContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='display:grid;width:200px;min-height:120px'><div style='height:40px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(target.ContentRect.Height >= 119);
        }

        [Fact]
        public void MaxHeight_OnGridContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='display:grid;width:200px;max-height:60px'><div style='height:150px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(target.ContentRect.Height <= 61);
        }

        [Fact]
        public void MinHeight_Zero_NoEffect()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;height:80px;min-height:0'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void MaxHeight_9999_NoEffect()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;height:80px;max-height:9999px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void MinHeight_BorderBox_IncludesPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:100px;height:30px;min-height:100px;padding:10px;border:5px solid black'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxHeight = target.ContentRect.Height + 20 + 10;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}, BorderBoxHeight={borderBoxHeight}");
            Assert.True(System.Math.Abs(borderBoxHeight - 100) < 2);
        }

        [Fact]
        public void MaxHeight_BorderBox_IncludesPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='box-sizing:border-box;width:100px;height:200px;max-height:100px;padding:10px;border:5px solid black'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float borderBoxHeight = target.ContentRect.Height + 20 + 10;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}, BorderBoxHeight={borderBoxHeight}");
            Assert.True(System.Math.Abs(borderBoxHeight - 100) < 2);
        }

        [Fact]
        public void MinHeight_GreaterThanMaxHeight_MinWins()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;height:60px;min-height:200px;max-height:100px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(target.ContentRect.Height >= 199);
        }

        [Fact]
        public void MinHeight_OnEmptyDiv()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;min-height:75px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(target.ContentRect.Height >= 74);
        }

        [Fact]
        public void MaxHeight_OnEmptyDiv()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;height:0;max-height:50px'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(target.ContentRect.Height <= 1);
        }

        [Fact]
        public void MinHeight_Vh_ResolvesAgainstViewport()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;min-height:50vh'></div></body>",
                viewportWidth: 400, viewportHeight: 300);
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(target.ContentRect.Height >= 149);
        }

        [Fact]
        public void MaxHeight_Calc_Expression()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;height:200px;max-height:calc(50px + 30px)'></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void MinHeight_AutoHeight_ContentExceedsMin()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;min-height:50px'><div style='height:120px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 120) < 2);
        }

        [Fact]
        public void MaxHeight_AutoHeight_ContentBelowMax()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;max-height:200px'><div style='height:80px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 80) < 2);
        }

        [Fact]
        public void MinHeight_Percentage_NoParentHeight_Ignored()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div><div id='t' style='width:100px;min-height:50%'><div style='height:20px'></div></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(target.ContentRect.Height >= 19);
        }

        [Fact]
        public void MinHeight_FlexContainer_ChildStretches()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='display:flex;width:200px;min-height:100px;height:100px'><div id='child' style='width:50px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            var child = LayoutTestHelper.FindById(root, "child")!;
            _output.WriteLine($"Container.Height={target.ContentRect.Height}, Child.Height={child.ContentRect.Height}");
            Assert.True(target.ContentRect.Height >= 99);
            Assert.True(child.ContentRect.Height >= 99);
        }

        [Fact]
        public void MaxHeight_GridContainer_ClipsContent()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='display:grid;width:200px;max-height:40px;overflow:hidden'><div style='height:100px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(target.ContentRect.Height <= 41);
        }

        [Fact]
        public void MinHeight_Calc_PxOnly()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div id='t' style='width:100px;min-height:calc(40px + 30px)'><div style='height:10px'></div></div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            _output.WriteLine($"ContentRect.Height={target.ContentRect.Height}");
            Assert.True(System.Math.Abs(target.ContentRect.Height - 70) < 2);
        }
    }
}
