using System;
using System.Collections.Generic;
using Xunit;
using Rend.Css;
using Rend.Layout.Internal;

namespace Rend.Tests.Layout
{
    public class SubgridTests
    {
        [Fact]
        public void IsSubgrid_SubgridKeyword_ReturnsTrue()
        {
            var value = new CssKeywordValue("subgrid");
            Assert.True(GridLayout.IsSubgrid(value));
        }

        [Fact]
        public void IsSubgrid_NoneKeyword_ReturnsFalse()
        {
            var value = new CssKeywordValue("none");
            Assert.False(GridLayout.IsSubgrid(value));
        }

        [Fact]
        public void IsSubgrid_AutoKeyword_ReturnsFalse()
        {
            var value = new CssKeywordValue("auto");
            Assert.False(GridLayout.IsSubgrid(value));
        }

        [Fact]
        public void IsSubgrid_Null_ReturnsFalse()
        {
            Assert.False(GridLayout.IsSubgrid(null));
        }

        [Fact]
        public void IsSubgrid_DimensionValue_ReturnsFalse()
        {
            var value = new CssDimensionValue(100, "px");
            Assert.False(GridLayout.IsSubgrid(value));
        }

        [Fact]
        public void IsSubgrid_ListValue_ReturnsFalse()
        {
            var value = new CssListValue(new List<CssValue>
            {
                new CssDimensionValue(100, "px"),
                new CssDimensionValue(200, "px")
            });
            Assert.False(GridLayout.IsSubgrid(value));
        }

        [Fact]
        public void ResolveTrackList_SubgridKeyword_ReturnsNull()
        {
            // When the value is "subgrid", ResolveTrackList should return null
            // because subgrid tracks are not parsed as normal tracks.
            // The subgrid logic is handled separately in GridLayout.Layout().
            var result = GridLayout.ResolveTrackList(new CssKeywordValue("subgrid"), 400);
            Assert.Null(result);
        }

        [Fact]
        public void SubgridColumns_InheritParentTracks_ProducesValidPdf()
        {
            // A 3-column parent grid with a nested grid that uses subgrid for columns.
            // The nested grid spans columns 1-3 of the parent and has grid-template-columns: subgrid.
            // Its children should be laid out using the parent's column track sizes.
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 150px 200px; width: 450px;'>
                        <div style='grid-column: 1 / 4; display: grid; grid-template-columns: subgrid;'>
                            <div style='background: red;'>A</div>
                            <div style='background: green;'>B</div>
                            <div style='background: blue;'>C</div>
                        </div>
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
        public void SubgridRows_InheritParentTracks_ProducesValidPdf()
        {
            // Parent grid with explicit rows; nested grid uses subgrid for rows.
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 200px 200px; grid-template-rows: 50px 80px 60px;'>
                        <div style='grid-row: 1 / 4; display: grid; grid-template-rows: subgrid;'>
                            <div style='background: red;'>Row1</div>
                            <div style='background: green;'>Row2</div>
                            <div style='background: blue;'>Row3</div>
                        </div>
                        <div style='background: yellow;'>Side</div>
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
        public void SubgridBothAxes_InheritParentTracks_ProducesValidPdf()
        {
            // Nested grid with subgrid on both columns and rows.
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 150px 200px; grid-template-rows: 40px 60px; width: 450px;'>
                        <div style='grid-column: 1 / 4; grid-row: 1 / 3; display: grid; grid-template-columns: subgrid; grid-template-rows: subgrid;'>
                            <div style='background: red;'>A</div>
                            <div style='background: green;'>B</div>
                            <div style='background: blue;'>C</div>
                            <div style='background: yellow;'>D</div>
                            <div style='background: purple;'>E</div>
                            <div style='background: orange;'>F</div>
                        </div>
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
        public void SubgridColumns_PartialSpan_InheritsSubsetOfTracks_ProducesValidPdf()
        {
            // Parent has 4 columns; nested grid spans columns 2-4 (3 tracks).
            // The subgrid should only inherit those 3 track sizes.
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 50px 100px 150px 200px; width: 500px;'>
                        <div style='background: gray;'>First</div>
                        <div style='grid-column: 2 / 5; display: grid; grid-template-columns: subgrid;'>
                            <div style='background: red;'>A</div>
                            <div style='background: green;'>B</div>
                            <div style='background: blue;'>C</div>
                        </div>
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
        public void SubgridFallback_NoParentGrid_ProducesValidPdf()
        {
            // A grid with subgrid that is NOT inside a parent grid.
            // Should gracefully fall back to normal grid behavior.
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: subgrid; width: 300px;'>
                        <div style='background: red;'>A</div>
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
        public void SubgridColumns_WithGap_InheritsParentGap_ProducesValidPdf()
        {
            // Parent grid with gap; subgrid should inherit the parent's gap.
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px 100px; gap: 10px; width: 320px;'>
                        <div style='grid-column: 1 / 4; display: grid; grid-template-columns: subgrid;'>
                            <div style='background: red;'>A</div>
                            <div style='background: green;'>B</div>
                            <div style='background: blue;'>C</div>
                        </div>
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
        public void SubgridColumns_MixedSubgridAndNormalRows_ProducesValidPdf()
        {
            // Subgrid on columns only; rows use normal template.
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 200px; grid-template-rows: 50px 50px;'>
                        <div style='grid-column: 1 / 3; display: grid; grid-template-columns: subgrid; grid-template-rows: 30px 30px;'>
                            <div style='background: red;'>A</div>
                            <div style='background: green;'>B</div>
                            <div style='background: blue;'>C</div>
                            <div style='background: yellow;'>D</div>
                        </div>
                        <div style='background: purple;'>Below</div>
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
        public void SubgridColumns_FrParentTracks_ProducesValidPdf()
        {
            // Parent uses fr units; subgrid inherits resolved px sizes.
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 1fr 2fr 1fr; width: 400px;'>
                        <div style='grid-column: 1 / 4; display: grid; grid-template-columns: subgrid;'>
                            <div style='background: red;'>1fr=100</div>
                            <div style='background: green;'>2fr=200</div>
                            <div style='background: blue;'>1fr=100</div>
                        </div>
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
        public void SubgridColumns_NestedSubgridItems_ProducesValidPdf()
        {
            // Subgrid with items that have explicit placement.
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <div style='display: grid; grid-template-columns: 100px 100px 100px; width: 300px;'>
                        <div style='grid-column: 1 / 4; display: grid; grid-template-columns: subgrid;'>
                            <div style='grid-column: 2; background: red;'>Middle</div>
                            <div style='background: green;'>Auto1</div>
                            <div style='background: blue;'>Auto2</div>
                        </div>
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
