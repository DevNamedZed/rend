using System;
using System.IO;
using System.Text;
using Rend.Core.Values;
using Xunit;

namespace Rend.Tests.EndToEnd
{
    /// <summary>
    /// End-to-end validation tests that render complex HTML documents to PDF/PNG.
    /// Outputs are written to test-output/ at the repo root with both the source HTML
    /// and the rendered output for side-by-side comparison.
    /// </summary>
    public class ComplexPdfValidation
    {
        private static readonly string OutputDir = GetOutputDir();

        private static string GetOutputDir()
        {
            // Walk up from bin/Release/net8.0 to find the repo root (has .git or src/)
            var dir = AppContext.BaseDirectory;
            for (int i = 0; i < 10; i++)
            {
                var parent = Path.GetDirectoryName(dir);
                if (parent == null) break;
                if (Directory.Exists(Path.Combine(parent, ".git")) ||
                    Directory.Exists(Path.Combine(parent, "src")))
                {
                    var output = Path.Combine(parent, "test-output");
                    Directory.CreateDirectory(output);
                    return output;
                }
                dir = parent;
            }
            // Fallback
            var fallback = Path.Combine(Path.GetTempPath(), "rend-test-output");
            Directory.CreateDirectory(fallback);
            return fallback;
        }

        private static void SaveOutput(string name, string html, byte[] rendered, string ext)
        {
            File.WriteAllText(Path.Combine(OutputDir, $"{name}.html"), html, Encoding.UTF8);
            File.WriteAllBytes(Path.Combine(OutputDir, $"{name}.{ext}"), rendered);
        }

        // ═══════════════════════════════════════════
        // Invoice: table, headers/footers, flex layout
        // ═══════════════════════════════════════════

        [Fact]
        public void Invoice_WithTable_HeadersFooters_ProducesValidPdf()
        {
            var html = @"
<!DOCTYPE html>
<html>
<head>
<style>
    body { font-family: Arial, sans-serif; color: #333; margin: 0; padding: 0; }
    .header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 30px; }
    .company { font-size: 24px; font-weight: bold; color: #2c3e50; }
    .company-info { font-size: 11px; color: #777; margin-top: 4px; }
    .invoice-meta { text-align: right; }
    .invoice-title { font-size: 28px; font-weight: bold; color: #e74c3c; margin-bottom: 8px; }
    .invoice-detail { font-size: 11px; color: #555; }
    .addresses { display: flex; gap: 40px; margin-bottom: 30px; }
    .address-block { flex: 1; }
    .address-label { font-size: 10px; text-transform: uppercase; color: #999; letter-spacing: 1px; margin-bottom: 6px; }
    .address-name { font-weight: bold; margin-bottom: 2px; }
    table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }
    thead th { background: #2c3e50; color: white; padding: 10px 12px; text-align: left; font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; }
    tbody td { padding: 10px 12px; border-bottom: 1px solid #e0e0e0; font-size: 12px; }
    tbody tr:nth-child(even) { background: #f9f9f9; }
    .text-right { text-align: right; }
    .totals { margin-left: auto; width: 280px; }
    .totals-row { display: flex; justify-content: space-between; padding: 6px 0; font-size: 12px; border-bottom: 1px solid #eee; }
    .totals-row.grand { font-size: 16px; font-weight: bold; color: #2c3e50; border-top: 2px solid #2c3e50; border-bottom: none; padding-top: 10px; }
    .notes { margin-top: 40px; padding: 16px; background: #f0f4f8; border-left: 4px solid #3498db; font-size: 11px; color: #555; }
    .footer-bar { margin-top: 40px; text-align: center; font-size: 10px; color: #999; border-top: 1px solid #ddd; padding-top: 12px; }
</style>
</head>
<body>
    <div class='header'>
        <div>
            <div class='company'>ACME CORP</div>
            <div class='company-info'>123 Business Ave, Suite 100<br>San Francisco, CA 94102<br>contact@acmecorp.com</div>
        </div>
        <div class='invoice-meta'>
            <div class='invoice-title'>INVOICE</div>
            <div class='invoice-detail'>Invoice #: INV-2024-0042</div>
            <div class='invoice-detail'>Date: January 15, 2024</div>
            <div class='invoice-detail'>Due: February 14, 2024</div>
        </div>
    </div>

    <div class='addresses'>
        <div class='address-block'>
            <div class='address-label'>Bill To</div>
            <div class='address-name'>Widget Industries LLC</div>
            <div>456 Manufacturing Blvd</div>
            <div>Austin, TX 78701</div>
            <div>billing@widgetindustries.com</div>
        </div>
        <div class='address-block'>
            <div class='address-label'>Ship To</div>
            <div class='address-name'>Widget Industries — Warehouse</div>
            <div>789 Distribution Way</div>
            <div>Dallas, TX 75201</div>
        </div>
    </div>

    <table>
        <thead>
            <tr>
                <th>Item</th>
                <th>Description</th>
                <th class='text-right'>Qty</th>
                <th class='text-right'>Unit Price</th>
                <th class='text-right'>Amount</th>
            </tr>
        </thead>
        <tbody>
            <tr><td>PRD-001</td><td>Enterprise Widget License (Annual)</td><td class='text-right'>5</td><td class='text-right'>$2,400.00</td><td class='text-right'>$12,000.00</td></tr>
            <tr><td>PRD-002</td><td>Widget Premium Support Package</td><td class='text-right'>5</td><td class='text-right'>$600.00</td><td class='text-right'>$3,000.00</td></tr>
            <tr><td>SVC-010</td><td>On-site Installation &amp; Configuration</td><td class='text-right'>2</td><td class='text-right'>$1,500.00</td><td class='text-right'>$3,000.00</td></tr>
            <tr><td>SVC-011</td><td>Staff Training (half-day session)</td><td class='text-right'>3</td><td class='text-right'>$800.00</td><td class='text-right'>$2,400.00</td></tr>
            <tr><td>SVC-020</td><td>Data Migration Service</td><td class='text-right'>1</td><td class='text-right'>$2,500.00</td><td class='text-right'>$2,500.00</td></tr>
            <tr><td>HW-100</td><td>Widget Server Appliance (rack-mount)</td><td class='text-right'>1</td><td class='text-right'>$8,500.00</td><td class='text-right'>$8,500.00</td></tr>
        </tbody>
    </table>

    <div class='totals'>
        <div class='totals-row'><span>Subtotal</span><span>$31,400.00</span></div>
        <div class='totals-row'><span>Tax (8.25%)</span><span>$2,590.50</span></div>
        <div class='totals-row'><span>Shipping</span><span>$350.00</span></div>
        <div class='totals-row grand'><span>Total Due</span><span>$34,340.50</span></div>
    </div>

    <div class='notes'>
        <strong>Notes:</strong> Payment is due within 30 days. Please include the invoice number on your check or wire transfer.
        Late payments are subject to a 1.5% monthly finance charge. For questions about this invoice, contact accounts@acmecorp.com.
    </div>

    <div class='footer-bar'>
        ACME CORP &bull; Tax ID: 12-3456789 &bull; Thank you for your business!
    </div>
</body>
</html>";

            var options = new RenderOptions
            {
                PageSize = PageSize.Letter,
                MarginTop = 48f,
                MarginRight = 48f,
                MarginBottom = 60f,
                MarginLeft = 48f,
                Title = "Invoice INV-2024-0042",
                Author = "ACME CORP",
                HeaderHtml = "<div style='font-size:8px;color:#bbb;text-align:right;padding-right:4px'>ACME CORP — Confidential</div>",
                FooterHtml = "<div style='font-size:8px;color:#bbb;text-align:center'>Page {pageNumber} of {totalPages}</div>",
            };

            var pdf = RenderAndSave("invoice", html, options);
            Assert.True(pdf.Length > 1000, $"Invoice PDF too small: {pdf.Length} bytes");
            AssertValidPdf(pdf);
        }

        // ═══════════════════════════════════════════
        // Report: grid, gradients, shadows, badges
        // ═══════════════════════════════════════════

        [Fact]
        public void Report_WithFlexGridGradients_ProducesValidPdf()
        {
            var html = @"
<!DOCTYPE html>
<html>
<head>
<style>
    * { box-sizing: border-box; }
    body { font-family: 'Segoe UI', Arial, sans-serif; color: #1a1a2e; margin: 0; padding: 0; }
    .hero { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 40px; text-align: center; }
    .hero h1 { font-size: 32px; margin: 0 0 8px 0; }
    .hero p { font-size: 14px; opacity: 0.9; margin: 0; }
    .content { padding: 30px; }
    .metrics { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin-bottom: 30px; }
    .metric-card { background: white; border-radius: 8px; padding: 20px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); text-align: center; }
    .metric-value { font-size: 28px; font-weight: bold; color: #667eea; }
    .metric-label { font-size: 11px; color: #888; text-transform: uppercase; letter-spacing: 1px; margin-top: 4px; }
    .section { margin-bottom: 24px; }
    .section h2 { font-size: 18px; color: #1a1a2e; border-bottom: 2px solid #667eea; padding-bottom: 6px; margin-bottom: 12px; }
    .two-col { display: flex; gap: 24px; }
    .col { flex: 1; }
    .chart-placeholder { background: linear-gradient(180deg, #f0f4ff 0%, #e8ecf8 100%); border: 1px solid #d0d8e8; border-radius: 6px; height: 180px; display: flex; align-items: center; justify-content: center; color: #8090b0; font-size: 13px; }
    table { width: 100%; border-collapse: collapse; }
    th { background: #f0f4ff; color: #4a5568; font-size: 11px; text-transform: uppercase; padding: 8px 10px; text-align: left; }
    td { padding: 8px 10px; border-bottom: 1px solid #eee; font-size: 12px; }
    .badge { display: inline-block; padding: 2px 8px; border-radius: 10px; font-size: 10px; font-weight: bold; }
    .badge-green { background: #d4edda; color: #155724; }
    .badge-yellow { background: #fff3cd; color: #856404; }
    .badge-red { background: #f8d7da; color: #721c24; }
    .footer { text-align: center; font-size: 10px; color: #aaa; margin-top: 30px; padding-top: 12px; border-top: 1px solid #eee; }
</style>
</head>
<body>
    <div class='hero'>
        <h1>Q4 2024 Performance Report</h1>
        <p>Widget Industries LLC — Generated January 15, 2025</p>
    </div>

    <div class='content'>
        <div class='metrics'>
            <div class='metric-card'><div class='metric-value'>$4.2M</div><div class='metric-label'>Revenue</div></div>
            <div class='metric-card'><div class='metric-value'>1,247</div><div class='metric-label'>New Customers</div></div>
            <div class='metric-card'><div class='metric-value'>94.3%</div><div class='metric-label'>Satisfaction</div></div>
            <div class='metric-card'><div class='metric-value'>23%</div><div class='metric-label'>Growth YoY</div></div>
        </div>

        <div class='section'>
            <h2>Executive Summary</h2>
            <p style='font-size:13px;line-height:1.6'>
                Q4 2024 marked a strong finish to the fiscal year, with revenue exceeding targets by 12%.
                Customer acquisition accelerated in November driven by the product launch, while retention
                rates remained stable at 94.3%. The engineering team delivered all planned features on schedule,
                including the new API platform that has already attracted 200+ integration partners.
            </p>
        </div>

        <div class='section'>
            <h2>Regional Performance</h2>
            <div class='two-col'>
                <div class='col'>
                    <div class='chart-placeholder'>Revenue by Region (Chart)</div>
                </div>
                <div class='col'>
                    <table>
                        <tr><th>Region</th><th>Revenue</th><th>Growth</th><th>Status</th></tr>
                        <tr><td>North America</td><td>$1.8M</td><td>+28%</td><td><span class='badge badge-green'>On Track</span></td></tr>
                        <tr><td>Europe</td><td>$1.1M</td><td>+19%</td><td><span class='badge badge-green'>On Track</span></td></tr>
                        <tr><td>Asia Pacific</td><td>$820K</td><td>+35%</td><td><span class='badge badge-green'>Exceeding</span></td></tr>
                        <tr><td>Latin America</td><td>$310K</td><td>+8%</td><td><span class='badge badge-yellow'>Caution</span></td></tr>
                        <tr><td>Middle East</td><td>$170K</td><td>-2%</td><td><span class='badge badge-red'>At Risk</span></td></tr>
                    </table>
                </div>
            </div>
        </div>

        <div class='section'>
            <h2>Product Roadmap Status</h2>
            <table>
                <tr><th>Feature</th><th>Target</th><th>Status</th><th>Notes</th></tr>
                <tr><td>API Platform v2.0</td><td>Q4 2024</td><td><span class='badge badge-green'>Shipped</span></td><td>200+ partners onboarded</td></tr>
                <tr><td>Mobile App Redesign</td><td>Q4 2024</td><td><span class='badge badge-green'>Shipped</span></td><td>4.8 star rating</td></tr>
                <tr><td>Enterprise SSO</td><td>Q4 2024</td><td><span class='badge badge-yellow'>Delayed</span></td><td>Moved to Q1 2025</td></tr>
                <tr><td>Analytics Dashboard v3</td><td>Q1 2025</td><td><span class='badge badge-green'>On Track</span></td><td>Beta in Feb</td></tr>
                <tr><td>AI-Powered Insights</td><td>Q2 2025</td><td><span class='badge badge-green'>Planning</span></td><td>RFP phase</td></tr>
            </table>
        </div>

        <div class='footer'>
            Confidential — Widget Industries LLC — Q4 2024 Performance Report — Page 1
        </div>
    </div>
</body>
</html>";

            var options = new RenderOptions
            {
                PageSize = PageSize.Letter,
                MarginTop = 0f, MarginRight = 0f, MarginBottom = 36f, MarginLeft = 0f,
                Title = "Q4 2024 Performance Report",
            };

            var pdf = RenderAndSave("report", html, options);
            Assert.True(pdf.Length > 5000, $"Report PDF too small: {pdf.Length} bytes");
            AssertValidPdf(pdf);
        }

        // ═══════════════════════════════════════════
        // Architecture doc: multi-page, code, columns
        // ═══════════════════════════════════════════

        [Fact]
        public void MultiPage_WithVariedContent_ProducesMultiPagePdf()
        {
            var html = @"
<!DOCTYPE html>
<html>
<head>
<style>
    body { font-family: Georgia, 'Times New Roman', serif; color: #2d2d2d; line-height: 1.6; }
    h1 { font-size: 26px; text-align: center; color: #1a1a2e; margin-bottom: 4px; }
    h2 { font-size: 18px; color: #16213e; border-bottom: 1px solid #ccc; padding-bottom: 4px; margin-top: 24px; }
    h3 { font-size: 14px; color: #0f3460; }
    .subtitle { text-align: center; font-size: 13px; color: #777; margin-bottom: 30px; }
    p { font-size: 12px; text-align: justify; margin: 8px 0; }
    .two-col-text { column-count: 2; column-gap: 24px; column-rule: 1px solid #ddd; }
    blockquote { border-left: 3px solid #667eea; padding: 12px 16px; margin: 16px 0; background: #f8f9ff; font-style: italic; font-size: 13px; }
    code { font-family: 'Courier New', monospace; background: #f4f4f4; padding: 1px 4px; font-size: 11px; border-radius: 2px; }
    pre { background: #2d2d2d; color: #f8f8f2; padding: 16px; border-radius: 6px; font-family: 'Courier New', monospace; font-size: 11px; overflow: hidden; line-height: 1.5; }
    ul, ol { font-size: 12px; padding-left: 24px; }
    li { margin-bottom: 4px; }
    .page-break { page-break-before: always; }
    .highlight { background: linear-gradient(to right, #fff9c4, #fff176); padding: 8px 12px; border-radius: 4px; font-size: 12px; }
    table { width: 100%; border-collapse: collapse; margin: 12px 0; font-size: 11px; }
    th { background: #e8ecf8; padding: 8px; text-align: left; border: 1px solid #ccc; }
    td { padding: 8px; border: 1px solid #ddd; }
    .toc { background: #fafafa; padding: 20px; border: 1px solid #eee; margin-bottom: 24px; }
    .toc a { color: #0f3460; text-decoration: none; }
    .toc li { margin-bottom: 6px; }
</style>
</head>
<body>
    <h1>Technical Architecture Document</h1>
    <div class='subtitle'>Version 2.1 — Prepared for Widget Industries LLC — January 2025</div>

    <div class='toc'>
        <h3 style='margin-top:0'>Table of Contents</h3>
        <ol>
            <li><a href='#'>System Overview</a></li>
            <li><a href='#'>Architecture Principles</a></li>
            <li><a href='#'>Component Design</a></li>
            <li><a href='#'>Data Flow</a></li>
            <li><a href='#'>API Specification</a></li>
            <li><a href='#'>Deployment</a></li>
        </ol>
    </div>

    <h2>1. System Overview</h2>
    <div class='two-col-text'>
        <p>The Widget Platform is a distributed system designed to handle high-throughput
        data processing with real-time analytics capabilities. The architecture follows
        a microservices pattern with event-driven communication between services.</p>
        <p>Core design goals include horizontal scalability, fault tolerance, and
        sub-second response times for 99th percentile queries. The system processes
        an average of 50,000 events per second during peak hours.</p>
        <p>The platform consists of five primary service clusters: ingestion, processing,
        storage, query, and presentation. Each cluster can be independently scaled
        based on workload characteristics and SLA requirements.</p>
    </div>

    <blockquote>
        ""The best architectures are those that emerge from the constraints of the problem,
        not from the ambitions of the architect."" — A wise engineer
    </blockquote>

    <h2>2. Architecture Principles</h2>
    <ul>
        <li><strong>Separation of concerns</strong> — each service owns its data and business logic</li>
        <li><strong>Event sourcing</strong> — all state changes are captured as immutable events</li>
        <li><strong>CQRS</strong> — read and write models are optimized independently</li>
        <li><strong>Circuit breaker</strong> — cascading failures are prevented at service boundaries</li>
        <li><strong>Idempotency</strong> — all operations can be safely retried</li>
    </ul>

    <div class='highlight'>
        <strong>Key Decision:</strong> We chose event sourcing over traditional CRUD to support
        full audit trails, temporal queries, and replay capabilities for disaster recovery.
    </div>

    <div class='page-break'></div>

    <h2>3. Component Design</h2>
    <h3>3.1 Ingestion Service</h3>
    <p>The ingestion service accepts data via REST API, gRPC, and WebSocket connections.
    Input validation and schema enforcement occur at this layer before events are published
    to the message broker.</p>

    <pre>
// Example: Event ingestion endpoint
public async Task&lt;IActionResult&gt; IngestEvent(EventPayload payload)
{
    await _validator.ValidateAsync(payload);
    var envelope = EventEnvelope.Create(payload, _clock.UtcNow);
    await _broker.PublishAsync(""events.ingested"", envelope);
    return Accepted(new { EventId = envelope.Id });
}
    </pre>

    <h3>3.2 Processing Pipeline</h3>
    <table>
        <tr><th>Stage</th><th>Technology</th><th>Throughput</th><th>Latency (p99)</th></tr>
        <tr><td>Ingestion</td><td>ASP.NET Core + gRPC</td><td>50K events/sec</td><td>12ms</td></tr>
        <tr><td>Enrichment</td><td>Apache Kafka Streams</td><td>45K events/sec</td><td>35ms</td></tr>
        <tr><td>Aggregation</td><td>Apache Flink</td><td>40K events/sec</td><td>120ms</td></tr>
        <tr><td>Storage</td><td>PostgreSQL + TimescaleDB</td><td>30K writes/sec</td><td>8ms</td></tr>
        <tr><td>Query</td><td>Redis + Elasticsearch</td><td>100K reads/sec</td><td>5ms</td></tr>
    </table>

    <h2>4. Data Flow</h2>
    <p>Events flow through the system in a strictly ordered pipeline. Each stage applies
    transformations and enrichments before passing data downstream. The pipeline guarantees
    at-least-once delivery with deduplication at the storage layer.</p>

    <ol>
        <li>Client sends event via API gateway</li>
        <li>Ingestion service validates schema and publishes to Kafka</li>
        <li>Enrichment consumer adds metadata (geo, device, user context)</li>
        <li>Aggregation service computes rolling metrics and anomaly detection</li>
        <li>Materialized views are updated in read-optimized stores</li>
        <li>WebSocket push delivers real-time updates to connected dashboards</li>
    </ol>

    <div class='page-break'></div>

    <h2>5. API Specification</h2>
    <pre>
GET  /api/v2/events?from=2024-01-01&amp;to=2024-01-31
POST /api/v2/events/ingest
GET  /api/v2/metrics/summary?period=7d
GET  /api/v2/metrics/timeseries?metric=revenue&amp;granularity=1h
POST /api/v2/reports/generate
GET  /api/v2/health
    </pre>

    <h3>5.1 Authentication</h3>
    <p>All API endpoints require Bearer token authentication using JWT. Tokens are issued
    by the identity service with a 1-hour expiry and can be refreshed using refresh tokens.</p>

    <h3>5.2 Rate Limiting</h3>
    <table>
        <tr><th>Plan</th><th>Requests/min</th><th>Burst</th><th>Concurrent</th></tr>
        <tr><td>Free</td><td>60</td><td>10</td><td>2</td></tr>
        <tr><td>Pro</td><td>600</td><td>100</td><td>10</td></tr>
        <tr><td>Enterprise</td><td>6,000</td><td>1,000</td><td>50</td></tr>
    </table>

    <h2>6. Deployment</h2>
    <p>The system runs on Kubernetes with automatic horizontal pod autoscaling.
    Database clusters use managed services (RDS, ElastiCache) for operational simplicity.
    CI/CD pipelines deploy via GitOps with Argo CD, ensuring declarative infrastructure.</p>

    <div class='highlight'>
        <strong>SLA Target:</strong> 99.95% uptime (21.9 minutes downtime/month).
        Current trailing 90-day uptime: 99.98%.
    </div>

    <div style='margin-top:40px;text-align:center;font-size:10px;color:#999'>
        Document Classification: Internal — Widget Industries LLC<br>
        Last Updated: January 15, 2025 — Version 2.1
    </div>
</body>
</html>";

            var options = new RenderOptions
            {
                PageSize = PageSize.Letter,
                MarginTop = 72f, MarginRight = 72f, MarginBottom = 72f, MarginLeft = 72f,
                Title = "Technical Architecture Document v2.1",
                Author = "Widget Industries LLC",
                GenerateBookmarks = true,
                GenerateLinks = true,
                FooterHtml = "<div style='font-size:8px;color:#999;text-align:center'>Page {pageNumber} of {totalPages} — Confidential</div>",
            };

            var pdf = RenderAndSave("architecture", html, options);
            Assert.True(pdf.Length > 10000, $"Architecture PDF too small: {pdf.Length} bytes");
            AssertValidPdf(pdf);

            var content = Encoding.ASCII.GetString(pdf);
            int pageCount = CountOccurrences(content, "/Type /Page") - CountOccurrences(content, "/Type /Pages");
            Assert.True(pageCount >= 3, $"Expected 3+ pages, got {pageCount}");
        }

        // ═══════════════════════════════════════════
        // Styled card: PNG image output
        // ═══════════════════════════════════════════

        [Fact]
        public void StyledCard_ToImage_ProducesValidPng()
        {
            var html = @"
<!DOCTYPE html>
<html>
<head>
<style>
    body { font-family: Arial, sans-serif; background: #f0f2f5; display: flex; justify-content: center; padding: 40px; margin: 0; }
    .card { background: white; border-radius: 12px; box-shadow: 0 4px 16px rgba(0,0,0,0.12); padding: 32px; max-width: 400px; width: 100%; }
    .avatar { width: 64px; height: 64px; border-radius: 50%; background: linear-gradient(135deg, #667eea, #764ba2); display: flex; align-items: center; justify-content: center; color: white; font-size: 24px; font-weight: bold; margin-bottom: 16px; }
    h2 { margin: 0 0 4px 0; font-size: 20px; color: #1a1a2e; }
    .role { color: #888; font-size: 13px; margin-bottom: 16px; }
    .stats { display: flex; gap: 20px; margin-bottom: 16px; }
    .stat { text-align: center; }
    .stat-value { font-size: 18px; font-weight: bold; color: #333; }
    .stat-label { font-size: 10px; color: #999; text-transform: uppercase; }
    .btn { display: block; text-align: center; background: #667eea; color: white; padding: 10px; border-radius: 6px; font-size: 14px; font-weight: bold; text-decoration: none; }
</style>
</head>
<body>
    <div class='card'>
        <div class='avatar'>JD</div>
        <h2>Jane Doe</h2>
        <div class='role'>Senior Software Engineer</div>
        <div class='stats'>
            <div class='stat'><div class='stat-value'>142</div><div class='stat-label'>Projects</div></div>
            <div class='stat'><div class='stat-value'>3.2K</div><div class='stat-label'>Commits</div></div>
            <div class='stat'><div class='stat-value'>98%</div><div class='stat-label'>Reviews</div></div>
        </div>
        <a class='btn' href='#'>View Profile</a>
    </div>
</body>
</html>";

            using var output = new MemoryStream();
            Render.ToImage(html, output, new RenderOptions { Dpi = 150f });
            var png = output.ToArray();

            SaveOutput("card", html, png, "png");

            Assert.True(png.Length > 1000, $"PNG too small: {png.Length} bytes");
            Assert.Equal(0x89, png[0]); // PNG signature
            Assert.Equal(0x50, png[1]);
            Assert.Equal(0x4E, png[2]);
            Assert.Equal(0x47, png[3]);
        }

        // ═══════════════════════════════════════════
        // CSS features: image output (no PDF conversion)
        // ═══════════════════════════════════════════

        [Fact]
        public void CssFeatures_ToImage_ProducesValidPng()
        {
            var html = @"
<!DOCTYPE html>
<html><head><style>
* { box-sizing: border-box; }
body { margin: 0; padding: 20px; background: #f5f5f5; font-family: Arial, sans-serif; }
.row { display: flex; gap: 16px; margin-bottom: 20px; }
.box { flex: 1; height: 100px; border-radius: 8px; display: flex; align-items: center;
  justify-content: center; color: white; font-weight: bold; font-size: 12px;
  text-shadow: 0 1px 2px rgba(0,0,0,0.4); }
.repeat-linear { background: repeating-linear-gradient(45deg, #606dbc, #606dbc 10px, #465298 10px, #465298 20px); }
.repeat-radial { background: repeating-radial-gradient(circle at 50% 50%, #606dbc, #606dbc 10px, #465298 10px, #465298 20px); }
.shadow-row { display: flex; gap: 24px; justify-content: center; margin-bottom: 20px; }
.sbox { width: 140px; height: 90px; background: white; border-radius: 8px;
  display: flex; align-items: center; justify-content: center; font-size: 11px; color: #555; }
.shadow-soft { box-shadow: 0 4px 20px rgba(0,0,0,0.15); }
.shadow-hard { box-shadow: 8px 8px 0 #667eea; }
.shadow-inset { box-shadow: inset 0 4px 12px rgba(0,0,0,0.25); }
.overlap { display: flex; position: relative; height: 100px; margin-bottom: 20px; }
.ov { position: absolute; width: 120px; height: 80px; border-radius: 8px;
  display: flex; align-items: center; justify-content: center;
  color: white; font-size: 11px; font-weight: bold; }
.ov1 { background: rgba(231,76,60,0.7); left: 10px; top: 10px; }
.ov2 { background: rgba(52,152,219,0.7); left: 80px; top: 0; }
.ov3 { background: rgba(46,204,113,0.7); left: 150px; top: 15px; }
</style></head><body>
<div class='row'>
  <div class='box repeat-linear'>Repeating Linear</div>
  <div class='box repeat-radial'>Repeating Radial</div>
</div>
<div class='shadow-row'>
  <div class='sbox shadow-soft'>Soft Shadow</div>
  <div class='sbox shadow-hard'>Hard Shadow</div>
  <div class='sbox shadow-inset'>Inset Shadow</div>
</div>
<div class='overlap'>
  <div class='ov ov1'>Red 70%</div>
  <div class='ov ov2'>Blue 70%</div>
  <div class='ov ov3'>Green 70%</div>
</div>
</body></html>";

            using var output = new MemoryStream();
            Render.ToImage(html, output, new RenderOptions
            {
                Dpi = 150f,
                PageSize = new Rend.Core.Values.SizeF(500, 700),
            });
            var png = output.ToArray();
            SaveOutput("css-features", html, png, "png");
            Assert.True(png.Length > 1000, $"CSS features PNG too small: {png.Length} bytes");
        }

        [Fact]
        public void GridFlexCenter_ToImage_VerifyCentering()
        {
            var html = @"<!DOCTYPE html>
<html><head><style>
* { box-sizing: border-box; }
body { margin: 0; padding: 20px; font-family: Arial, sans-serif; background: #f5f5f5; }
.flex { display: flex; align-items: center; justify-content: center;
  width: 100px; height: 80px; background: #667eea; color: red; font-size: 14px; font-weight: bold; }
.grid { display: grid; grid-template-columns: 100px 100px; grid-auto-rows: 80px; gap: 8px; margin-top: 10px; }
.item { background: #667eea; display: flex;
  align-items: center; justify-content: center; color: red; font-size: 14px; font-weight: bold; }
</style></head><body>
<div class='flex'>FLEX</div>
<div class='grid'>
  <div class='item'>GRID</div>
</div>
</body></html>";

            using var output = new MemoryStream();
            Render.ToImage(html, output, new RenderOptions
            {
                Dpi = 96f,
                PageSize = new Rend.Core.Values.SizeF(400, 400),
            });
            var png = output.ToArray();
            SaveOutput("grid-flex-center", html, png, "png");
            Assert.True(png.Length > 500, $"Grid flex center PNG too small: {png.Length} bytes");
        }

        [Fact]
        public void AbsoluteFlexCenter_ToImage_VerifyCentering()
        {
            var html = @"<!DOCTYPE html>
<html><head><style>
* { box-sizing: border-box; }
body { margin: 0; padding: 20px; font-family: Arial, sans-serif; background: #fff; }
.container { position: relative; height: 200px; background: #eee; }
.abs { position: absolute; display: flex; align-items: center; justify-content: center;
  width: 150px; height: 80px; border-radius: 8px;
  color: white; font-size: 14px; font-weight: bold; }
.a { background: rgba(231,76,60,0.8); left: 10px; top: 10px; }
.b { background: rgba(52,152,219,0.8); left: 100px; top: 60px; }
</style></head><body>
<div class='container'>
  <div class='abs a'>Red Box</div>
  <div class='abs b'>Blue Box</div>
</div>
</body></html>";

            using var output = new MemoryStream();
            Render.ToImage(html, output, new RenderOptions
            {
                Dpi = 96f,
                PageSize = new Rend.Core.Values.SizeF(400, 300),
            });
            var png = output.ToArray();
            SaveOutput("abs-flex-center", html, png, "png");
            Assert.True(png.Length > 500, $"Abs flex center PNG too small: {png.Length} bytes");
        }

        // ═══════════════════════════════════════════
        // Minimal: sanity check
        // ═══════════════════════════════════════════

        [Fact]
        public void Minimal_HelloWorld_ProducesValidPdf()
        {
            var html = "<!DOCTYPE html><html><body><p>Hello World</p></body></html>";
            var pdf = RenderAndSave("minimal", html, new RenderOptions
            {
                PageSize = PageSize.Letter,
                MarginTop = 72f, MarginRight = 72f, MarginBottom = 72f, MarginLeft = 72f,
            });
            AssertValidPdf(pdf);
        }

        // ═══════════════════════════════════════════
        // Photo gallery: large inline images, grid, overlaid text
        // ═══════════════════════════════════════════

        [Fact]
        public void PhotoGallery_WithLargeImages_ProducesValidPdf()
        {
            // Generate 6 different-colored 200x150 PNG images (each ~900 bytes base64)
            var colors = new[] { "#e74c3c", "#3498db", "#2ecc71", "#f39c12", "#9b59b6", "#1abc9c" };
            var labels = new[] { "Sunset Beach", "Mountain Peak", "Forest Trail", "Desert Dunes", "Ocean Waves", "City Skyline" };
            var images = new string[6];
            for (int i = 0; i < 6; i++)
                images[i] = GeneratePngDataUri(200, 150, colors[i]);

            var html = $@"
<!DOCTYPE html>
<html>
<head>
<style>
    * {{ box-sizing: border-box; }}
    body {{ font-family: 'Segoe UI', Arial, sans-serif; color: #333; margin: 0; padding: 0; background: #fafafa; }}
    .hero {{ position: relative; overflow: hidden; height: 280px; }}
    .hero img {{ width: 100%; height: 280px; display: block; }}
    .hero-overlay {{ position: absolute; bottom: 0; left: 0; right: 0; padding: 30px 40px;
        background: linear-gradient(transparent, rgba(0,0,0,0.7)); color: white; }}
    .hero-overlay h1 {{ font-size: 32px; margin: 0 0 4px 0; text-shadow: 0 2px 4px rgba(0,0,0,0.3); }}
    .hero-overlay p {{ font-size: 14px; opacity: 0.9; margin: 0; }}
    .gallery {{ display: grid; grid-template-columns: repeat(3, 1fr); gap: 16px; padding: 24px 40px; }}
    .gallery-item {{ border-radius: 8px; overflow: hidden; box-shadow: 0 2px 12px rgba(0,0,0,0.1);
        background: white; }}
    .gallery-item img {{ width: 100%; height: 150px; display: block; }}
    .gallery-caption {{ padding: 12px 14px; }}
    .gallery-caption h3 {{ margin: 0 0 4px 0; font-size: 14px; color: #1a1a2e; }}
    .gallery-caption p {{ margin: 0; font-size: 11px; color: #888; line-height: 1.4; }}
    .gallery-caption .tags {{ margin-top: 8px; }}
    .tag {{ display: inline-block; padding: 2px 8px; border-radius: 10px; font-size: 9px;
        background: #e8ecf8; color: #4a5568; margin-right: 4px; }}
    .stats-bar {{ display: flex; justify-content: space-around; padding: 20px 40px;
        background: linear-gradient(135deg, #667eea, #764ba2); color: white; }}
    .stat {{ text-align: center; }}
    .stat-num {{ font-size: 24px; font-weight: bold; }}
    .stat-lbl {{ font-size: 10px; text-transform: uppercase; opacity: 0.8; letter-spacing: 1px; }}
    .featured {{ display: flex; gap: 20px; padding: 24px 40px; align-items: center; }}
    .featured img {{ width: 320px; height: 200px; border-radius: 8px; box-shadow: 0 4px 16px rgba(0,0,0,0.15); flex-shrink: 0; }}
    .featured-text {{ flex: 1; }}
    .featured-text h2 {{ font-size: 22px; margin: 0 0 8px 0; color: #1a1a2e; }}
    .featured-text p {{ font-size: 12px; line-height: 1.6; color: #555; }}
    .footer {{ text-align: center; padding: 16px; font-size: 10px; color: #999;
        border-top: 1px solid #eee; margin-top: 20px; }}
</style>
</head>
<body>
    <div class='hero'>
        <img src='{images[0]}' alt='Hero'>
        <div class='hero-overlay'>
            <h1>World Photography Collection</h1>
            <p>Curated landscapes from around the globe — 2024 Edition</p>
        </div>
    </div>

    <div class='stats-bar'>
        <div class='stat'><div class='stat-num'>2,847</div><div class='stat-lbl'>Photos</div></div>
        <div class='stat'><div class='stat-num'>42</div><div class='stat-lbl'>Countries</div></div>
        <div class='stat'><div class='stat-num'>128</div><div class='stat-lbl'>Photographers</div></div>
        <div class='stat'><div class='stat-num'>1.2M</div><div class='stat-lbl'>Downloads</div></div>
    </div>

    <div class='gallery'>
        <div class='gallery-item'>
            <img src='{images[0]}' alt='{labels[0]}'>
            <div class='gallery-caption'>
                <h3>{labels[0]}</h3>
                <p>Golden hour light illuminates the shoreline with warm amber tones across the Pacific coast.</p>
                <div class='tags'><span class='tag'>Nature</span><span class='tag'>Beach</span><span class='tag'>Sunset</span></div>
            </div>
        </div>
        <div class='gallery-item'>
            <img src='{images[1]}' alt='{labels[1]}'>
            <div class='gallery-caption'>
                <h3>{labels[1]}</h3>
                <p>Snow-capped summit piercing through the cloud layer at 4,200 meters elevation.</p>
                <div class='tags'><span class='tag'>Mountain</span><span class='tag'>Snow</span></div>
            </div>
        </div>
        <div class='gallery-item'>
            <img src='{images[2]}' alt='{labels[2]}'>
            <div class='gallery-caption'>
                <h3>{labels[2]}</h3>
                <p>Ancient redwood forest with dappled sunlight filtering through the canopy.</p>
                <div class='tags'><span class='tag'>Forest</span><span class='tag'>Trail</span></div>
            </div>
        </div>
        <div class='gallery-item'>
            <img src='{images[3]}' alt='{labels[3]}'>
            <div class='gallery-caption'>
                <h3>{labels[3]}</h3>
                <p>Wind-sculpted sand formations stretching across the Saharan landscape.</p>
                <div class='tags'><span class='tag'>Desert</span><span class='tag'>Sand</span></div>
            </div>
        </div>
        <div class='gallery-item'>
            <img src='{images[4]}' alt='{labels[4]}'>
            <div class='gallery-caption'>
                <h3>{labels[4]}</h3>
                <p>Dramatic wave crashing against volcanic rock formations at high tide.</p>
                <div class='tags'><span class='tag'>Ocean</span><span class='tag'>Waves</span></div>
            </div>
        </div>
        <div class='gallery-item'>
            <img src='{images[5]}' alt='{labels[5]}'>
            <div class='gallery-caption'>
                <h3>{labels[5]}</h3>
                <p>Twilight cityscape with reflections on the river and illuminated skyscrapers.</p>
                <div class='tags'><span class='tag'>City</span><span class='tag'>Night</span></div>
            </div>
        </div>
    </div>

    <div class='featured'>
        <img src='{images[2]}' alt='Featured'>
        <div class='featured-text'>
            <h2>Editor's Pick: Forest Trail</h2>
            <p>This stunning capture of the ancient redwood forest was taken during the golden hour
            in Northern California. The photographer spent three days camping in the backcountry
            to find the perfect combination of light and mist. The image has been downloaded over
            50,000 times and featured in National Geographic's annual landscape collection.</p>
            <p style='margin-top:12px'><strong>Camera:</strong> Sony A7R V &bull;
            <strong>Lens:</strong> 24-70mm f/2.8 &bull;
            <strong>Settings:</strong> f/8, 1/125s, ISO 200</p>
        </div>
    </div>

    <div class='footer'>
        World Photography Collection &copy; 2024 — All rights reserved — Printed with Rend
    </div>
</body>
</html>";

            var options = new RenderOptions
            {
                PageSize = PageSize.A4,
                MarginTop = 0f, MarginRight = 0f, MarginBottom = 36f, MarginLeft = 0f,
                Title = "World Photography Collection",
            };

            var pdf = RenderAndSave("photo-gallery", html, options);
            Assert.True(pdf.Length > 5000, $"Photo gallery PDF too small: {pdf.Length} bytes");
            AssertValidPdf(pdf);
        }

        // ═══════════════════════════════════════════
        // E-commerce product page: images, reviews, related items
        // ═══════════════════════════════════════════

        [Fact]
        public void EcommercePage_WithProductImages_ProducesValidPdf()
        {
            var productImg = GeneratePngDataUri(240, 240, "#2c3e50");
            var thumbs = new[]
            {
                GeneratePngDataUri(60, 60, "#34495e"),
                GeneratePngDataUri(60, 60, "#7f8c8d"),
                GeneratePngDataUri(60, 60, "#95a5a6"),
                GeneratePngDataUri(60, 60, "#bdc3c7"),
            };
            var relatedImgs = new[]
            {
                GeneratePngDataUri(120, 120, "#e74c3c"),
                GeneratePngDataUri(120, 120, "#3498db"),
                GeneratePngDataUri(120, 120, "#2ecc71"),
                GeneratePngDataUri(120, 120, "#f39c12"),
            };

            var html = $@"
<!DOCTYPE html>
<html>
<head>
<style>
    * {{ box-sizing: border-box; }}
    body {{ font-family: 'Segoe UI', Arial, sans-serif; color: #333; margin: 0; padding: 0; }}
    .breadcrumb {{ padding: 12px 32px; font-size: 11px; color: #888; background: #f8f8f8; border-bottom: 1px solid #eee; }}
    .breadcrumb a {{ color: #3498db; text-decoration: none; }}
    .product {{ display: flex; gap: 24px; padding: 24px; }}
    .product-images {{ flex: 0 0 240px; }}
    .main-img {{ width: 240px; height: 240px; border-radius: 8px; border: 1px solid #eee; display: block; }}
    .thumbs {{ display: flex; gap: 6px; margin-top: 8px; }}
    .thumb {{ width: 54px; height: 54px; border-radius: 4px; border: 2px solid #eee; cursor: pointer; display: block; }}
    .thumb:first-child {{ border-color: #3498db; }}
    .product-info {{ flex: 1; }}
    .product-info h1 {{ font-size: 24px; margin: 0 0 8px 0; color: #1a1a2e; }}
    .brand {{ font-size: 13px; color: #3498db; margin-bottom: 12px; }}
    .price {{ font-size: 28px; font-weight: bold; color: #e74c3c; margin-bottom: 4px; }}
    .price-original {{ font-size: 16px; color: #999; text-decoration: line-through; margin-left: 8px; }}
    .discount {{ display: inline-block; background: #e74c3c; color: white; padding: 2px 8px; border-radius: 4px;
        font-size: 11px; font-weight: bold; margin-left: 8px; }}
    .rating {{ display: flex; align-items: center; gap: 8px; margin: 12px 0; font-size: 13px; }}
    .stars {{ color: #f39c12; font-size: 16px; }}
    .rating-count {{ color: #3498db; }}
    .availability {{ font-size: 13px; color: #27ae60; font-weight: bold; margin-bottom: 16px; }}
    .specs {{ margin: 16px 0; }}
    .specs h3 {{ font-size: 14px; margin: 0 0 8px 0; }}
    .spec-grid {{ display: grid; grid-template-columns: 120px 1fr; gap: 4px 12px; font-size: 12px; }}
    .spec-label {{ color: #888; }}
    .buy-box {{ background: #f0f4f8; border-radius: 8px; padding: 20px; margin-top: 20px; }}
    .buy-btn {{ display: block; text-align: center; background: #f39c12; color: white; padding: 14px;
        border-radius: 6px; font-size: 16px; font-weight: bold; text-decoration: none; margin-bottom: 8px; }}
    .wishlist-btn {{ display: block; text-align: center; background: white; color: #333; padding: 12px;
        border-radius: 6px; font-size: 14px; border: 1px solid #ddd; text-decoration: none; }}
    .reviews {{ padding: 32px; border-top: 4px solid #f0f0f0; }}
    .reviews h2 {{ font-size: 18px; margin: 0 0 16px 0; }}
    .review {{ border-bottom: 1px solid #eee; padding: 16px 0; }}
    .review:last-child {{ border-bottom: none; }}
    .review-header {{ display: flex; justify-content: space-between; align-items: center; margin-bottom: 6px; }}
    .reviewer {{ font-weight: bold; font-size: 13px; }}
    .review-date {{ font-size: 11px; color: #999; }}
    .review-title {{ font-weight: bold; font-size: 13px; margin-bottom: 4px; }}
    .review-body {{ font-size: 12px; line-height: 1.5; color: #555; }}
    .verified {{ font-size: 10px; color: #27ae60; font-weight: bold; }}
    .related {{ padding: 32px; background: #f8f8f8; }}
    .related h2 {{ font-size: 18px; margin: 0 0 16px 0; }}
    .related-grid {{ display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; }}
    .related-item {{ background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 1px 4px rgba(0,0,0,0.08); }}
    .related-item img {{ width: 100%; height: 100px; display: block; }}
    .related-item .item-info {{ padding: 12px; }}
    .related-item .item-name {{ font-size: 12px; font-weight: bold; color: #1a1a2e; margin-bottom: 4px; }}
    .related-item .item-price {{ font-size: 14px; color: #e74c3c; font-weight: bold; }}
</style>
</head>
<body>
    <div class='breadcrumb'>
        <a href='#'>Home</a> &gt; <a href='#'>Electronics</a> &gt; <a href='#'>Headphones</a> &gt; Premium Wireless
    </div>

    <div class='product'>
        <div class='product-images'>
            <img class='main-img' src='{productImg}' alt='Product'>
            <div class='thumbs'>
                <img class='thumb' src='{thumbs[0]}' alt='View 1'>
                <img class='thumb' src='{thumbs[1]}' alt='View 2'>
                <img class='thumb' src='{thumbs[2]}' alt='View 3'>
                <img class='thumb' src='{thumbs[3]}' alt='View 4'>
            </div>
        </div>
        <div class='product-info'>
            <div class='brand'>SONANCE AUDIO</div>
            <h1>Premium Wireless Noise-Cancelling Headphones Pro X</h1>
            <div class='rating'>
                <span class='stars'>&#9733;&#9733;&#9733;&#9733;&#9734;</span>
                <span>4.3 out of 5</span>
                <span class='rating-count'>(2,847 ratings)</span>
            </div>
            <div>
                <span class='price'>$279.99</span>
                <span class='price-original'>$349.99</span>
                <span class='discount'>-20%</span>
            </div>
            <div class='availability'>&#10003; In Stock — Ships within 24 hours</div>

            <div class='specs'>
                <h3>Key Specifications</h3>
                <div class='spec-grid'>
                    <span class='spec-label'>Driver Size</span><span>40mm Beryllium</span>
                    <span class='spec-label'>Frequency</span><span>4Hz - 40kHz</span>
                    <span class='spec-label'>Impedance</span><span>32 Ohms</span>
                    <span class='spec-label'>Battery Life</span><span>38 hours (ANC on)</span>
                    <span class='spec-label'>Bluetooth</span><span>5.3 with LDAC, aptX HD</span>
                    <span class='spec-label'>Weight</span><span>254g</span>
                    <span class='spec-label'>ANC</span><span>Adaptive Hybrid (8 mics)</span>
                    <span class='spec-label'>Connectivity</span><span>Bluetooth + 3.5mm + USB-C</span>
                </div>
            </div>

            <div class='buy-box'>
                <a class='buy-btn' href='#'>Add to Cart</a>
                <a class='wishlist-btn' href='#'>&#9825; Add to Wishlist</a>
            </div>
        </div>
    </div>

    <div class='reviews'>
        <h2>Customer Reviews (2,847)</h2>
        <div class='review'>
            <div class='review-header'>
                <div><span class='reviewer'>AudioEnthusiast92</span> <span class='verified'>&#10003; Verified Purchase</span></div>
                <span class='review-date'>December 28, 2024</span>
            </div>
            <div class='stars' style='font-size:13px;margin-bottom:4px'>&#9733;&#9733;&#9733;&#9733;&#9733;</div>
            <div class='review-title'>Best headphones I've ever owned</div>
            <div class='review-body'>The sound quality is absolutely phenomenal. The beryllium drivers deliver crystal-clear
            highs and deep, punchy bass without any distortion even at high volumes. The ANC is the best in class — it
            completely blocks out airplane engine noise and office chatter. Battery life easily lasts a full week of commuting.
            The build quality feels premium with the aluminum headband and memory foam ear cushions. Worth every penny.</div>
        </div>
        <div class='review'>
            <div class='review-header'>
                <div><span class='reviewer'>TechReviewer_Kate</span> <span class='verified'>&#10003; Verified Purchase</span></div>
                <span class='review-date'>December 15, 2024</span>
            </div>
            <div class='stars' style='font-size:13px;margin-bottom:4px'>&#9733;&#9733;&#9733;&#9733;&#9734;</div>
            <div class='review-title'>Great sound, slightly heavy for long sessions</div>
            <div class='review-body'>Sound quality and ANC are top-tier. My only complaint is that after 3+ hours of
            continuous use, the clamping force starts to feel noticeable. The carrying case is excellent and the
            multipoint connection works flawlessly between my laptop and phone. The app EQ customization is intuitive.</div>
        </div>
        <div class='review'>
            <div class='review-header'>
                <div><span class='reviewer'>MusicProducer_Jake</span> <span class='verified'>&#10003; Verified Purchase</span></div>
                <span class='review-date'>November 30, 2024</span>
            </div>
            <div class='stars' style='font-size:13px;margin-bottom:4px'>&#9733;&#9733;&#9733;&#9733;&#9733;</div>
            <div class='review-title'>Studio-quality monitoring on the go</div>
            <div class='review-body'>As a music producer, I'm very picky about frequency response accuracy. These headphones
            deliver remarkably flat response with the ""Studio"" EQ preset. The LDAC codec support means I can hear every
            nuance over Bluetooth. The low-latency mode is great for video editing too. Highly recommended for creators.</div>
        </div>
    </div>

    <div class='related'>
        <h2>Customers Also Bought</h2>
        <div class='related-grid'>
            <div class='related-item'>
                <img src='{relatedImgs[0]}' alt='Related 1'>
                <div class='item-info'>
                    <div class='item-name'>Premium Headphone Stand — Walnut</div>
                    <div class='item-price'>$49.99</div>
                </div>
            </div>
            <div class='related-item'>
                <img src='{relatedImgs[1]}' alt='Related 2'>
                <div class='item-info'>
                    <div class='item-name'>Replacement Ear Cushions (Pair)</div>
                    <div class='item-price'>$29.99</div>
                </div>
            </div>
            <div class='related-item'>
                <img src='{relatedImgs[2]}' alt='Related 3'>
                <div class='item-info'>
                    <div class='item-name'>USB-C DAC/Amp — Portable</div>
                    <div class='item-price'>$89.99</div>
                </div>
            </div>
            <div class='related-item'>
                <img src='{relatedImgs[3]}' alt='Related 4'>
                <div class='item-info'>
                    <div class='item-name'>Carrying Case — Hard Shell</div>
                    <div class='item-price'>$34.99</div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>";

            var options = new RenderOptions
            {
                PageSize = PageSize.A4,
                MarginTop = 0f, MarginRight = 0f, MarginBottom = 0f, MarginLeft = 0f,
                Title = "Sonance Audio Pro X — Product Page",
            };

            var pdf = RenderAndSave("ecommerce", html, options);
            Assert.True(pdf.Length > 10000, $"E-commerce PDF too small: {pdf.Length} bytes");
            AssertValidPdf(pdf);
        }

        // ═══════════════════════════════════════════
        // Resume/CV: sidebar, skills, photo, multi-column
        // ═══════════════════════════════════════════

        [Fact]
        public void Resume_WithPhotoAndSidebar_ProducesValidPdf()
        {
            var photo = GeneratePngDataUri(120, 120, "#2c3e50");

            var html = $@"
<!DOCTYPE html>
<html>
<head>
<style>
    * {{ box-sizing: border-box; margin: 0; padding: 0; }}
    body {{ font-family: 'Segoe UI', Arial, sans-serif; color: #333; }}
    .page {{ display: flex; min-height: 100%; }}
    .sidebar {{ width: 220px; background: #1a1a2e; color: #ccc; padding: 30px 20px; flex-shrink: 0; }}
    .main {{ flex: 1; padding: 30px 32px; }}
    .photo {{ width: 120px; height: 120px; border-radius: 50%; border: 3px solid #667eea;
        margin: 0 auto 16px auto; display: block; }}
    .sidebar h2 {{ font-size: 13px; text-transform: uppercase; letter-spacing: 2px; color: #667eea;
        margin: 20px 0 10px 0; padding-bottom: 4px; border-bottom: 1px solid #333; }}
    .sidebar p, .sidebar li {{ font-size: 11px; line-height: 1.6; }}
    .sidebar ul {{ list-style: none; padding: 0; }}
    .sidebar li {{ margin-bottom: 6px; }}
    .skill {{ margin-bottom: 8px; }}
    .skill-name {{ font-size: 11px; margin-bottom: 2px; display: flex; justify-content: space-between; }}
    .skill-bar {{ height: 6px; background: #333; border-radius: 3px; }}
    .skill-fill {{ height: 6px; background: #667eea; border-radius: 3px; }}
    .name {{ font-size: 28px; font-weight: bold; color: #1a1a2e; }}
    .title-role {{ font-size: 16px; color: #667eea; margin-bottom: 4px; }}
    .summary {{ font-size: 12px; line-height: 1.6; color: #555; margin-bottom: 20px;
        padding-bottom: 16px; border-bottom: 2px solid #eee; }}
    .section {{ margin-bottom: 20px; }}
    .section h2 {{ font-size: 16px; color: #1a1a2e; border-bottom: 2px solid #667eea;
        padding-bottom: 4px; margin-bottom: 12px; }}
    .job {{ margin-bottom: 16px; }}
    .job-header {{ display: flex; justify-content: space-between; align-items: baseline; }}
    .job-title {{ font-size: 14px; font-weight: bold; color: #1a1a2e; }}
    .job-date {{ font-size: 11px; color: #888; }}
    .job-company {{ font-size: 12px; color: #667eea; margin-bottom: 4px; }}
    .job ul {{ padding-left: 18px; font-size: 11px; line-height: 1.6; }}
    .job li {{ margin-bottom: 3px; }}
    .edu {{ margin-bottom: 10px; }}
    .edu-degree {{ font-size: 13px; font-weight: bold; }}
    .edu-school {{ font-size: 12px; color: #667eea; }}
    .edu-date {{ font-size: 11px; color: #888; }}
    .certs {{ font-size: 11px; line-height: 1.8; }}
</style>
</head>
<body>
    <div class='page'>
        <div class='sidebar'>
            <img class='photo' src='{photo}' alt='Photo'>

            <h2>Contact</h2>
            <ul>
                <li>&#9993; alex.morgan@email.com</li>
                <li>&#9742; +1 (555) 234-5678</li>
                <li>&#127968; San Francisco, CA</li>
                <li>&#128279; linkedin.com/in/alexmorgan</li>
                <li>&#128187; github.com/alexmorgan</li>
            </ul>

            <h2>Skills</h2>
            <div class='skill'>
                <div class='skill-name'><span>C# / .NET</span><span>95%</span></div>
                <div class='skill-bar'><div class='skill-fill' style='width:95%'></div></div>
            </div>
            <div class='skill'>
                <div class='skill-name'><span>TypeScript</span><span>90%</span></div>
                <div class='skill-bar'><div class='skill-fill' style='width:90%'></div></div>
            </div>
            <div class='skill'>
                <div class='skill-name'><span>Python</span><span>85%</span></div>
                <div class='skill-bar'><div class='skill-fill' style='width:85%'></div></div>
            </div>
            <div class='skill'>
                <div class='skill-name'><span>SQL / PostgreSQL</span><span>88%</span></div>
                <div class='skill-bar'><div class='skill-fill' style='width:88%'></div></div>
            </div>
            <div class='skill'>
                <div class='skill-name'><span>Kubernetes</span><span>80%</span></div>
                <div class='skill-bar'><div class='skill-fill' style='width:80%'></div></div>
            </div>
            <div class='skill'>
                <div class='skill-name'><span>React</span><span>82%</span></div>
                <div class='skill-bar'><div class='skill-fill' style='width:82%'></div></div>
            </div>

            <h2>Languages</h2>
            <ul>
                <li>English — Native</li>
                <li>Spanish — Professional</li>
                <li>French — Conversational</li>
            </ul>

            <h2>Interests</h2>
            <ul>
                <li>Open Source Contributions</li>
                <li>Conference Speaking</li>
                <li>Mountain Biking</li>
                <li>Photography</li>
            </ul>
        </div>

        <div class='main'>
            <div class='name'>Alex Morgan</div>
            <div class='title-role'>Senior Software Engineer &bull; Platform Architecture</div>
            <div class='summary'>
                Seasoned software engineer with 10+ years of experience building scalable distributed
                systems and developer platforms. Led teams of 5-15 engineers at high-growth startups and
                Fortune 500 companies. Passionate about performance optimization, developer experience,
                and mentoring the next generation of engineers.
            </div>

            <div class='section'>
                <h2>Experience</h2>
                <div class='job'>
                    <div class='job-header'>
                        <span class='job-title'>Staff Software Engineer</span>
                        <span class='job-date'>Mar 2022 — Present</span>
                    </div>
                    <div class='job-company'>Cloudscale Inc. — San Francisco, CA</div>
                    <ul>
                        <li>Architected the next-generation API gateway serving 2B+ requests/day with p99 latency under 15ms</li>
                        <li>Led migration from monolith to microservices, reducing deployment time from 4 hours to 8 minutes</li>
                        <li>Designed and implemented a real-time event processing pipeline handling 500K events/sec</li>
                        <li>Mentored 8 engineers and established the company's first architecture review board</li>
                    </ul>
                </div>
                <div class='job'>
                    <div class='job-header'>
                        <span class='job-title'>Senior Software Engineer</span>
                        <span class='job-date'>Jun 2019 — Feb 2022</span>
                    </div>
                    <div class='job-company'>DataFlow Systems — Austin, TX</div>
                    <ul>
                        <li>Built a distributed task scheduling system processing 10M+ jobs daily with 99.99% reliability</li>
                        <li>Reduced infrastructure costs by 40% through auto-scaling optimization and spot instance utilization</li>
                        <li>Introduced observability stack (OpenTelemetry, Grafana, Loki) reducing MTTR from 2 hours to 15 minutes</li>
                    </ul>
                </div>
                <div class='job'>
                    <div class='job-header'>
                        <span class='job-title'>Software Engineer</span>
                        <span class='job-date'>Aug 2015 — May 2019</span>
                    </div>
                    <div class='job-company'>TechStart Labs — Seattle, WA</div>
                    <ul>
                        <li>Full-stack development on the core SaaS platform serving 50K+ enterprise users</li>
                        <li>Implemented OAuth 2.0 / OIDC authentication layer with multi-tenant support</li>
                        <li>Built CI/CD pipeline reducing release cycle from monthly to continuous deployment</li>
                    </ul>
                </div>
            </div>

            <div class='section'>
                <h2>Education</h2>
                <div class='edu'>
                    <div class='edu-degree'>M.S. Computer Science</div>
                    <div class='edu-school'>Stanford University</div>
                    <div class='edu-date'>2013 — 2015 &bull; Focus: Distributed Systems</div>
                </div>
                <div class='edu'>
                    <div class='edu-degree'>B.S. Computer Engineering</div>
                    <div class='edu-school'>University of California, Berkeley</div>
                    <div class='edu-date'>2009 — 2013 &bull; Magna Cum Laude</div>
                </div>
            </div>

            <div class='section'>
                <h2>Certifications</h2>
                <div class='certs'>
                    AWS Solutions Architect Professional &bull;
                    Google Cloud Professional Cloud Architect &bull;
                    Certified Kubernetes Administrator (CKA) &bull;
                    HashiCorp Terraform Associate
                </div>
            </div>
        </div>
    </div>
</body>
</html>";

            var options = new RenderOptions
            {
                PageSize = PageSize.Letter,
                MarginTop = 0f, MarginRight = 0f, MarginBottom = 0f, MarginLeft = 0f,
                Title = "Alex Morgan — Resume",
            };

            var pdf = RenderAndSave("resume", html, options);
            Assert.True(pdf.Length > 5000, $"Resume PDF too small: {pdf.Length} bytes");
            AssertValidPdf(pdf);
        }

        // ═══════════════════════════════════════════
        // Newsletter: multi-column, mixed media, callouts
        // ═══════════════════════════════════════════

        [Fact]
        public void Newsletter_WithMixedMedia_ProducesMultiPagePdf()
        {
            var bannerImg = GeneratePngDataUri(600, 200, "#1a1a2e");
            var articleImgs = new[]
            {
                GeneratePngDataUri(300, 200, "#e74c3c"),
                GeneratePngDataUri(300, 200, "#3498db"),
                GeneratePngDataUri(300, 200, "#2ecc71"),
            };
            var adImg = GeneratePngDataUri(500, 100, "#f39c12");

            var html = $@"
<!DOCTYPE html>
<html>
<head>
<style>
    * {{ box-sizing: border-box; }}
    body {{ font-family: Georgia, 'Times New Roman', serif; color: #333; margin: 0; padding: 0; }}
    .banner {{ position: relative; }}
    .banner img {{ width: 100%; height: 200px; display: block; }}
    .banner-text {{ position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);
        text-align: center; color: white; }}
    .banner-text h1 {{ font-size: 36px; margin: 0; text-shadow: 0 2px 8px rgba(0,0,0,0.5);
        letter-spacing: 4px; text-transform: uppercase; }}
    .banner-text p {{ font-size: 14px; opacity: 0.9; margin-top: 4px; }}
    .edition {{ text-align: center; font-size: 11px; color: #888; padding: 8px;
        border-bottom: 3px double #ddd; margin-bottom: 24px; }}
    .content {{ padding: 0 32px; }}
    .lead-article {{ margin-bottom: 24px; }}
    .lead-article h2 {{ font-size: 24px; color: #1a1a2e; margin: 0 0 8px 0; line-height: 1.3; }}
    .lead-article .byline {{ font-size: 11px; color: #888; font-style: italic; margin-bottom: 12px; }}
    .lead-article img {{ width: 100%; height: 200px; display: block; border-radius: 4px; margin-bottom: 12px; }}
    .lead-article p {{ font-size: 13px; line-height: 1.7; text-align: justify; margin-bottom: 8px; }}
    .two-articles {{ display: flex; gap: 24px; margin-bottom: 24px; }}
    .article {{ flex: 1; }}
    .article img {{ width: 100%; height: 160px; display: block; border-radius: 4px; margin-bottom: 8px; }}
    .article h3 {{ font-size: 16px; margin: 0 0 4px 0; color: #1a1a2e; line-height: 1.3; }}
    .article .byline {{ font-size: 10px; color: #888; font-style: italic; margin-bottom: 8px; }}
    .article p {{ font-size: 12px; line-height: 1.6; text-align: justify; }}
    .callout {{ background: #f0f4f8; border-left: 4px solid #667eea; padding: 16px 20px;
        margin: 20px 0; border-radius: 0 4px 4px 0; }}
    .callout h3 {{ font-size: 14px; color: #667eea; margin: 0 0 6px 0; }}
    .callout p {{ font-size: 12px; line-height: 1.5; margin: 0; }}
    .callout ul {{ font-size: 12px; line-height: 1.6; padding-left: 18px; margin: 6px 0 0 0; }}
    .ad {{ text-align: center; margin: 24px 0; padding: 16px; background: #fffbe6;
        border: 1px dashed #f0d060; border-radius: 4px; }}
    .ad img {{ max-width: 100%; height: auto; display: block; margin: 0 auto; }}
    .ad p {{ font-size: 9px; color: #999; margin-top: 4px; }}
    .opinion {{ background: #fafafa; padding: 20px; margin: 24px 0; border-radius: 4px; }}
    .opinion h3 {{ font-size: 16px; color: #1a1a2e; margin: 0 0 4px 0; }}
    .opinion .author {{ font-size: 11px; color: #667eea; margin-bottom: 8px; }}
    .opinion p {{ font-size: 12px; line-height: 1.7; text-align: justify; font-style: italic; }}
    .footer {{ text-align: center; padding: 20px; font-size: 10px; color: #999;
        border-top: 3px double #ddd; margin-top: 24px; }}
    .footer a {{ color: #667eea; text-decoration: none; }}
    .page-break {{ page-break-before: always; }}
</style>
</head>
<body>
    <div class='banner'>
        <img src='{bannerImg}' alt='Banner'>
        <div class='banner-text'>
            <h1>The Tech Weekly</h1>
            <p>Your source for technology news and analysis</p>
        </div>
    </div>
    <div class='edition'>Volume 12, Issue 3 &mdash; January 20, 2025 &mdash; Weekly Edition</div>

    <div class='content'>
        <div class='lead-article'>
            <h2>Quantum Computing Reaches New Milestone: 1,000 Logical Qubits Achieved</h2>
            <div class='byline'>By Sarah Chen &bull; January 18, 2025 &bull; 8 min read</div>
            <img src='{articleImgs[0]}' alt='Quantum'>
            <p>In a breakthrough that scientists are calling a watershed moment for the field,
            researchers at the Quantum Computing Institute have successfully demonstrated a
            system with over 1,000 logical qubits — a milestone that was widely predicted to
            be at least five years away. The achievement, published in Nature this week,
            represents a fundamental shift in the practical viability of quantum computation.</p>
            <p>The team used a novel error correction scheme based on topological codes that
            dramatically reduces the physical-to-logical qubit ratio from 1000:1 to just 50:1.
            This means that instead of needing millions of physical qubits, practical quantum
            advantage can now be achieved with systems in the tens of thousands — a scale that
            current manufacturing techniques can already support.</p>
            <p>""This changes everything about our timeline,"" said Dr. Elena Vasquez, lead
            researcher on the project. ""Problems in drug discovery, materials science, and
            cryptography that we thought were a decade away are now within reach of the next
            generation of machines, which we expect to see in 18 to 24 months.""</p>
        </div>

        <div class='callout'>
            <h3>This Week's Numbers</h3>
            <ul>
                <li><strong>$42.7B</strong> — Global AI infrastructure spending in Q4 2024 (+67% YoY)</li>
                <li><strong>2.1M</strong> — New developer accounts on cloud platforms in December</li>
                <li><strong>147</strong> — AI-related bills introduced in US Congress this session</li>
                <li><strong>89%</strong> — Fortune 500 companies now using generative AI in production</li>
            </ul>
        </div>

        <div class='two-articles'>
            <div class='article'>
                <img src='{articleImgs[1]}' alt='AI'>
                <h3>Open-Source AI Models Close Gap with Commercial Offerings</h3>
                <div class='byline'>By Michael Torres &bull; Jan 17, 2025</div>
                <p>The latest round of open-source language models has narrowed the performance
                gap with proprietary systems to less than 5% on standard benchmarks. Industry
                analysts note that this trend is accelerating enterprise adoption of self-hosted
                AI solutions, with cost savings of 60-80% compared to API-based approaches.
                The implications for data sovereignty and regulatory compliance are significant.</p>
            </div>
            <div class='article'>
                <img src='{articleImgs[2]}' alt='Robotics'>
                <h3>Humanoid Robots Enter Warehouse Operations at Scale</h3>
                <div class='byline'>By Lisa Park &bull; Jan 16, 2025</div>
                <p>Three major logistics companies announced this week that they will deploy
                over 10,000 humanoid robots across their distribution networks by mid-2025.
                The robots, capable of picking, packing, and sorting operations, represent the
                first large-scale commercial deployment of bipedal robots. Workers unions have
                called for comprehensive retraining programs and transition support.</p>
            </div>
        </div>

        <div class='ad'>
            <img src='{adImg}' alt='Advertisement'>
            <p>ADVERTISEMENT</p>
        </div>

        <div class='page-break'></div>

        <div class='opinion'>
            <h3>Opinion: The Privacy Paradox of Smart Cities</h3>
            <div class='author'>Dr. James Wright, Professor of Digital Ethics, MIT</div>
            <p>As cities around the world rush to deploy sensor networks, facial recognition systems,
            and predictive analytics platforms, we find ourselves at a critical juncture. The promise
            of smarter, safer, more efficient urban environments is real — but so are the risks of
            creating surveillance infrastructure that fundamentally alters the relationship between
            citizens and their government. We must ask ourselves: what kind of cities do we want to
            live in, and what safeguards must be in place before we cross lines that cannot be uncrossed?</p>
            <p>The answer lies not in rejecting technology but in demanding transparency, accountability,
            and democratic oversight. Cities like Barcelona and Amsterdam have shown that it's possible
            to embrace innovation while maintaining strong privacy protections. Their models deserve
            serious study as other cities chart their digital futures.</p>
        </div>

        <div class='callout'>
            <h3>Upcoming Events</h3>
            <ul>
                <li><strong>Jan 27-31:</strong> International Consumer Electronics Show (CES) — Las Vegas</li>
                <li><strong>Feb 10-12:</strong> AI Summit Global — London</li>
                <li><strong>Feb 24-28:</strong> Mobile World Congress (MWC) — Barcelona</li>
                <li><strong>Mar 10-14:</strong> SXSW Interactive — Austin</li>
            </ul>
        </div>

        <div class='footer'>
            The Tech Weekly &copy; 2025 &bull; <a href='#'>Unsubscribe</a> &bull;
            <a href='#'>View Online</a> &bull; <a href='#'>Privacy Policy</a><br>
            123 Media Street, Suite 400, San Francisco, CA 94102<br>
            You received this because you subscribed at thetechweekly.com
        </div>
    </div>
</body>
</html>";

            var options = new RenderOptions
            {
                PageSize = PageSize.A4,
                MarginTop = 0f, MarginRight = 0f, MarginBottom = 36f, MarginLeft = 0f,
                Title = "The Tech Weekly — January 20, 2025",
                FooterHtml = "<div style='font-size:8px;color:#bbb;text-align:center'>The Tech Weekly — Page {pageNumber} of {totalPages}</div>",
            };

            var pdf = RenderAndSave("newsletter", html, options);
            Assert.True(pdf.Length > 10000, $"Newsletter PDF too small: {pdf.Length} bytes");
            AssertValidPdf(pdf);

            // Should be multi-page due to page-break
            var content = Encoding.ASCII.GetString(pdf);
            int pageCount = CountOccurrences(content, "/Type /Page") - CountOccurrences(content, "/Type /Pages");
            Assert.True(pageCount >= 2, $"Newsletter: expected 2+ pages, got {pageCount}");
        }

        // ═══════════════════════════════════════════
        // Data-heavy: 100-row table spanning multiple pages
        // ═══════════════════════════════════════════

        [Fact]
        public void LargeTable_100Rows_ProducesMultiPagePdf()
        {
            var sb = new StringBuilder();
            sb.Append(@"
<!DOCTYPE html>
<html>
<head>
<style>
    * { box-sizing: border-box; }
    body { font-family: 'Segoe UI', Arial, sans-serif; color: #333; margin: 0; padding: 24px; }
    h1 { font-size: 20px; color: #1a1a2e; margin: 0 0 4px 0; }
    .subtitle { font-size: 12px; color: #888; margin-bottom: 16px; }
    .summary { display: flex; gap: 16px; margin-bottom: 20px; }
    .summary-card { flex: 1; background: linear-gradient(135deg, #667eea, #764ba2); color: white;
        padding: 10px; border-radius: 6px; text-align: center; }
    .summary-card .num { font-size: 18px; font-weight: bold; }
    .summary-card .lbl { font-size: 9px; text-transform: uppercase; opacity: 0.8; }
    table { width: 100%; border-collapse: collapse; font-size: 11px; }
    thead th { background: #2c3e50; color: white; padding: 8px 10px; text-align: left;
        font-size: 10px; text-transform: uppercase; letter-spacing: 0.5px; position: sticky; top: 0; }
    tbody td { padding: 6px 10px; border-bottom: 1px solid #eee; }
    tbody tr:nth-child(even) { background: #f9f9f9; }
    tbody tr:hover { background: #e8f4ff; }
    .status-active { color: #27ae60; font-weight: bold; }
    .status-inactive { color: #e74c3c; }
    .status-pending { color: #f39c12; }
    .text-right { text-align: right; }
    .footer-note { margin-top: 16px; font-size: 10px; color: #999; text-align: center;
        border-top: 1px solid #eee; padding-top: 8px; }
</style>
</head>
<body>
    <h1>Customer Account Registry</h1>
    <div class='subtitle'>Generated: January 15, 2025 — Sorted by Account Value (descending)</div>

    <div class='summary'>
        <div class='summary-card'><div class='num'>100</div><div class='lbl'>Total Accounts</div></div>
        <div class='summary-card'><div class='num'>72</div><div class='lbl'>Active</div></div>
        <div class='summary-card'><div class='num'>$4.2M</div><div class='lbl'>Total Value</div></div>
        <div class='summary-card'><div class='num'>94.3%</div><div class='lbl'>Retention</div></div>
    </div>

    <table>
        <thead>
            <tr>
                <th>#</th>
                <th>Account ID</th>
                <th>Company Name</th>
                <th>Contact</th>
                <th>Industry</th>
                <th>Status</th>
                <th class='text-right'>Annual Value</th>
                <th class='text-right'>Since</th>
            </tr>
        </thead>
        <tbody>");

            var companies = new[] { "Acme Corp", "GlobalTech", "NovaStar", "Pinnacle", "Vertex", "Horizon", "Catalyst",
                "Meridian", "Apex", "Summit", "Quantum", "Zenith", "Atlas", "Prism", "Forge" };
            var industries = new[] { "Technology", "Healthcare", "Finance", "Manufacturing", "Retail",
                "Energy", "Education", "Logistics", "Media", "Telecom" };
            var contacts = new[] { "J. Smith", "A. Johnson", "M. Williams", "S. Brown", "L. Davis",
                "R. Garcia", "K. Martinez", "P. Anderson", "T. Wilson", "C. Taylor" };
            var statuses = new[] { "Active", "Active", "Active", "Active", "Active", "Active", "Active",
                "Pending", "Pending", "Inactive" };

            var rng = new Random(42);
            for (int i = 0; i < 100; i++)
            {
                var company = companies[i % companies.Length] + (i >= companies.Length ? $" {(char)('A' + i / companies.Length)}" : "");
                var contact = contacts[i % contacts.Length];
                var industry = industries[i % industries.Length];
                var status = statuses[i % statuses.Length];
                var statusClass = status == "Active" ? "status-active" : status == "Pending" ? "status-pending" : "status-inactive";
                var value = rng.Next(5000, 200000);
                var year = rng.Next(2015, 2025);

                sb.Append($@"
            <tr>
                <td>{i + 1}</td>
                <td>ACC-{10000 + i}</td>
                <td>{company}</td>
                <td>{contact}</td>
                <td>{industry}</td>
                <td><span class='{statusClass}'>{status}</span></td>
                <td class='text-right'>${value:N0}</td>
                <td class='text-right'>{year}</td>
            </tr>");
            }

            sb.Append(@"
        </tbody>
    </table>
    <div class='footer-note'>
        Confidential — For internal use only — Generated by Customer Management System v3.2
    </div>
</body>
</html>");

            var options = new RenderOptions
            {
                PageSize = PageSize.Letter,
                MarginTop = 36f, MarginRight = 36f, MarginBottom = 48f, MarginLeft = 36f,
                Title = "Customer Account Registry",
                FooterHtml = "<div style='font-size:8px;color:#999;text-align:right;padding-right:8px'>Page {pageNumber} of {totalPages}</div>",
            };

            var pdf = RenderAndSave("large-table", sb.ToString(), options);
            Assert.True(pdf.Length > 10000, $"Large table PDF too small: {pdf.Length} bytes");
            AssertValidPdf(pdf);

            // 100 rows should produce multiple pages
            var pdfContent = Encoding.ASCII.GetString(pdf);
            int pageCount = CountOccurrences(pdfContent, "/Type /Page") - CountOccurrences(pdfContent, "/Type /Pages");
            Assert.True(pageCount >= 3, $"Large table: expected 3+ pages for 100 rows, got {pageCount}");
        }

        // ═══════════════════════════════════════════
        // CSS stress test: transforms, filters, clip-path, gradients, shadows
        // ═══════════════════════════════════════════

        [Fact]
        public void CssStressTest_AdvancedFeatures_ProducesValidPdf()
        {
            var bgImg = GeneratePngDataUri(100, 100, "#e8ecf8");

            var html = $@"
<!DOCTYPE html>
<html>
<head>
<style>
    * {{ box-sizing: border-box; }}
    body {{ font-family: Arial, sans-serif; color: #333; margin: 0; padding: 24px; background: #f5f5f5; }}
    h1 {{ text-align: center; font-size: 22px; color: #1a1a2e; margin-bottom: 24px; }}

    /* Gradient backgrounds */
    .gradient-row {{ display: flex; gap: 12px; margin-bottom: 20px; }}
    .grad-box {{ flex: 1; height: 80px; border-radius: 8px; display: flex; align-items: center;
        justify-content: center; color: white; font-weight: bold; font-size: 11px;
        text-shadow: 0 1px 2px rgba(0,0,0,0.3); }}
    .grad-linear {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); }}
    .grad-radial {{ background: radial-gradient(circle at 30% 30%, #f093fb 0%, #f5576c 100%); }}
    .grad-multi {{ background: linear-gradient(45deg, #FF6B6B, #FFA07A, #FFD700, #90EE90, #87CEEB, #9370DB); }}
    .grad-repeating {{ background: repeating-linear-gradient(45deg, #606dbc, #606dbc 10px, #465298 10px, #465298 20px); }}

    /* Box shadows */
    .shadow-row {{ display: flex; gap: 20px; margin-bottom: 24px; justify-content: center; }}
    .shadow-box {{ width: 120px; height: 80px; background: white; border-radius: 8px;
        display: flex; align-items: center; justify-content: center; font-size: 10px; color: #666; }}
    .shadow-soft {{ box-shadow: 0 4px 20px rgba(0,0,0,0.1); }}
    .shadow-hard {{ box-shadow: 8px 8px 0 #333; }}
    .shadow-inset {{ box-shadow: inset 0 4px 12px rgba(0,0,0,0.2); }}
    .shadow-multi {{ box-shadow: 0 4px 6px rgba(0,0,0,0.1), 0 10px 20px rgba(0,0,0,0.08),
        inset 0 -2px 4px rgba(0,0,0,0.05); }}

    /* Border radius variations */
    .radius-row {{ display: flex; gap: 16px; margin-bottom: 24px; justify-content: center; align-items: center; }}
    .radius-box {{ width: 80px; height: 80px; display: flex; align-items: center; justify-content: center;
        font-size: 10px; color: white; font-weight: bold; }}
    .radius-none {{ border-radius: 0; background: #e74c3c; }}
    .radius-sm {{ border-radius: 8px; background: #3498db; }}
    .radius-lg {{ border-radius: 20px; background: #2ecc71; }}
    .radius-pill {{ border-radius: 40px; background: #f39c12; }}
    .radius-circle {{ border-radius: 50%; background: #9b59b6; }}
    .radius-asym {{ border-radius: 30px 0 30px 0; background: #1abc9c; }}

    /* Opacity and overlapping */
    .overlap-container {{ position: relative; height: 120px; margin-bottom: 24px; }}
    .overlap-box {{ position: absolute; width: 120px; height: 80px; border-radius: 8px;
        display: flex; align-items: center; justify-content: center; font-size: 11px;
        color: white; font-weight: bold; }}
    .ov1 {{ background: rgba(231, 76, 60, 0.8); left: 20px; top: 10px; }}
    .ov2 {{ background: rgba(52, 152, 219, 0.8); left: 100px; top: 20px; }}
    .ov3 {{ background: rgba(46, 204, 113, 0.8); left: 180px; top: 0px; }}
    .ov4 {{ background: rgba(243, 156, 18, 0.8); left: 260px; top: 30px; }}

    /* Transforms */
    .transform-row {{ display: flex; gap: 24px; margin-bottom: 24px; justify-content: center;
        align-items: center; height: 100px; }}
    .t-box {{ width: 70px; height: 70px; background: #667eea; border-radius: 4px;
        display: flex; align-items: center; justify-content: center;
        color: white; font-size: 9px; font-weight: bold; }}
    .t-rotate {{ transform: rotate(15deg); }}
    .t-scale {{ transform: scale(1.2); }}
    .t-skew {{ transform: skewX(-10deg); }}
    .t-combo {{ transform: rotate(-5deg) scale(0.9); }}

    /* Nested borders and outlines */
    .nested-container {{ display: flex; gap: 16px; margin-bottom: 24px; justify-content: center; }}
    .nested-box {{ padding: 12px; font-size: 10px; text-align: center; }}
    .nb1 {{ border: 3px solid #e74c3c; border-radius: 8px; }}
    .nb2 {{ border: 2px dashed #3498db; }}
    .nb3 {{ border: 3px double #2ecc71; border-radius: 12px; }}
    .nb4 {{ border-top: 4px solid #f39c12; border-bottom: 4px solid #e74c3c;
        border-left: 4px solid #3498db; border-right: 4px solid #2ecc71; }}

    /* Background image with text overlay */
    .bg-section {{ position: relative; background-image: url('{bgImg}');
        background-size: 40px 40px; padding: 24px; margin-bottom: 24px; border-radius: 8px;
        border: 1px solid #ddd; }}
    .bg-overlay {{ background: rgba(255,255,255,0.92); padding: 16px; border-radius: 6px; }}
    .bg-overlay h3 {{ margin: 0 0 6px 0; font-size: 14px; color: #1a1a2e; }}
    .bg-overlay p {{ margin: 0; font-size: 12px; line-height: 1.5; color: #555; }}

    /* Grid with varied sizes */
    .mosaic {{ display: grid; grid-template-columns: repeat(4, 1fr); grid-auto-rows: 60px;
        gap: 8px; margin-bottom: 20px; }}
    .mosaic-item {{ background: #667eea; border-radius: 6px; display: flex;
        align-items: center; justify-content: center; color: white; font-size: 12px; font-weight: bold; }}
    .span-2c {{ grid-column: span 2; }}
    .span-2r {{ grid-row: span 2; }}
    .span-2c2r {{ grid-column: span 2; grid-row: span 2; }}
    .mi1 {{ background: #e74c3c; }}
    .mi2 {{ background: #3498db; }}
    .mi3 {{ background: #2ecc71; }}
    .mi4 {{ background: #f39c12; }}
    .mi5 {{ background: #9b59b6; }}
    .mi6 {{ background: #1abc9c; }}

    .section-label {{ font-size: 12px; font-weight: bold; color: #888; text-transform: uppercase;
        letter-spacing: 2px; margin-bottom: 8px; }}
</style>
</head>
<body>
    <h1>CSS Feature Showcase</h1>

    <div class='section-label'>Gradients</div>
    <div class='gradient-row'>
        <div class='grad-box grad-linear'>Linear</div>
        <div class='grad-box grad-radial'>Radial</div>
        <div class='grad-box grad-multi'>Multi-Stop</div>
        <div class='grad-box grad-repeating'>Repeating</div>
    </div>

    <div class='section-label'>Box Shadows</div>
    <div class='shadow-row'>
        <div class='shadow-box shadow-soft'>Soft</div>
        <div class='shadow-box shadow-hard'>Hard</div>
        <div class='shadow-box shadow-inset'>Inset</div>
        <div class='shadow-box shadow-multi'>Multiple</div>
    </div>

    <div class='section-label'>Border Radius</div>
    <div class='radius-row'>
        <div class='radius-box radius-none'>0</div>
        <div class='radius-box radius-sm'>8px</div>
        <div class='radius-box radius-lg'>20px</div>
        <div class='radius-box radius-pill'>Pill</div>
        <div class='radius-box radius-circle'>50%</div>
        <div class='radius-box radius-asym'>Asym</div>
    </div>

    <div class='section-label'>Opacity &amp; Overlapping</div>
    <div class='overlap-container'>
        <div class='overlap-box ov1'>Red 80%</div>
        <div class='overlap-box ov2'>Blue 80%</div>
        <div class='overlap-box ov3'>Green 80%</div>
        <div class='overlap-box ov4'>Orange 80%</div>
    </div>

    <div class='section-label'>Transforms</div>
    <div class='transform-row'>
        <div class='t-box'>Normal</div>
        <div class='t-box t-rotate'>Rotate 15&deg;</div>
        <div class='t-box t-scale'>Scale 1.2</div>
        <div class='t-box t-skew'>Skew -10&deg;</div>
        <div class='t-box t-combo'>Combo</div>
    </div>

    <div class='section-label'>Border Styles</div>
    <div class='nested-container'>
        <div class='nested-box nb1'>Solid Round</div>
        <div class='nested-box nb2'>Dashed</div>
        <div class='nested-box nb3'>Double Round</div>
        <div class='nested-box nb4'>Multi-Color</div>
    </div>

    <div class='section-label'>Background Pattern + Overlay</div>
    <div class='bg-section'>
        <div class='bg-overlay'>
            <h3>Tiled Background with Semi-Transparent Overlay</h3>
            <p>This section demonstrates a repeating background image pattern with a semi-transparent
            white overlay containing readable text. This pattern is commonly used for watermarks,
            decorative backgrounds, and branded document sections.</p>
        </div>
    </div>

    <div class='section-label'>CSS Grid Mosaic</div>
    <div class='mosaic'>
        <div class='mosaic-item mi1 span-2c2r'>A</div>
        <div class='mosaic-item mi2'>B</div>
        <div class='mosaic-item mi3'>C</div>
        <div class='mosaic-item mi4 span-2c'>D</div>
        <div class='mosaic-item mi5 span-2r'>E</div>
        <div class='mosaic-item mi6'>F</div>
        <div class='mosaic-item mi2'>G</div>
        <div class='mosaic-item mi3 span-2c'>H</div>
    </div>
</body>
</html>";

            var options = new RenderOptions
            {
                PageSize = PageSize.A4,
                MarginTop = 36f, MarginRight = 36f, MarginBottom = 36f, MarginLeft = 36f,
                Title = "CSS Feature Showcase",
            };

            var pdf = RenderAndSave("css-stress", html, options);
            Assert.True(pdf.Length > 5000, $"CSS stress test PDF too small: {pdf.Length} bytes");
            AssertValidPdf(pdf);
        }

        // ═══════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════

        /// <summary>
        /// Generate a gradient PNG as a data URI. Creates a real, valid PNG
        /// with IHDR, IDAT, and IEND chunks. Produces a diagonal gradient
        /// from the given color to a lighter version, simulating a photograph.
        /// </summary>
        private static string GeneratePngDataUri(int width, int height, string hexColor)
        {
            // Parse hex color
            byte r = Convert.ToByte(hexColor.Substring(1, 2), 16);
            byte g = Convert.ToByte(hexColor.Substring(3, 2), 16);
            byte b = Convert.ToByte(hexColor.Substring(5, 2), 16);

            // Create a lighter version for gradient end
            byte r2 = (byte)Math.Min(255, r + (255 - r) * 2 / 3);
            byte g2 = (byte)Math.Min(255, g + (255 - g) * 2 / 3);
            byte b2 = (byte)Math.Min(255, b + (255 - b) * 2 / 3);

            // Build raw pixel rows with diagonal gradient + noise pattern
            int rowBytes = 1 + width * 3;
            byte[] rawData = new byte[rowBytes * height];
            var rng = new Random(width * 1000 + height); // deterministic per size
            for (int y = 0; y < height; y++)
            {
                int offset = y * rowBytes;
                rawData[offset] = 0; // No filter
                for (int x = 0; x < width; x++)
                {
                    // Diagonal gradient factor (0..1)
                    float t = ((float)x / Math.Max(1, width - 1) + (float)y / Math.Max(1, height - 1)) / 2f;
                    // Add subtle noise for texture
                    int noise = rng.Next(-12, 13);
                    rawData[offset + 1 + x * 3] = (byte)Math.Clamp(r + (int)((r2 - r) * t) + noise, 0, 255);
                    rawData[offset + 2 + x * 3] = (byte)Math.Clamp(g + (int)((g2 - g) * t) + noise, 0, 255);
                    rawData[offset + 3 + x * 3] = (byte)Math.Clamp(b + (int)((b2 - b) * t) + noise, 0, 255);
                }
            }

            // Compress with zlib (deflate + zlib wrapper)
            byte[] compressed;
            using (var ms = new MemoryStream())
            {
                // Zlib header
                ms.WriteByte(0x78);
                ms.WriteByte(0x01);
                using (var deflate = new System.IO.Compression.DeflateStream(ms,
                    System.IO.Compression.CompressionLevel.Fastest, leaveOpen: true))
                {
                    deflate.Write(rawData, 0, rawData.Length);
                }
                // Adler32 checksum
                uint a = 1, adlerB = 0;
                for (int i = 0; i < rawData.Length; i++)
                {
                    a = (a + rawData[i]) % 65521;
                    adlerB = (adlerB + a) % 65521;
                }
                uint adler = (adlerB << 16) | a;
                ms.WriteByte((byte)(adler >> 24));
                ms.WriteByte((byte)(adler >> 16));
                ms.WriteByte((byte)(adler >> 8));
                ms.WriteByte((byte)adler);
                compressed = ms.ToArray();
            }

            // Build PNG
            using var png = new MemoryStream();
            // Signature
            png.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0, 8);

            void WriteChunk(string type, byte[] data)
            {
                var lenBytes = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(data.Length));
                png.Write(lenBytes, 0, 4);
                var typeBytes = Encoding.ASCII.GetBytes(type);
                png.Write(typeBytes, 0, 4);
                png.Write(data, 0, data.Length);
                // CRC32 over type+data
                uint crc = Crc32(typeBytes, data);
                var crcBytes = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder((int)crc));
                png.Write(crcBytes, 0, 4);
            }

            // IHDR: 13 bytes
            var ihdr = new byte[13];
            Array.Copy(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(width)), 0, ihdr, 0, 4);
            Array.Copy(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(height)), 0, ihdr, 4, 4);
            ihdr[8] = 8;  // bit depth
            ihdr[9] = 2;  // color type RGB
            ihdr[10] = 0; // compression
            ihdr[11] = 0; // filter
            ihdr[12] = 0; // interlace
            WriteChunk("IHDR", ihdr);

            // IDAT
            WriteChunk("IDAT", compressed);

            // IEND
            WriteChunk("IEND", Array.Empty<byte>());

            return "data:image/png;base64," + Convert.ToBase64String(png.ToArray());
        }

        private static uint Crc32(byte[] typeBytes, byte[] data)
        {
            // CRC32 lookup table
            uint[] table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint c = i;
                for (int j = 0; j < 8; j++)
                    c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
                table[i] = c;
            }

            uint crc = 0xFFFFFFFF;
            foreach (byte b in typeBytes)
                crc = table[(crc ^ b) & 0xFF] ^ (crc >> 8);
            foreach (byte b in data)
                crc = table[(crc ^ b) & 0xFF] ^ (crc >> 8);
            return crc ^ 0xFFFFFFFF;
        }

        private static byte[] RenderAndSave(string name, string html, RenderOptions options)
        {
            using var output = new MemoryStream();
            Render.ToPdf(html, output, options);
            var pdf = output.ToArray();
            SaveOutput(name, html, pdf, "pdf");
            return pdf;
        }

        private static void AssertValidPdf(byte[] pdf)
        {
            var header = Encoding.ASCII.GetString(pdf, 0, Math.Min(20, pdf.Length));
            Assert.StartsWith("%PDF-", header);
            var tail = Encoding.ASCII.GetString(pdf, Math.Max(0, pdf.Length - 32), Math.Min(32, pdf.Length));
            Assert.Contains("%%EOF", tail);
        }

        private static int CountOccurrences(string source, string substring)
        {
            int count = 0, index = 0;
            while ((index = source.IndexOf(substring, index, StringComparison.Ordinal)) != -1)
            {
                count++;
                index += substring.Length;
            }
            return count;
        }
    }
}
