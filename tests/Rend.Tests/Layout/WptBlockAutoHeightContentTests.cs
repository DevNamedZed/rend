using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests for CSS 2.1 §10.6.3: auto height computation from content
    /// for block-level elements in normal flow.
    /// </summary>
    public class WptBlockAutoHeightContentTests
    {
        private readonly ITestOutputHelper _output;
        public WptBlockAutoHeightContentTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void AutoHeight_SingleChild_50px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='height:50px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 50) < 1,
                $"Expected 50, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_TwoChildren_30Plus40()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='height:30px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 70) < 1,
                $"Expected 70, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_ThreeChildren_20Plus30Plus40()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='height:20px'></div>
                    <div style='height:30px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 90) < 1,
                $"Expected 90, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_FourChildren_25Each()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='height:25px'></div>
                    <div style='height:25px'></div>
                    <div style='height:25px'></div>
                    <div style='height:25px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 100) < 1,
                $"Expected 100, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_FiveChildren_20Each()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='height:20px'></div>
                    <div style='height:20px'></div>
                    <div style='height:20px'></div>
                    <div style='height:20px'></div>
                    <div style='height:20px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 100) < 1,
                $"Expected 100, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_NestedChildPropagates()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div>
                        <div style='height:60px'></div>
                    </div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 60) < 1,
                $"Expected 60, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_DisplayNoneExcluded()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='height:40px'></div>
                    <div style='display:none;height:100px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 70) < 1,
                $"Expected 70, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_VisibilityHiddenIncluded()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='height:40px'></div>
                    <div style='visibility:hidden;height:60px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 100) < 1,
                $"Expected 100, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_AbsposExcluded()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;position:relative'>
                    <div style='height:30px'></div>
                    <div style='position:absolute;height:200px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 30) < 1,
                $"Expected 30, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_WithPadding_ContentUnchanged()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;padding:20px'>
                    <div style='height:50px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 50) < 1,
                $"Content height expected 50, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_WithBorder_ContentUnchanged()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;border:10px solid black'>
                    <div style='height:50px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 50) < 1,
                $"Content height expected 50, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_BfcContainsFloats()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;overflow:hidden'>
                    <div style='float:left;width:80px;height:90px'></div>
                    <div style='height:30px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(parent.ContentRect.Height >= 89,
                $"BFC should contain float, expected >=89, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_EmptyContainer_Zero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'></div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(parent.ContentRect.Height < 1,
                $"Expected 0, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_WithFlexChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='display:flex;height:80px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 80) < 1,
                $"Expected 80, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_WithGridChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='display:grid;height:70px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 70) < 1,
                $"Expected 70, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_WithInlineBlockChild()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <span style='display:inline-block;width:50px;height:45px'></span>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(parent.ContentRect.Height >= 44,
                $"Expected >=44 for inline-block child, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_ExplicitHeightOverrides()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;height:150px'>
                    <div style='height:50px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 150) < 1,
                $"Expected 150 (explicit), got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_MinHeightEnforced()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;min-height:120px'>
                    <div style='height:30px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(parent.ContentRect.Height >= 119,
                $"Expected >=119 (min-height enforced), got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_MaxHeightClamps()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;max-height:60px'>
                    <div style='height:200px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(parent.ContentRect.Height <= 61,
                $"Expected <=61 (max-height clamps), got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_CalcHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;height:calc(50px + 30px)'>
                    <div style='height:10px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 80) < 1,
                $"Expected 80 (calc), got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_VhHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;height:50vh'>
                    <div style='height:10px'></div>
                </div></body>", viewportHeight: 300);
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 150) < 1,
                $"Expected 150 (50vh of 300), got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_WithMarginOnChildren()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;overflow:hidden'>
                    <div style='height:30px;margin-bottom:20px'></div>
                    <div style='height:30px;margin-top:10px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            // Margins collapse: max(20,10)=20. Total = 30+20+30 = 80.
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 80) < 2,
                $"Expected 80 (collapsed margins), got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_DeepNesting_ThreeLevels()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div>
                        <div>
                            <div style='height:75px'></div>
                        </div>
                    </div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 75) < 1,
                $"Expected 75, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_MixedDisplayNoneAndVisible()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px'>
                    <div style='display:none;height:999px'></div>
                    <div style='height:25px'></div>
                    <div style='display:none;height:999px'></div>
                    <div style='height:25px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 50) < 1,
                $"Expected 50, got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_AbsposAndInFlowMixed()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:200px;position:relative'>
                    <div style='height:40px'></div>
                    <div style='position:absolute;top:0;height:500px'></div>
                    <div style='height:35px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 75) < 1,
                $"Expected 75 (abspos excluded), got {parent.ContentRect.Height}");
        }

        [Fact]
        public void AutoHeight_WithPaddingAndBorder_ContentUnchanged()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='parent' style='width:300px;padding:15px;border:5px solid black'>
                    <div style='height:60px'></div>
                </div></body>");
            var parent = LayoutTestHelper.FindById(root, "parent")!;
            Assert.True(System.Math.Abs(parent.ContentRect.Height - 60) < 1,
                $"Content height expected 60, got {parent.ContentRect.Height}");
        }
    }
}
