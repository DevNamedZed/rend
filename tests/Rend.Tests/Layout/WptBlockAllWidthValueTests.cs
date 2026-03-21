using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Exhaustive block width value tests: auto, fixed px, percentages,
    /// calc(), em, vw, intrinsic keywords, and zero width.
    /// All tests use a 400px containing block (default viewport).
    /// </summary>
    public class WptBlockAllWidthValueTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockAllWidthValueTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void WidthAuto_FillsContainingBlock400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2,
                $"Auto width should fill 400px container (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width50px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:50px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 50) < 2,
                $"width:50px (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width100px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:100px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2,
                $"width:100px (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width150px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:150px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 150) < 2,
                $"width:150px (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width200px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:200px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"width:200px (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width250px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:250px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 250) < 2,
                $"width:250px (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:300px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 2,
                $"width:300px (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width350px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:350px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 350) < 2,
                $"width:350px (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width400px_MatchesContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:400px;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2,
                $"width:400px should match container (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width10Percent_Of400_Is40()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:10%;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 40) < 2,
                $"10% of 400 = 40 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width20Percent_Of400_Is80()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:20%;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 80) < 2,
                $"20% of 400 = 80 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width25Percent_Of400_Is100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:25%;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2,
                $"25% of 400 = 100 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width30Percent_Of400_Is120()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:30%;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 120) < 2,
                $"30% of 400 = 120 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width40Percent_Of400_Is160()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:40%;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 160) < 2,
                $"40% of 400 = 160 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width50Percent_Of400_Is200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:50%;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"50% of 400 = 200 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width60Percent_Of400_Is240()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:60%;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 240) < 2,
                $"60% of 400 = 240 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width70Percent_Of400_Is280()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:70%;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 280) < 2,
                $"70% of 400 = 280 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width75Percent_Of400_Is300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:75%;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 300) < 2,
                $"75% of 400 = 300 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width80Percent_Of400_Is320()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:80%;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 320) < 2,
                $"80% of 400 = 320 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width90Percent_Of400_Is360()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:90%;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 360) < 2,
                $"90% of 400 = 360 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width100Percent_Of400_Is400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:100%;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2,
                $"100% of 400 = 400 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void WidthCalc_50PercentMinus20px_Is180()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:calc(50% - 20px);height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 180) < 2,
                $"calc(50% - 20px) of 400 = 180 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void WidthCalc_100PercentMinus60px_Is340()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:calc(100% - 60px);height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 340) < 2,
                $"calc(100% - 60px) of 400 = 340 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width10em_At16px_Is160()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0;font-size:16px'>
                <div style='width:400px'>
                    <div id='test' style='width:10em;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 160) < 2,
                $"10em at 16px = 160 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width50vw_AtViewport400_Is200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:50vw;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"50vw at viewport 400 = 200 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void WidthMinContent_MatchesWidestChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:min-content'>
                        <div style='width:90px;height:10px'></div>
                        <div style='width:130px;height:10px'></div>
                        <div style='width:70px;height:10px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 130) < 2,
                $"min-content = widest child 130 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void WidthMaxContent_MatchesWidestChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:max-content'>
                        <div style='width:90px;height:10px'></div>
                        <div style='width:180px;height:10px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 180) < 2,
                $"max-content = widest child 180 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void WidthFitContent_ClampedByContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:100px'>
                    <div id='test' style='width:fit-content'>
                        <div style='width:200px;height:10px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(box.ContentRect.Width >= 99,
                $"fit-content with 200px child in 100px container (got {box.ContentRect.Width})");
        }

        [Fact]
        public void WidthFitContent_ShrinkToContent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:fit-content'>
                        <div style='width:120px;height:10px'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 120) < 2,
                $"fit-content shrinks to 120px child (got {box.ContentRect.Width})");
        }

        [Fact]
        public void WidthZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:0;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(box.ContentRect.Width < 1,
                $"width:0 should be zero (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width50Percent_NestedContainer200_Is100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='test' style='width:50%;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2,
                $"50% of 200 = 100 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void WidthCalc_50PercentPlus50px_Of400_Is250()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:calc(50% + 50px);height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 250) < 2,
                $"calc(50% + 50px) of 400 = 250 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width100vw_MatchesViewport400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='test' style='width:100vw;height:20px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 400) < 2,
                $"100vw = viewport 400 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width5em_At16px_Is80()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0;font-size:16px'>
                <div style='width:400px'>
                    <div id='test' style='width:5em;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 80) < 2,
                $"5em at 16px = 80 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width2em_InheritedFontSize24_Is48()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;font-size:24px'>
                    <div id='test' style='width:2em;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 48) < 2,
                $"2em at inherited 24px = 48 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void Width25vw_AtViewport400_Is100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:25vw;height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 100) < 2,
                $"25vw at viewport 400 = 100 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void WidthCalc_25PercentPlus25Percent_Of400_Is200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:calc(25% + 25%);height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"calc(25% + 25%) of 400 = 200 (got {box.ContentRect.Width})");
        }

        [Fact]
        public void WidthCalc_100pxPlus100px_Is200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='test' style='width:calc(100px + 100px);height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            _output.WriteLine($"w={box!.ContentRect.Width}");
            Assert.True(System.Math.Abs(box.ContentRect.Width - 200) < 2,
                $"calc(100px + 100px) = 200 (got {box.ContentRect.Width})");
        }
    }
}
