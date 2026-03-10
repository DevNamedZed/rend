using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rend
{
    /// <summary>
    /// Renders HTML to PDF or image output.
    /// </summary>
    public interface IRenderer
    {
        byte[] ToPdf(string html, RenderOptions? options = null);
        void ToPdf(string html, Stream output, RenderOptions? options = null);
        byte[] ToPdf(TextReader html, RenderOptions? options = null);
        void ToPdf(TextReader html, Stream output, RenderOptions? options = null);
        Task<byte[]> ToPdfAsync(string html, RenderOptions? options = null, CancellationToken cancellationToken = default);
        Task ToPdfAsync(string html, Stream output, RenderOptions? options = null, CancellationToken cancellationToken = default);
        Task<byte[]> ToPdfAsync(TextReader html, RenderOptions? options = null, CancellationToken cancellationToken = default);
        Task ToPdfAsync(TextReader html, Stream output, RenderOptions? options = null, CancellationToken cancellationToken = default);

        byte[] ToImage(string html, RenderOptions? options = null);
        void ToImage(string html, Stream output, RenderOptions? options = null);
        byte[] ToImage(TextReader html, RenderOptions? options = null);
        void ToImage(TextReader html, Stream output, RenderOptions? options = null);
        Task<byte[]> ToImageAsync(string html, RenderOptions? options = null, CancellationToken cancellationToken = default);
        Task ToImageAsync(string html, Stream output, RenderOptions? options = null, CancellationToken cancellationToken = default);
        Task<byte[]> ToImageAsync(TextReader html, RenderOptions? options = null, CancellationToken cancellationToken = default);
        Task ToImageAsync(TextReader html, Stream output, RenderOptions? options = null, CancellationToken cancellationToken = default);
    }
}
