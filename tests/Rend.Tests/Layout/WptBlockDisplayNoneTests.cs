using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for display:none behavior per CSS Display Module Level 3.
    /// Verifies that display:none elements are excluded from the layout tree,
    /// and contrasts with visibility:hidden and opacity:0 which still occupy space.
    /// </summary>
    public class WptBlockDisplayNoneTests
    {
        private readonly ITestOutputHelper _output;

        public WptBlockDisplayNoneTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void DisplayNone_ElementNotInLayoutTree()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='gone' style='display:none;width:100px;height:50px'></div>
                </div></body>");
            var gone = LayoutTestHelper.FindById(root, "gone");
            Assert.Null(gone);
        }

        [Fact]
        public void DisplayNone_DoesNotAffectSiblingY()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:40px'></div>
                    <div style='display:none;height:200px'></div>
                    <div id='after' style='height:30px'></div>
                </div></body>");
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"after.Y={after.ContentRect.Y}");
            Assert.True(System.Math.Abs(after.ContentRect.Y - 40) < 2,
                $"Sibling Y should be 40, got {after.ContentRect.Y}");
        }

        [Fact]
        public void DisplayNone_DoesNotAffectParentHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;overflow:hidden'>
                    <div style='height:30px'></div>
                    <div style='display:none;height:500px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"parent.Height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 30) < 2,
                $"Parent height should be 30, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void DisplayNone_WithExplicitWidthHeight_StillNull()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='gone' style='display:none;width:300px;height:400px'></div>
                </div></body>");
            Assert.Null(LayoutTestHelper.FindById(root, "gone"));
        }

        [Fact]
        public void DisplayNone_InFlexContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='a' style='width:50px;height:30px'></div>
                    <div id='hidden' style='display:none;width:50px;height:30px'></div>
                    <div id='b' style='width:50px;height:30px'></div>
                </div></body>");
            Assert.Null(LayoutTestHelper.FindById(root, "hidden"));
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.X={itemA.ContentRect.X} b.X={itemB.ContentRect.X}");
            Assert.True(System.Math.Abs(itemB.ContentRect.X - 50) < 2,
                $"B should be at X=50, got {itemB.ContentRect.X}");
        }

        [Fact]
        public void DisplayNone_InGridContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div id='a' style='height:30px'></div>
                    <div style='display:none;height:30px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            _output.WriteLine($"a.X={itemA.ContentRect.X} b.X={itemB.ContentRect.X}");
            // With one item gone, the remaining items fill the grid slots in order
            Assert.True(itemA.ContentRect.X < 2, $"A should be at X=0, got {itemA.ContentRect.X}");
        }

        [Fact]
        public void DisplayNone_WithChildren_AllInvisible()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='display:none'>
                        <div id='child1' style='width:100px;height:50px'></div>
                        <div id='child2' style='width:100px;height:50px'></div>
                        <div id='child3' style='width:100px;height:50px'></div>
                    </div>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            Assert.Null(LayoutTestHelper.FindById(root, "child1"));
            Assert.Null(LayoutTestHelper.FindById(root, "child2"));
            Assert.Null(LayoutTestHelper.FindById(root, "child3"));
            Assert.True(LayoutTestHelper.FindById(root, "after")!.ContentRect.Y < 2,
                "After should be at top since display:none parent hides all children");
        }

        [Fact]
        public void VisibilityHidden_TakesSpace_ContrastWithDisplayNone()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='visibility:hidden;height:60px'></div>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"after.Y={after.ContentRect.Y}");
            Assert.True(System.Math.Abs(after.ContentRect.Y - 60) < 2,
                $"visibility:hidden takes space, after.Y should be 60, got {after.ContentRect.Y}");
        }

        [Fact]
        public void VisibilityHidden_SiblingYOffset()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:20px'></div>
                    <div style='visibility:hidden;height:40px'></div>
                    <div id='sibling' style='height:20px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling")!;
            _output.WriteLine($"sibling.Y={sibling.ContentRect.Y}");
            Assert.True(System.Math.Abs(sibling.ContentRect.Y - 60) < 2,
                $"Sibling Y should be 60, got {sibling.ContentRect.Y}");
        }

        [Fact]
        public void Opacity0_TakesSpace_ContrastWithDisplayNone()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='opacity:0;height:70px'></div>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"after.Y={after.ContentRect.Y}");
            Assert.True(System.Math.Abs(after.ContentRect.Y - 70) < 2,
                $"opacity:0 takes space, after.Y should be 70, got {after.ContentRect.Y}");
        }

        [Fact]
        public void DisplayNone_BetweenSiblings()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='first' style='height:25px'></div>
                    <div style='display:none;height:100px'></div>
                    <div id='second' style='height:25px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"first.Y={first.ContentRect.Y} second.Y={second.ContentRect.Y}");
            Assert.True(System.Math.Abs(second.ContentRect.Y - 25) < 2,
                $"Second should be right after first at Y=25, got {second.ContentRect.Y}");
        }

        [Fact]
        public void MultipleDisplayNone_NoAccumulatedSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='display:none;height:100px'></div>
                    <div style='display:none;height:200px'></div>
                    <div style='display:none;height:300px'></div>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"after.Y={after.ContentRect.Y}");
            Assert.True(after.ContentRect.Y < 2,
                $"Multiple display:none should take no space, got Y={after.ContentRect.Y}");
        }

        [Fact]
        public void DisplayNone_FirstChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;overflow:hidden'>
                    <div style='display:none;height:50px'></div>
                    <div id='visible' style='height:30px'></div>
                </div></body>");
            var visible = LayoutTestHelper.FindById(root, "visible")!;
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"visible.Y={visible.ContentRect.Y} parent.Height={parent.ContentRect.Height}");
            Assert.True(visible.ContentRect.Y < 2,
                $"Visible should be at top, got Y={visible.ContentRect.Y}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 30) < 2,
                $"Parent height should be 30, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void DisplayNone_LastChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;overflow:hidden'>
                    <div id='visible' style='height:30px'></div>
                    <div style='display:none;height:50px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"parent.Height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 30) < 2,
                $"Parent height should be 30, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void DisplayNone_MiddleChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='first' style='height:20px'></div>
                    <div style='display:none;height:100px'></div>
                    <div id='last' style='height:20px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var last = LayoutTestHelper.FindById(root, "last")!;
            float gap = last.ContentRect.Y - (first.ContentRect.Y + first.ContentRect.Height);
            _output.WriteLine($"first.Y={first.ContentRect.Y} last.Y={last.ContentRect.Y} gap={gap}");
            Assert.True(gap < 2,
                $"No gap between first and last, got {gap}");
        }

        [Fact]
        public void DisplayNone_OnFlexContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='display:none'>
                        <div id='flexchild' style='width:50px;height:30px'></div>
                    </div>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            Assert.Null(LayoutTestHelper.FindById(root, "flexchild"));
            Assert.True(LayoutTestHelper.FindById(root, "after")!.ContentRect.Y < 2);
        }

        [Fact]
        public void DisplayNone_OnGridContainer()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='display:none'>
                        <div id='gridchild' style='width:50px;height:30px'></div>
                    </div>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            Assert.Null(LayoutTestHelper.FindById(root, "gridchild"));
            Assert.True(LayoutTestHelper.FindById(root, "after")!.ContentRect.Y < 2);
        }

        [Fact]
        public void DisplayNone_OnTable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <table style='display:none'>
                        <tr><td id='cell'>Content</td></tr>
                    </table>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            Assert.Null(LayoutTestHelper.FindById(root, "cell"));
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"after.Y={after.ContentRect.Y}");
            Assert.True(after.ContentRect.Y < 2,
                $"After should be at top, got Y={after.ContentRect.Y}");
        }

        [Fact]
        public void DisplayNone_DeepNestedChildren()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='display:none'>
                        <div>
                            <div>
                                <div id='deep' style='width:50px;height:50px'></div>
                            </div>
                        </div>
                    </div>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            Assert.Null(LayoutTestHelper.FindById(root, "deep"));
            Assert.True(LayoutTestHelper.FindById(root, "after")!.ContentRect.Y < 2);
        }

        [Fact]
        public void DisplayNone_WithMargin_NoEffect()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div style='height:20px'></div>
                    <div style='display:none;margin:50px;height:100px'></div>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"after.Y={after.ContentRect.Y}");
            Assert.True(System.Math.Abs(after.ContentRect.Y - 20) < 2,
                $"Margin on display:none should have no effect, got Y={after.ContentRect.Y}");
        }

        [Fact]
        public void DisplayNone_WithPadding_NoEffect()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;overflow:hidden'>
                    <div style='display:none;padding:30px;height:100px'></div>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"parent.Height={parent.ContentRect.Height} after.Y={after.ContentRect.Y}");
            Assert.True(after.ContentRect.Y < 2,
                $"Padding on display:none should have no effect, got Y={after.ContentRect.Y}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 20) < 2,
                $"Parent height should be 20, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void DisplayNone_WithBorder_NoEffect()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;overflow:hidden'>
                    <div style='display:none;border:10px solid red;height:100px'></div>
                    <div id='after' style='height:20px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            var after = LayoutTestHelper.FindById(root, "after")!;
            _output.WriteLine($"parent.Height={parent.ContentRect.Height} after.Y={after.ContentRect.Y}");
            Assert.True(after.ContentRect.Y < 2,
                $"Border on display:none should have no effect, got Y={after.ContentRect.Y}");
        }

        [Fact]
        public void DisplayNone_FlexItemSkipped_GapNotApplied()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;gap:10px;width:200px'>
                    <div id='a' style='width:40px;height:30px'></div>
                    <div style='display:none;width:40px;height:30px'></div>
                    <div id='b' style='width:40px;height:30px'></div>
                </div></body>");
            var itemA = LayoutTestHelper.FindById(root, "a")!;
            var itemB = LayoutTestHelper.FindById(root, "b")!;
            float spacing = itemB.ContentRect.X - (itemA.ContentRect.X + itemA.ContentRect.Width);
            _output.WriteLine($"a.X={itemA.ContentRect.X} b.X={itemB.ContentRect.X} spacing={spacing}");
            // Only one gap between two visible items
            Assert.True(System.Math.Abs(spacing - 10) < 2,
                $"Gap should be 10 (one gap, hidden item skipped), got {spacing}");
        }

        [Fact]
        public void DisplayNone_GridItemSkipped_SlotReused()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:grid;grid-template-columns:100px 100px;width:200px'>
                    <div style='display:none;height:30px'></div>
                    <div id='first' style='height:30px'></div>
                    <div id='second' style='height:30px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first")!;
            var second = LayoutTestHelper.FindById(root, "second")!;
            _output.WriteLine($"first.X={first.ContentRect.X} second.X={second.ContentRect.X}");
            // display:none item is skipped, so first visible item takes first slot
            Assert.True(first.ContentRect.X < 2,
                $"First visible should be at X=0, got {first.ContentRect.X}");
        }

        [Fact]
        public void VisibilityHidden_ElementStillInLayoutTree()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='hidden' style='visibility:hidden;width:100px;height:50px'></div>
                </div></body>");
            var hidden = LayoutTestHelper.FindById(root, "hidden");
            Assert.NotNull(hidden);
            Assert.True(System.Math.Abs(hidden!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(hidden.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void Opacity0_ElementStillInLayoutTree()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='width:200px'>
                    <div id='transparent' style='opacity:0;width:100px;height:50px'></div>
                </div></body>");
            var transparent = LayoutTestHelper.FindById(root, "transparent");
            Assert.NotNull(transparent);
            Assert.True(System.Math.Abs(transparent!.ContentRect.Width - 100) < 2);
            Assert.True(System.Math.Abs(transparent.ContentRect.Height - 50) < 2);
        }

        [Fact]
        public void DisplayNone_ParentAutoHeight_ExcludesHiddenChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;overflow:hidden'>
                    <div style='height:20px'></div>
                    <div style='display:none;height:300px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"parent.Height={parent.ContentRect.Height}");
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 40) < 2,
                $"Parent auto height should be 40, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void DisplayNone_AllChildrenHidden_ParentHeightZero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;overflow:hidden'>
                    <div style='display:none;height:100px'></div>
                    <div style='display:none;height:200px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            _output.WriteLine($"parent.Height={parent.ContentRect.Height}");
            Assert.True(parent.ContentRect.Height < 2,
                $"Parent height should be 0 when all children are display:none, got {parent.ContentRect.Height}");
        }
    }
}
