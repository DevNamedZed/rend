using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptBlockWidthPercentContextTests
    {
        private readonly ITestOutputHelper _output;
        public WptBlockWidthPercentContextTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void Width50PercentOf400Equals200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:50%;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"50% of 400 should be 200, got {target.ContentRect.Width}");
        }

        [Fact]
        public void Width50PercentOf300Equals150()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='width:50%;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 1,
                $"50% of 300 should be 150, got {target.ContentRect.Width}");
        }

        [Fact]
        public void Width50PercentOf200Equals100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='t' style='width:50%;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 1,
                $"50% of 200 should be 100, got {target.ContentRect.Width}");
        }

        [Fact]
        public void Width25PercentOf400Equals100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:25%;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 1,
                $"25% of 400 should be 100, got {target.ContentRect.Width}");
        }

        [Fact]
        public void Width75PercentOf400Equals300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:75%;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 1,
                $"75% of 400 should be 300, got {target.ContentRect.Width}");
        }

        [Fact]
        public void Width100PercentOf300Equals300()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:300px'>
                    <div id='t' style='width:100%;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 1,
                $"100% of 300 should be 300, got {target.ContentRect.Width}");
        }

        [Fact]
        public void Width10PercentOf400Equals40()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:10%;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 40) < 1,
                $"10% of 400 should be 40, got {target.ContentRect.Width}");
        }

        [Fact]
        public void Nested50PercentOf50PercentEquals25Percent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div style='width:50%'>
                        <div id='t' style='width:50%;height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 1,
                $"50% of 50% of 400 should be 100, got {target.ContentRect.Width}");
        }

        [Fact]
        public void TripleNested50PercentEquals12Point5Percent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div style='width:50%'>
                        <div style='width:50%'>
                            <div id='t' style='width:50%;height:20px'></div>
                        </div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 50) < 1,
                $"50% of 50% of 50% of 400 should be 50, got {target.ContentRect.Width}");
        }

        [Fact]
        public void PercentWidthInsideFlexItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div style='width:200px;height:40px'>
                        <div id='t' style='width:50%;height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 1,
                $"50% of 200px flex item should be 100, got {target.ContentRect.Width}");
        }

        [Fact]
        public void PercentWidthInsideGridItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:300px;width:300px'>
                    <div>
                        <div id='t' style='width:50%;height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 1,
                $"50% of 300px grid track should be 150, got {target.ContentRect.Width}");
        }

        [Fact]
        public void PercentWidthOnFloat()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='float:left;width:50%;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"50% float of 400 should be 200, got {target.ContentRect.Width}");
        }

        [Fact]
        public void PercentWidthOnInlineBlock()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='display:inline-block;width:50%;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"50% inline-block of 400 should be 200, got {target.ContentRect.Width}");
        }

        [Fact]
        public void PercentWidthOnAbsolutelyPositioned()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div id='t' style='position:absolute;width:50%;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"50% abspos of 400 should be 200, got {target.ContentRect.Width}");
        }

        [Fact]
        public void CalcWidth50PercentMinus20px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:calc(50% - 20px);height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 1,
                $"calc(50% - 20px) of 400 should be 180, got {target.ContentRect.Width}");
        }

        [Fact]
        public void CalcWidth25PercentPlus50px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:calc(25% + 50px);height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 1,
                $"calc(25% + 50px) of 400 should be 150, got {target.ContentRect.Width}");
        }

        [Fact]
        public void PercentWidthWithPaddingOnParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;padding:0 50px'>
                    <div id='t' style='width:50%;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"50% of 400px content-box (parent has padding) should be 200, got {target.ContentRect.Width}");
        }

        [Fact]
        public void PercentWidthWithBorderOnParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;border:10px solid black'>
                    <div id='t' style='width:50%;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"50% of 400px content-box (parent has border) should be 200, got {target.ContentRect.Width}");
        }

        [Fact]
        public void PercentWidthWithBorderBoxOnParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='box-sizing:border-box;width:400px;padding:0 50px'>
                    <div id='t' style='width:50%;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 1,
                $"50% of 300px content-box (parent border-box 400px with 50px padding each side) should be 150, got {target.ContentRect.Width}");
        }

        [Fact]
        public void MarginPercentageResolvesAgainstParentWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0;overflow:hidden'>
                <div style='width:400px'>
                    <div id='t' style='margin-left:25%;width:50px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.X - 100) < 1,
                $"margin-left 25% of 400 should position at X=100, got {target.ContentRect.X}");
        }

        [Fact]
        public void PaddingPercentageResolvesAgainstParentWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='padding-left:10%;width:50px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.PaddingLeft - 40) < 1,
                $"padding-left 10% of 400 should be 40, got {target.PaddingLeft}");
        }

        [Fact]
        public void TwoSiblings50PercentEach()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='width:50%;height:20px'></div>
                    <div id='b' style='width:50%;height:20px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            Assert.True(System.Math.Abs(first.ContentRect.Width - 200) < 1,
                $"First 50% should be 200, got {first.ContentRect.Width}");
            Assert.True(System.Math.Abs(second.ContentRect.Width - 200) < 1,
                $"Second 50% should be 200, got {second.ContentRect.Width}");
        }

        [Fact]
        public void ThreeSiblings33PercentEach()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='width:33.333%;height:20px'></div>
                    <div id='b' style='width:33.333%;height:20px'></div>
                    <div id='c' style='width:33.333%;height:20px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "a")!;
            var second = LayoutTestHelper.FindById(root, "b")!;
            var third = LayoutTestHelper.FindById(root, "c")!;
            Assert.True(System.Math.Abs(first.ContentRect.Width - 100) < 1,
                $"First 33.333% of 300 should be ~100, got {first.ContentRect.Width}");
            Assert.True(System.Math.Abs(second.ContentRect.Width - 100) < 1,
                $"Second 33.333% of 300 should be ~100, got {second.ContentRect.Width}");
            Assert.True(System.Math.Abs(third.ContentRect.Width - 100) < 1,
                $"Third 33.333% of 300 should be ~100, got {third.ContentRect.Width}");
        }

        [Fact]
        public void Width0PercentCollapsesToZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:0%;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(target.ContentRect.Width < 1,
                $"0% width should be 0, got {target.ContentRect.Width}");
        }

        [Fact]
        public void Width100PercentFillsParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:100%;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 400) < 1,
                $"100% of 400 should be 400, got {target.ContentRect.Width}");
        }

        [Fact]
        public void Width150PercentOverflows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px;overflow:hidden'>
                    <div id='t' style='width:150%;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 600) < 1,
                $"150% of 400 should be 600, got {target.ContentRect.Width}");
        }

        [Fact]
        public void PercentWidthOnFlexItemDirectChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='t' style='width:50%;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"50% flex item of 400 should be 200, got {target.ContentRect.Width}");
        }

        [Fact]
        public void PercentWidthOnGridItemDirectChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr;width:400px'>
                    <div id='t' style='width:50%;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 200) < 1,
                $"50% grid item of 400 should be 200, got {target.ContentRect.Width}");
        }

        [Fact]
        public void PercentWidthInsideFloatContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div style='float:left;width:200px'>
                        <div id='t' style='width:50%;height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 1,
                $"50% inside 200px float should be 100, got {target.ContentRect.Width}");
        }

        [Fact]
        public void PercentWidthInsideInlineBlockContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div style='display:inline-block;width:200px'>
                        <div id='t' style='width:50%;height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 100) < 1,
                $"50% inside 200px inline-block should be 100, got {target.ContentRect.Width}");
        }

        [Fact]
        public void PercentWidthInsideAbsposContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='position:relative;width:400px;height:200px'>
                    <div style='position:absolute;width:300px;height:100px'>
                        <div id='t' style='width:50%;height:20px'></div>
                    </div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 150) < 1,
                $"50% inside 300px abspos should be 150, got {target.ContentRect.Width}");
        }

        [Fact]
        public void PercentWidthWithBorderBoxOnChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='box-sizing:border-box;width:50%;padding:0 20px;border:5px solid;height:30px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            float totalWidth = target.ContentRect.Width + target.PaddingLeft + target.PaddingRight
                + target.BorderLeftWidth + target.BorderRightWidth;
            Assert.True(System.Math.Abs(totalWidth - 200) < 1,
                $"border-box 50% of 400 should have total width 200, got {totalWidth}");
        }

        [Fact]
        public void MarginTopPercentageResolvesAgainstWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0;overflow:hidden'>
                <div style='width:400px;height:400px'>
                    <div id='t' style='margin-top:10%;width:50px;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.MarginTop - 40) < 1,
                $"margin-top 10% of 400px width should be 40, got {target.MarginTop}");
        }

        [Fact]
        public void PaddingTopPercentageResolvesAgainstWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='padding-top:10%;height:0'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.PaddingTop - 40) < 1,
                $"padding-top 10% of 400px width should be 40, got {target.PaddingTop}");
        }

        [Fact]
        public void PercentWidthChangesWithDifferentViewportWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='t' style='width:50%;height:20px'></div>
            </body>", viewportWidth: 600);
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 300) < 1,
                $"50% of 600px viewport should be 300, got {target.ContentRect.Width}");
        }

        [Fact]
        public void PercentWidthWithBorderBoxParentAndBorderOnParent()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='box-sizing:border-box;width:400px;border:20px solid black'>
                    <div id='t' style='width:50%;height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 180) < 1,
                $"50% of 360px content-box (parent border-box 400px with 20px border each side) should be 180, got {target.ContentRect.Width}");
        }

        [Fact]
        public void CalcWidth100PercentMinus40px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:400px'>
                    <div id='t' style='width:calc(100% - 40px);height:20px'></div>
                </div></body>");
            var target = LayoutTestHelper.FindById(root, "t")!;
            Assert.True(System.Math.Abs(target.ContentRect.Width - 360) < 1,
                $"calc(100% - 40px) of 400 should be 360, got {target.ContentRect.Width}");
        }
    }
}
