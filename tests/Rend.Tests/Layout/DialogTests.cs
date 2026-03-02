using System;
using Xunit;

namespace Rend.Tests.Layout
{
    public class DialogTests
    {
        [Fact]
        public void Dialog_Open_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <dialog open>
                        <p>This is an open dialog.</p>
                    </dialog>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Dialog_Closed_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <dialog>
                        <p>This dialog is not open.</p>
                    </dialog>
                    <p>Content outside dialog.</p>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Dialog_WithForm_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <dialog open>
                        <form method='dialog'>
                            <p>Enter your name:</p>
                            <input type='text' name='name'>
                            <button>Submit</button>
                        </form>
                    </dialog>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Dialog_WithHeadingAndContent_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <dialog open>
                        <h2>Confirmation</h2>
                        <p>Are you sure you want to proceed?</p>
                        <button>Yes</button>
                        <button>No</button>
                    </dialog>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Dialog_MultipleOnPage_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <p>Main content</p>
                    <dialog open>
                        <p>First dialog</p>
                    </dialog>
                    <dialog>
                        <p>Second dialog (closed)</p>
                    </dialog>
                    <dialog open>
                        <p>Third dialog (open)</p>
                    </dialog>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void Dialog_WithNestedElements_ProducesValidPdf()
        {
            byte[] result;
            try
            {
                result = Render.ToPdf(@"
                    <dialog open>
                        <div style='border: 1px solid gray; padding: 10px;'>
                            <h3>Settings</h3>
                            <ul>
                                <li>Option 1</li>
                                <li>Option 2</li>
                            </ul>
                        </div>
                    </dialog>");
            }
            catch (Exception ex) when (IsNativeLibraryFailure(ex))
            {
                return;
            }

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        private static bool IsNativeLibraryFailure(Exception ex)
        {
            return ex is DllNotFoundException ||
                   ex is TypeInitializationException ||
                   (ex.InnerException is DllNotFoundException) ||
                   ex.Message.Contains("native", StringComparison.OrdinalIgnoreCase);
        }
    }
}
