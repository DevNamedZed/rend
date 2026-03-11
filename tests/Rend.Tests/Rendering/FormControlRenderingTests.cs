using Xunit;

namespace Rend.Tests.Rendering
{
    /// <summary>
    /// Tests that HTML form controls render non-empty PDF output.
    /// Covers text inputs, checkboxes, radio buttons, submit buttons,
    /// select dropdowns, textareas, and the button element.
    /// </summary>
    public class FormControlRenderingTests
    {
        [Fact]
        public void InputText_Renders_NonEmptyPdf()
        {
            var result = Render.ToPdf("<input type=\"text\" value=\"Hello\">");

            Assert.NotNull(result);
            Assert.True(result.Length > 100, "PDF output for text input should have reasonable length");
            Assert.Equal((byte)'%', result[0]);
        }

        [Fact]
        public void InputText_WithPlaceholder_Renders_NonEmptyPdf()
        {
            var result = Render.ToPdf("<input type=\"text\" placeholder=\"Enter name\">");

            Assert.NotNull(result);
            Assert.True(result.Length > 100, "PDF output for text input with placeholder should have reasonable length");
        }

        [Fact]
        public void InputPassword_Renders_NonEmptyPdf()
        {
            var result = Render.ToPdf("<input type=\"password\" value=\"secret\">");

            Assert.NotNull(result);
            Assert.True(result.Length > 100, "PDF output for password input should have reasonable length");
        }

        [Fact]
        public void InputCheckbox_Checked_Renders_NonEmptyPdf()
        {
            var result = Render.ToPdf("<input type=\"checkbox\" checked>");

            Assert.NotNull(result);
            Assert.True(result.Length > 100, "PDF output for checked checkbox should have reasonable length");
        }

        [Fact]
        public void InputCheckbox_Unchecked_Renders_NonEmptyPdf()
        {
            var result = Render.ToPdf("<input type=\"checkbox\">");

            Assert.NotNull(result);
            Assert.True(result.Length > 100, "PDF output for unchecked checkbox should have reasonable length");
        }

        [Fact]
        public void InputRadio_Checked_Renders_NonEmptyPdf()
        {
            var result = Render.ToPdf("<input type=\"radio\" checked>");

            Assert.NotNull(result);
            Assert.True(result.Length > 100, "PDF output for checked radio should have reasonable length");
        }

        [Fact]
        public void InputSubmit_Renders_NonEmptyPdf()
        {
            var result = Render.ToPdf("<input type=\"submit\" value=\"Send\">");

            Assert.NotNull(result);
            Assert.True(result.Length > 100, "PDF output for submit button should have reasonable length");
        }

        [Fact]
        public void InputSubmit_DefaultLabel_Renders_NonEmptyPdf()
        {
            var result = Render.ToPdf("<input type=\"submit\">");

            Assert.NotNull(result);
            Assert.True(result.Length > 100, "PDF output for submit button with default label should have reasonable length");
        }

        [Fact]
        public void Select_WithOptions_Renders_NonEmptyPdf()
        {
            var result = Render.ToPdf("<select><option>A</option><option>B</option></select>");

            Assert.NotNull(result);
            Assert.True(result.Length > 100, "PDF output for select should have reasonable length");
        }

        [Fact]
        public void Textarea_WithContent_Renders_NonEmptyPdf()
        {
            var result = Render.ToPdf("<textarea>Hello world\nLine 2</textarea>");

            Assert.NotNull(result);
            Assert.True(result.Length > 100, "PDF output for textarea should have reasonable length");
        }

        [Fact]
        public void Textarea_WithRowsCols_Renders_NonEmptyPdf()
        {
            var result = Render.ToPdf("<textarea rows=\"5\" cols=\"40\">content</textarea>");

            Assert.NotNull(result);
            Assert.True(result.Length > 100, "PDF output for textarea with rows/cols should have reasonable length");
        }

        [Fact]
        public void Button_Renders_AsInlineBlock_WithChildren()
        {
            // <button> is NOT a replaced element; it renders children normally as inline-block
            var result = Render.ToPdf("<button>Click me</button>");

            Assert.NotNull(result);
            Assert.True(result.Length > 100, "PDF output for button should have reasonable length");
        }

        [Fact]
        public void MultipleFormControls_Render_Together()
        {
            string html = @"
                <form>
                    <label>Name: <input type=""text"" value=""John""></label><br>
                    <label>Password: <input type=""password"" value=""secret""></label><br>
                    <label><input type=""checkbox"" checked> Remember me</label><br>
                    <label><input type=""radio"" checked> Option A</label><br>
                    <select><option>Choice 1</option></select><br>
                    <textarea>Notes here</textarea><br>
                    <input type=""submit"" value=""Submit"">
                </form>";

            var result = Render.ToPdf(html);

            Assert.NotNull(result);
            Assert.True(result.Length > 200, "PDF output for multiple form controls should have reasonable length");
            Assert.Equal((byte)'%', result[0]);
        }
    }
}
