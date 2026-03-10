using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Rend;
using Rend.Pdf;

// Step 1: Generate a PDF
Console.WriteLine("Generating PDF...");
byte[] pdfBytes = Render.ToPdf(@"
<html><body>
    <h1>Signed Document</h1>
    <p>This document has been digitally signed.</p>
</body></html>");

File.WriteAllBytes("unsigned.pdf", pdfBytes);
Console.WriteLine("Created unsigned.pdf");

// Step 2: Create a self-signed certificate for demo purposes
Console.WriteLine("Creating self-signed certificate...");
using var rsa = RSA.Create(2048);
var request = new CertificateRequest(
    "CN=Demo Signer, O=Example Corp",
    rsa,
    HashAlgorithmName.SHA256,
    RSASignaturePadding.Pkcs1);

request.CertificateExtensions.Add(
    new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));

using var cert = request.CreateSelfSigned(
    DateTimeOffset.UtcNow,
    DateTimeOffset.UtcNow.AddYears(1));

// Step 3: Sign the PDF with the certificate
Console.WriteLine("Signing PDF...");
using (var input = new MemoryStream(pdfBytes))
using (var output = File.Create("signed.pdf"))
{
    PdfSigning.Sign(input, output, new PdfSignatureOptions
    {
        Signer = new Pkcs12Signer(cert),
        SignerName = "Demo Signer",
        Reason = "Document approved",
        Location = "New York, NY",
    });
}

Console.WriteLine("Created signed.pdf");

// Step 4: You can also sign using just the convenience overload
using (var input = new MemoryStream(pdfBytes))
using (var output = File.Create("signed-simple.pdf"))
{
    PdfSigning.Sign(input, output, cert, signerName: "Demo Signer");
}

Console.WriteLine("Created signed-simple.pdf");

// Step 5: Or sign any existing PDF file
if (File.Exists("existing-document.pdf"))
{
    using var input = File.OpenRead("existing-document.pdf");
    using var output = File.Create("existing-signed.pdf");
    PdfSigning.Sign(input, output, cert);
    Console.WriteLine("Signed existing-document.pdf -> existing-signed.pdf");
}
