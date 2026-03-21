using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for flex:1 equal distribution across varying item counts,
    /// container sizes, gap values, and flex-direction:column.
    /// Validates both computed widths/heights and X/Y positions.
    /// </summary>
    public class WptFlexGrowEqualTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexGrowEqualTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-FLEXBOX §9.7] 1 item in 200px container gets full width
        [Fact]
        public void EqualGrow_1Item_200px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:1;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "a");
            Assert.NotNull(item);
            _output.WriteLine($"a.width={item!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item.ContentRect.Width - 200) < 2,
                $"Single item should fill 200px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 1 item in 300px container gets full width
        [Fact]
        public void EqualGrow_1Item_300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "a");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 300) < 2,
                $"Single item should fill 300px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 1 item in 400px container gets full width
        [Fact]
        public void EqualGrow_1Item_400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "a");
            Assert.NotNull(item);
            Assert.True(System.Math.Abs(item!.ContentRect.Width - 400) < 2,
                $"Single item should fill 400px (got {item.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 2 items in 200px = 100px each
        [Fact]
        public void EqualGrow_2Items_200px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.width={itemA!.ContentRect.Width}, b.width={itemB!.ContentRect.Width}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 100) < 2,
                $"2 items in 200px: each should be 100px (a={itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"2 items in 200px: each should be 100px (b={itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 2 items in 300px = 150px each
        [Fact]
        public void EqualGrow_2Items_300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.True(System.Math.Abs(itemA!.ContentRect.Width - 150) < 2,
                $"2 items in 300px: each 150px (a={itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB!.ContentRect.Width - 150) < 2,
                $"2 items in 300px: each 150px (b={itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 2 items in 400px = 200px each
        [Fact]
        public void EqualGrow_2Items_400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.True(System.Math.Abs(itemA!.ContentRect.Width - 200) < 2,
                $"2 items in 400px: each 200px (a={itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB!.ContentRect.Width - 200) < 2,
                $"2 items in 400px: each 200px (b={itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 2 items in 600px = 300px each
        [Fact]
        public void EqualGrow_2Items_600px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:600px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                </div></body>", viewportWidth: 800);
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.True(System.Math.Abs(itemA!.ContentRect.Width - 300) < 2,
                $"2 items in 600px: each 300px (a={itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB!.ContentRect.Width - 300) < 2,
                $"2 items in 600px: each 300px (b={itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 3 items in 300px = 100px each
        [Fact]
        public void EqualGrow_3Items_300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            Assert.True(System.Math.Abs(itemA!.ContentRect.Width - 100) < 2,
                $"3 items in 300px: each 100px (a={itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB!.ContentRect.Width - 100) < 2,
                $"3 items in 300px: each 100px (b={itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC!.ContentRect.Width - 100) < 2,
                $"3 items in 300px: each 100px (c={itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 3 items in 600px = 200px each
        [Fact]
        public void EqualGrow_3Items_600px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:600px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                </div></body>", viewportWidth: 800);
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            Assert.True(System.Math.Abs(itemA!.ContentRect.Width - 200) < 2,
                $"3 items in 600px: each 200px (a={itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB!.ContentRect.Width - 200) < 2,
                $"3 items in 600px: each 200px (b={itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemC!.ContentRect.Width - 200) < 2,
                $"3 items in 600px: each 200px (c={itemC.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 4 items in 400px = 100px each
        [Fact]
        public void EqualGrow_4Items_400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                    <div id='d' style='flex:1;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] 4 items in 600px = 150px each
        [Fact]
        public void EqualGrow_4Items_600px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:600px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                    <div id='d' style='flex:1;height:30px'></div>
                </div></body>", viewportWidth: 800);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Width - 150) < 2);
        }

        // [CSS-FLEXBOX §9.7] 5 items in 400px = 80px each
        [Fact]
        public void EqualGrow_5Items_400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                    <div id='d' style='flex:1;height:30px'></div>
                    <div id='e' style='flex:1;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Width - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "e")!.ContentRect.Width - 80) < 2);
        }

        // [CSS-FLEXBOX §9.7] 5 items in 500px = 100px each
        [Fact]
        public void EqualGrow_5Items_500px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:500px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                    <div id='d' style='flex:1;height:30px'></div>
                    <div id='e' style='flex:1;height:30px'></div>
                </div></body>", viewportWidth: 600);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "e")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] 6 items in 600px = 100px each
        [Fact]
        public void EqualGrow_6Items_600px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:600px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                    <div id='d' style='flex:1;height:30px'></div>
                    <div id='e' style='flex:1;height:30px'></div>
                    <div id='f' style='flex:1;height:30px'></div>
                </div></body>", viewportWidth: 800);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "e")!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "f")!.ContentRect.Width - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] 2 items in 200px with 20px gap = 90px each
        [Fact]
        public void EqualGrow_2Items_WithGap_200px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px;gap:20px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.width={itemA!.ContentRect.Width}, b.width={itemB!.ContentRect.Width}");
            // 200 - 20 gap = 180 / 2 = 90 each
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 90) < 2,
                $"2 items with 20px gap in 200px: each 90px (a={itemA.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 90) < 2,
                $"2 items with 20px gap in 200px: each 90px (b={itemB.ContentRect.Width})");
        }

        // [CSS-FLEXBOX §9.7] 3 items in 300px with 15px gap = 90px each
        [Fact]
        public void EqualGrow_3Items_WithGap_300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;gap:15px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                </div></body>");
            // 300 - 2*15 = 270 / 3 = 90 each
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 90) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 90) < 2);
        }

        // [CSS-FLEXBOX §9.7] 4 items in 400px with 20px gap = 85px each
        [Fact]
        public void EqualGrow_4Items_WithGap_400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;gap:20px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                    <div id='d' style='flex:1;height:30px'></div>
                </div></body>");
            // 400 - 3*20 = 340 / 4 = 85 each
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Width - 85) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Width - 85) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Width - 85) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Width - 85) < 2);
        }

        // [CSS-FLEXBOX §9.7] column direction: 2 items in 200px height = 100px each
        [Fact]
        public void EqualGrow_Column_2Items_200px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:200px;width:100px'>
                    <div id='a' style='flex:1'></div>
                    <div id='b' style='flex:1'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.height={itemA!.ContentRect.Height}, b.height={itemB!.ContentRect.Height}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 100) < 2,
                $"Column 2 items in 200px: each 100px (a={itemA.ContentRect.Height})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Height - 100) < 2,
                $"Column 2 items in 200px: each 100px (b={itemB.ContentRect.Height})");
        }

        // [CSS-FLEXBOX §9.7] column direction: 3 items in 300px height = 100px each
        [Fact]
        public void EqualGrow_Column_3Items_300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='a' style='flex:1'></div>
                    <div id='b' style='flex:1'></div>
                    <div id='c' style='flex:1'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] column direction: 4 items in 400px height = 100px each
        [Fact]
        public void EqualGrow_Column_4Items_400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:400px;width:100px'>
                    <div id='a' style='flex:1'></div>
                    <div id='b' style='flex:1'></div>
                    <div id='c' style='flex:1'></div>
                    <div id='d' style='flex:1'></div>
                </div></body>", viewportHeight: 500);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Height - 100) < 2);
        }

        // [CSS-FLEXBOX §9.7] 2 items X positions: a at 0, b at 150
        [Fact]
        public void EqualGrow_2Items_Positions_300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.x={itemA!.ContentRect.X}, b.x={itemB!.ContentRect.X}");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2,
                $"First item at X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 150) < 2,
                $"Second item at X=150 (got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] 3 items X positions: 0, 100, 200
        [Fact]
        public void EqualGrow_3Items_Positions_300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"a.x={itemA!.ContentRect.X}, b.x={itemB!.ContentRect.X}, c.x={itemC!.ContentRect.X}");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2,
                $"a at X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2,
                $"b at X=100 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 200) < 2,
                $"c at X=200 (got {itemC.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] 4 items X positions: 0, 100, 200, 300
        [Fact]
        public void EqualGrow_4Items_Positions_400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                    <div id='d' style='flex:1;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 300) < 2);
        }

        // [CSS-FLEXBOX §9.7] 5 items X positions: 0, 80, 160, 240, 320
        [Fact]
        public void EqualGrow_5Items_Positions_400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                    <div id='d' style='flex:1;height:30px'></div>
                    <div id='e' style='flex:1;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 80) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 160) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.X - 240) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "e")!.ContentRect.X - 320) < 2);
        }

        // [CSS-FLEXBOX §9.7] gap affects X positions: 2 items in 200px with 20px gap
        [Fact]
        public void EqualGrow_2Items_GapPositions_200px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px;gap:20px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.x={itemA!.ContentRect.X}, b.x={itemB!.ContentRect.X}");
            // a at 0, width 90, gap 20, b at 110
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2,
                $"a at X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 110) < 2,
                $"b at X=110 (got {itemB.ContentRect.X})");
        }

        // [CSS-FLEXBOX §9.7] gap affects X positions: 3 items in 300px with 15px gap
        [Fact]
        public void EqualGrow_3Items_GapPositions_300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;gap:15px'>
                    <div id='a' style='flex:1;height:30px'></div>
                    <div id='b' style='flex:1;height:30px'></div>
                    <div id='c' style='flex:1;height:30px'></div>
                </div></body>");
            // 300 - 2*15 = 270 / 3 = 90 each. Positions: 0, 105, 210
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.X - 105) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.X - 210) < 2);
        }

        // [CSS-FLEXBOX §9.7] column Y positions: 2 items in 200px height
        [Fact]
        public void EqualGrow_Column_2Items_Positions_200px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:200px;width:100px'>
                    <div id='a' style='flex:1'></div>
                    <div id='b' style='flex:1'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.y={itemA!.ContentRect.Y}, b.y={itemB!.ContentRect.Y}");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 0) < 2,
                $"a at Y=0 (got {itemA.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 100) < 2,
                $"b at Y=100 (got {itemB.ContentRect.Y})");
        }

        // [CSS-FLEXBOX §9.7] column Y positions: 3 items in 300px height
        [Fact]
        public void EqualGrow_Column_3Items_Positions_300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='a' style='flex:1'></div>
                    <div id='b' style='flex:1'></div>
                    <div id='c' style='flex:1'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 200) < 2);
        }

        // [CSS-FLEXBOX §9.7] column Y positions: 4 items in 400px height
        [Fact]
        public void EqualGrow_Column_4Items_Positions_400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:400px;width:100px'>
                    <div id='a' style='flex:1'></div>
                    <div id='b' style='flex:1'></div>
                    <div id='c' style='flex:1'></div>
                    <div id='d' style='flex:1'></div>
                </div></body>", viewportHeight: 500);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "a")!.ContentRect.Y - 0) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "b")!.ContentRect.Y - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "c")!.ContentRect.Y - 200) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(root, "d")!.ContentRect.Y - 300) < 2);
        }
    }
}
