using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexColumnTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexColumnTests(ITestOutputHelper output) { _output = output; }

        // column flex: items stack vertically, fill container width
        [Fact]
        public void Column_ItemsFillWidth()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:250px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 250) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 250) < 2);
        }

        // column flex: explicit height distributes with flex:1
        [Fact]
        public void Column_FlexGrow_Distributes()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='a' style='flex:1'></div>
                    <div id='b' style='flex:2'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Height - 100) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Height - 200) < 2);
        }

        // column flex: auto height = sum of items
        [Fact]
        public void Column_AutoHeight_SumsItems()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-direction:column;width:100px'>
                    <div style='height:40px'></div>
                    <div style='height:60px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "flex")!.ContentRect.Height - 100) < 2);
        }

        // column flex: auto height with gap
        [Fact]
        public void Column_AutoHeight_WithGap()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='flex' style='display:flex;flex-direction:column;gap:20px;width:100px'>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                    <div style='height:40px'></div>
                </div></body>");
            // 40*3 + 20*2 = 160
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "flex")!.ContentRect.Height - 160) < 2);
        }

        // column flex: flex-basis percentage resolves against container height
        [Fact]
        public void Column_FlexBasisPercent()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:200px;width:100px'>
                    <div id='t' style='flex:0 0 50%'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height - 100) < 2);
        }

        // column flex: cross-axis (width) alignment center
        [Fact]
        public void Column_AlignItems_Center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;align-items:center;width:200px'>
                    <div id='t' style='width:80px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 60) < 2,
                $"center X=60 (got {LayoutTestHelper.FindById(r, "t")!.ContentRect.X})");
        }

        // column flex: cross-axis alignment flex-end
        [Fact]
        public void Column_AlignItems_FlexEnd()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;align-items:flex-end;width:200px'>
                    <div id='t' style='width:80px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 120) < 2);
        }

        // column flex: justify-content center
        [Fact]
        public void Column_JustifyContent_Center()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;justify-content:center;height:200px;width:100px'>
                    <div id='t' style='height:60px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 70) < 2);
        }

        // column flex: justify-content flex-end
        [Fact]
        public void Column_JustifyContent_FlexEnd()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;justify-content:flex-end;height:200px;width:100px'>
                    <div id='t' style='height:60px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 140) < 2);
        }

        // column flex: justify-content space-between
        [Fact]
        public void Column_JustifyContent_SpaceBetween()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;justify-content:space-between;height:200px;width:100px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Y - 170) < 2);
        }

        // column-reverse: items reversed
        [Fact]
        public void ColumnReverse_ItemsReversed()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column-reverse;height:200px;width:100px'>
                    <div id='a' style='height:30px'></div>
                    <div id='b' style='height:30px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "a")!.ContentRect.Y > LayoutTestHelper.FindById(r, "b")!.ContentRect.Y);
        }

        // column flex: shrink with explicit height
        [Fact]
        public void Column_Shrink_WithExplicitHeight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:100px;width:100px'>
                    <div id='a' style='flex-shrink:1;height:80px'></div>
                    <div id='b' style='flex-shrink:1;height:80px'></div>
                </div></body>");
            // Overflow = 60. Each shrinks by 30. a=50, b=50.
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Height - 50) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Height - 50) < 2);
        }

        // column flex: min-height:0 allows shrink below content
        [Fact]
        public void Column_MinHeight0_AllowsShrink()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:50px;width:100px'>
                    <div id='t' style='flex-shrink:1;min-height:0'>
                        <div style='height:200px'></div>
                    </div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height <= 51);
        }

        // column flex: max-height constrains item
        [Fact]
        public void Column_MaxHeight_Constrains()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:300px;width:100px'>
                    <div id='t' style='flex:1;max-height:80px'></div>
                </div></body>");
            Assert.True(LayoutTestHelper.FindById(r, "t")!.ContentRect.Height <= 81);
        }

        // column flex: nested row flex inside column
        [Fact]
        public void Column_NestedRowFlex()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:300px'>
                    <div style='display:flex'>
                        <div id='a' style='flex:1;height:30px'></div>
                        <div id='b' style='flex:1;height:30px'></div>
                    </div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "a")!.ContentRect.Width - 150) < 2);
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "b")!.ContentRect.Width - 150) < 2);
        }

        // column flex: margin auto on cross axis pushes right
        [Fact]
        public void Column_CrossMarginAuto_PushesRight()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px'>
                    <div id='t' style='margin-left:auto;width:60px;height:30px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.X - 140) < 2);
        }

        // column flex: margin auto on main axis centers vertically
        [Fact]
        public void Column_MainMarginAuto_Centers()
        {
            var r = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;height:200px;width:100px'>
                    <div id='t' style='margin-top:auto;margin-bottom:auto;height:40px'></div>
                </div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r, "t")!.ContentRect.Y - 80) < 2);
        }
    }
}
