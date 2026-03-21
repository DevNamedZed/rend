using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptFlexAllContainerSizeTests
    {
        private readonly ITestOutputHelper _output;
        public WptFlexAllContainerSizeTests(ITestOutputHelper output) { _output = output; }

        private void VerifyEqualFlexItems(int containerWidth, int itemCount)
        {
            var items = "";
            for (int index = 0; index < itemCount; index++)
            {
                items += $"<div id='item{index}' style='flex:1;height:30px'></div>";
            }

            var html = $"<body style='margin:0'><div style='display:flex;width:{containerWidth}px'>{items}</div></body>";
            var root = LayoutTestHelper.Layout(html);
            float expectedWidth = (float)containerWidth / itemCount;

            for (int index = 0; index < itemCount; index++)
            {
                var item = LayoutTestHelper.FindById(root, $"item{index}");
                Assert.NotNull(item);
                Assert.True(
                    System.Math.Abs(item!.ContentRect.Width - expectedWidth) < 1,
                    $"Item {index} width: expected {expectedWidth}, got {item.ContentRect.Width} (container={containerWidth}, items={itemCount})");
            }
        }

        [Fact] public void Width100_1Item() { VerifyEqualFlexItems(100, 1); }
        [Fact] public void Width100_2Items() { VerifyEqualFlexItems(100, 2); }
        [Fact] public void Width100_3Items() { VerifyEqualFlexItems(100, 3); }
        [Fact] public void Width100_4Items() { VerifyEqualFlexItems(100, 4); }

        [Fact] public void Width150_1Item() { VerifyEqualFlexItems(150, 1); }
        [Fact] public void Width150_2Items() { VerifyEqualFlexItems(150, 2); }
        [Fact] public void Width150_3Items() { VerifyEqualFlexItems(150, 3); }
        [Fact] public void Width150_4Items() { VerifyEqualFlexItems(150, 4); }

        [Fact] public void Width200_1Item() { VerifyEqualFlexItems(200, 1); }
        [Fact] public void Width200_2Items() { VerifyEqualFlexItems(200, 2); }
        [Fact] public void Width200_3Items() { VerifyEqualFlexItems(200, 3); }
        [Fact] public void Width200_4Items() { VerifyEqualFlexItems(200, 4); }

        [Fact] public void Width250_1Item() { VerifyEqualFlexItems(250, 1); }
        [Fact] public void Width250_2Items() { VerifyEqualFlexItems(250, 2); }
        [Fact] public void Width250_3Items() { VerifyEqualFlexItems(250, 3); }
        [Fact] public void Width250_4Items() { VerifyEqualFlexItems(250, 4); }

        [Fact] public void Width300_1Item() { VerifyEqualFlexItems(300, 1); }
        [Fact] public void Width300_2Items() { VerifyEqualFlexItems(300, 2); }
        [Fact] public void Width300_3Items() { VerifyEqualFlexItems(300, 3); }
        [Fact] public void Width300_4Items() { VerifyEqualFlexItems(300, 4); }

        [Fact] public void Width350_1Item() { VerifyEqualFlexItems(350, 1); }
        [Fact] public void Width350_2Items() { VerifyEqualFlexItems(350, 2); }
        [Fact] public void Width350_3Items() { VerifyEqualFlexItems(350, 3); }
        [Fact] public void Width350_4Items() { VerifyEqualFlexItems(350, 4); }

        [Fact] public void Width400_1Item() { VerifyEqualFlexItems(400, 1); }
        [Fact] public void Width400_2Items() { VerifyEqualFlexItems(400, 2); }
        [Fact] public void Width400_3Items() { VerifyEqualFlexItems(400, 3); }
        [Fact] public void Width400_4Items() { VerifyEqualFlexItems(400, 4); }

        [Fact] public void Width450_1Item() { VerifyEqualFlexItems(450, 1); }
        [Fact] public void Width450_2Items() { VerifyEqualFlexItems(450, 2); }
        [Fact] public void Width450_3Items() { VerifyEqualFlexItems(450, 3); }
        [Fact] public void Width450_4Items() { VerifyEqualFlexItems(450, 4); }

        [Fact] public void Width500_1Item() { VerifyEqualFlexItems(500, 1); }
        [Fact] public void Width500_2Items() { VerifyEqualFlexItems(500, 2); }
        [Fact] public void Width500_3Items() { VerifyEqualFlexItems(500, 3); }
        [Fact] public void Width500_4Items() { VerifyEqualFlexItems(500, 4); }

        [Fact] public void Width600_1Item() { VerifyEqualFlexItems(600, 1); }
        [Fact] public void Width600_2Items() { VerifyEqualFlexItems(600, 2); }
        [Fact] public void Width600_3Items() { VerifyEqualFlexItems(600, 3); }
        [Fact] public void Width600_4Items() { VerifyEqualFlexItems(600, 4); }
    }
}
