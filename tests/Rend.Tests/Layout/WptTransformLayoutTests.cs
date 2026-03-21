using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests verifying that CSS transforms do not affect layout geometry.
    /// Per CSS Transforms Level 1, transforms are visual-only and must not
    /// influence box sizing, sibling positioning, or parent sizing.
    /// </summary>
    public class WptTransformLayoutTests
    {
        private readonly ITestOutputHelper _output;

        public WptTransformLayoutTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // [CSS-TRANSFORMS §2] transform does not affect flow layout of siblings
        [Fact]
        public void TranslateX_DoesNotMoveSibling()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:300px'>
                    <div style='transform:translateX(150px);width:100px;height:40px'></div>
                    <div id='sibling' style='width:100px;height:40px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(sibling);
            _output.WriteLine($"sibling X={sibling!.ContentRect.X} Y={sibling.ContentRect.Y}");
            Assert.True(System.Math.Abs(sibling.ContentRect.X - 0) < 2,
                $"TranslateX on prev sibling should not shift this element horizontally (X={sibling.ContentRect.X})");
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 40) < 2,
                $"Sibling should be at normal flow Y=40 (Y={sibling.ContentRect.Y})");
        }

        // [CSS-TRANSFORMS §2] translateY does not push subsequent siblings down
        [Fact]
        public void TranslateY_DoesNotMoveSibling()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div style='transform:translateY(100px);width:200px;height:30px'></div>
                    <div id='sibling' style='width:200px;height:30px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(sibling);
            _output.WriteLine($"sibling Y={sibling!.ContentRect.Y}");
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 30) < 2,
                $"TranslateY should not affect sibling Y (got {sibling.ContentRect.Y}, expected 30)");
        }

        // [CSS-TRANSFORMS §2] scale does not affect parent auto height
        [Fact]
        public void Scale_DoesNotAffectParentHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='transform:scale(3);width:50px;height:50px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent");
            Assert.NotNull(parent);
            _output.WriteLine($"parent height={parent!.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 50) < 2,
                $"scale(3) should not expand parent height (got {parent.ContentRect.Height}, expected 50)");
        }

        // [CSS-TRANSFORMS §2] translate does not affect parent auto height
        [Fact]
        public void TranslateY_DoesNotAffectParentHeight()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='transform:translateY(200px);width:100px;height:40px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent");
            Assert.NotNull(parent);
            _output.WriteLine($"parent height={parent!.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 40) < 2,
                $"translateY should not affect parent auto height (got {parent.ContentRect.Height}, expected 40)");
        }

        // [CSS-TRANSFORMS §6] any non-none transform creates stacking context
        [Fact]
        public void Transform_CreatesStackingContext()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='transformed' style='transform:translateX(0);width:100px;height:100px'></div></body>");
            var transformed = LayoutTestHelper.FindById(root, "transformed");
            Assert.NotNull(transformed);
            Assert.True(transformed!.EstablishesStackingContext,
                "Any transform value (even identity) should establish a stacking context");
        }

        // [CSS-TRANSFORMS §2] element without transform does not create stacking context
        [Fact]
        public void NoTransform_NoStackingContext()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='plain' style='width:100px;height:100px'></div></body>");
            var box = LayoutTestHelper.FindById(root, "plain");
            Assert.NotNull(box);
            Assert.False(box!.EstablishesStackingContext,
                "Element without transform should not create a stacking context");
        }

        // [CSS-TRANSFORMS §2] transform:none does not alter layout dimensions
        [Fact]
        public void TransformNone_DoesNotAffectDimensions()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:300px'>
                    <div id='box' style='transform:none;width:150px;height:60px'></div>
                    <div id='after' style='width:150px;height:40px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "box");
            var after = LayoutTestHelper.FindById(root, "after");
            Assert.NotNull(box);
            Assert.NotNull(after);
            Assert.True(System.Math.Abs(box!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(box.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(after!.ContentRect.Y - 60) < 2,
                $"Element after transform:none should be at Y=60 (got {after.ContentRect.Y})");
        }

        // [CSS-TRANSFORMS §2] transform on relatively positioned element
        [Fact]
        public void Transform_OnRelativePosition_DoesNotAffectSiblingLayout()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:300px'>
                    <div style='position:relative;top:10px;left:10px;transform:rotate(45deg);width:80px;height:80px'></div>
                    <div id='sibling' style='width:80px;height:40px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(sibling);
            _output.WriteLine($"sibling X={sibling!.ContentRect.X} Y={sibling.ContentRect.Y}");
            Assert.True(System.Math.Abs(sibling.ContentRect.X - 0) < 2,
                $"Sibling X unaffected by transform+relative (got {sibling.ContentRect.X})");
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 80) < 2,
                $"Sibling Y follows normal flow (got {sibling.ContentRect.Y}, expected 80)");
        }

        // [CSS-TRANSFORMS §2] transform on absolutely positioned element
        [Fact]
        public void Transform_OnAbsolutePosition_DoesNotAffectSiblingLayout()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='position:relative;width:300px;height:200px'>
                    <div style='position:absolute;top:0;left:0;transform:scale(2);width:60px;height:60px'></div>
                    <div id='sibling' style='width:100px;height:50px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(sibling);
            _output.WriteLine($"sibling X={sibling!.ContentRect.X} Y={sibling.ContentRect.Y}");
            Assert.True(System.Math.Abs(sibling.ContentRect.X - 0) < 2);
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 0) < 2,
                $"Abspos element with transform doesn't affect flow sibling (Y={sibling.ContentRect.Y})");
        }

        // [CSS-TRANSFORMS §2] transform on flex item does not change flex sizing
        [Fact]
        public void Transform_OnFlexItem_DoesNotChangeFlexSizing()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item1' style='flex:1;height:40px;transform:scale(0.5)'></div>
                    <div id='item2' style='flex:1;height:40px'></div>
                    <div id='item3' style='flex:1;height:40px'></div>
                </div></body>");
            var item1 = LayoutTestHelper.FindById(root, "item1");
            var item2 = LayoutTestHelper.FindById(root, "item2");
            var item3 = LayoutTestHelper.FindById(root, "item3");
            Assert.NotNull(item1);
            Assert.NotNull(item2);
            Assert.NotNull(item3);
            _output.WriteLine($"item1 W={item1!.ContentRect.Width} item2 W={item2!.ContentRect.Width} item3 W={item3!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item1.ContentRect.Width - 100) < 2,
                $"Flex item with transform should still get flex:1 width (got {item1.ContentRect.Width})");
            Assert.True(System.Math.Abs(item2.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(item3.ContentRect.Width - 100) < 2);
        }

        // [CSS-TRANSFORMS §2] transform on grid item does not change grid track sizing
        [Fact]
        public void Transform_OnGridItem_DoesNotChangeGridSizing()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:grid;grid-template-columns:1fr 1fr;width:200px'>
                    <div id='cell1' style='height:30px;transform:rotate(30deg)'></div>
                    <div id='cell2' style='height:30px'></div>
                </div></body>");
            var cell1 = LayoutTestHelper.FindById(root, "cell1");
            var cell2 = LayoutTestHelper.FindById(root, "cell2");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"cell1 W={cell1!.ContentRect.Width} cell2 W={cell2!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - 100) < 2,
                $"Grid item with transform should keep track width (got {cell1.ContentRect.Width})");
            Assert.True(System.Math.Abs(cell2.ContentRect.Width - 100) < 2);
        }

        // [CSS-TRANSFORMS §2] width and height of the box are not affected by transform
        [Fact]
        public void Scale_DoesNotChangeBoxDimensions()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='scaled' style='transform:scale(2);width:120px;height:80px'></div></body>");
            var scaled = LayoutTestHelper.FindById(root, "scaled");
            Assert.NotNull(scaled);
            _output.WriteLine($"scaled W={scaled!.ContentRect.Width} H={scaled.ContentRect.Height}");
            Assert.True(System.Math.Abs(scaled.ContentRect.Width - 120) < 2,
                $"scale(2) should not change layout width (got {scaled.ContentRect.Width})");
            Assert.True(System.Math.Abs(scaled.ContentRect.Height - 80) < 2,
                $"scale(2) should not change layout height (got {scaled.ContentRect.Height})");
        }

        // [CSS-TRANSFORMS §2] rotate does not change layout box size
        [Fact]
        public void Rotate_DoesNotChangeBoxDimensions()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='rotated' style='transform:rotate(45deg);width:100px;height:100px'></div></body>");
            var rotated = LayoutTestHelper.FindById(root, "rotated");
            Assert.NotNull(rotated);
            Assert.True(System.Math.Abs(rotated!.ContentRect.Width - 100) < 2,
                $"rotate(45deg) should not change layout width (got {rotated.ContentRect.Width})");
            Assert.True(System.Math.Abs(rotated.ContentRect.Height - 100) < 2,
                $"rotate(45deg) should not change layout height (got {rotated.ContentRect.Height})");
        }

        // [CSS-TRANSFORMS §3] transform-origin does not affect layout position
        [Fact]
        public void TransformOrigin_DoesNotAffectLayoutPosition()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:300px'>
                    <div id='origin' style='transform:rotate(45deg);transform-origin:0 0;width:80px;height:80px'></div>
                    <div id='sibling' style='width:80px;height:40px'></div>
                </div></body>");
            var origin = LayoutTestHelper.FindById(root, "origin");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(origin);
            Assert.NotNull(sibling);
            _output.WriteLine($"origin X={origin!.ContentRect.X} Y={origin.ContentRect.Y}");
            _output.WriteLine($"sibling X={sibling!.ContentRect.X} Y={sibling.ContentRect.Y}");
            Assert.True(System.Math.Abs(origin.ContentRect.X - 0) < 2,
                $"transform-origin should not move layout X (got {origin.ContentRect.X})");
            Assert.True(System.Math.Abs(origin.ContentRect.Y - 0) < 2,
                $"transform-origin should not move layout Y (got {origin.ContentRect.Y})");
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 80) < 2,
                $"Sibling after transform-origin element at normal Y (got {sibling.ContentRect.Y})");
        }

        // [CSS-TRANSFORMS §2] transform on inline-block does not affect line layout
        [Fact]
        public void Transform_OnInlineBlock_DoesNotAffectLineLayout()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div id='container' style='width:300px;font-size:0'><span style='display:inline-block;transform:translateX(50px);width:60px;height:30px'></span><span id='after' style='display:inline-block;width:60px;height:30px'></span></div></body>");
            var after = LayoutTestHelper.FindById(root, "after");
            Assert.NotNull(after);
            _output.WriteLine($"after X={after!.ContentRect.X}");
            Assert.True(System.Math.Abs(after.ContentRect.X - 60) < 2,
                $"Inline-block after translated inline-block at normal X (got {after.ContentRect.X}, expected 60)");
        }

        // [CSS-WILL-CHANGE §3] will-change:transform does not affect layout sizing
        [Fact]
        public void WillChangeTransform_DoesNotAffectLayout()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:300px'>
                    <div style='will-change:transform;width:100px;height:50px'></div>
                    <div id='sibling' style='width:100px;height:40px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(sibling);
            Assert.True(System.Math.Abs(sibling!.ContentRect.Y - 50) < 2,
                $"will-change:transform should not affect sibling layout (Y={sibling.ContentRect.Y})");
        }

        // [CSS-TRANSFORMS §5] backface-visibility does not affect layout
        [Fact]
        public void BackfaceVisibility_DoesNotAffectLayout()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:300px'>
                    <div id='hidden' style='backface-visibility:hidden;width:100px;height:50px'></div>
                    <div id='sibling' style='width:100px;height:50px'></div>
                </div></body>");
            var hidden = LayoutTestHelper.FindById(root, "hidden");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(hidden);
            Assert.NotNull(sibling);
            Assert.True(System.Math.Abs(hidden!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(hidden.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(sibling!.ContentRect.Y - 50) < 2,
                $"backface-visibility should not affect sibling layout (Y={sibling.ContentRect.Y})");
        }

        // [CSS-TRANSFORMS-2 §7] perspective on parent does not affect 2D child layout
        [Fact]
        public void Perspective_DoesNotAffect2DChildLayout()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='perspective:500px;width:300px'>
                    <div id='child1' style='width:100px;height:40px'></div>
                    <div id='child2' style='width:100px;height:40px'></div>
                </div></body>");
            var child1 = LayoutTestHelper.FindById(root, "child1");
            var child2 = LayoutTestHelper.FindById(root, "child2");
            Assert.NotNull(child1);
            Assert.NotNull(child2);
            Assert.True(System.Math.Abs(child1!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(child1.ContentRect.Height - 40) < 2);
            Assert.True(System.Math.Abs(child2!.ContentRect.Y - 40) < 2,
                $"perspective should not alter 2D child stacking (Y={child2.ContentRect.Y})");
        }

        // [CSS-TRANSFORMS-2 §4] transform-style:preserve-3d does not affect layout
        [Fact]
        public void TransformStyle_DoesNotAffectLayout()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='transform-style:preserve-3d;width:200px'>
                    <div id='child' style='width:120px;height:60px'></div>
                    <div id='sibling' style='width:120px;height:40px'></div>
                </div></body>");
            var child = LayoutTestHelper.FindById(root, "child");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(child);
            Assert.NotNull(sibling);
            Assert.True(System.Math.Abs(child!.ContentRect.Width - 120) < 2);
            Assert.True(System.Math.Abs(child.ContentRect.Height - 60) < 2);
            Assert.True(System.Math.Abs(sibling!.ContentRect.Y - 60) < 2,
                $"transform-style should not affect sibling layout (Y={sibling.ContentRect.Y})");
        }

        // [CSS-TRANSFORMS §2] multiple transforms combined do not affect layout
        [Fact]
        public void MultipleTransforms_DoNotAffectLayout()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div style='transform:translateX(50px) rotate(30deg) scale(1.5);width:100px;height:60px'></div>
                    <div id='sibling' style='width:100px;height:40px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(sibling);
            _output.WriteLine($"sibling X={sibling!.ContentRect.X} Y={sibling.ContentRect.Y}");
            Assert.True(System.Math.Abs(sibling.ContentRect.X - 0) < 2,
                $"Multiple transforms should not shift sibling X (got {sibling.ContentRect.X})");
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 60) < 2,
                $"Multiple transforms should not shift sibling Y (got {sibling.ContentRect.Y})");
        }

        // [CSS-TRANSFORMS §2] skew does not affect layout
        [Fact]
        public void Skew_DoesNotAffectLayout()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:300px'>
                    <div id='skewed' style='transform:skewX(30deg);width:100px;height:50px'></div>
                    <div id='sibling' style='width:100px;height:50px'></div>
                </div></body>");
            var skewed = LayoutTestHelper.FindById(root, "skewed");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(skewed);
            Assert.NotNull(sibling);
            Assert.True(System.Math.Abs(skewed!.ContentRect.Width - 100) < 2,
                $"skewX should not change width (got {skewed.ContentRect.Width})");
            Assert.True(System.Math.Abs(sibling!.ContentRect.Y - 50) < 2,
                $"skewX should not affect sibling Y (got {sibling.ContentRect.Y})");
        }

        // [CSS-TRANSFORMS §6] transform creates containing block for abspos descendants
        [Fact]
        public void Transform_CreatesContainingBlockForAbspos()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div id='outer' style='transform:translateX(0);width:200px;height:200px'>
                    <div id='abspos' style='position:absolute;top:10px;left:10px;width:50px;height:50px'></div>
                </div></body>");
            var outer = LayoutTestHelper.FindById(root, "outer");
            var abspos = LayoutTestHelper.FindById(root, "abspos");
            Assert.NotNull(outer);
            Assert.NotNull(abspos);
            _output.WriteLine($"outer X={outer!.ContentRect.X} abspos X={abspos!.ContentRect.X} Y={abspos.ContentRect.Y}");
            float expectedX = outer.ContentRect.X + 10;
            float expectedY = outer.ContentRect.Y + 10;
            Assert.True(System.Math.Abs(abspos.ContentRect.X - expectedX) < 2,
                $"Abspos child should position relative to transformed parent (X={abspos.ContentRect.X}, expected {expectedX})");
            Assert.True(System.Math.Abs(abspos.ContentRect.Y - expectedY) < 2,
                $"Abspos child should position relative to transformed parent (Y={abspos.ContentRect.Y}, expected {expectedY})");
        }

        // [CSS-TRANSFORMS §2] transform on flex container does not change children sizing
        [Fact]
        public void Transform_OnFlexContainer_DoesNotChangeChildrenSizing()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:flex;transform:rotate(10deg);width:300px'>
                    <div id='item1' style='flex:1;height:40px'></div>
                    <div id='item2' style='flex:2;height:40px'></div>
                </div></body>");
            var item1 = LayoutTestHelper.FindById(root, "item1");
            var item2 = LayoutTestHelper.FindById(root, "item2");
            Assert.NotNull(item1);
            Assert.NotNull(item2);
            _output.WriteLine($"item1 W={item1!.ContentRect.Width} item2 W={item2!.ContentRect.Width}");
            Assert.True(System.Math.Abs(item1.ContentRect.Width - 100) < 2,
                $"Flex item width should be 1/3 of 300 (got {item1.ContentRect.Width})");
            Assert.True(System.Math.Abs(item2.ContentRect.Width - 200) < 2,
                $"Flex item width should be 2/3 of 300 (got {item2.ContentRect.Width})");
        }

        // [CSS-TRANSFORMS §2] transform on grid container does not change track sizing
        [Fact]
        public void Transform_OnGridContainer_DoesNotChangeTrackSizing()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 1fr;transform:scale(0.8);width:300px'>
                    <div id='fixed' style='height:30px'></div>
                    <div id='flexible' style='height:30px'></div>
                </div></body>");
            var fixedCol = LayoutTestHelper.FindById(root, "fixed");
            var flexible = LayoutTestHelper.FindById(root, "flexible");
            Assert.NotNull(fixedCol);
            Assert.NotNull(flexible);
            _output.WriteLine($"fixed W={fixedCol!.ContentRect.Width} flexible W={flexible!.ContentRect.Width}");
            Assert.True(System.Math.Abs(fixedCol.ContentRect.Width - 100) < 2,
                $"Fixed grid column should be 100px (got {fixedCol.ContentRect.Width})");
            Assert.True(System.Math.Abs(flexible.ContentRect.Width - 200) < 2,
                $"Flexible grid column should be 200px (got {flexible.ContentRect.Width})");
        }

        // [CSS-TRANSFORMS §2] matrix transform does not affect layout
        [Fact]
        public void Matrix_DoesNotAffectLayout()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:300px'>
                    <div style='transform:matrix(1,0.5,-0.5,1,0,0);width:80px;height:60px'></div>
                    <div id='sibling' style='width:80px;height:40px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(sibling);
            Assert.True(System.Math.Abs(sibling!.ContentRect.Y - 60) < 2,
                $"matrix() transform should not shift sibling Y (got {sibling.ContentRect.Y})");
        }

        // [CSS-TRANSFORMS §2] scale(0) box still occupies layout space
        [Fact]
        public void ScaleZero_StillOccupiesLayoutSpace()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:200px'>
                    <div style='transform:scale(0);width:100px;height:70px'></div>
                    <div id='sibling' style='width:100px;height:30px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(sibling);
            _output.WriteLine($"sibling Y={sibling!.ContentRect.Y}");
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 70) < 2,
                $"scale(0) element should still occupy 70px of layout space (sibling Y={sibling.ContentRect.Y})");
        }

        // [CSS-TRANSFORMS §2] transform with padding and border does not alter border box size in layout
        [Fact]
        public void Transform_WithPaddingBorder_DoesNotAlterBorderBoxLayout()
        {
            var root = LayoutTestHelper.Layout(@"
                <body style='margin:0'>
                <div style='width:400px'>
                    <div id='box' style='transform:rotate(15deg);width:100px;height:60px;padding:10px;border:5px solid black'></div>
                    <div id='sibling' style='height:20px'></div>
                </div></body>");
            var box = LayoutTestHelper.FindById(root, "box");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(box);
            Assert.NotNull(sibling);
            float expectedBorderHeight = 60 + 10 + 10 + 5 + 5;
            _output.WriteLine($"box border height={box!.BorderRect.Height} sibling Y={sibling!.ContentRect.Y}");
            Assert.True(System.Math.Abs(box.BorderRect.Height - expectedBorderHeight) < 2,
                $"Border box height unchanged by transform (got {box.BorderRect.Height}, expected {expectedBorderHeight})");
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - expectedBorderHeight) < 2,
                $"Sibling after border+padding+transform at correct Y (got {sibling.ContentRect.Y})");
        }
    }
}
