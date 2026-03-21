using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexItemCrossSizingTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexItemCrossSizingTests(ITestOutputHelper output) { _output = output; }

        // [CSS-FLEXBOX 9.4] align-items defaults to stretch; item fills container cross size
        [Fact]
        public void StretchDefault_FillsContainerHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='t' style='width:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"height={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 200) < 2,
                $"Stretch default should fill 200px (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] explicit height on item overrides stretch
        [Fact]
        public void ExplicitHeight_OverridesStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='t' style='width:50px;height:80px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"height={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 80) < 2,
                $"Explicit height 80px should override stretch (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] align-items:flex-start preserves intrinsic height
        [Fact]
        public void AlignFlexStart_PreservesHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;height:200px;width:300px'>
                    <div id='t' style='width:50px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"height={item!.ContentRect.Height}, Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 40) < 2,
                $"flex-start preserves height 40px (got {item.ContentRect.Height})");
            Assert.True(item.ContentRect.Y < 2,
                $"flex-start positions at top (got Y={item.ContentRect.Y})");
        }

        // [CSS-FLEXBOX 9.4] align-items:center preserves intrinsic height, centers vertically
        [Fact]
        public void AlignCenter_PreservesHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:center;height:200px;width:300px'>
                    <div id='t' style='width:50px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"height={item!.ContentRect.Height}, Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 40) < 2,
                $"center preserves height 40px (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 80) < 2,
                $"center positions at (200-40)/2=80 (got Y={item.ContentRect.Y})");
        }

        // [CSS-FLEXBOX 9.4] align-items:flex-end preserves intrinsic height, aligns to bottom
        [Fact]
        public void AlignFlexEnd_PreservesHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-end;height:200px;width:300px'>
                    <div id='t' style='width:50px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"height={item!.ContentRect.Height}, Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 40) < 2,
                $"flex-end preserves height 40px (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 160) < 2,
                $"flex-end positions at 200-40=160 (got Y={item.ContentRect.Y})");
        }

        // [CSS-FLEXBOX 9.4] align-self overrides align-items on individual item
        [Fact]
        public void AlignSelf_OverridesAlignItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;height:200px;width:300px'>
                    <div id='a' style='width:50px;height:40px'></div>
                    <div id='b' style='align-self:stretch;width:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.height={itemA!.ContentRect.Height}, b.height={itemB!.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 40) < 2,
                $"a keeps 40px with flex-start (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 200) < 2,
                $"b stretches to 200px via align-self:stretch (got {itemB.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] align-self:flex-end on one item while others stretch
        [Fact]
        public void AlignSelf_FlexEnd_WhileOthersStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:150px;width:300px'>
                    <div id='a' style='width:50px'></div>
                    <div id='b' style='align-self:flex-end;width:50px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.height={itemA!.ContentRect.Height}, b.Y={itemB!.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 150) < 2,
                $"a stretches to 150px (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 120) < 2,
                $"b at flex-end Y=120 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX 9.4] stretch with padding: padding included in cross size
        [Fact]
        public void Stretch_WithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='t' style='width:50px;padding:10px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"content height={item!.ContentRect.Height}");
            // Stretched to 200px total, content = 200 - 10 - 10 = 180
            Assert.True(System.Math.Abs(item.ContentRect.Height - 180) < 2,
                $"Content height should be 200-20=180 (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] stretch with border: border included in cross size
        [Fact]
        public void Stretch_WithBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='t' style='width:50px;border:5px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"content height={item!.ContentRect.Height}");
            // Stretched to 200px total, content = 200 - 5 - 5 = 190
            Assert.True(System.Math.Abs(item.ContentRect.Height - 190) < 2,
                $"Content height should be 200-10=190 (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] stretch with margin: margins reduce stretched size
        [Fact]
        public void Stretch_WithMargin()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='t' style='width:50px;margin-top:20px;margin-bottom:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"content height={item!.ContentRect.Height}");
            // Stretched cross size = 200 - 20 - 30 = 150
            Assert.True(System.Math.Abs(item.ContentRect.Height - 150) < 2,
                $"Stretched height should be 200-50=150 (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] stretch clamped by max-height
        [Fact]
        public void Stretch_ClampedByMaxHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='t' style='width:50px;max-height:100px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"height={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2,
                $"max-height 100 clamps stretch (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] stretch honored but min-height enforces minimum
        [Fact]
        public void Stretch_MinHeight_Enforced()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='t' style='width:50px;min-height:250px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"height={item!.ContentRect.Height}");
            Assert.True(item.ContentRect.Height >= 249,
                $"min-height 250 overrides stretch to 200 (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] min-height smaller than container: stretch still fills container
        [Fact]
        public void Stretch_MinHeightSmallerThanContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='t' style='width:50px;min-height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"height={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 200) < 2,
                $"min-height 50 < container 200, stretch fills to 200 (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] column flex: cross axis is width, stretch fills width
        [Fact]
        public void Column_StretchFillsWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:300px;height:200px'>
                    <div id='t' style='height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 300) < 2,
                $"Column stretch fills width 300 (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX 9.4] column flex: explicit width overrides cross-axis stretch
        [Fact]
        public void Column_ExplicitWidth_OverridesStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:300px;height:200px'>
                    <div id='t' style='width:120px;height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 120) < 2,
                $"Explicit width 120 overrides stretch (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX 9.4] column flex: align-items:center preserves width and centers
        [Fact]
        public void Column_AlignCenter_PreservesWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;align-items:center;width:300px;height:200px'>
                    <div id='t' style='width:100px;height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}, X={item.ContentRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2,
                $"center preserves width 100 (got {item.ContentRect.Width})");
            Assert.True(System.Math.Abs(item.ContentRect.X - 100) < 2,
                $"center positions at (300-100)/2=100 (got X={item.ContentRect.X})");
        }

        // [CSS-FLEXBOX 9.4] column flex: max-width clamps cross-axis stretch
        [Fact]
        public void Column_Stretch_ClampedByMaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:300px;height:200px'>
                    <div id='t' style='height:50px;max-width:150px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 150) < 2,
                $"max-width 150 clamps column stretch (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX 9.4] align-items:baseline does not stretch items
        [Fact]
        public void AlignBaseline_DoesNotStretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:baseline;height:200px;width:300px'>
                    <div id='t' style='width:50px;height:60px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"height={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2,
                $"baseline preserves height 60 (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] cross-axis percentage height resolves against container
        [Fact]
        public void CrossAxis_PercentageHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='t' style='width:50px;height:50%'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"height={item!.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 100) < 2,
                $"50% of 200 = 100 (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 8.1] cross-axis auto margin absorbs free space
        [Fact]
        public void CrossAxis_AutoMargin_AbsorbsFreeSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='t' style='width:50px;height:60px;margin-top:auto;margin-bottom:auto'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"Y={item!.ContentRect.Y}, height={item.ContentRect.Height}");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 60) < 2,
                $"auto margin preserves height 60 (got {item.ContentRect.Height})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 70) < 2,
                $"auto margin centers at (200-60)/2=70 (got Y={item.ContentRect.Y})");
        }

        // [CSS-FLEXBOX 8.1] single auto margin on cross axis pushes to opposite edge
        [Fact]
        public void CrossAxis_SingleAutoMarginTop_PushesToBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='t' style='width:50px;height:60px;margin-top:auto'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"Y={item!.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 140) < 2,
                $"margin-top:auto pushes to bottom at Y=140 (got {item.ContentRect.Y})");
        }

        // [CSS-FLEXBOX 9.4] stretch in flex-wrap:wrap, multiple lines
        [Fact]
        public void Stretch_InWrap_EachLineSizedByTallestItem()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:200px;height:200px'>
                    <div id='a' style='width:120px;height:30px'></div>
                    <div id='b' style='width:120px;height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.height={itemA!.ContentRect.Height}, b.height={itemB!.ContentRect.Height}");
            // In wrap with definite container height, lines share the space
            // Line 1 has item a (30px intrinsic), line 2 has item b (50px intrinsic)
            // With stretch + definite height, each line gets a share of the 200px
            Assert.True(itemA.ContentRect.Height >= 29,
                $"Item a should be at least its intrinsic 30px (got {itemA.ContentRect.Height})");
            Assert.True(itemB.ContentRect.Height >= 49,
                $"Item b should be at least its intrinsic 50px (got {itemB.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] stretch with row-gap in wrap mode
        [Fact]
        public void Stretch_InWrap_WithGap()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-wrap:wrap;width:100px;height:200px;gap:20px'>
                    <div id='a' style='width:80px'></div>
                    <div id='b' style='width:80px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.height={itemA!.ContentRect.Height}, b.height={itemB!.ContentRect.Height}");
            // 200px - 20px gap = 180px / 2 lines = 90px each
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 90) < 2,
                $"Line 1 stretch = (200-20)/2=90 (got {itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 90) < 2,
                $"Line 2 stretch = (200-20)/2=90 (got {itemB.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] multiple items in one line: all stretch to tallest
        [Fact]
        public void Stretch_MultipleItems_AllFillToTallest()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='b' style='width:50px'></div>
                    <div id='c' style='width:50px;height:80px'></div>
                </div></body>");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemB);
            _output.WriteLine($"b.height={itemB!.ContentRect.Height}");
            // Auto-height container = tallest item = 80px. b stretches to 80.
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 80) < 2,
                $"b stretches to tallest item 80 (got {itemB.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] stretch with padding and border combined
        [Fact]
        public void Stretch_WithPaddingAndBorder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='t' style='width:50px;padding:10px;border:5px solid black'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"content height={item!.ContentRect.Height}");
            // Total cross = 200. Content = 200 - 10 - 10 (padding) - 5 - 5 (border) = 170
            Assert.True(System.Math.Abs(item.ContentRect.Height - 170) < 2,
                $"Content height = 200-20-10=170 (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] align-self:center on single item in auto-height container
        [Fact]
        public void AlignSelfCenter_AutoHeightContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div style='width:50px;height:100px'></div>
                    <div id='t' style='align-self:center;width:50px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"Y={item!.ContentRect.Y}, height={item.ContentRect.Height}");
            // Auto height = 100 (tallest). Center = (100-40)/2 = 30
            Assert.True(System.Math.Abs(item.ContentRect.Y - 30) < 2,
                $"center in auto-height at Y=30 (got {item.ContentRect.Y})");
            Assert.True(System.Math.Abs(item.ContentRect.Height - 40) < 2,
                $"center preserves height 40 (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] stretch with box-sizing:border-box
        [Fact]
        public void Stretch_WithBorderBox()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='t' style='width:50px;padding:20px;box-sizing:border-box'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"content height={item!.ContentRect.Height}");
            // border-box: total = 200, content = 200 - 20 - 20 = 160
            Assert.True(System.Math.Abs(item.ContentRect.Height - 160) < 2,
                $"border-box stretch content = 200-40=160 (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] column: cross-axis auto margin centers horizontally
        [Fact]
        public void Column_CrossAxisAutoMargin_CentersHorizontally()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:300px;height:200px'>
                    <div id='t' style='width:100px;height:50px;margin-left:auto;margin-right:auto'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"X={item!.ContentRect.X}, width={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.X - 100) < 2,
                $"auto margins center at (300-100)/2=100 (got X={item.ContentRect.X})");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 100) < 2,
                $"auto margins preserve width 100 (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX 9.4] stretch with margin on cross axis reduces available space
        [Fact]
        public void Stretch_WithCrossMarginAndPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='t' style='width:50px;margin-top:10px;margin-bottom:10px;padding:15px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"content height={item!.ContentRect.Height}");
            // Stretch cross = 200 - 10 - 10 (margins) = 180 total box
            // Content = 180 - 15 - 15 (padding) = 150
            Assert.True(System.Math.Abs(item.ContentRect.Height - 150) < 2,
                $"Content = 200-20(margin)-30(padding)=150 (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] column: align-items:flex-end positions item at right edge
        [Fact]
        public void Column_AlignFlexEnd_PositionsAtRight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;align-items:flex-end;width:300px;height:200px'>
                    <div id='t' style='width:80px;height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"X={item!.ContentRect.X}, width={item.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.X - 220) < 2,
                $"flex-end in column at X=300-80=220 (got X={item.ContentRect.X})");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 80) < 2,
                $"flex-end preserves width 80 (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX 9.4] stretch with max-height and padding: max-height applies to border box
        [Fact]
        public void Stretch_MaxHeightWithPadding()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px'>
                    <div id='t' style='width:50px;padding:10px;max-height:80px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "t");
            Assert.NotNull(item);
            _output.WriteLine($"content height={item!.ContentRect.Height}");
            // max-height:80px is content-box by default, so content=80, total=80+20=100
            Assert.True(item.ContentRect.Height <= 81,
                $"max-height 80 clamps content height (got {item.ContentRect.Height})");
        }

        // [CSS-FLEXBOX 9.4] gap on main axis does not affect cross-axis sizing
        [Fact]
        public void MainAxisGap_DoesNotAffectCrossSize()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;height:200px;width:300px;column-gap:20px'>
                    <div id='a' style='width:50px'></div>
                    <div id='b' style='width:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.height={itemA!.ContentRect.Height}, b.height={itemB!.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 200) < 2,
                $"column-gap doesn't reduce cross size (got a.height={itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 200) < 2,
                $"column-gap doesn't reduce cross size (got b.height={itemB.ContentRect.Height})");
        }
    }
}
