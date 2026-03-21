using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptGridEqualFrTests
    {
        private readonly ITestOutputHelper _output;

        public WptGridEqualFrTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void OneColumn_Width200_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;width:200px'>
                    <div id='item' style='height:20px'></div>
                  </div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 1);
        }

        [Fact]
        public void OneColumn_Width300_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;width:300px'>
                    <div id='item' style='height:20px'></div>
                  </div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 1);
        }

        [Fact]
        public void OneColumn_Width400_FillsContainer()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr;width:400px'>
                    <div id='item' style='height:20px'></div>
                  </div></body>");

            var item = LayoutTestHelper.FindById(root, "item")!;
            Assert.True(System.Math.Abs(item.ContentRect.Width - 400) < 1);
        }

        [Fact]
        public void TwoColumns_Width200_EachGets100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:200px'>
                    <div id='colA' style='height:20px'></div>
                    <div id='colB' style='height:20px'></div>
                  </div></body>");

            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colB = LayoutTestHelper.FindById(root, "colB")!;
            Assert.True(System.Math.Abs(colA.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(colB.ContentRect.Width - 100) < 1);
        }

        [Fact]
        public void TwoColumns_Width300_EachGets150()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:300px'>
                    <div id='colA' style='height:20px'></div>
                    <div id='colB' style='height:20px'></div>
                  </div></body>");

            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colB = LayoutTestHelper.FindById(root, "colB")!;
            Assert.True(System.Math.Abs(colA.ContentRect.Width - 150) < 1);
            Assert.True(System.Math.Abs(colB.ContentRect.Width - 150) < 1);
        }

        [Fact]
        public void TwoColumns_Width400_EachGets200()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <div id='colA' style='height:20px'></div>
                    <div id='colB' style='height:20px'></div>
                  </div></body>");

            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colB = LayoutTestHelper.FindById(root, "colB")!;
            Assert.True(System.Math.Abs(colA.ContentRect.Width - 200) < 1);
            Assert.True(System.Math.Abs(colB.ContentRect.Width - 200) < 1);
        }

        [Fact]
        public void TwoColumns_Width600_EachGets300()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:600px'>
                    <div id='colA' style='height:20px'></div>
                    <div id='colB' style='height:20px'></div>
                  </div></body>",
                viewportWidth: 600);

            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colB = LayoutTestHelper.FindById(root, "colB")!;
            Assert.True(System.Math.Abs(colA.ContentRect.Width - 300) < 1);
            Assert.True(System.Math.Abs(colB.ContentRect.Width - 300) < 1);
        }

        [Fact]
        public void ThreeColumns_Width300_EachGets100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:300px'>
                    <div id='colA' style='height:20px'></div>
                    <div id='colB' style='height:20px'></div>
                    <div id='colC' style='height:20px'></div>
                  </div></body>");

            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colB = LayoutTestHelper.FindById(root, "colB")!;
            var colC = LayoutTestHelper.FindById(root, "colC")!;
            Assert.True(System.Math.Abs(colA.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(colB.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(colC.ContentRect.Width - 100) < 1);
        }

        [Fact]
        public void ThreeColumns_Width600_EachGets200()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:600px'>
                    <div id='colA' style='height:20px'></div>
                    <div id='colB' style='height:20px'></div>
                    <div id='colC' style='height:20px'></div>
                  </div></body>",
                viewportWidth: 600);

            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colB = LayoutTestHelper.FindById(root, "colB")!;
            var colC = LayoutTestHelper.FindById(root, "colC")!;
            Assert.True(System.Math.Abs(colA.ContentRect.Width - 200) < 1);
            Assert.True(System.Math.Abs(colB.ContentRect.Width - 200) < 1);
            Assert.True(System.Math.Abs(colC.ContentRect.Width - 200) < 1);
        }

        [Fact]
        public void FourColumns_Width400_EachGets100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(4,1fr);width:400px'>
                    <div id='colA' style='height:20px'></div>
                    <div id='colB' style='height:20px'></div>
                    <div id='colC' style='height:20px'></div>
                    <div id='colD' style='height:20px'></div>
                  </div></body>");

            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colD = LayoutTestHelper.FindById(root, "colD")!;
            Assert.True(System.Math.Abs(colA.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(colD.ContentRect.Width - 100) < 1);
        }

        [Fact]
        public void FourColumns_Width600_EachGets150()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(4,1fr);width:600px'>
                    <div id='colA' style='height:20px'></div>
                    <div id='colB' style='height:20px'></div>
                    <div id='colC' style='height:20px'></div>
                    <div id='colD' style='height:20px'></div>
                  </div></body>",
                viewportWidth: 600);

            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colD = LayoutTestHelper.FindById(root, "colD")!;
            Assert.True(System.Math.Abs(colA.ContentRect.Width - 150) < 1);
            Assert.True(System.Math.Abs(colD.ContentRect.Width - 150) < 1);
        }

        [Fact]
        public void FiveColumns_Width400_EachGets80()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(5,1fr);width:400px'>
                    <div id='colA' style='height:20px'></div>
                    <div id='colB' style='height:20px'></div>
                    <div id='colC' style='height:20px'></div>
                    <div id='colD' style='height:20px'></div>
                    <div id='colE' style='height:20px'></div>
                  </div></body>");

            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colE = LayoutTestHelper.FindById(root, "colE")!;
            Assert.True(System.Math.Abs(colA.ContentRect.Width - 80) < 1);
            Assert.True(System.Math.Abs(colE.ContentRect.Width - 80) < 1);
        }

        [Fact]
        public void FiveColumns_Width500_EachGets100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(5,1fr);width:500px'>
                    <div id='colA' style='height:20px'></div>
                    <div id='colB' style='height:20px'></div>
                    <div id='colC' style='height:20px'></div>
                    <div id='colD' style='height:20px'></div>
                    <div id='colE' style='height:20px'></div>
                  </div></body>",
                viewportWidth: 500);

            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colE = LayoutTestHelper.FindById(root, "colE")!;
            Assert.True(System.Math.Abs(colA.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(colE.ContentRect.Width - 100) < 1);
        }

        [Fact]
        public void SixColumns_Width600_EachGets100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(6,1fr);width:600px'>
                    <div id='colA' style='height:20px'></div>
                    <div id='colB' style='height:20px'></div>
                    <div id='colC' style='height:20px'></div>
                    <div id='colD' style='height:20px'></div>
                    <div id='colE' style='height:20px'></div>
                    <div id='colF' style='height:20px'></div>
                  </div></body>",
                viewportWidth: 600);

            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colF = LayoutTestHelper.FindById(root, "colF")!;
            Assert.True(System.Math.Abs(colA.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(colF.ContentRect.Width - 100) < 1);
        }

        [Fact]
        public void TwoColumns_Gap20_Width220_EachGets100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;column-gap:20px;width:220px'>
                    <div id='colA' style='height:20px'></div>
                    <div id='colB' style='height:20px'></div>
                  </div></body>");

            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colB = LayoutTestHelper.FindById(root, "colB")!;
            Assert.True(System.Math.Abs(colA.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(colB.ContentRect.Width - 100) < 1);
        }

        [Fact]
        public void ThreeColumns_Gap10_Width320_EachGets100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;column-gap:10px;width:320px'>
                    <div id='colA' style='height:20px'></div>
                    <div id='colB' style='height:20px'></div>
                    <div id='colC' style='height:20px'></div>
                  </div></body>");

            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colB = LayoutTestHelper.FindById(root, "colB")!;
            var colC = LayoutTestHelper.FindById(root, "colC")!;
            Assert.True(System.Math.Abs(colA.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(colB.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(colC.ContentRect.Width - 100) < 1);
        }

        [Fact]
        public void FourColumns_Gap20_Width460_EachGets100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(4,1fr);column-gap:20px;width:460px'>
                    <div id='colA' style='height:20px'></div>
                    <div id='colB' style='height:20px'></div>
                    <div id='colC' style='height:20px'></div>
                    <div id='colD' style='height:20px'></div>
                  </div></body>",
                viewportWidth: 460);

            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colD = LayoutTestHelper.FindById(root, "colD")!;
            Assert.True(System.Math.Abs(colA.ContentRect.Width - 100) < 1);
            Assert.True(System.Math.Abs(colD.ContentRect.Width - 100) < 1);
        }

        [Fact]
        public void TwoRows_Height200_EachGets100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:1fr 1fr;width:200px;height:200px'>
                    <div id='rowA'></div>
                    <div id='rowB'></div>
                  </div></body>");

            var rowA = LayoutTestHelper.FindById(root, "rowA")!;
            var rowB = LayoutTestHelper.FindById(root, "rowB")!;
            Assert.True(System.Math.Abs(rowA.ContentRect.Height - 100) < 1);
            Assert.True(System.Math.Abs(rowB.ContentRect.Height - 100) < 1);
        }

        [Fact]
        public void ThreeRows_Height300_EachGets100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:1fr 1fr 1fr;width:200px;height:300px'>
                    <div id='rowA'></div>
                    <div id='rowB'></div>
                    <div id='rowC'></div>
                  </div></body>");

            var rowA = LayoutTestHelper.FindById(root, "rowA")!;
            var rowB = LayoutTestHelper.FindById(root, "rowB")!;
            var rowC = LayoutTestHelper.FindById(root, "rowC")!;
            Assert.True(System.Math.Abs(rowA.ContentRect.Height - 100) < 1);
            Assert.True(System.Math.Abs(rowB.ContentRect.Height - 100) < 1);
            Assert.True(System.Math.Abs(rowC.ContentRect.Height - 100) < 1);
        }

        [Fact]
        public void FourRows_Height400_EachGets100()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:repeat(4,1fr);width:200px;height:400px'>
                    <div id='rowA'></div>
                    <div id='rowB'></div>
                    <div id='rowC'></div>
                    <div id='rowD'></div>
                  </div></body>");

            var rowA = LayoutTestHelper.FindById(root, "rowA")!;
            var rowD = LayoutTestHelper.FindById(root, "rowD")!;
            Assert.True(System.Math.Abs(rowA.ContentRect.Height - 100) < 1);
            Assert.True(System.Math.Abs(rowD.ContentRect.Height - 100) < 1);
        }

        [Fact]
        public void TwoColumns_XPositions_Width400()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr;width:400px'>
                    <div id='colA' style='height:20px'></div>
                    <div id='colB' style='height:20px'></div>
                  </div></body>");

            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colB = LayoutTestHelper.FindById(root, "colB")!;
            Assert.True(System.Math.Abs(colA.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(colB.ContentRect.X - 200) < 1);
        }

        [Fact]
        public void ThreeColumns_XPositions_Width300()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:1fr 1fr 1fr;width:300px'>
                    <div id='colA' style='height:20px'></div>
                    <div id='colB' style='height:20px'></div>
                    <div id='colC' style='height:20px'></div>
                  </div></body>");

            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colB = LayoutTestHelper.FindById(root, "colB")!;
            var colC = LayoutTestHelper.FindById(root, "colC")!;
            Assert.True(System.Math.Abs(colA.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(colB.ContentRect.X - 100) < 1);
            Assert.True(System.Math.Abs(colC.ContentRect.X - 200) < 1);
        }

        [Fact]
        public void FourColumns_XPositions_Width400()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(4,1fr);width:400px'>
                    <div id='colA' style='height:20px'></div>
                    <div id='colB' style='height:20px'></div>
                    <div id='colC' style='height:20px'></div>
                    <div id='colD' style='height:20px'></div>
                  </div></body>");

            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colB = LayoutTestHelper.FindById(root, "colB")!;
            var colC = LayoutTestHelper.FindById(root, "colC")!;
            var colD = LayoutTestHelper.FindById(root, "colD")!;
            Assert.True(System.Math.Abs(colA.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(colB.ContentRect.X - 100) < 1);
            Assert.True(System.Math.Abs(colC.ContentRect.X - 200) < 1);
            Assert.True(System.Math.Abs(colD.ContentRect.X - 300) < 1);
        }

        [Fact]
        public void FiveColumns_XPositions_Width500()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:repeat(5,1fr);width:500px'>
                    <div id='colA' style='height:20px'></div>
                    <div id='colB' style='height:20px'></div>
                    <div id='colC' style='height:20px'></div>
                    <div id='colD' style='height:20px'></div>
                    <div id='colE' style='height:20px'></div>
                  </div></body>",
                viewportWidth: 500);

            var colA = LayoutTestHelper.FindById(root, "colA")!;
            var colB = LayoutTestHelper.FindById(root, "colB")!;
            var colC = LayoutTestHelper.FindById(root, "colC")!;
            var colD = LayoutTestHelper.FindById(root, "colD")!;
            var colE = LayoutTestHelper.FindById(root, "colE")!;
            Assert.True(System.Math.Abs(colA.ContentRect.X - 0) < 1);
            Assert.True(System.Math.Abs(colB.ContentRect.X - 100) < 1);
            Assert.True(System.Math.Abs(colC.ContentRect.X - 200) < 1);
            Assert.True(System.Math.Abs(colD.ContentRect.X - 300) < 1);
            Assert.True(System.Math.Abs(colE.ContentRect.X - 400) < 1);
        }

        [Fact]
        public void TwoRows_YPositions_Height200()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:1fr 1fr;width:200px;height:200px'>
                    <div id='rowA'></div>
                    <div id='rowB'></div>
                  </div></body>");

            var rowA = LayoutTestHelper.FindById(root, "rowA")!;
            var rowB = LayoutTestHelper.FindById(root, "rowB")!;
            Assert.True(System.Math.Abs(rowA.ContentRect.Y - 0) < 1);
            Assert.True(System.Math.Abs(rowB.ContentRect.Y - 100) < 1);
        }

        [Fact]
        public void ThreeRows_YPositions_Height300()
        {
            var root = LayoutTestHelper.Layout(
                @"<body style='margin:0'><div style='display:grid;grid-template-columns:200px;grid-template-rows:1fr 1fr 1fr;width:200px;height:300px'>
                    <div id='rowA'></div>
                    <div id='rowB'></div>
                    <div id='rowC'></div>
                  </div></body>");

            var rowA = LayoutTestHelper.FindById(root, "rowA")!;
            var rowB = LayoutTestHelper.FindById(root, "rowB")!;
            var rowC = LayoutTestHelper.FindById(root, "rowC")!;
            Assert.True(System.Math.Abs(rowA.ContentRect.Y - 0) < 1);
            Assert.True(System.Math.Abs(rowB.ContentRect.Y - 100) < 1);
            Assert.True(System.Math.Abs(rowC.ContentRect.Y - 200) < 1);
        }
    }
}
