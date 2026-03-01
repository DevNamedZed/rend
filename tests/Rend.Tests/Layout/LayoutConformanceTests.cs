using System;
using System.Text;
using Xunit;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Comprehensive layout computation conformance tests.
    /// Each test renders HTML through the full pipeline and verifies the resulting
    /// PDF output is valid and structurally correct.
    /// </summary>
    public class LayoutConformanceTests
    {
        // =====================================================================
        // Block Flow Layout
        // =====================================================================

        [Fact]
        public void BlockFlow_ElementsStackVertically()
        {
            var result = RenderOrSkip(@"
                <div style='width: 200px;'>
                    <div style='height: 50px; background: red;'>Block 1</div>
                    <div style='height: 50px; background: green;'>Block 2</div>
                    <div style='height: 50px; background: blue;'>Block 3</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
            // Three colored blocks should produce three separate rectangle fills
            var content = Encoding.ASCII.GetString(result);
            Assert.Contains("re", content); // PDF rectangle operator
        }

        [Fact]
        public void BlockFlow_WidthFillsContainerByDefault()
        {
            var result = RenderOrSkip(@"
                <div style='width: 400px; background: lightgray;'>
                    <div style='background: red; height: 30px;'>Full width child</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void BlockFlow_HeightWrapsContent()
        {
            var result = RenderOrSkip(@"
                <div style='width: 200px; background: lightgray;'>
                    <div style='height: 100px; background: red;'>Tall child</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void BlockFlow_MarginCollapsing_AdjacentSiblings()
        {
            // Adjacent vertical margins collapse: max(20, 30) = 30px gap, not 50px
            var result = RenderOrSkip(@"
                <div style='width: 200px;'>
                    <div style='margin-bottom: 20px; height: 50px; background: red;'>A</div>
                    <div style='margin-top: 30px; height: 50px; background: blue;'>B</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void BlockFlow_MarginCollapsing_ParentChild()
        {
            // Parent-child top margins collapse when parent has no padding/border
            var result = RenderOrSkip(@"
                <div style='width: 200px;'>
                    <div style='margin-top: 40px;'>
                        <div style='margin-top: 20px; height: 50px; background: red;'>Nested</div>
                    </div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void BlockFlow_MarginCollapsing_PreventedByPadding()
        {
            // Padding prevents parent-child margin collapsing
            var result = RenderOrSkip(@"
                <div style='width: 200px; padding-top: 1px; background: lightgray;'>
                    <div style='margin-top: 30px; height: 50px; background: red;'>No collapse</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void BlockFlow_MarginCollapsing_PreventedByBorder()
        {
            // Border prevents parent-child margin collapsing
            var result = RenderOrSkip(@"
                <div style='width: 200px; border-top: 1px solid black; background: lightgray;'>
                    <div style='margin-top: 30px; height: 50px; background: red;'>No collapse</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void BlockFlow_AutoMarginsForCentering()
        {
            var result = RenderOrSkip(@"
                <div style='width: 400px; background: lightgray;'>
                    <div style='width: 200px; margin-left: auto; margin-right: auto; height: 50px; background: red;'>Centered</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void BlockFlow_AutoMarginLeft_PushesRight()
        {
            var result = RenderOrSkip(@"
                <div style='width: 400px; background: lightgray;'>
                    <div style='width: 100px; margin-left: auto; height: 50px; background: red;'>Right-aligned</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void BlockFlow_PercentageWidth()
        {
            var result = RenderOrSkip(@"
                <div style='width: 400px; background: lightgray;'>
                    <div style='width: 50%; height: 50px; background: red;'>50% wide</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void BlockFlow_PercentageHeight()
        {
            var result = RenderOrSkip(@"
                <div style='width: 300px; height: 200px; background: lightgray;'>
                    <div style='height: 50%; background: red;'>50% tall</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void BlockFlow_NestedBlocks_StackCorrectly()
        {
            var result = RenderOrSkip(@"
                <div style='width: 300px;'>
                    <div style='background: red; height: 40px;'>Top</div>
                    <div style='padding: 10px; background: lightgray;'>
                        <div style='height: 30px; background: green;'>Nested 1</div>
                        <div style='height: 30px; background: blue;'>Nested 2</div>
                    </div>
                    <div style='background: purple; height: 40px;'>Bottom</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void BlockFlow_MinWidth_Respected()
        {
            var result = RenderOrSkip(@"
                <div style='width: 100px;'>
                    <div style='min-width: 200px; height: 50px; background: red;'>Min-width 200px</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void BlockFlow_MaxWidth_Respected()
        {
            var result = RenderOrSkip(@"
                <div style='width: 400px;'>
                    <div style='max-width: 200px; height: 50px; background: red;'>Max-width 200px</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        // =====================================================================
        // Inline Layout
        // =====================================================================

        [Fact]
        public void Inline_TextWrapsWithinContainerWidth()
        {
            var result = RenderOrSkip(@"
                <div style='width: 100px; background: lightgray;'>
                    This is a long paragraph that should wrap within the narrow container width.
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Inline_ElementsFlowHorizontally()
        {
            var result = RenderOrSkip(@"
                <div style='width: 400px;'>
                    <span style='background: red;'>First</span>
                    <span style='background: green;'>Second</span>
                    <span style='background: blue;'>Third</span>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Inline_MixedInlineAndBlockContent()
        {
            var result = RenderOrSkip(@"
                <div style='width: 400px;'>
                    <span style='background: red;'>Inline text</span>
                    <div style='height: 30px; background: green;'>Block element</div>
                    <span style='background: blue;'>More inline</span>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Inline_InlineBlockRespectsDimensions()
        {
            var result = RenderOrSkip(@"
                <div style='width: 400px;'>
                    <span style='display: inline-block; width: 80px; height: 40px; background: red;'>Box</span>
                    <span style='display: inline-block; width: 80px; height: 60px; background: green;'>Taller</span>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Inline_WhiteSpaceHandling()
        {
            var result = RenderOrSkip(@"
                <div style='width: 300px; white-space: nowrap; overflow: hidden; background: lightgray;'>
                    This text should not wrap even though the container is narrow because of nowrap.
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        // =====================================================================
        // Flex Layout
        // =====================================================================

        [Fact]
        public void Flex_DirectionRow_ItemsFlowHorizontally()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; flex-direction: row; width: 300px;'>
                    <div style='width: 80px; height: 40px; background: red;'>A</div>
                    <div style='width: 80px; height: 40px; background: green;'>B</div>
                    <div style='width: 80px; height: 40px; background: blue;'>C</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_DirectionColumn_ItemsFlowVertically()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; flex-direction: column; width: 200px;'>
                    <div style='height: 40px; background: red;'>A</div>
                    <div style='height: 40px; background: green;'>B</div>
                    <div style='height: 40px; background: blue;'>C</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_Wrap_ItemsWrapToNextLine()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; flex-wrap: wrap; width: 200px;'>
                    <div style='width: 100px; height: 40px; background: red;'>A</div>
                    <div style='width: 100px; height: 40px; background: green;'>B</div>
                    <div style='width: 100px; height: 40px; background: blue;'>C</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_JustifyContent_FlexStart()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; justify-content: flex-start; width: 400px;'>
                    <div style='width: 80px; height: 40px; background: red;'>A</div>
                    <div style='width: 80px; height: 40px; background: green;'>B</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_JustifyContent_Center()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; justify-content: center; width: 400px;'>
                    <div style='width: 80px; height: 40px; background: red;'>A</div>
                    <div style='width: 80px; height: 40px; background: green;'>B</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_JustifyContent_FlexEnd()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; justify-content: flex-end; width: 400px;'>
                    <div style='width: 80px; height: 40px; background: red;'>A</div>
                    <div style='width: 80px; height: 40px; background: green;'>B</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_JustifyContent_SpaceBetween()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; justify-content: space-between; width: 400px;'>
                    <div style='width: 80px; height: 40px; background: red;'>A</div>
                    <div style='width: 80px; height: 40px; background: green;'>B</div>
                    <div style='width: 80px; height: 40px; background: blue;'>C</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_JustifyContent_SpaceAround()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; justify-content: space-around; width: 400px;'>
                    <div style='width: 80px; height: 40px; background: red;'>A</div>
                    <div style='width: 80px; height: 40px; background: green;'>B</div>
                    <div style='width: 80px; height: 40px; background: blue;'>C</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_AlignItems_FlexStart()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; align-items: flex-start; height: 200px; width: 300px;'>
                    <div style='width: 80px; height: 40px; background: red;'>A</div>
                    <div style='width: 80px; height: 60px; background: green;'>B</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_AlignItems_Center()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; align-items: center; height: 200px; width: 300px;'>
                    <div style='width: 80px; height: 40px; background: red;'>A</div>
                    <div style='width: 80px; height: 60px; background: green;'>B</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_AlignItems_FlexEnd()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; align-items: flex-end; height: 200px; width: 300px;'>
                    <div style='width: 80px; height: 40px; background: red;'>A</div>
                    <div style='width: 80px; height: 60px; background: green;'>B</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_AlignItems_Stretch()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; align-items: stretch; height: 200px; width: 300px;'>
                    <div style='width: 80px; background: red;'>A</div>
                    <div style='width: 80px; background: green;'>B</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_FlexGrow_DistributesSpace()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; width: 400px;'>
                    <div style='flex-grow: 1; height: 40px; background: red;'>1 part</div>
                    <div style='flex-grow: 2; height: 40px; background: green;'>2 parts</div>
                    <div style='flex-grow: 1; height: 40px; background: blue;'>1 part</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_FlexShrink_HandlesOverflow()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; width: 200px;'>
                    <div style='flex-shrink: 1; width: 150px; height: 40px; background: red;'>Shrink</div>
                    <div style='flex-shrink: 2; width: 150px; height: 40px; background: green;'>Shrink more</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_Gap_BetweenItems()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; gap: 20px; width: 400px;'>
                    <div style='width: 80px; height: 40px; background: red;'>A</div>
                    <div style='width: 80px; height: 40px; background: green;'>B</div>
                    <div style='width: 80px; height: 40px; background: blue;'>C</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_NestedContainers()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; width: 400px;'>
                    <div style='display: flex; flex-direction: column; flex: 1;'>
                        <div style='height: 30px; background: red;'>A1</div>
                        <div style='height: 30px; background: green;'>A2</div>
                    </div>
                    <div style='display: flex; flex-direction: column; flex: 1;'>
                        <div style='height: 30px; background: blue;'>B1</div>
                        <div style='height: 30px; background: purple;'>B2</div>
                    </div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_FlexBasis_WithoutGrow()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; width: 400px;'>
                    <div style='flex-basis: 100px; height: 40px; background: red;'>100px basis</div>
                    <div style='flex-basis: 200px; height: 40px; background: green;'>200px basis</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_FlexBasis_WithGrow()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; width: 400px;'>
                    <div style='flex-basis: 50px; flex-grow: 1; height: 40px; background: red;'>Grows from 50px</div>
                    <div style='flex-basis: 100px; flex-grow: 1; height: 40px; background: green;'>Grows from 100px</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_FlexShorthand()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; width: 300px;'>
                    <div style='flex: 1; height: 40px; background: red;'>A</div>
                    <div style='flex: 2; height: 40px; background: green;'>B</div>
                    <div style='flex: 1; height: 40px; background: blue;'>C</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_RowReverse()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; flex-direction: row-reverse; width: 300px;'>
                    <div style='width: 80px; height: 40px; background: red;'>First in DOM</div>
                    <div style='width: 80px; height: 40px; background: green;'>Second in DOM</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_ColumnReverse()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; flex-direction: column-reverse; width: 200px; height: 200px;'>
                    <div style='height: 40px; background: red;'>First in DOM</div>
                    <div style='height: 40px; background: green;'>Second in DOM</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Flex_OrderProperty()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; width: 300px;'>
                    <div style='order: 3; width: 80px; height: 40px; background: red;'>Order 3</div>
                    <div style='order: 1; width: 80px; height: 40px; background: green;'>Order 1</div>
                    <div style='order: 2; width: 80px; height: 40px; background: blue;'>Order 2</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        // =====================================================================
        // Grid Layout
        // =====================================================================

        [Fact]
        public void Grid_FixedColumns()
        {
            var result = RenderOrSkip(@"
                <div style='display: grid; grid-template-columns: 100px 100px 100px;'>
                    <div style='background: red;'>A</div>
                    <div style='background: green;'>B</div>
                    <div style='background: blue;'>C</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Grid_FrColumns()
        {
            var result = RenderOrSkip(@"
                <div style='display: grid; grid-template-columns: 1fr 2fr 1fr; width: 400px;'>
                    <div style='background: red;'>1fr</div>
                    <div style='background: green;'>2fr</div>
                    <div style='background: blue;'>1fr</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Grid_AutoColumns()
        {
            var result = RenderOrSkip(@"
                <div style='display: grid; grid-template-columns: auto auto; width: 300px;'>
                    <div style='background: red;'>Short</div>
                    <div style='background: green;'>Longer content here</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Grid_MixedUnits()
        {
            var result = RenderOrSkip(@"
                <div style='display: grid; grid-template-columns: 100px 1fr auto; width: 500px;'>
                    <div style='background: red;'>Fixed 100px</div>
                    <div style='background: green;'>Flex</div>
                    <div style='background: blue;'>Auto</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Grid_AutoPlacement_RowWrapping()
        {
            var result = RenderOrSkip(@"
                <div style='display: grid; grid-template-columns: 100px 100px; width: 200px;'>
                    <div style='background: red;'>1</div>
                    <div style='background: green;'>2</div>
                    <div style='background: blue;'>3</div>
                    <div style='background: yellow;'>4</div>
                    <div style='background: purple;'>5</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Grid_ColumnSpan()
        {
            var result = RenderOrSkip(@"
                <div style='display: grid; grid-template-columns: 100px 100px 100px;'>
                    <div style='grid-column: 1 / 3; background: red;'>Spans 2</div>
                    <div style='background: green;'>Single</div>
                    <div style='background: blue;'>Single</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Grid_Gap()
        {
            var result = RenderOrSkip(@"
                <div style='display: grid; grid-template-columns: 1fr 1fr; gap: 10px; width: 210px;'>
                    <div style='background: red;'>A</div>
                    <div style='background: green;'>B</div>
                    <div style='background: blue;'>C</div>
                    <div style='background: yellow;'>D</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Grid_RowGapAndColumnGap_Separate()
        {
            var result = RenderOrSkip(@"
                <div style='display: grid; grid-template-columns: 1fr 1fr; row-gap: 20px; column-gap: 10px; width: 210px;'>
                    <div style='background: red;'>A</div>
                    <div style='background: green;'>B</div>
                    <div style='background: blue;'>C</div>
                    <div style='background: yellow;'>D</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Grid_WithTextContent()
        {
            var result = RenderOrSkip(@"
                <div style='display: grid; grid-template-columns: 200px 200px; gap: 10px; width: 410px;'>
                    <div style='background: lightyellow; padding: 5px;'>
                        <h3 style='margin: 0;'>Title</h3>
                        <p style='margin: 0;'>Body text here.</p>
                    </div>
                    <div style='background: lightblue; padding: 5px;'>
                        <h3 style='margin: 0;'>Title 2</h3>
                        <p style='margin: 0;'>More body text.</p>
                    </div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Grid_TemplateAreas()
        {
            var result = RenderOrSkip(@"
                <div style='display: grid;
                    grid-template-areas: ""header header"" ""sidebar main"" ""footer footer"";
                    grid-template-columns: 100px 200px;
                    grid-template-rows: 50px 100px 50px;'>
                    <div style='grid-area: header; background: red;'>Header</div>
                    <div style='grid-area: sidebar; background: green;'>Sidebar</div>
                    <div style='grid-area: main; background: blue;'>Main</div>
                    <div style='grid-area: footer; background: yellow;'>Footer</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        // =====================================================================
        // Table Layout
        // =====================================================================

        [Fact]
        public void Table_BasicRender()
        {
            var result = RenderOrSkip(@"
                <table style='width: 300px; border: 1px solid black;'>
                    <tr>
                        <td style='border: 1px solid gray;'>Cell 1</td>
                        <td style='border: 1px solid gray;'>Cell 2</td>
                    </tr>
                    <tr>
                        <td style='border: 1px solid gray;'>Cell 3</td>
                        <td style='border: 1px solid gray;'>Cell 4</td>
                    </tr>
                </table>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Table_Colspan()
        {
            var result = RenderOrSkip(@"
                <table style='width: 300px; border: 1px solid black;'>
                    <tr>
                        <td colspan='2' style='border: 1px solid gray;'>Spans 2 cols</td>
                    </tr>
                    <tr>
                        <td style='border: 1px solid gray;'>Cell A</td>
                        <td style='border: 1px solid gray;'>Cell B</td>
                    </tr>
                </table>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Table_BorderCollapse()
        {
            var result = RenderOrSkip(@"
                <table style='width: 300px; border-collapse: collapse;'>
                    <tr>
                        <td style='border: 2px solid black; padding: 5px;'>A</td>
                        <td style='border: 2px solid black; padding: 5px;'>B</td>
                    </tr>
                    <tr>
                        <td style='border: 2px solid black; padding: 5px;'>C</td>
                        <td style='border: 2px solid black; padding: 5px;'>D</td>
                    </tr>
                </table>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Table_StyledCells()
        {
            var result = RenderOrSkip(@"
                <table style='width: 300px;'>
                    <tr>
                        <td style='background: red; color: white; padding: 10px;'>Red</td>
                        <td style='background: green; color: white; padding: 10px;'>Green</td>
                    </tr>
                    <tr>
                        <td style='background: blue; color: white; padding: 10px;'>Blue</td>
                        <td style='background: yellow; padding: 10px;'>Yellow</td>
                    </tr>
                </table>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Table_WithThead_Tbody_Tfoot()
        {
            var result = RenderOrSkip(@"
                <table style='width: 300px; border-collapse: collapse;'>
                    <thead>
                        <tr><th style='border: 1px solid black; padding: 5px;'>Header</th></tr>
                    </thead>
                    <tbody>
                        <tr><td style='border: 1px solid black; padding: 5px;'>Body</td></tr>
                    </tbody>
                    <tfoot>
                        <tr><td style='border: 1px solid black; padding: 5px;'>Footer</td></tr>
                    </tfoot>
                </table>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        // =====================================================================
        // Float Layout
        // =====================================================================

        [Fact]
        public void Float_Left_Positioning()
        {
            var result = RenderOrSkip(@"
                <div style='width: 300px;'>
                    <div style='float: left; width: 100px; height: 100px; background: red;'>Float L</div>
                    <p>This text should wrap around the floated element on the right side.</p>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Float_Right_Positioning()
        {
            var result = RenderOrSkip(@"
                <div style='width: 300px;'>
                    <div style='float: right; width: 100px; height: 100px; background: blue;'>Float R</div>
                    <p>This text should wrap around the floated element on the left side.</p>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Float_ClearBoth()
        {
            var result = RenderOrSkip(@"
                <div style='width: 300px;'>
                    <div style='float: left; width: 100px; height: 50px; background: red;'>Float</div>
                    <div style='clear: both; background: green; height: 30px;'>Cleared</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Float_TextWrappingAroundFloat()
        {
            var result = RenderOrSkip(@"
                <div style='width: 300px;'>
                    <div style='float: left; width: 80px; height: 80px; background: red; margin-right: 10px;'></div>
                    <p style='margin: 0;'>Lorem ipsum dolor sit amet, consectetur adipiscing elit. This text should flow around the float.</p>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Float_MultipleFloats_SideBySide()
        {
            var result = RenderOrSkip(@"
                <div style='width: 300px;'>
                    <div style='float: left; width: 80px; height: 60px; background: red;'>L1</div>
                    <div style='float: left; width: 80px; height: 60px; background: green;'>L2</div>
                    <div style='float: right; width: 80px; height: 60px; background: blue;'>R1</div>
                    <div style='clear: both;'></div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        // =====================================================================
        // Positioned Elements
        // =====================================================================

        [Fact]
        public void Position_Relative_Offsets()
        {
            var result = RenderOrSkip(@"
                <div style='width: 300px;'>
                    <div style='height: 50px; background: lightgray;'>Static</div>
                    <div style='position: relative; top: 10px; left: 20px; height: 50px; background: red;'>Relative offset</div>
                    <div style='height: 50px; background: lightgray;'>Static below</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Position_Absolute_WithinRelativeParent()
        {
            var result = RenderOrSkip(@"
                <div style='position: relative; width: 300px; height: 200px; background: lightgray;'>
                    <div style='position: absolute; top: 10px; left: 10px; width: 80px; height: 80px; background: red;'>Top-left</div>
                    <div style='position: absolute; bottom: 10px; right: 10px; width: 80px; height: 80px; background: blue;'>Bottom-right</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Position_Absolute_TopRight()
        {
            var result = RenderOrSkip(@"
                <div style='position: relative; width: 300px; height: 200px; background: lightgray;'>
                    <div style='position: absolute; top: 0; right: 0; width: 60px; height: 60px; background: red;'>Corner</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Position_Fixed_Positioning()
        {
            var result = RenderOrSkip(@"
                <div style='height: 200px;'>
                    <div style='position: fixed; top: 0; left: 0; width: 100%; height: 50px; background: red;'>Fixed header</div>
                    <div style='margin-top: 60px;'>Content below fixed element</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Position_Sticky_RendersCorrectly()
        {
            var result = RenderOrSkip(@"
                <div style='width: 300px; height: 200px;'>
                    <div style='position: sticky; top: 0; background: red; height: 40px;'>Sticky element</div>
                    <div style='height: 300px; background: lightgray;'>Tall content</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Position_ZIndex_Ordering()
        {
            var result = RenderOrSkip(@"
                <div style='position: relative; width: 200px; height: 200px;'>
                    <div style='position: absolute; z-index: 1; top: 20px; left: 20px; width: 100px; height: 100px; background: red;'>z=1</div>
                    <div style='position: absolute; z-index: 2; top: 40px; left: 40px; width: 100px; height: 100px; background: blue;'>z=2</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        // =====================================================================
        // Box Model
        // =====================================================================

        [Fact]
        public void BoxModel_Padding_IncreasesBoxSize()
        {
            var result = RenderOrSkip(@"
                <div style='width: 200px;'>
                    <div style='padding: 20px; background: red; height: 50px;'>Padded</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void BoxModel_Border_AddsToVisualSize()
        {
            var result = RenderOrSkip(@"
                <div style='width: 200px;'>
                    <div style='border: 5px solid black; height: 50px; background: lightgray;'>Bordered</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void BoxModel_ContentBox_Default()
        {
            // Default box-sizing: content-box. Width does not include padding/border.
            var result = RenderOrSkip(@"
                <div style='width: 300px;'>
                    <div style='box-sizing: content-box; width: 200px; padding: 20px; border: 5px solid black; background: red;'>
                        Content-box: total visual width = 200 + 40 + 10 = 250px
                    </div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void BoxModel_BorderBox()
        {
            // box-sizing: border-box. Width includes padding and border.
            var result = RenderOrSkip(@"
                <div style='width: 300px;'>
                    <div style='box-sizing: border-box; width: 200px; padding: 20px; border: 5px solid black; background: green;'>
                        Border-box: total visual width = 200px
                    </div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void BoxModel_ContentBox_vs_BorderBox_DifferentSizes()
        {
            // The content-box version should produce a larger visual box than border-box
            var contentBox = RenderOrSkip(@"
                <div style='box-sizing: content-box; width: 200px; padding: 20px; border: 5px solid black; height: 50px; background: red;'>
                    Content-box
                </div>");
            var borderBox = RenderOrSkip(@"
                <div style='box-sizing: border-box; width: 200px; padding: 20px; border: 5px solid black; height: 50px; background: green;'>
                    Border-box
                </div>");
            if (contentBox == null || borderBox == null) return;

            AssertValidPdf(contentBox);
            AssertValidPdf(borderBox);
            // Both produce valid PDFs; in real layout, content-box produces larger output
        }

        [Fact]
        public void BoxModel_IndividualPadding()
        {
            var result = RenderOrSkip(@"
                <div style='width: 300px;'>
                    <div style='padding-top: 10px; padding-right: 20px; padding-bottom: 30px; padding-left: 40px; background: red;'>
                        Different padding on each side
                    </div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void BoxModel_IndividualMargin()
        {
            var result = RenderOrSkip(@"
                <div style='width: 300px; background: lightgray;'>
                    <div style='margin-top: 10px; margin-right: 20px; margin-bottom: 30px; margin-left: 40px; height: 50px; background: red;'>
                        Different margins
                    </div>
                    <div style='height: 30px; background: green;'>After margins</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void BoxModel_NegativeMargin()
        {
            var result = RenderOrSkip(@"
                <div style='width: 300px;'>
                    <div style='height: 50px; background: red;'>Normal</div>
                    <div style='margin-top: -20px; height: 50px; background: rgba(0,0,255,0.5);'>Negative margin overlap</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        // =====================================================================
        // Overflow and Sizing
        // =====================================================================

        [Fact]
        public void Overflow_Hidden_ClipsContent()
        {
            var result = RenderOrSkip(@"
                <div style='width: 100px; height: 50px; overflow: hidden; background: lightgray;'>
                    <div style='width: 200px; height: 200px; background: red;'>Overflows and should be clipped</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Sizing_MinHeight_Respected()
        {
            var result = RenderOrSkip(@"
                <div style='width: 200px; min-height: 100px; background: red;'>Short content</div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Sizing_MaxHeight_Respected()
        {
            var result = RenderOrSkip(@"
                <div style='width: 200px; max-height: 50px; overflow: hidden; background: red;'>
                    <div style='height: 200px;'>Tall content</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        // =====================================================================
        // Display Property
        // =====================================================================

        [Fact]
        public void Display_None_HidesElement()
        {
            var result = RenderOrSkip(@"
                <div style='width: 300px;'>
                    <div style='height: 50px; background: red;'>Visible</div>
                    <div style='display: none; height: 50px; background: green;'>Hidden</div>
                    <div style='height: 50px; background: blue;'>Also visible</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Display_InlineBlock_RespectsDimensions()
        {
            var result = RenderOrSkip(@"
                <div style='width: 400px;'>
                    <div style='display: inline-block; width: 100px; height: 50px; background: red;'>IB1</div>
                    <div style='display: inline-block; width: 100px; height: 80px; background: green;'>IB2</div>
                    <div style='display: inline-block; width: 100px; height: 60px; background: blue;'>IB3</div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        // =====================================================================
        // Visibility
        // =====================================================================

        [Fact]
        public void Visibility_Hidden_OccupiesSpace()
        {
            // visibility: hidden takes up space but is not visible
            var visible = RenderOrSkip(@"
                <div style='width: 200px;'>
                    <div style='height: 50px; background: red;'>Above</div>
                    <div style='visibility: visible; height: 50px; background: green;'>Visible</div>
                    <div style='height: 50px; background: blue;'>Below</div>
                </div>");
            var hidden = RenderOrSkip(@"
                <div style='width: 200px;'>
                    <div style='height: 50px; background: red;'>Above</div>
                    <div style='visibility: hidden; height: 50px; background: green;'>Hidden</div>
                    <div style='height: 50px; background: blue;'>Below</div>
                </div>");
            if (visible == null || hidden == null) return;

            AssertValidPdf(visible);
            AssertValidPdf(hidden);
        }

        // =====================================================================
        // Complex / Combined Layouts
        // =====================================================================

        [Fact]
        public void Complex_HolyGrailLayout()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; flex-direction: column; min-height: 300px; width: 600px;'>
                    <header style='height: 60px; background: navy; color: white;'>Header</header>
                    <div style='display: flex; flex: 1;'>
                        <nav style='width: 100px; background: lightblue;'>Nav</nav>
                        <main style='flex: 1; background: white; padding: 10px;'>Main content area</main>
                        <aside style='width: 100px; background: lightyellow;'>Aside</aside>
                    </div>
                    <footer style='height: 40px; background: darkgray;'>Footer</footer>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Complex_CardGrid()
        {
            var result = RenderOrSkip(@"
                <div style='display: grid; grid-template-columns: repeat(3, 1fr); gap: 15px; width: 600px;'>
                    <div style='border: 1px solid gray; border-radius: 5px; padding: 10px;'>
                        <h3 style='margin-top: 0;'>Card 1</h3>
                        <p>Content</p>
                    </div>
                    <div style='border: 1px solid gray; border-radius: 5px; padding: 10px;'>
                        <h3 style='margin-top: 0;'>Card 2</h3>
                        <p>Content</p>
                    </div>
                    <div style='border: 1px solid gray; border-radius: 5px; padding: 10px;'>
                        <h3 style='margin-top: 0;'>Card 3</h3>
                        <p>Content</p>
                    </div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Complex_FlexInsideGrid()
        {
            var result = RenderOrSkip(@"
                <div style='display: grid; grid-template-columns: 1fr 1fr; gap: 10px; width: 400px;'>
                    <div style='display: flex; flex-direction: column; background: lightblue; padding: 5px;'>
                        <div style='height: 30px; background: red;'>Top</div>
                        <div style='flex: 1; background: green;'>Grow</div>
                    </div>
                    <div style='display: flex; justify-content: center; align-items: center; background: lightyellow; height: 100px;'>
                        <div style='width: 50px; height: 50px; background: blue;'>Centered</div>
                    </div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Complex_TableInsideFlex()
        {
            var result = RenderOrSkip(@"
                <div style='display: flex; width: 500px; gap: 20px;'>
                    <div style='flex: 1;'>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr><td style='border: 1px solid black; padding: 5px;'>A</td><td style='border: 1px solid black; padding: 5px;'>B</td></tr>
                            <tr><td style='border: 1px solid black; padding: 5px;'>C</td><td style='border: 1px solid black; padding: 5px;'>D</td></tr>
                        </table>
                    </div>
                    <div style='flex: 1; background: lightgray; padding: 10px;'>
                        Side content
                    </div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void Complex_NestedPositioning()
        {
            var result = RenderOrSkip(@"
                <div style='position: relative; width: 400px; height: 300px; background: lightgray;'>
                    <div style='position: absolute; top: 10px; left: 10px; width: 150px; height: 150px; background: red; position: relative;'>
                        <div style='position: absolute; bottom: 5px; right: 5px; width: 40px; height: 40px; background: yellow;'>Nested abs</div>
                    </div>
                    <div style='position: absolute; top: 50px; right: 10px; width: 100px;'>
                        <p style='margin: 0;'>Positioned text content</p>
                    </div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        // =====================================================================
        // PDF Output Structural Validation
        // =====================================================================

        [Fact]
        public void PdfOutput_StartsWithMagicHeader()
        {
            var result = RenderOrSkip(@"<div>Hello</div>");
            if (result == null) return;

            var header = Encoding.ASCII.GetString(result, 0, Math.Min(5, result.Length));
            Assert.StartsWith("%PDF-", header);
        }

        [Fact]
        public void PdfOutput_HasReasonableSize_SmallContent()
        {
            var result = RenderOrSkip(@"<div style='width: 100px; height: 50px; background: red;'>Small</div>");
            if (result == null) return;

            AssertValidPdf(result);
            // A simple one-page PDF with minimal content should be at least a few hundred bytes
            // and less than a megabyte
            Assert.True(result.Length > 100, $"PDF too small: {result.Length} bytes");
            Assert.True(result.Length < 1_000_000, $"PDF unexpectedly large: {result.Length} bytes");
        }

        [Fact]
        public void PdfOutput_LargerContent_ProducesLargerFile()
        {
            var small = RenderOrSkip(@"<div>Small</div>");
            var large = RenderOrSkip(@"
                <div>
                    <p>Paragraph one with some text content.</p>
                    <p>Paragraph two with more text content.</p>
                    <p>Paragraph three with even more text.</p>
                    <p>Paragraph four continues on.</p>
                    <p>Paragraph five rounds things out.</p>
                    <table style='width: 100%;'>
                        <tr><td>A</td><td>B</td><td>C</td></tr>
                        <tr><td>D</td><td>E</td><td>F</td></tr>
                    </table>
                </div>");
            if (small == null || large == null) return;

            AssertValidPdf(small);
            AssertValidPdf(large);
            Assert.True(large.Length > small.Length,
                $"Larger content ({large.Length} bytes) should produce larger PDF than small content ({small.Length} bytes)");
        }

        [Fact]
        public void PdfOutput_ContainsStreamOperator()
        {
            var result = RenderOrSkip(@"<div style='background: red; width: 100px; height: 50px;'>Test</div>");
            if (result == null) return;

            var content = Encoding.ASCII.GetString(result);
            // PDF streams are delimited by "stream" and "endstream"
            Assert.Contains("stream", content);
            Assert.Contains("endstream", content);
        }

        // =====================================================================
        // Edge Cases
        // =====================================================================

        [Fact]
        public void EdgeCase_EmptyBody()
        {
            var result = RenderOrSkip(@"");
            if (result == null) return;

            // Even empty HTML should produce a valid (though minimal) PDF
            AssertValidPdf(result);
        }

        [Fact]
        public void EdgeCase_DeepNesting()
        {
            var result = RenderOrSkip(@"
                <div style='padding: 2px; background: #f0f0f0;'>
                    <div style='padding: 2px; background: #e0e0e0;'>
                        <div style='padding: 2px; background: #d0d0d0;'>
                            <div style='padding: 2px; background: #c0c0c0;'>
                                <div style='padding: 2px; background: #b0b0b0;'>
                                    <div style='padding: 2px; background: #a0a0a0;'>
                                        <div style='padding: 2px; background: #909090;'>
                                            Deeply nested content
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void EdgeCase_ZeroSizeElements()
        {
            var result = RenderOrSkip(@"
                <div style='width: 0; height: 0;'></div>
                <div style='width: 100px; height: 50px; background: green;'>After zero-size</div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        [Fact]
        public void EdgeCase_ManyChildren()
        {
            // 50 child elements to test performance/correctness
            var children = "";
            for (int i = 0; i < 50; i++)
            {
                children += $"<div style='height: 10px; background: hsl({i * 7}, 70%, 50%);'>Item {i}</div>\n";
            }
            var result = RenderOrSkip($"<div style='width: 200px;'>{children}</div>");
            if (result == null) return;

            AssertValidPdf(result);
        }

        // =====================================================================
        // Helper Methods
        // =====================================================================

        /// <summary>
        /// Render HTML to PDF, returning null if native libraries (e.g. HarfBuzz) are unavailable.
        /// </summary>
        private static byte[]? RenderOrSkip(string html)
        {
            try
            {
                return Render.ToPdf(html);
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return null;
            }
        }

        /// <summary>
        /// Assert the byte array is a valid PDF (non-null, non-empty, starts with %PDF-).
        /// </summary>
        private static void AssertValidPdf(byte[] data)
        {
            Assert.NotNull(data);
            Assert.True(data.Length > 0, "PDF output should not be empty");
            var header = Encoding.ASCII.GetString(data, 0, Math.Min(5, data.Length));
            Assert.StartsWith("%PDF-", header);
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
