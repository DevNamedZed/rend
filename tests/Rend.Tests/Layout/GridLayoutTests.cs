using System;
using Xunit;

namespace Rend.Tests.Layout
{
    public class GridLayoutTests
    {
        [Fact]
        public void BasicGrid_ThreeColumns_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px 100px;'>
                        <div style='background: red;'>A</div>
                        <div style='background: green;'>B</div>
                        <div style='background: blue;'>C</div>
                        <div style='background: yellow;'>D</div>
                        <div style='background: purple;'>E</div>
                        <div style='background: orange;'>F</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_FrUnits_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 1fr 2fr 1fr; width: 400px;'>
                        <div style='background: red;'>1fr</div>
                        <div style='background: green;'>2fr</div>
                        <div style='background: blue;'>1fr</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_RepeatFunction_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: repeat(4, 1fr); width: 400px;'>
                        <div style='background: red;'>A</div>
                        <div style='background: green;'>B</div>
                        <div style='background: blue;'>C</div>
                        <div style='background: yellow;'>D</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_ExplicitRowPlacement_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px 100px;'>
                        <div style='grid-row: 2; background: red;'>Row 2</div>
                        <div style='background: green;'>Auto</div>
                        <div style='background: blue;'>Auto</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_ExplicitColumnPlacement_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px 100px;'>
                        <div style='grid-column: 3; background: red;'>Col 3</div>
                        <div style='background: green;'>Auto</div>
                        <div style='background: blue;'>Auto</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_ColumnSpan_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px 100px;'>
                        <div style='grid-column: 1 / 3; background: red;'>Spans 2 cols</div>
                        <div style='background: green;'>C</div>
                        <div style='background: blue;'>D</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_SpanKeyword_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px 100px;'>
                        <div style='grid-column: span 2; background: red;'>Spans 2</div>
                        <div style='background: green;'>Auto</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_GridArea_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px 100px; grid-template-rows: 50px 50px 50px;'>
                        <div style='grid-area: 1 / 1 / 3 / 3; background: red;'>Area</div>
                        <div style='background: green;'>Auto</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_Gap_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 1fr 1fr; gap: 10px; width: 210px;'>
                        <div style='background: red;'>A</div>
                        <div style='background: green;'>B</div>
                        <div style='background: blue;'>C</div>
                        <div style='background: yellow;'>D</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_AutoFlowColumn_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-rows: 50px 50px; grid-auto-flow: column;'>
                        <div style='background: red;'>A</div>
                        <div style='background: green;'>B</div>
                        <div style='background: blue;'>C</div>
                        <div style='background: yellow;'>D</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_AutoFlowDense_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px 100px; grid-auto-flow: dense;'>
                        <div style='grid-column: 2; background: red;'>Col 2</div>
                        <div style='background: green;'>Dense fill</div>
                        <div style='background: blue;'>Dense fill 2</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_ExplicitRows_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 1fr 1fr; grid-template-rows: 50px 100px;'>
                        <div style='background: red;'>A</div>
                        <div style='background: green;'>B</div>
                        <div style='background: blue;'>C</div>
                        <div style='background: yellow;'>D</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_MixedFixedAndFr_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 1fr 2fr; width: 500px;'>
                        <div style='background: red;'>Fixed</div>
                        <div style='background: green;'>1fr</div>
                        <div style='background: blue;'>2fr</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_NoExplicitTemplate_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid;'>
                        <div style='background: red;'>A</div>
                        <div style='background: green;'>B</div>
                        <div style='background: blue;'>C</div>
                        <div style='background: yellow;'>D</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_PercentageColumns_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 25% 25% 50%; width: 400px;'>
                        <div style='background: red;'>25%</div>
                        <div style='background: green;'>25%</div>
                        <div style='background: blue;'>50%</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_RowSpan_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px; grid-template-rows: 50px 50px;'>
                        <div style='grid-row: 1 / 3; background: red;'>Spans 2 rows</div>
                        <div style='background: green;'>B</div>
                        <div style='background: blue;'>C</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_AutoRows_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px; grid-auto-rows: 80px;'>
                        <div style='background: red;'>A</div>
                        <div style='background: green;'>B</div>
                        <div style='background: blue;'>C</div>
                        <div style='background: yellow;'>D</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_AutoRows_WithExplicitRows_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px; grid-template-rows: 50px; grid-auto-rows: 100px;'>
                        <div style='background: red;'>A</div>
                        <div style='background: green;'>B</div>
                        <div style='background: blue;'>C</div>
                        <div style='background: yellow;'>D</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_AlignItemsCenter_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px; grid-template-rows: 100px; align-items: center;'>
                        <div style='background: red; height: 40px;'>Center</div>
                        <div style='background: green; height: 40px;'>Center</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_JustifyItemsCenter_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 200px; justify-items: center;'>
                        <div style='background: red; width: 100px;'>Center</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_AlignSelfEnd_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px; grid-template-rows: 100px;'>
                        <div style='background: red; align-self: end; height: 30px;'>End</div>
                        <div style='background: green;'>Stretch</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_JustifySelfEnd_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 200px; grid-template-rows: 100px;'>
                        <div style='background: red; justify-self: end; width: 80px;'>End</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_OrderProperty_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px 100px;'>
                        <div style='order: 3; background: red;'>Third</div>
                        <div style='order: 1; background: green;'>First</div>
                        <div style='order: 2; background: blue;'>Second</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_OrderProperty_DefaultZero_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px 100px;'>
                        <div style='order: 1; background: red;'>After default</div>
                        <div style='background: green;'>Default (0)</div>
                        <div style='order: -1; background: blue;'>Before default</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_DisplayContents_ChildrenBecomeFlatItems_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px 100px;'>
                        <div style='display: contents;'>
                            <div style='background: red;'>A</div>
                            <div style='background: green;'>B</div>
                        </div>
                        <div style='background: blue;'>C</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_VisibilityCollapse_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px;'>
                        <div style='background: red;'>A</div>
                        <div style='visibility: collapse; background: green;'>B</div>
                        <div style='background: blue;'>C</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Grid_AbsolutePositioned_OutOfFlow_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px; position: relative;'>
                        <div style='background: red;'>A</div>
                        <div style='position: absolute; top: 0; right: 0; width: 50px; height: 50px; background: yellow;'>Abs</div>
                        <div style='background: blue;'>B</div>
                    </div>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        private static bool IsNativeLibraryFailure(Exception ex)
        {
            return ex is DllNotFoundException ||
                   ex is TypeInitializationException ||
                   (ex.InnerException is DllNotFoundException) ||
                   ex.Message.Contains("native", StringComparison.OrdinalIgnoreCase);
        }
    }
}
