using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexItemOrderTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexItemOrderTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void OrderDefault_SourceOrder_ThreeItems()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:300px'>
                    <div id='a' style='width:100px; height:30px'></div>
                    <div id='b' style='width:100px; height:30px'></div>
                    <div id='c' style='width:100px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"a.X={itemA!.ContentRect.X} b.X={itemB!.ContentRect.X} c.X={itemC!.ContentRect.X}");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 0) < 2, $"A at X=0 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 100) < 2, $"B at X=100 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC.ContentRect.X - 200) < 2, $"C at X=200 (got {itemC.ContentRect.X})");
        }

        [Fact]
        public void OrderOne_MovesAfterOrderZero()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:300px'>
                    <div id='first' style='order:1; width:100px; height:30px'></div>
                    <div id='second' style='width:100px; height:30px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            _output.WriteLine($"first.X={first!.ContentRect.X} second.X={second!.ContentRect.X}");
            Assert.True(second.ContentRect.X < first.ContentRect.X,
                $"order:0 item should appear before order:1 (second.X={second.ContentRect.X}, first.X={first.ContentRect.X})");
        }

        [Fact]
        public void OrderNegativeOne_MovesBeforeOrderZero()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:300px'>
                    <div id='normal' style='width:100px; height:30px'></div>
                    <div id='early' style='order:-1; width:100px; height:30px'></div>
                </div></body>");
            var normal = LayoutTestHelper.FindById(root, "normal");
            var early = LayoutTestHelper.FindById(root, "early");
            Assert.NotNull(normal);
            Assert.NotNull(early);
            _output.WriteLine($"normal.X={normal!.ContentRect.X} early.X={early!.ContentRect.X}");
            Assert.True(early.ContentRect.X < normal.ContentRect.X,
                $"order:-1 should appear before order:0 (early.X={early.ContentRect.X}, normal.X={normal.ContentRect.X})");
        }

        [Fact]
        public void OrderReorders_ThreeItems()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:300px'>
                    <div id='a' style='order:3; width:100px; height:30px'></div>
                    <div id='b' style='order:1; width:100px; height:30px'></div>
                    <div id='c' style='order:2; width:100px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"a.X={itemA!.ContentRect.X} b.X={itemB!.ContentRect.X} c.X={itemC!.ContentRect.X}");
            Assert.True(itemB.ContentRect.X < itemC!.ContentRect.X, "B(order:1) before C(order:2)");
            Assert.True(itemC.ContentRect.X < itemA.ContentRect.X, "C(order:2) before A(order:3)");
        }

        [Fact]
        public void OrderReorders_FiveItems()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:500px'>
                    <div id='a' style='order:5; width:50px; height:30px'></div>
                    <div id='b' style='order:2; width:50px; height:30px'></div>
                    <div id='c' style='order:4; width:50px; height:30px'></div>
                    <div id='d' style='order:1; width:50px; height:30px'></div>
                    <div id='e' style='order:3; width:50px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            var itemD = LayoutTestHelper.FindById(root, "d");
            var itemE = LayoutTestHelper.FindById(root, "e");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            Assert.NotNull(itemD);
            Assert.NotNull(itemE);
            _output.WriteLine($"d.X={itemD!.ContentRect.X} b.X={itemB!.ContentRect.X} e.X={itemE!.ContentRect.X} c.X={itemC!.ContentRect.X} a.X={itemA!.ContentRect.X}");
            Assert.True(itemD.ContentRect.X < itemB.ContentRect.X, "D(1) before B(2)");
            Assert.True(itemB.ContentRect.X < itemE.ContentRect.X, "B(2) before E(3)");
            Assert.True(itemE.ContentRect.X < itemC.ContentRect.X, "E(3) before C(4)");
            Assert.True(itemC.ContentRect.X < itemA.ContentRect.X, "C(4) before A(5)");
        }

        [Fact]
        public void SameOrder_UsesSourceOrder()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:300px'>
                    <div id='a' style='order:1; width:100px; height:30px'></div>
                    <div id='b' style='order:1; width:100px; height:30px'></div>
                    <div id='c' style='order:1; width:100px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"a.X={itemA!.ContentRect.X} b.X={itemB!.ContentRect.X} c.X={itemC!.ContentRect.X}");
            Assert.True(itemA.ContentRect.X < itemB!.ContentRect.X, "Same order preserves source: A before B");
            Assert.True(itemB.ContentRect.X < itemC!.ContentRect.X, "Same order preserves source: B before C");
        }

        [Fact]
        public void OrderWithFlexGrow_DistributesSpaceAfterReorder()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:300px'>
                    <div id='a' style='order:2; flex-grow:1; height:30px'></div>
                    <div id='b' style='order:1; width:100px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"b.X={itemB!.ContentRect.X} b.W={itemB.ContentRect.Width} a.X={itemA!.ContentRect.X} a.W={itemA.ContentRect.Width}");
            Assert.True(itemB.ContentRect.X < itemA.ContentRect.X,
                $"B(order:1) before A(order:2) (B.X={itemB.ContentRect.X}, A.X={itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 200) < 2,
                $"A flex-grow:1 fills remaining 200px (got {itemA.ContentRect.Width})");
        }

        [Fact]
        public void OrderInColumnDirection()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; flex-direction:column; width:100px; height:300px'>
                    <div id='a' style='order:2; height:50px'></div>
                    <div id='b' style='order:1; height:50px'></div>
                    <div id='c' style='order:3; height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"b.Y={itemB!.ContentRect.Y} a.Y={itemA!.ContentRect.Y} c.Y={itemC!.ContentRect.Y}");
            Assert.True(itemB.ContentRect.Y < itemA.ContentRect.Y, "B(order:1) above A(order:2)");
            Assert.True(itemA.ContentRect.Y < itemC!.ContentRect.Y, "A(order:2) above C(order:3)");
        }

        [Fact]
        public void OrderInRowReverse()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; flex-direction:row-reverse; width:300px'>
                    <div id='a' style='order:1; width:100px; height:30px'></div>
                    <div id='b' style='order:2; width:100px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.X={itemA!.ContentRect.X} b.X={itemB!.ContentRect.X}");
            // row-reverse starts from the right; order:1 placed first (rightmost), order:2 second
            Assert.True(itemA.ContentRect.X > itemB!.ContentRect.X,
                $"row-reverse: A(order:1) placed right of B(order:2) (A.X={itemA.ContentRect.X}, B.X={itemB.ContentRect.X})");
        }

        [Fact]
        public void OrderWithWrap_ReordersWithinLine()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; flex-wrap:wrap; width:200px'>
                    <div id='a' style='order:2; width:100px; height:30px'></div>
                    <div id='b' style='order:1; width:100px; height:30px'></div>
                    <div id='c' style='order:3; width:100px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"b.X={itemB!.ContentRect.X} b.Y={itemB.ContentRect.Y} a.X={itemA!.ContentRect.X} a.Y={itemA.ContentRect.Y} c.X={itemC!.ContentRect.X} c.Y={itemC.ContentRect.Y}");
            // Visual order: B(1), A(2), C(3). B and A fit on first line, C wraps.
            Assert.True(itemB.ContentRect.X < itemA.ContentRect.X, "B(order:1) before A(order:2) on first line");
            Assert.True(itemC.ContentRect.Y > itemB.ContentRect.Y, "C(order:3) wraps to second line");
        }

        [Fact]
        public void OrderWithJustifyContent_Center()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; justify-content:center; width:400px'>
                    <div id='a' style='order:2; width:50px; height:30px'></div>
                    <div id='b' style='order:1; width:50px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"b.X={itemB!.ContentRect.X} a.X={itemA!.ContentRect.X}");
            // Total content 100px, centered in 400px => offset ~150px
            Assert.True(itemB.ContentRect.X < itemA!.ContentRect.X, "B(order:1) before A(order:2)");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 150) < 2,
                $"B centered at X=150 (got {itemB.ContentRect.X})");
        }

        [Fact]
        public void OrderWithGap()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; gap:20px; width:400px'>
                    <div id='a' style='order:2; width:50px; height:30px'></div>
                    <div id='b' style='order:1; width:50px; height:30px'></div>
                    <div id='c' style='order:3; width:50px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"b.X={itemB!.ContentRect.X} a.X={itemA!.ContentRect.X} c.X={itemC!.ContentRect.X}");
            // Visual order: B(1) at 0, A(2) at 70, C(3) at 140
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 0) < 2, $"B at X=0 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 70) < 2, $"A at X=70 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC!.ContentRect.X - 140) < 2, $"C at X=140 (got {itemC.ContentRect.X})");
        }

        [Fact]
        public void NegativeOrderValues_SortCorrectly()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:400px'>
                    <div id='a' style='order:-1; width:50px; height:30px'></div>
                    <div id='b' style='order:-3; width:50px; height:30px'></div>
                    <div id='c' style='order:-2; width:50px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"b.X={itemB!.ContentRect.X} c.X={itemC!.ContentRect.X} a.X={itemA!.ContentRect.X}");
            // Visual order: B(-3), C(-2), A(-1)
            Assert.True(itemB.ContentRect.X < itemC.ContentRect.X, "B(-3) before C(-2)");
            Assert.True(itemC.ContentRect.X < itemA!.ContentRect.X, "C(-2) before A(-1)");
        }

        [Fact]
        public void LargePositiveOrder_PlacedLast()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:400px'>
                    <div id='a' style='order:9999; width:50px; height:30px'></div>
                    <div id='b' style='width:50px; height:30px'></div>
                    <div id='c' style='order:1; width:50px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"b.X={itemB!.ContentRect.X} c.X={itemC!.ContentRect.X} a.X={itemA!.ContentRect.X}");
            // Visual order: B(0), C(1), A(9999)
            Assert.True(itemB.ContentRect.X < itemC!.ContentRect.X, "B(0) before C(1)");
            Assert.True(itemC.ContentRect.X < itemA!.ContentRect.X, "C(1) before A(9999)");
        }

        [Fact]
        public void OrderDoesNotAffectAbspos()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; position:relative; width:300px; height:100px'>
                    <div id='normal' style='width:100px; height:30px'></div>
                    <div id='abspos' style='position:absolute; order:-1; top:10px; left:10px; width:50px; height:50px'></div>
                </div></body>");
            var normal = LayoutTestHelper.FindById(root, "normal");
            var abspos = LayoutTestHelper.FindById(root, "abspos");
            Assert.NotNull(normal);
            Assert.NotNull(abspos);
            _output.WriteLine($"normal.X={normal!.ContentRect.X} abspos.X={abspos!.ContentRect.X} abspos.Y={abspos.ContentRect.Y}");
            // Abspos is positioned by top/left, not flex order
            Assert.True(System.Math.Abs(abspos.ContentRect.X - 10) < 2,
                $"Abspos at left:10 (got {abspos.ContentRect.X})");
            Assert.True(System.Math.Abs(abspos.ContentRect.Y - 10) < 2,
                $"Abspos at top:10 (got {abspos.ContentRect.Y})");
            Assert.True(System.Math.Abs(normal.ContentRect.X - 0) < 2,
                $"Normal item at X=0 (got {normal.ContentRect.X})");
        }

        [Fact]
        public void OrderOnSingleItem_NoEffect()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:300px'>
                    <div id='only' style='order:5; width:100px; height:30px'></div>
                </div></body>");
            var only = LayoutTestHelper.FindById(root, "only");
            Assert.NotNull(only);
            _output.WriteLine($"only.X={only!.ContentRect.X} only.W={only.ContentRect.Width}");
            Assert.True(System.Math.Abs(only.ContentRect.X - 0) < 2,
                $"Single item at X=0 regardless of order (got {only.ContentRect.X})");
            Assert.True(System.Math.Abs(only.ContentRect.Width - 100) < 2,
                $"Single item width unchanged (got {only.ContentRect.Width})");
        }

        [Fact]
        public void OrderMixed_ZeroPositiveNegative()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:400px'>
                    <div id='a' style='order:0; width:50px; height:30px'></div>
                    <div id='b' style='order:1; width:50px; height:30px'></div>
                    <div id='c' style='order:-1; width:50px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"c.X={itemC!.ContentRect.X} a.X={itemA!.ContentRect.X} b.X={itemB!.ContentRect.X}");
            // Visual order: C(-1), A(0), B(1)
            Assert.True(itemC.ContentRect.X < itemA!.ContentRect.X, "C(-1) before A(0)");
            Assert.True(itemA.ContentRect.X < itemB!.ContentRect.X, "A(0) before B(1)");
        }

        [Fact]
        public void OrderWithFlexShrink()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:200px'>
                    <div id='a' style='order:2; width:150px; flex-shrink:1; height:30px'></div>
                    <div id='b' style='order:1; width:150px; flex-shrink:1; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"b.X={itemB!.ContentRect.X} b.W={itemB.ContentRect.Width} a.X={itemA!.ContentRect.X} a.W={itemA.ContentRect.Width}");
            Assert.True(itemB.ContentRect.X < itemA.ContentRect.X, "B(order:1) before A(order:2)");
            Assert.True(itemB.ContentRect.Width < 150, $"B should shrink (got {itemB.ContentRect.Width})");
            Assert.True(itemA.ContentRect.Width < 150, $"A should shrink (got {itemA.ContentRect.Width})");
        }

        [Fact]
        public void OrderInColumnReverse()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; flex-direction:column-reverse; width:100px; height:200px'>
                    <div id='a' style='order:1; height:50px'></div>
                    <div id='b' style='order:2; height:50px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.Y={itemA!.ContentRect.Y} b.Y={itemB!.ContentRect.Y}");
            // column-reverse starts from bottom; order:1 placed first (bottommost), order:2 second
            Assert.True(itemA.ContentRect.Y > itemB.ContentRect.Y,
                $"column-reverse: A(order:1) below B(order:2) (A.Y={itemA.ContentRect.Y}, B.Y={itemB.ContentRect.Y})");
        }

        [Fact]
        public void OrderWithJustifyContent_SpaceBetween()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; justify-content:space-between; width:400px'>
                    <div id='a' style='order:3; width:50px; height:30px'></div>
                    <div id='b' style='order:1; width:50px; height:30px'></div>
                    <div id='c' style='order:2; width:50px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"b.X={itemB!.ContentRect.X} c.X={itemC!.ContentRect.X} a.X={itemA!.ContentRect.X}");
            // Visual order: B(1), C(2), A(3). space-between: first at 0, last at 350
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 0) < 2, $"B at X=0 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA!.ContentRect.X - 350) < 2, $"A at X=350 (got {itemA.ContentRect.X})");
            Assert.True(itemC!.ContentRect.X > itemB.ContentRect.X, "C between B and A");
            Assert.True(itemC.ContentRect.X < itemA.ContentRect.X, "C between B and A");
        }

        [Fact]
        public void OrderWithAlignItems_Center()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; align-items:center; height:100px; width:300px'>
                    <div id='a' style='order:2; width:50px; height:20px'></div>
                    <div id='b' style='order:1; width:50px; height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"b.X={itemB!.ContentRect.X} b.Y={itemB.ContentRect.Y} a.X={itemA!.ContentRect.X} a.Y={itemA.ContentRect.Y}");
            Assert.True(itemB.ContentRect.X < itemA.ContentRect.X, "B(order:1) before A(order:2)");
            // Vertically centered: B(h=40) at Y=30, A(h=20) at Y=40
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 30) < 2,
                $"B centered at Y=30 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 40) < 2,
                $"A centered at Y=40 (got {itemA.ContentRect.Y})");
        }

        [Fact]
        public void OrderDefault_ExplicitZero_SameAsOmitted()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:300px'>
                    <div id='a' style='order:0; width:100px; height:30px'></div>
                    <div id='b' style='width:100px; height:30px'></div>
                    <div id='c' style='order:0; width:100px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"a.X={itemA!.ContentRect.X} b.X={itemB!.ContentRect.X} c.X={itemC!.ContentRect.X}");
            // All order:0 (explicit or default) — source order preserved
            Assert.True(itemA.ContentRect.X < itemB.ContentRect.X, "A before B in source order");
            Assert.True(itemB.ContentRect.X < itemC!.ContentRect.X, "B before C in source order");
        }

        [Fact]
        public void OrderNegativeLarge_PlacedFirst()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:400px'>
                    <div id='a' style='width:50px; height:30px'></div>
                    <div id='b' style='order:-9999; width:50px; height:30px'></div>
                    <div id='c' style='order:-1; width:50px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"b.X={itemB!.ContentRect.X} c.X={itemC!.ContentRect.X} a.X={itemA!.ContentRect.X}");
            // Visual order: B(-9999), C(-1), A(0)
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 0) < 2, $"B(-9999) at X=0 (got {itemB.ContentRect.X})");
            Assert.True(itemB.ContentRect.X < itemC!.ContentRect.X, "B(-9999) before C(-1)");
            Assert.True(itemC.ContentRect.X < itemA!.ContentRect.X, "C(-1) before A(0)");
        }

        [Fact]
        public void OrderWithFlexBasis()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:300px'>
                    <div id='a' style='order:2; flex:0 0 80px; height:30px'></div>
                    <div id='b' style='order:1; flex:0 0 120px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"b.X={itemB!.ContentRect.X} b.W={itemB.ContentRect.Width} a.X={itemA!.ContentRect.X} a.W={itemA.ContentRect.Width}");
            // Visual order: B(order:1) then A(order:2)
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 0) < 2, $"B at X=0 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 120) < 2, $"B width=120 (got {itemB.ContentRect.Width})");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 120) < 2, $"A at X=120 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Width - 80) < 2, $"A width=80 (got {itemA.ContentRect.Width})");
        }

        [Fact]
        public void OrderWithWrapReverse()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; flex-wrap:wrap-reverse; width:150px; height:200px'>
                    <div id='a' style='order:2; width:100px; height:40px'></div>
                    <div id='b' style='order:1; width:100px; height:40px'></div>
                    <div id='c' style='order:3; width:100px; height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"b.X={itemB!.ContentRect.X} b.Y={itemB.ContentRect.Y} a.X={itemA!.ContentRect.X} a.Y={itemA.ContentRect.Y} c.X={itemC!.ContentRect.X} c.Y={itemC.ContentRect.Y}");
            // Visual order: B(1), A(2), C(3). Each 100px wide in 150px container, so one per line.
            // wrap-reverse: first line at bottom
            Assert.True(itemB.ContentRect.Y > itemA!.ContentRect.Y, "wrap-reverse: B(first) line below A(second) line");
            Assert.True(itemA.ContentRect.Y > itemC!.ContentRect.Y, "wrap-reverse: A(second) line below C(third) line");
        }

        [Fact]
        public void OrderPositions_FourItems_WithMixedValues()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:400px'>
                    <div id='a' style='order:0; width:50px; height:30px'></div>
                    <div id='b' style='order:-2; width:50px; height:30px'></div>
                    <div id='c' style='order:3; width:50px; height:30px'></div>
                    <div id='d' style='order:-1; width:50px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            var itemD = LayoutTestHelper.FindById(root, "d");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            Assert.NotNull(itemD);
            _output.WriteLine($"b.X={itemB!.ContentRect.X} d.X={itemD!.ContentRect.X} a.X={itemA!.ContentRect.X} c.X={itemC!.ContentRect.X}");
            // Visual order: B(-2), D(-1), A(0), C(3)
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 0) < 2, $"B(-2) at X=0 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemD.ContentRect.X - 50) < 2, $"D(-1) at X=50 (got {itemD.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 100) < 2, $"A(0) at X=100 (got {itemA.ContentRect.X})");
            Assert.True(System.Math.Abs(itemC!.ContentRect.X - 150) < 2, $"C(3) at X=150 (got {itemC.ContentRect.X})");
        }

        [Fact]
        public void OrderColumnWithGap()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; flex-direction:column; gap:10px; width:100px'>
                    <div id='a' style='order:2; height:40px'></div>
                    <div id='b' style='order:1; height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"b.Y={itemB!.ContentRect.Y} a.Y={itemA!.ContentRect.Y}");
            // Visual order: B(1) at Y=0, A(2) at Y=50 (40+10 gap)
            Assert.True(System.Math.Abs(itemB.ContentRect.Y - 0) < 2, $"B at Y=0 (got {itemB.ContentRect.Y})");
            Assert.True(System.Math.Abs(itemA.ContentRect.Y - 50) < 2, $"A at Y=50 (got {itemA.ContentRect.Y})");
        }

        [Fact]
        public void OrderWithFlexGrow_EqualGrow()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:300px'>
                    <div id='a' style='order:2; flex-grow:1; height:30px'></div>
                    <div id='b' style='order:1; flex-grow:1; height:30px'></div>
                    <div id='c' style='order:3; flex-grow:1; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            _output.WriteLine($"b.X={itemB!.ContentRect.X} b.W={itemB.ContentRect.Width} a.X={itemA!.ContentRect.X} a.W={itemA.ContentRect.Width} c.X={itemC!.ContentRect.X} c.W={itemC.ContentRect.Width}");
            // Visual order: B(1), A(2), C(3), each ~100px wide
            Assert.True(itemB.ContentRect.X < itemA.ContentRect.X, "B(1) before A(2)");
            Assert.True(itemA.ContentRect.X < itemC!.ContentRect.X, "A(2) before C(3)");
            Assert.True(System.Math.Abs(itemB.ContentRect.Width - 100) < 2,
                $"B grows to 100px (got {itemB.ContentRect.Width})");
        }

        [Fact]
        public void OrderPreservesHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:300px'>
                    <div id='a' style='order:2; width:100px; height:60px'></div>
                    <div id='b' style='order:1; width:100px; height:40px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"a.H={itemA!.ContentRect.Height} b.H={itemB!.ContentRect.Height}");
            // Heights should be as specified (align-items:stretch may stretch B to match A)
            Assert.True(System.Math.Abs(itemA.ContentRect.Height - 60) < 2,
                $"A height=60 (got {itemA.ContentRect.Height})");
        }

        [Fact]
        public void OrderWithMargin()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:400px'>
                    <div id='a' style='order:2; width:50px; height:30px; margin-left:10px'></div>
                    <div id='b' style='order:1; width:50px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            _output.WriteLine($"b.X={itemB!.ContentRect.X} a.X={itemA!.ContentRect.X}");
            // Visual order: B(1) at X=0, then A(2) at X=60 (50 + 10 margin-left)
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 0) < 2, $"B at X=0 (got {itemB.ContentRect.X})");
            Assert.True(System.Math.Abs(itemA.ContentRect.X - 60) < 2,
                $"A at X=60 (50 + 10 margin) (got {itemA.ContentRect.X})");
        }

        [Fact]
        public void OrderTiebreaker_SourceOrder_FiveItems()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex; width:500px'>
                    <div id='a' style='order:1; width:50px; height:30px'></div>
                    <div id='b' style='order:0; width:50px; height:30px'></div>
                    <div id='c' style='order:1; width:50px; height:30px'></div>
                    <div id='d' style='order:0; width:50px; height:30px'></div>
                    <div id='e' style='order:1; width:50px; height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a");
            var itemB = LayoutTestHelper.FindById(root, "b");
            var itemC = LayoutTestHelper.FindById(root, "c");
            var itemD = LayoutTestHelper.FindById(root, "d");
            var itemE = LayoutTestHelper.FindById(root, "e");
            Assert.NotNull(itemA);
            Assert.NotNull(itemB);
            Assert.NotNull(itemC);
            Assert.NotNull(itemD);
            Assert.NotNull(itemE);
            _output.WriteLine($"b.X={itemB!.ContentRect.X} d.X={itemD!.ContentRect.X} a.X={itemA!.ContentRect.X} c.X={itemC!.ContentRect.X} e.X={itemE!.ContentRect.X}");
            // Visual order: B(0,src2), D(0,src4), A(1,src1), C(1,src3), E(1,src5)
            Assert.True(itemB.ContentRect.X < itemD.ContentRect.X, "B(0) before D(0) by source");
            Assert.True(itemD.ContentRect.X < itemA!.ContentRect.X, "D(0) before A(1)");
            Assert.True(itemA.ContentRect.X < itemC!.ContentRect.X, "A(1) before C(1) by source");
            Assert.True(itemC.ContentRect.X < itemE!.ContentRect.X, "C(1) before E(1) by source");
        }
    }
}
