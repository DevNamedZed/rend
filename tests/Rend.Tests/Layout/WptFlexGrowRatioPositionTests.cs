using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Flex grow ratio distribution and resulting X/Y positions for
    /// row and column directions, with and without gap.
    /// </summary>
    public class WptFlexGrowRatioPositionTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexGrowRatioPositionTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX §9.7] 1:1 grow in 200px container => 100px each
        [Fact]
        public void Row_Grow1_1_Width200_EachIs100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:1 0 0px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:1 grow in 300px container => 150px each
        [Fact]
        public void Row_Grow1_1_Width300_EachIs150()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:1 0 0px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 150) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:1 grow in 400px container => 200px each
        [Fact]
        public void Row_Grow1_1_Width400_EachIs200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:1 0 0px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:2 grow in 300px => 100px and 200px
        [Fact]
        public void Row_Grow1_2_Width300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:2 0 0px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:2 grow in 600px => 200px and 400px
        [Fact]
        public void Row_Grow1_2_Width600()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:600px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:2 0 0px;height:20px'></div>
                </div></body>", viewportWidth: 800);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 400) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:3 grow in 400px => 100px and 300px
        [Fact]
        public void Row_Grow1_3_Width400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:3 0 0px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 300) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:3 grow in 800px => 200px and 600px
        [Fact]
        public void Row_Grow1_3_Width800()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:800px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:3 0 0px;height:20px'></div>
                </div></body>", viewportWidth: 1000);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 600) < 2);
        }

        // [CSS-FLEXBOX §9.7] 2:3 grow in 500px => 200px and 300px
        [Fact]
        public void Row_Grow2_3_Width500()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:500px'>
                    <div id='a' style='flex:2 0 0px;height:20px'></div>
                    <div id='b' style='flex:3 0 0px;height:20px'></div>
                </div></body>", viewportWidth: 600);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 300) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:1:2 grow in 400px => 100, 100, 200
        [Fact]
        public void Row_Grow1_1_2_Width400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:1 0 0px;height:20px'></div>
                    <div id='c' style='flex:2 0 0px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:2:3 grow in 600px => 100, 200, 300
        [Fact]
        public void Row_Grow1_2_3_Width600()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:600px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:2 0 0px;height:20px'></div>
                    <div id='c' style='flex:3 0 0px;height:20px'></div>
                </div></body>", viewportWidth: 800);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 300) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:1:1 grow in 300px => 100px each
        [Fact]
        public void Row_Grow1_1_1_Width300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:1 0 0px;height:20px'></div>
                    <div id='c' style='flex:1 0 0px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:1:1 grow in 600px => 200px each
        [Fact]
        public void Row_Grow1_1_1_Width600()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:600px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:1 0 0px;height:20px'></div>
                    <div id='c' style='flex:1 0 0px;height:20px'></div>
                </div></body>", viewportWidth: 800);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:1:1:1 grow in 400px => 100px each
        [Fact]
        public void Row_Grow1_1_1_1_Width400()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:1 0 0px;height:20px'></div>
                    <div id='c' style='flex:1 0 0px;height:20px'></div>
                    <div id='d' style='flex:1 0 0px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:1:1:1:1 grow in 500px => 100px each
        [Fact]
        public void Row_Grow1_1_1_1_1_Width500()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:500px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:1 0 0px;height:20px'></div>
                    <div id='c' style='flex:1 0 0px;height:20px'></div>
                    <div id='d' style='flex:1 0 0px;height:20px'></div>
                    <div id='e' style='flex:1 0 0px;height:20px'></div>
                </div></body>", viewportWidth: 600);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "e")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:1 in 200px: positions X=0, X=100
        [Fact]
        public void Row_Grow1_1_Width200_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:1 0 0px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:2 in 300px: positions X=0, X=100
        [Fact]
        public void Row_Grow1_2_Width300_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:2 0 0px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:3 in 400px: positions X=0, X=100
        [Fact]
        public void Row_Grow1_3_Width400_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:3 0 0px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:1:2 in 400px: positions X=0, 100, 200
        [Fact]
        public void Row_Grow1_1_2_Width400_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:1 0 0px;height:20px'></div>
                    <div id='c' style='flex:2 0 0px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 200) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:2:3 in 600px: positions X=0, 100, 300
        [Fact]
        public void Row_Grow1_2_3_Width600_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:600px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:2 0 0px;height:20px'></div>
                    <div id='c' style='flex:3 0 0px;height:20px'></div>
                </div></body>", viewportWidth: 800);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 300) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:1:1 in 300px: positions X=0, 100, 200
        [Fact]
        public void Row_Grow1_1_1_Width300_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:1 0 0px;height:20px'></div>
                    <div id='c' style='flex:1 0 0px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 200) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:1:1:1 in 400px: positions X=0, 100, 200, 300
        [Fact]
        public void Row_Grow1_1_1_1_Width400_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:1 0 0px;height:20px'></div>
                    <div id='c' style='flex:1 0 0px;height:20px'></div>
                    <div id='d' style='flex:1 0 0px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 300) < 2);
        }

        // [CSS-FLEXBOX §9.7] 1:1:1:1:1 in 500px: positions X=0, 100, 200, 300, 400
        [Fact]
        public void Row_Grow1_1_1_1_1_Width500_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:500px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:1 0 0px;height:20px'></div>
                    <div id='c' style='flex:1 0 0px;height:20px'></div>
                    <div id='d' style='flex:1 0 0px;height:20px'></div>
                    <div id='e' style='flex:1 0 0px;height:20px'></div>
                </div></body>", viewportWidth: 600);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 300) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "e")!.ContentRect.X - 400) < 2);
        }

        // [CSS-FLEXBOX §9.7] column 1:1 grow in 200px height => 100px each
        [Fact]
        public void Column_Grow1_1_Height200_EachIs100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:100px;height:200px'>
                    <div id='a' style='flex:1 0 0px'></div>
                    <div id='b' style='flex:1 0 0px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] column 1:1 grow in 400px height => 200px each
        [Fact]
        public void Column_Grow1_1_Height400_EachIs200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:100px;height:400px'>
                    <div id='a' style='flex:1 0 0px'></div>
                    <div id='b' style='flex:1 0 0px'></div>
                </div></body>", viewportHeight: 500);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Height - 200) < 2);
        }

        // [CSS-FLEXBOX §9.7] column 1:2 grow in 300px height => 100 and 200
        [Fact]
        public void Column_Grow1_2_Height300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:100px;height:300px'>
                    <div id='a' style='flex:1 0 0px'></div>
                    <div id='b' style='flex:2 0 0px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Height - 200) < 2);
        }

        // [CSS-FLEXBOX §9.7] column 1:1 in 200px: positions Y=0, Y=100
        [Fact]
        public void Column_Grow1_1_Height200_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:100px;height:200px'>
                    <div id='a' style='flex:1 0 0px'></div>
                    <div id='b' style='flex:1 0 0px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] column 1:1 in 400px: positions Y=0, Y=200
        [Fact]
        public void Column_Grow1_1_Height400_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:100px;height:400px'>
                    <div id='a' style='flex:1 0 0px'></div>
                    <div id='b' style='flex:1 0 0px'></div>
                </div></body>", viewportHeight: 500);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 200) < 2);
        }

        // [CSS-FLEXBOX §9.7] column 1:2 in 300px: positions Y=0, Y=100
        [Fact]
        public void Column_Grow1_2_Height300_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:100px;height:300px'>
                    <div id='a' style='flex:1 0 0px'></div>
                    <div id='b' style='flex:2 0 0px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 100) < 2);
        }

        // [CSS-FLEXBOX §8.2] row 1:1 with gap:20px in 220px => 100px each
        [Fact]
        public void Row_Grow1_1_WithGap_Width220()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:220px;gap:20px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:1 0 0px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §8.2] row 1:1 with gap:20px in 220px: positions X=0, X=120
        [Fact]
        public void Row_Grow1_1_WithGap_Width220_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:220px;gap:20px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:1 0 0px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 120) < 2);
        }

        // [CSS-FLEXBOX §8.2] row 1:2 with gap:20px in 320px => 100 and 200
        [Fact]
        public void Row_Grow1_2_WithGap_Width320()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:320px;gap:20px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:2 0 0px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 200) < 2);
        }

        // [CSS-FLEXBOX §8.2] row 1:2 with gap:20px in 320px: positions X=0, X=120
        [Fact]
        public void Row_Grow1_2_WithGap_Width320_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:320px;gap:20px'>
                    <div id='a' style='flex:1 0 0px;height:20px'></div>
                    <div id='b' style='flex:2 0 0px;height:20px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 120) < 2);
        }

        // [CSS-FLEXBOX §9.7] 2:3 in 500px: positions X=0, X=200
        [Fact]
        public void Row_Grow2_3_Width500_Positions()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:500px'>
                    <div id='a' style='flex:2 0 0px;height:20px'></div>
                    <div id='b' style='flex:3 0 0px;height:20px'></div>
                </div></body>", viewportWidth: 600);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 200) < 2);
        }
    }
}
