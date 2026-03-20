using Xunit;

namespace Rend.Tests.Layout
{
    public class IntrinsicSizingTests
    {
        [Fact]
        public void MinContent_Block_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='width: min-content; background: red;'>
                    <div>Hello World</div>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void MaxContent_Block_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='width: max-content; background: blue;'>
                    <div>Hello World</div>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void FitContent_Block_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='width: fit-content; background: green;'>
                    <div>Short</div>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void MinContent_Float_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='float: left; width: min-content; background: red;'>
                    <div>Hello World This is text</div>
                </div>
                <div>After float</div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void MaxContent_InlineBlock_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <span style='display: inline-block; width: max-content; background: blue;'>
                    Hello World
                </span>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void MinContent_GridItem_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <div style='display: grid; grid-template-columns: min-content 1fr;'>
                    <div style='background: red;'>Narrow</div>
                    <div style='background: blue;'>Fills remaining</div>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void FitContent_ShrinkWrapsToChild()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='test' style='width: fit-content;'>
                    <div style='width: 80px; height: 10px;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            Assert.True(box!.ContentRect.Width <= 81 && box.ContentRect.Width >= 79,
                $"fit-content should wrap to 80px child (got {box.ContentRect.Width})");
        }

        [Fact]
        public void AbsPos_AutoWidth_ShrinkToFit()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position: relative; width: 300px; height: 100px;'>
                    <div id='abs' style='position: absolute; top: 0; left: 0;'>
                        <div style='width: 80px; height: 20px;'></div>
                    </div>
                </div></body>");
            var abs = LayoutTestHelper.FindById(root, "abs");
            Assert.NotNull(abs);
            Assert.True(abs!.ContentRect.Width <= 81,
                $"abspos auto width should shrink-to-fit (got {abs.ContentRect.Width})");
        }

        [Fact]
        public void Float_AutoWidth_ShrinkToFit()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 300px;'>
                    <div id='float' style='float: left;'>
                        <div style='width: 80px; height: 20px;'></div>
                    </div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "float");
            Assert.NotNull(box);
            Assert.True(box!.ContentRect.Width <= 81,
                $"float auto width should shrink-to-fit (got {box.ContentRect.Width})");
        }

        [Fact]
        public void PercentHeight_AutoParent_ResolvesToZero()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px;'>
                    <div id='test' style='height: 50%;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            Assert.True(box!.ContentRect.Height < 1,
                $"50% height in auto parent should be 0 (got {box.ContentRect.Height})");
        }

        [Fact]
        public void PercentHeight_DefiniteParent_Resolves()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width: 200px; height: 100px;'>
                    <div id='test' style='height: 50%;'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "test");
            Assert.NotNull(box);
            Assert.True(System.Math.Abs(box!.ContentRect.Height - 50) < 2,
                $"50% of 100px should be 50 (got {box.ContentRect.Height})");
        }
    }
}

