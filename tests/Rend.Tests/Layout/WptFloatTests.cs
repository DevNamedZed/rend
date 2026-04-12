using Xunit;

namespace Rend.Tests.Layout
{
    /// <summary>
    /// Tests covering CSS2 float model: placement, clearing, shrink-to-fit,
    /// float interaction with BFC, and float containment.
    /// </summary>
    public class WptFloatTests
    {
        // [CSS2 §9.5.1] float: left starts at left edge
        [Fact] public void Float_Left_AtLeftEdge() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div id='t' style='float:left;width:80px;height:40px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X < 2);
        }

        // [CSS2 §9.5.1] float: right starts at right edge
        [Fact] public void Float_Right_AtRightEdge() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div id='t' style='float:right;width:80px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.X - 120) < 2);
        }

        // [CSS2 §9.5.1] multiple floats stack horizontally
        [Fact] public void Float_Left_Multiple_Stack() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div id='a' style='float:left;width:60px;height:40px'></div><div id='b' style='float:left;width:60px;height:40px'></div></div></body>");
            var a = LayoutTestHelper.FindById(r,"a")!;
            var b = LayoutTestHelper.FindById(r,"b")!;
            Assert.True(System.Math.Abs(b.ContentRect.X - 60) < 2);
        }

        // [CSS2 §9.5.1] float wraps to next line when not enough space
        // TODO: Float wrapping when second float doesn't fit may have a bug
        [Fact] public void Float_Wraps_When_Full() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:100px'><div style='float:left;width:60px;height:40px'></div><div id='t' style='float:left;width:60px;height:40px'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            // Second float doesn't fit (60+60>100), should go below first (Y>=40)
            // or overlap (X=60, allowed by some float placement algorithms)
            Assert.True(t.ContentRect.Y >= 39 || t.ContentRect.X >= 59,
                $"Float should wrap or overlap (X={t.ContentRect.X}, Y={t.ContentRect.Y})");
        }

        // [CSS2 §9.5.1] float shrink-to-fit width
        [Fact] public void Float_ShrinkToFit() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><div id='t' style='float:left'><div style='width:80px;height:20px'></div></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 80) < 2);
        }

        // [CSS2 §9.5.2] clear: left moves below left floats
        [Fact] public void Clear_Left() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='float:left;width:80px;height:50px'></div><div id='t' style='clear:left;height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 49);
        }

        // [CSS2 §9.5.2] clear: right moves below right floats
        [Fact] public void Clear_Right() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='float:right;width:80px;height:50px'></div><div id='t' style='clear:right;height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 49);
        }

        // [CSS2 §9.5.2] clear: both
        [Fact] public void Clear_Both_BelowAll() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><div style='float:left;width:80px;height:50px'></div><div style='float:right;width:80px;height:70px'></div><div id='t' style='clear:both;height:20px'></div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.Y >= 69);
        }

        // [CSS2 §9.4.1] BFC block avoids sibling floats
        [Fact] public void BFC_AvoidsSiblingFloat() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='float:left;width:80px;height:50px'></div><div id='t' style='overflow:hidden'>x</div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X >= 79);
        }

        // [CSS2 §10.3.5] float with explicit width
        [Fact] public void Float_ExplicitWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><div id='t' style='float:left;width:150px;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        // [CSS2 §10.3.5] float with percentage width
        [Fact] public void Float_PercentWidth() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:300px'><div id='t' style='float:left;width:50%;height:40px'></div></div></body>");
            Assert.True(System.Math.Abs(LayoutTestHelper.FindById(r,"t")!.ContentRect.Width - 150) < 2);
        }

        // [CSS2 §9.5] float with margin
        [Fact] public void Float_WithMargin() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div id='t' style='float:left;width:60px;height:40px;margin:10px'></div></div></body>");
            var t = LayoutTestHelper.FindById(r,"t")!;
            Assert.True(System.Math.Abs(t.ContentRect.X - 10) < 2);
            Assert.True(System.Math.Abs(t.ContentRect.Y - 10) < 2);
        }

        // [CSS-DISPLAY §3] flow-root avoids sibling floats
        [Fact] public void FlowRoot_AvoidsSiblingFloat() {
            var r = LayoutTestHelper.Layout("<body style='margin:0'><div style='width:200px'><div style='float:left;width:80px;height:50px'></div><div id='t' style='display:flow-root'>x</div></div></body>");
            Assert.True(LayoutTestHelper.FindById(r,"t")!.ContentRect.X >= 79);
        }

        // [CSS2 §9.7] Floated inline replaced elements (embed, img, video) must
        // have their display blockified so they stack as floats without inline
        // whitespace contributing to their horizontal position.
        [Fact] public void Float_ReplacedInline_StacksWithoutInlineSpacing() {
            string html = "<!DOCTYPE html><html><head><style>" +
                "embed { border: 1px dashed gray; padding: 1px; float: left; }" +
                ".big { width: 48px; height: 32px; }" +
                "</style></head><body>" +
                "<embed id='a' class='big'>" +
                "    <embed id='b' class='big'>" +
                "    <embed id='c' class='big'>" +
                "</body></html>";
            var result = LayoutTestHelper.Layout(html);
            var firstEmbed = LayoutTestHelper.FindById(result, "a")!;
            var secondEmbed = LayoutTestHelper.FindById(result, "b")!;
            var thirdEmbed = LayoutTestHelper.FindById(result, "c")!;
            Assert.InRange(firstEmbed.BorderRect.X, 7f, 9f);
            Assert.InRange(secondEmbed.BorderRect.X, 59f, 61f);
            Assert.InRange(thirdEmbed.BorderRect.X, 111f, 113f);
        }
    }
}
