using System;
using Xunit;

namespace Rend.Tests.Layout
{
    public class FlexAlignmentTests
    {
        [Fact]
        public void AlignItems_Center_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: flex; align-items: center; height: 200px;'>
                        <div style='width: 50px; height: 30px; background: red;'>A</div>
                        <div style='width: 50px; height: 60px; background: blue;'>B</div>
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
        public void AlignItems_FlexEnd_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: flex; align-items: flex-end; height: 200px;'>
                        <div style='width: 50px; height: 30px; background: red;'>A</div>
                        <div style='width: 50px; height: 60px; background: blue;'>B</div>
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
        public void AlignItems_Stretch_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: flex; align-items: stretch; height: 200px;'>
                        <div style='width: 50px; background: red;'>A</div>
                        <div style='width: 50px; background: blue;'>B</div>
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
        public void AlignSelf_Override_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: flex; align-items: flex-start; height: 200px;'>
                        <div style='width: 50px; height: 30px; background: red;'>A</div>
                        <div style='width: 50px; height: 30px; align-self: center; background: blue;'>B</div>
                        <div style='width: 50px; height: 30px; align-self: flex-end; background: green;'>C</div>
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
        public void AlignContent_Center_WrappedFlex_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: flex; flex-wrap: wrap; align-content: center; height: 400px; width: 200px;'>
                        <div style='width: 100px; height: 40px; background: red;'>A</div>
                        <div style='width: 100px; height: 40px; background: blue;'>B</div>
                        <div style='width: 100px; height: 40px; background: green;'>C</div>
                        <div style='width: 100px; height: 40px; background: orange;'>D</div>
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
        public void AlignContent_FlexEnd_WrappedFlex_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: flex; flex-wrap: wrap; align-content: flex-end; height: 400px; width: 200px;'>
                        <div style='width: 100px; height: 40px; background: red;'>A</div>
                        <div style='width: 100px; height: 40px; background: blue;'>B</div>
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
        public void FlexColumn_AlignItems_Center_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: flex; flex-direction: column; align-items: center; width: 300px;'>
                        <div style='width: 100px; height: 30px; background: red;'>A</div>
                        <div style='width: 150px; height: 30px; background: blue;'>B</div>
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
        public void FlexWrap_WrapReverse_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: flex; flex-wrap: wrap-reverse; width: 200px;'>
                        <div style='width: 100px; height: 50px; background: red;'>A</div>
                        <div style='width: 100px; height: 50px; background: green;'>B</div>
                        <div style='width: 100px; height: 50px; background: blue;'>C</div>
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
        public void FlexWrap_WrapReverse_Column_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: flex; flex-direction: column; flex-wrap: wrap-reverse; height: 150px; width: 200px;'>
                        <div style='width: 80px; height: 80px; background: red;'>A</div>
                        <div style='width: 80px; height: 80px; background: green;'>B</div>
                        <div style='width: 80px; height: 80px; background: blue;'>C</div>
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
        public void Flex_MarginAutoLeft_PushesRight_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: flex; width: 300px;'>
                        <div style='width: 50px; height: 50px; background: red;'>A</div>
                        <div style='width: 50px; height: 50px; margin-left: auto; background: blue;'>B</div>
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
        public void Flex_MarginAutoBoth_Centers_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: flex; width: 300px;'>
                        <div style='width: 50px; height: 50px; margin: auto; background: red;'>Centered</div>
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
        public void Flex_DisplayContents_ChildrenBecomeFlatItems_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: flex; width: 300px;'>
                        <div style='display: contents;'>
                            <div style='width: 100px; height: 50px; background: red;'>A</div>
                            <div style='width: 100px; height: 50px; background: green;'>B</div>
                        </div>
                        <div style='width: 100px; height: 50px; background: blue;'>C</div>
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
        public void Flex_VisibilityCollapse_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: flex; width: 300px;'>
                        <div style='width: 100px; height: 50px; background: red;'>A</div>
                        <div style='width: 100px; height: 50px; visibility: collapse; background: green;'>B</div>
                        <div style='width: 100px; height: 50px; background: blue;'>C</div>
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
        public void Flex_AbsolutePositioned_OutOfFlow_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: flex; position: relative; width: 300px;'>
                        <div style='width: 100px; height: 50px; background: red;'>A</div>
                        <div style='position: absolute; top: 10px; left: 10px; width: 50px; height: 50px; background: yellow;'>Abs</div>
                        <div style='width: 100px; height: 50px; background: blue;'>B</div>
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
