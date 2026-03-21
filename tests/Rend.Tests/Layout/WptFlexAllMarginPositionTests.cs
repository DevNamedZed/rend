using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexAllMarginPositionTests
    {
        private readonly ITestOutputHelper _output;

        public WptFlexAllMarginPositionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void MarginLeft_10px_OffsetsItemX()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='margin-left:10px;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.X={item!.ContentRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.X - 10) < 2,
                $"margin-left:10px should offset X to 10 (got {item.ContentRect.X})");
        }

        [Fact]
        public void MarginLeft_20px_OffsetsItemX()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='margin-left:20px;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.X={item!.ContentRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.X - 20) < 2,
                $"margin-left:20px should offset X to 20 (got {item.ContentRect.X})");
        }

        [Fact]
        public void MarginLeft_30px_OffsetsItemX()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='margin-left:30px;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.X={item!.ContentRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.X - 30) < 2,
                $"margin-left:30px should offset X to 30 (got {item.ContentRect.X})");
        }

        [Fact]
        public void MarginLeft_40px_OffsetsItemX()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='margin-left:40px;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.X={item!.ContentRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.X - 40) < 2,
                $"margin-left:40px should offset X to 40 (got {item.ContentRect.X})");
        }

        [Fact]
        public void MarginLeft_50px_OffsetsItemX()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='margin-left:50px;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.X={item!.ContentRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.X - 50) < 2,
                $"margin-left:50px should offset X to 50 (got {item.ContentRect.X})");
        }

        [Fact]
        public void MarginRight_10px_OffsetsSiblingX()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div style='margin-right:10px;width:50px;height:30px'></div>
                    <div id='sibling' style='width:50px;height:30px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(sibling);
            _output.WriteLine($"sibling.X={sibling!.ContentRect.X}");
            Assert.True(System.Math.Abs(sibling.ContentRect.X - 60) < 2,
                $"sibling after 50px item + 10px margin-right should be at X=60 (got {sibling.ContentRect.X})");
        }

        [Fact]
        public void MarginRight_20px_OffsetsSiblingX()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div style='margin-right:20px;width:50px;height:30px'></div>
                    <div id='sibling' style='width:50px;height:30px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(sibling);
            _output.WriteLine($"sibling.X={sibling!.ContentRect.X}");
            Assert.True(System.Math.Abs(sibling.ContentRect.X - 70) < 2,
                $"sibling after 50px item + 20px margin-right should be at X=70 (got {sibling.ContentRect.X})");
        }

        [Fact]
        public void MarginRight_30px_OffsetsSiblingX()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div style='margin-right:30px;width:50px;height:30px'></div>
                    <div id='sibling' style='width:50px;height:30px'></div>
                </div></body>");
            var sibling = LayoutTestHelper.FindById(root, "sibling");
            Assert.NotNull(sibling);
            _output.WriteLine($"sibling.X={sibling!.ContentRect.X}");
            Assert.True(System.Math.Abs(sibling.ContentRect.X - 80) < 2,
                $"sibling after 50px item + 30px margin-right should be at X=80 (got {sibling.ContentRect.X})");
        }

        [Fact]
        public void MarginTop_10px_InFlexStart()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:300px;height:100px'>
                    <div id='item' style='margin-top:10px;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.Y={item!.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 10) < 2,
                $"margin-top:10px in flex-start should offset Y to 10 (got {item.ContentRect.Y})");
        }

        [Fact]
        public void MarginTop_20px_InFlexStart()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:300px;height:100px'>
                    <div id='item' style='margin-top:20px;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.Y={item!.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 20) < 2,
                $"margin-top:20px in flex-start should offset Y to 20 (got {item.ContentRect.Y})");
        }

        [Fact]
        public void MarginTop_30px_InFlexStart()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:300px;height:100px'>
                    <div id='item' style='margin-top:30px;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.Y={item!.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 30) < 2,
                $"margin-top:30px in flex-start should offset Y to 30 (got {item.ContentRect.Y})");
        }

        [Fact]
        public void MarginAuto_CentersInContainer_200px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px;height:200px'>
                    <div id='item' style='margin:auto;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedX = (200 - 50) / 2f;
            float expectedY = (200 - 30) / 2f;
            _output.WriteLine($"item.X={item!.ContentRect.X} item.Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.X - expectedX) < 2,
                $"margin:auto should center X at {expectedX} (got {item.ContentRect.X})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"margin:auto should center Y at {expectedY} (got {item.ContentRect.Y})");
        }

        [Fact]
        public void MarginAuto_CentersInContainer_300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px;height:300px'>
                    <div id='item' style='margin:auto;width:60px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedX = (300 - 60) / 2f;
            float expectedY = (300 - 40) / 2f;
            _output.WriteLine($"item.X={item!.ContentRect.X} item.Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.X - expectedX) < 2,
                $"margin:auto should center X at {expectedX} (got {item.ContentRect.X})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"margin:auto should center Y at {expectedY} (got {item.ContentRect.Y})");
        }

        [Fact]
        public void MarginAuto_CentersInContainer_400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px;height:400px'>
                    <div id='item' style='margin:auto;width:80px;height:50px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedX = (400 - 80) / 2f;
            float expectedY = (400 - 50) / 2f;
            _output.WriteLine($"item.X={item!.ContentRect.X} item.Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.X - expectedX) < 2,
                $"margin:auto should center X at {expectedX} (got {item.ContentRect.X})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"margin:auto should center Y at {expectedY} (got {item.ContentRect.Y})");
        }

        [Fact]
        public void MarginLeftAuto_PushesRightInContainer_200px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='item' style='margin-left:auto;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedX = 200 - 50;
            _output.WriteLine($"item.X={item!.ContentRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.X - expectedX) < 2,
                $"margin-left:auto should push item to X={expectedX} (got {item.ContentRect.X})");
        }

        [Fact]
        public void MarginLeftAuto_PushesRightInContainer_300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='margin-left:auto;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedX = 300 - 50;
            _output.WriteLine($"item.X={item!.ContentRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.X - expectedX) < 2,
                $"margin-left:auto should push item to X={expectedX} (got {item.ContentRect.X})");
        }

        [Fact]
        public void MarginLeftAuto_PushesRightInContainer_400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='item' style='margin-left:auto;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedX = 400 - 50;
            _output.WriteLine($"item.X={item!.ContentRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.X - expectedX) < 2,
                $"margin-left:auto should push item to X={expectedX} (got {item.ContentRect.X})");
        }

        [Fact]
        public void MarginRightAuto_KeepsLeftAndPushesSiblingRight_300px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='first' style='margin-right:auto;width:50px;height:30px'></div>
                    <div id='second' style='width:50px;height:30px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            _output.WriteLine($"first.X={first!.ContentRect.X} second.X={second!.ContentRect.X}");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2,
                $"first item should stay at X=0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(second.ContentRect.X - 250) < 2,
                $"second item should be pushed to X=250 (got {second.ContentRect.X})");
        }

        [Fact]
        public void MarginRightAuto_KeepsLeftAndPushesSiblingRight_400px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='first' style='margin-right:auto;width:60px;height:30px'></div>
                    <div id='second' style='width:60px;height:30px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            _output.WriteLine($"first.X={first!.ContentRect.X} second.X={second!.ContentRect.X}");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2,
                $"first item should stay at X=0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(second.ContentRect.X - 340) < 2,
                $"second item should be pushed to X=340 (got {second.ContentRect.X})");
        }

        [Fact]
        public void MarginTopAuto_PushesDown_Height100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px;height:100px'>
                    <div id='item' style='margin-top:auto;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedY = 100 - 30;
            _output.WriteLine($"item.Y={item!.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"margin-top:auto should push item to Y={expectedY} (got {item.ContentRect.Y})");
        }

        [Fact]
        public void MarginTopAuto_PushesDown_Height200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px;height:200px'>
                    <div id='item' style='margin-top:auto;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedY = 200 - 30;
            _output.WriteLine($"item.Y={item!.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"margin-top:auto should push item to Y={expectedY} (got {item.ContentRect.Y})");
        }

        [Fact]
        public void MarginBottomAuto_KeepsAtTop()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px;height:100px'>
                    <div id='item' style='margin-bottom:auto;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.Y={item!.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y) < 2,
                $"margin-bottom:auto should keep item at Y=0 (got {item.ContentRect.Y})");
        }

        [Fact]
        public void MarginTopBottomAuto_CentersVertically_Height100()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px;height:100px'>
                    <div id='item' style='margin-top:auto;margin-bottom:auto;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedY = (100 - 30) / 2f;
            _output.WriteLine($"item.Y={item!.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"both auto Y margins should center at Y={expectedY} (got {item.ContentRect.Y})");
        }

        [Fact]
        public void MarginTopBottomAuto_CentersVertically_Height200()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px;height:200px'>
                    <div id='item' style='margin-top:auto;margin-bottom:auto;width:50px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedY = (200 - 40) / 2f;
            _output.WriteLine($"item.Y={item!.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"both auto Y margins should center at Y={expectedY} (got {item.ContentRect.Y})");
        }

        [Fact]
        public void NegativeMarginLeft_ShiftsItemLeft()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div style='width:50px;height:30px'></div>
                    <div id='item' style='margin-left:-10px;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedX = 50 - 10;
            _output.WriteLine($"item.X={item!.ContentRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.X - expectedX) < 2,
                $"negative margin-left:-10px should shift item to X={expectedX} (got {item.ContentRect.X})");
        }

        [Fact]
        public void NegativeMarginTop_ShiftsItemUp()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:300px;height:100px'>
                    <div id='item' style='margin-top:-5px;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.Y={item!.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - (-5)) < 2,
                $"negative margin-top:-5px should shift item to Y=-5 (got {item.ContentRect.Y})");
        }

        [Fact]
        public void Column_MarginTopAuto_PushesItemDown()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:200px'>
                    <div style='width:50px;height:30px'></div>
                    <div id='item' style='margin-top:auto;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedY = 200 - 30;
            _output.WriteLine($"item.Y={item!.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"column margin-top:auto should push item to Y={expectedY} (got {item.ContentRect.Y})");
        }

        [Fact]
        public void Column_MarginAuto_CentersHorizontally()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:200px'>
                    <div id='item' style='margin-left:auto;margin-right:auto;width:60px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedX = (200 - 60) / 2f;
            _output.WriteLine($"item.X={item!.ContentRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.X - expectedX) < 2,
                $"column margin:auto should center X at {expectedX} (got {item.ContentRect.X})");
        }

        [Fact]
        public void MarginBetweenItems_10px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='first' style='width:50px;height:30px'></div>
                    <div id='second' style='margin-left:10px;width:50px;height:30px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            float expectedGap = second!.ContentRect.X - (first!.ContentRect.X + first.ContentRect.Width);
            _output.WriteLine($"gap={expectedGap}");
            Assert.True(System.Math.Abs(expectedGap - 10) < 2,
                $"margin-left:10px should create 10px gap between items (got {expectedGap})");
        }

        [Fact]
        public void MarginBetweenItems_20px()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='first' style='width:50px;height:30px'></div>
                    <div id='second' style='margin-left:20px;width:50px;height:30px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            float actualGap = second!.ContentRect.X - (first!.ContentRect.X + first.ContentRect.Width);
            _output.WriteLine($"gap={actualGap}");
            Assert.True(System.Math.Abs(actualGap - 20) < 2,
                $"margin-left:20px should create 20px gap between items (got {actualGap})");
        }

        [Fact]
        public void MarginPercentage_ResolvesAgainstContainerWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:200px'>
                    <div id='item' style='margin-left:10%;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedX = 20;
            _output.WriteLine($"item.X={item!.ContentRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.X - expectedX) < 2,
                $"margin-left:10% of 200px should offset X to {expectedX} (got {item.ContentRect.X})");
        }

        [Fact]
        public void MarginLeftRight_BothFixed_PositionAndSibling()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='first' style='margin-left:15px;margin-right:25px;width:50px;height:30px'></div>
                    <div id='second' style='width:50px;height:30px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            _output.WriteLine($"first.X={first!.ContentRect.X} second.X={second!.ContentRect.X}");
            Assert.True(System.Math.Abs(first.ContentRect.X - 15) < 2,
                $"first item X should be 15 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(second.ContentRect.X - 90) < 2,
                $"second item X should be 15+50+25=90 (got {second.ContentRect.X})");
        }

        [Fact]
        public void MarginTopBottom_FixedInCrossAxis()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:200px;height:100px'>
                    <div id='item' style='margin-top:15px;margin-bottom:10px;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.Y={item!.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 15) < 2,
                $"margin-top:15px should offset Y to 15 (got {item.ContentRect.Y})");
        }

        [Fact]
        public void MarginLeftAutoWithTwoItems_SeparatesItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='first' style='width:50px;height:30px'></div>
                    <div id='second' style='margin-left:auto;width:50px;height:30px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            _output.WriteLine($"first.X={first!.ContentRect.X} second.X={second!.ContentRect.X}");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2,
                $"first stays at X=0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(second.ContentRect.X - 250) < 2,
                $"second pushed to right edge X=250 (got {second.ContentRect.X})");
        }

        [Fact]
        public void Column_MarginBottom_SpacesBetweenItems()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px'>
                    <div id='first' style='margin-bottom:15px;width:50px;height:30px'></div>
                    <div id='second' style='width:50px;height:30px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first");
            var second = LayoutTestHelper.FindById(root, "second");
            Assert.NotNull(first);
            Assert.NotNull(second);
            float expectedSecondY = 30 + 15;
            _output.WriteLine($"first.Y={first!.ContentRect.Y} second.Y={second!.ContentRect.Y}");
            Assert.True(System.Math.Abs(second.ContentRect.Y - expectedSecondY) < 2,
                $"second item Y should be {expectedSecondY} (got {second!.ContentRect.Y})");
        }

        [Fact]
        public void MarginAllSides_ItemPosition()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:300px;height:100px'>
                    <div id='item' style='margin:10px 20px 10px 30px;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            _output.WriteLine($"item.X={item!.ContentRect.X} item.Y={item.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.X - 30) < 2,
                $"margin-left:30px should offset X to 30 (got {item.ContentRect.X})");
            Assert.True(System.Math.Abs(item.ContentRect.Y - 10) < 2,
                $"margin-top:10px should offset Y to 10 (got {item.ContentRect.Y})");
        }

        [Fact]
        public void MarginLeftAutoRight_SplitsSpace()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:300px'>
                    <div id='item' style='margin-left:auto;margin-right:auto;width:60px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedX = (300 - 60) / 2f;
            _output.WriteLine($"item.X={item!.ContentRect.X}");
            Assert.True(System.Math.Abs(item.ContentRect.X - expectedX) < 2,
                $"margin-left/right:auto should center at X={expectedX} (got {item.ContentRect.X})");
        }

        [Fact]
        public void Column_MarginBottomAuto_KeepsAtTop()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:200px'>
                    <div id='item' style='margin-bottom:auto;width:50px;height:30px'></div>
                    <div id='last' style='width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            var last = LayoutTestHelper.FindById(root, "last");
            Assert.NotNull(item);
            Assert.NotNull(last);
            float expectedLastY = 200 - 30;
            _output.WriteLine($"item.Y={item!.ContentRect.Y} last.Y={last!.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y) < 2,
                $"first item should stay at Y=0 (got {item.ContentRect.Y})");
            Assert.True(System.Math.Abs(last.ContentRect.Y - expectedLastY) < 2,
                $"last item should be at Y={expectedLastY} (got {last.ContentRect.Y})");
        }

        [Fact]
        public void Column_MarginTopBottomAuto_CentersVertically()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;flex-direction:column;width:200px;height:200px'>
                    <div id='item' style='margin-top:auto;margin-bottom:auto;width:50px;height:40px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedY = (200 - 40) / 2f;
            _output.WriteLine($"item.Y={item!.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"column margin-top/bottom:auto should center at Y={expectedY} (got {item.ContentRect.Y})");
        }

        [Fact]
        public void ThreeItems_MarginLeftOnMiddle()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;width:400px'>
                    <div id='first' style='width:50px;height:30px'></div>
                    <div id='middle' style='margin-left:30px;width:50px;height:30px'></div>
                    <div id='last' style='width:50px;height:30px'></div>
                </div></body>");
            var first = LayoutTestHelper.FindById(root, "first");
            var middle = LayoutTestHelper.FindById(root, "middle");
            var last = LayoutTestHelper.FindById(root, "last");
            Assert.NotNull(first);
            Assert.NotNull(middle);
            Assert.NotNull(last);
            _output.WriteLine($"first.X={first!.ContentRect.X} middle.X={middle!.ContentRect.X} last.X={last!.ContentRect.X}");
            Assert.True(System.Math.Abs(first.ContentRect.X) < 2,
                $"first at X=0 (got {first.ContentRect.X})");
            Assert.True(System.Math.Abs(middle.ContentRect.X - 80) < 2,
                $"middle at X=50+30=80 (got {middle.ContentRect.X})");
            Assert.True(System.Math.Abs(last.ContentRect.X - 130) < 2,
                $"last at X=80+50=130 (got {last.ContentRect.X})");
        }

        [Fact]
        public void MarginPercentage_TopResolvesAgainstContainerWidth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:flex;align-items:flex-start;width:200px;height:200px'>
                    <div id='item' style='margin-top:5%;width:50px;height:30px'></div>
                </div></body>");
            var item = LayoutTestHelper.FindById(root, "item");
            Assert.NotNull(item);
            float expectedY = 10;
            _output.WriteLine($"item.Y={item!.ContentRect.Y}");
            Assert.True(System.Math.Abs(item.ContentRect.Y - expectedY) < 2,
                $"margin-top:5% of 200px width should offset Y to {expectedY} (got {item.ContentRect.Y})");
        }
    }
}
