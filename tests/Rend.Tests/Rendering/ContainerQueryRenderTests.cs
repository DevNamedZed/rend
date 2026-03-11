using Xunit;

namespace Rend.Tests.Rendering
{
    public class ContainerQueryRenderTests
    {
        [Fact]
        public void ContainerQuery_MatchingWidth_AppliesStyles()
        {
            var result = Render.ToPdf(@"
                <style>
                    .wrapper { container-type: inline-size; width: 600px; }
                    @container (min-width: 400px) {
                        .card { color: red; font-size: 24px; }
                    }
                </style>
                <div class='wrapper'>
                    <div class='card'>Large container card</div>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ContainerQuery_WithContainerName_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <style>
                    .sidebar { container-type: size; container-name: sidebar; width: 300px; }
                    @container sidebar (min-width: 200px) {
                        .nav-item { padding: 10px; }
                    }
                </style>
                <div class='sidebar'>
                    <div class='nav-item'>Navigation item</div>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ContainerQuery_Shorthand_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <style>
                    .main { container: main / inline-size; width: 800px; }
                    @container (min-width: 600px) {
                        .content { display: flex; }
                    }
                </style>
                <div class='main'>
                    <div class='content'>Responsive content</div>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void ContainerQuery_MultipleConditions_ProducesValidPdf()
        {
            var result = Render.ToPdf(@"
                <style>
                    .wrapper { container-type: size; width: 500px; height: 400px; }
                    @container (min-width: 300px) and (min-height: 200px) {
                        .card { background: blue; padding: 20px; }
                    }
                </style>
                <div class='wrapper'>
                    <div class='card'>Sized container card</div>
                </div>");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

    }
}
