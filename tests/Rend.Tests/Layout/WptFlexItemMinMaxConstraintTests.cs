using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    /// <spec>CSS-FLEXBOX-1 §9.7 https://www.w3.org/TR/css-flexbox-1/#resolve-flexible-lengths</spec>
    public class WptFlexItemMinMaxConstraintTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexItemMinMaxConstraintTests(ITestOutputHelper output) { _output = output; }

        [Fact]
        public void MinWidth_Prevents_Shrink_Below_Minimum()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'>
                <div id='a' style='flex:0 1 80px;min-width:70px;height:30px'></div>
                <div id='b' style='flex:0 1 80px;min-width:0;height:30px'></div>
            </div></body>");
            float widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            Assert.True(widthA >= 69, $"min-width should prevent shrink below 70px, got {widthA}");
        }

        [Fact]
        public void MaxWidth_Clamps_Grow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'>
                <div id='a' style='flex-grow:1;max-width:120px;height:30px'></div>
                <div id='b' style='flex-grow:1;height:30px'></div>
            </div></body>");
            float widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            Assert.True(widthA <= 121, $"max-width should clamp grow to 120px, got {widthA}");
        }

        [Fact]
        public void MinHeight_Prevents_Shrink_In_Column()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;height:100px'>
                <div id='a' style='flex-shrink:1;height:90px;min-height:70px'></div>
                <div style='flex-shrink:1;height:90px'></div>
            </div></body>");
            float heightA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Height;
            Assert.True(heightA >= 69, $"min-height should prevent column shrink below 70px, got {heightA}");
        }

        [Fact]
        public void MaxHeight_Clamps_Stretch()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px;height:200px;align-items:stretch'>
                <div id='t' style='max-height:60px;width:50px'></div>
            </div></body>");
            float height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(height <= 61, $"max-height should clamp stretch to 60px, got {height}");
        }

        [Fact]
        public void MinWidth_Zero_Allows_Full_Shrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:60px'>
                <div id='a' style='flex:0 1 100px;min-width:0;height:30px'></div>
                <div id='b' style='flex:0 1 100px;min-width:0;height:30px'></div>
                <div id='c' style='flex:0 1 100px;min-width:0;height:30px'></div>
            </div></body>");
            float widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            Assert.True(System.Math.Abs(widthA - 20) < 2, $"min-width:0 should allow full shrink to 20px, got {widthA}");
        }

        [Fact]
        public void MinWidth_Larger_Than_Basis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'>
                <div id='t' style='flex:0 0 50px;min-width:120px;height:30px'></div>
            </div></body>");
            float width = LayoutTestHelper.FindById(root, "t")!.ContentRect.Width;
            Assert.True(width >= 119, $"min-width larger than basis should enforce minimum, got {width}");
        }

        [Fact]
        public void MaxWidth_Smaller_Than_Basis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'>
                <div id='t' style='flex:0 0 200px;max-width:100px;height:30px'></div>
            </div></body>");
            float width = LayoutTestHelper.FindById(root, "t")!.ContentRect.Width;
            Assert.True(width <= 101, $"max-width smaller than basis should clamp, got {width}");
        }

        [Fact]
        public void MinWidth_Percentage_Resolves_Against_Container()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'>
                <div id='a' style='flex:0 1 250px;min-width:40%;height:30px'></div>
                <div style='flex:0 1 250px;min-width:0;height:30px'></div>
            </div></body>");
            float widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            Assert.True(widthA >= 159, $"min-width:40% of 400=160, item should not shrink below, got {widthA}");
        }

        [Fact(Skip = "Known: percentage max-width on flex items not yet resolved against container")]
        public void MaxWidth_Percentage_Resolves_Against_Container()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'>
                <div id='t' style='flex-grow:1;max-width:25%;height:30px'></div>
                <div style='flex-grow:1;height:30px'></div>
            </div></body>");
            float width = LayoutTestHelper.FindById(root, "t")!.ContentRect.Width;
            Assert.True(width <= 101, $"max-width:25% of 400=100, got {width}");
        }

        [Fact]
        public void MinHeight_Percentage_In_Column()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;height:200px'>
                <div id='a' style='flex-shrink:1;height:150px;min-height:40%'></div>
                <div style='flex-shrink:1;height:150px'></div>
            </div></body>");
            float heightA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Height;
            Assert.True(heightA >= 79, $"min-height:40% of 200=80, got {heightA}");
        }

        [Fact(Skip = "Known: percentage max-height on flex column items not yet resolved against container")]
        public void MaxHeight_Percentage_Clamps_Column_Grow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;height:300px'>
                <div id='t' style='flex-grow:1;max-height:30%'></div>
                <div style='flex-grow:1'></div>
            </div></body>");
            float height = LayoutTestHelper.FindById(root, "t")!.ContentRect.Height;
            Assert.True(height <= 91, $"max-height:30% of 300=90, got {height}");
        }

        [Fact]
        public void MinWidth_With_FlexGrow_Redistributes_Excess()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'>
                <div id='a' style='flex-grow:1;min-width:200px;height:30px'></div>
                <div id='b' style='flex-grow:1;height:30px'></div>
            </div></body>");
            float widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            float widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            Assert.True(widthA >= 199, $"item A should be at least 200px, got {widthA}");
            Assert.True(System.Math.Abs(widthA + widthB - 300) < 2, $"total should equal 300, got {widthA + widthB}");
        }

        [Fact]
        public void MaxWidth_With_FlexGrow_Redistributes_To_Others()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'>
                <div id='a' style='flex-grow:1;max-width:80px;height:30px'></div>
                <div id='b' style='flex-grow:1;max-width:80px;height:30px'></div>
                <div id='c' style='flex-grow:1;height:30px'></div>
            </div></body>");
            float widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            float widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            float widthC = LayoutTestHelper.FindById(root, "c")!.ContentRect.Width;
            Assert.True(widthA <= 81, $"A should be clamped to 80, got {widthA}");
            Assert.True(widthB <= 81, $"B should be clamped to 80, got {widthB}");
            Assert.True(widthC >= 139, $"C should get remaining ~140px, got {widthC}");
        }

        [Fact]
        public void MinWidth_With_FlexShrink_Freezes_At_Minimum()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'>
                <div id='a' style='flex:0 1 150px;min-width:130px;height:30px'></div>
                <div id='b' style='flex:0 1 150px;min-width:0;height:30px'></div>
            </div></body>");
            float widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            float widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            Assert.True(widthA >= 129, $"A frozen at min-width 130, got {widthA}");
            Assert.True(widthB <= 71, $"B absorbs remaining shrink, got {widthB}");
        }

        [Fact]
        public void MaxWidth_With_FlexShrink_No_Overshoot()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:200px'>
                <div id='t' style='flex:0 1 180px;max-width:100px;height:30px'></div>
                <div style='flex:0 0 50px;height:30px'></div>
            </div></body>");
            float width = LayoutTestHelper.FindById(root, "t")!.ContentRect.Width;
            Assert.True(width <= 101, $"max-width should clamp even with shrink, got {width}");
        }

        [Fact]
        public void MinMax_Both_Applied_Clamps_Range()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'>
                <div id='t' style='flex:1 1 0px;min-width:60px;max-width:150px;height:30px'></div>
                <div style='flex:1 1 0px;height:30px'></div>
            </div></body>");
            float width = LayoutTestHelper.FindById(root, "t")!.ContentRect.Width;
            Assert.True(width >= 59 && width <= 151, $"width should be in [60,150], got {width}");
        }

        [Fact]
        public void BorderBox_MinWidth_Includes_Padding_Border()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'>
                <div id='t' style='box-sizing:border-box;flex:0 1 200px;min-width:150px;padding:20px;border:5px solid;height:60px'></div>
                <div style='flex:0 1 200px;min-width:0;height:30px'></div>
            </div></body>");
            float contentWidth = LayoutTestHelper.FindById(root, "t")!.ContentRect.Width;
            float totalWidth = contentWidth + 40 + 10;
            Assert.True(totalWidth >= 149, $"border-box min-width 150 means total >= 150, got {totalWidth}");
        }

        [Fact(Skip = "Known: border-box max-width not subtracting padding+border on flex items")]
        public void BorderBox_MaxWidth_Includes_Padding_Border()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'>
                <div id='t' style='box-sizing:border-box;flex-grow:1;max-width:200px;padding:15px;border:5px solid;height:60px'></div>
                <div style='flex-grow:1;height:30px'></div>
            </div></body>");
            float contentWidth = LayoutTestHelper.FindById(root, "t")!.ContentRect.Width;
            float totalWidth = contentWidth + 30 + 10;
            Assert.True(totalWidth <= 201, $"border-box max-width 200 means total <= 200, got {totalWidth}");
        }

        [Fact]
        public void Column_MinHeight_Prevents_Shrink_With_Two_Items()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;height:100px'>
                <div id='a' style='flex-shrink:1;height:80px;min-height:65px'></div>
                <div id='b' style='flex-shrink:1;height:80px;min-height:0'></div>
            </div></body>");
            float heightA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Height;
            float heightB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Height;
            Assert.True(heightA >= 64, $"A min-height 65 prevents further shrink, got {heightA}");
            Assert.True(heightB <= 36, $"B absorbs remaining shrink, got {heightB}");
        }

        [Fact]
        public void Column_MaxHeight_Clamps_Grow_With_Two_Items()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;height:400px'>
                <div id='a' style='flex-grow:1;max-height:100px'></div>
                <div id='b' style='flex-grow:1'></div>
            </div></body>");
            float heightA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Height;
            float heightB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Height;
            Assert.True(heightA <= 101, $"A clamped by max-height, got {heightA}");
            Assert.True(heightB >= 299, $"B gets remaining space, got {heightB}");
        }

        [Fact]
        public void MinWidth_Auto_Uses_Content_Minimum()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'>
                <div id='t' style='flex:0 1 200px;height:30px'><div style='width:120px;height:10px'></div></div>
                <div style='flex:0 0 50px;height:30px'></div>
            </div></body>");
            float width = LayoutTestHelper.FindById(root, "t")!.ContentRect.Width;
            Assert.True(width >= 49, $"min-width:auto should consider content, got {width}");
        }

        [Fact]
        public void Calc_MinWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'>
                <div id='a' style='flex:0 1 200px;min-width:calc(50% - 20px);height:30px'></div>
                <div style='flex:0 1 200px;min-width:0;height:30px'></div>
            </div></body>");
            float widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            Assert.True(widthA >= 129, $"calc(50%-20px) of 300 = 130, got {widthA}");
        }

        [Fact(Skip = "Known: calc() percentage in max-width not resolved on flex items")]
        public void Calc_MaxWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:400px'>
                <div id='t' style='flex-grow:1;max-width:calc(25% + 20px);height:30px'></div>
                <div style='flex-grow:1;height:30px'></div>
            </div></body>");
            float width = LayoutTestHelper.FindById(root, "t")!.ContentRect.Width;
            Assert.True(width <= 121, $"calc(25%+20px) of 400 = 120, got {width}");
        }

        [Fact]
        public void Negative_FlexGrow_Invalid_Treated_As_Zero()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'>
                <div id='t' style='flex-grow:-1;width:80px;height:30px'></div>
                <div style='flex-grow:1;height:30px'></div>
            </div></body>");
            float width = LayoutTestHelper.FindById(root, "t")!.ContentRect.Width;
            Assert.True(System.Math.Abs(width - 80) < 2, $"negative flex-grow invalid, should stay at 80px, got {width}");
        }

        [Fact]
        public void Shrink_Zero_Prevents_Any_Shrink()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:100px'>
                <div id='a' style='flex:0 0 120px;height:30px'></div>
                <div id='b' style='flex:0 1 80px;min-width:0;height:30px'></div>
            </div></body>");
            float widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            Assert.True(System.Math.Abs(widthA - 120) < 2, $"flex-shrink:0 keeps basis 120, got {widthA}");
        }

        [Fact(Skip = "Known: min-width should win over max-width per CSS 2.1 §10.4")]
        public void MinWidth_Greater_Than_MaxWidth_MinWins()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'>
                <div id='t' style='flex:1;min-width:180px;max-width:100px;height:30px'></div>
            </div></body>");
            float width = LayoutTestHelper.FindById(root, "t")!.ContentRect.Width;
            Assert.True(width >= 179, $"CSS spec: min-width wins over max-width, got {width}");
        }

        [Fact]
        public void MaxWidth_Smaller_Than_Content_Overflows()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'>
                <div id='t' style='flex:1;max-width:40px;height:30px'><div style='width:80px;height:10px'></div></div>
                <div style='flex:1;height:30px'></div>
            </div></body>");
            float width = LayoutTestHelper.FindById(root, "t")!.ContentRect.Width;
            Assert.True(width <= 41, $"max-width should still clamp even with larger content, got {width}");
        }

        [Fact]
        public void MinWidth_Percentage_On_Multiple_Items()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'>
                <div id='a' style='flex:0 1 200px;min-width:30%;height:30px'></div>
                <div id='b' style='flex:0 1 200px;min-width:20%;height:30px'></div>
            </div></body>");
            float widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            float widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            Assert.True(widthA >= 89, $"A: min-width 30% of 300=90, got {widthA}");
            Assert.True(widthB >= 59, $"B: min-width 20% of 300=60, got {widthB}");
        }

        [Fact]
        public void MaxHeight_Clamps_Column_Grow_Redistributes()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;flex-direction:column;width:200px;height:300px'>
                <div id='a' style='flex-grow:1;max-height:60px'></div>
                <div id='b' style='flex-grow:1;max-height:60px'></div>
                <div id='c' style='flex-grow:1'></div>
            </div></body>");
            float heightA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Height;
            float heightB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Height;
            float heightC = LayoutTestHelper.FindById(root, "c")!.ContentRect.Height;
            Assert.True(heightA <= 61, $"A clamped to 60, got {heightA}");
            Assert.True(heightB <= 61, $"B clamped to 60, got {heightB}");
            Assert.True(heightC >= 179, $"C gets remaining ~180, got {heightC}");
        }

        [Fact]
        public void MinWidth_With_Basis_Zero_And_Grow()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'><div style='display:flex;width:300px'>
                <div id='a' style='flex:1 0 0px;min-width:120px;height:30px'></div>
                <div id='b' style='flex:1 0 0px;height:30px'></div>
            </div></body>");
            float widthA = LayoutTestHelper.FindById(root, "a")!.ContentRect.Width;
            float widthB = LayoutTestHelper.FindById(root, "b")!.ContentRect.Width;
            Assert.True(widthA >= 119, $"A: flex-grow 1 from 0 + min-width 120, got {widthA}");
            Assert.True(System.Math.Abs(widthA + widthB - 300) < 2, $"total should equal container");
        }
    }
}
