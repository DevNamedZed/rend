using System;
using System.IO;
using Rend;

var html = @"
<html><body style='margin:0;padding:20px;font-family:Arial,sans-serif'>
    <div style='background:linear-gradient(135deg,#667eea,#764ba2);color:white;padding:40px;border-radius:12px'>
        <h1 style='margin:0'>Dashboard</h1>
        <p style='opacity:0.8'>Monthly Report - January 2024</p>
    </div>
    <div style='display:flex;gap:16px;margin-top:20px'>
        <div style='flex:1;background:#f8f9fa;padding:20px;border-radius:8px;border:1px solid #e9ecef'>
            <div style='font-size:14px;color:#6c757d'>Revenue</div>
            <div style='font-size:28px;font-weight:bold;color:#28a745'>$12,450</div>
        </div>
        <div style='flex:1;background:#f8f9fa;padding:20px;border-radius:8px;border:1px solid #e9ecef'>
            <div style='font-size:14px;color:#6c757d'>Users</div>
            <div style='font-size:28px;font-weight:bold;color:#007bff'>1,234</div>
        </div>
        <div style='flex:1;background:#f8f9fa;padding:20px;border-radius:8px;border:1px solid #e9ecef'>
            <div style='font-size:14px;color:#6c757d'>Orders</div>
            <div style='font-size:28px;font-weight:bold;color:#fd7e14'>567</div>
        </div>
    </div>
</body></html>";

// PNG at default 96 DPI
using (var output = File.Create("dashboard.png"))
{
    Render.ToImage(html, output);
}
Console.WriteLine("Created dashboard.png");

// High-res PNG at 2x
using (var output = File.Create("dashboard@2x.png"))
{
    Render.ToImage(html, output, new RenderOptions { Dpi = 192f });
}
Console.WriteLine("Created dashboard@2x.png");

// JPEG
using (var output = File.Create("dashboard.jpg"))
{
    Render.ToImage(html, output, new RenderOptions
    {
        ImageFormat = "jpeg",
        ImageQuality = 85,
    });
}
Console.WriteLine("Created dashboard.jpg");

// WebP
using (var output = File.Create("dashboard.webp"))
{
    Render.ToImage(html, output, new RenderOptions
    {
        ImageFormat = "webp",
        ImageQuality = 80,
    });
}
Console.WriteLine("Created dashboard.webp");
