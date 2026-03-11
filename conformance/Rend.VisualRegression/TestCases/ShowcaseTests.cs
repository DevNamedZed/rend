using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class ShowcaseTests
    {
        static ShowcaseTests()
        {
            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "playground-hello",
                Name = "Playground: Hello from Rend",
                Category = "Playground",
                ViewportWidth = 595,
                ViewportHeight = 842,
                Html = @"<!DOCTYPE html>
<html>
<head>
<style>
  body {
    font-family: sans-serif;
    padding: 20px;
    background: #f5f5f5;
  }
  .card {
    background: white;
    border-radius: 12px;
    padding: 24px;
    box-shadow: 0 4px 20px rgba(0,0,0,0.1);
    max-width: 400px;
    margin: 20px auto;
  }
  h1 {
    color: #6C5CE7;
    margin-top: 0;
  }
  .badge {
    display: inline-block;
    background: linear-gradient(135deg, #6C5CE7, #a29bfe);
    color: white;
    padding: 4px 12px;
    border-radius: 20px;
    font-size: 14px;
  }
  .grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 12px;
    margin-top: 16px;
  }
  .stat {
    background: #f0f0f0;
    padding: 12px;
    border-radius: 8px;
    text-align: center;
  }
  .stat-value {
    font-size: 24px;
    font-weight: bold;
    color: #2d3436;
  }
  .stat-label {
    font-size: 12px;
    color: #636e72;
  }
</style>
</head>
<body>
  <div class=""card"">
    <h1>Hello from Rend!</h1>
    <p>This HTML is rendered entirely in your browser using <span class=""badge"">WebAssembly</span></p>
    <div class=""grid"">
      <div class=""stat"">
        <div class=""stat-value"">100%</div>
        <div class=""stat-label"">Client-side</div>
      </div>
      <div class=""stat"">
        <div class=""stat-value"">0</div>
        <div class=""stat-label"">Server calls</div>
      </div>
      <div class=""stat"">
        <div class=""stat-value"">CSS3</div>
        <div class=""stat-label"">Flexbox + Grid</div>
      </div>
      <div class=""stat"">
        <div class=""stat-value"">PDF</div>
        <div class=""stat-label"">+ Image output</div>
      </div>
    </div>
  </div>
</body>
</html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "playground-invoice",
                Name = "Playground: Invoice",
                Category = "Playground",
                ViewportWidth = 595,
                ViewportHeight = 842,
                Html = @"<!DOCTYPE html>
<html><head><style>
body { font-family: sans-serif; padding: 20px; }
h1 { color: #2d3436; border-bottom: 3px solid #6C5CE7; padding-bottom: 10px; }
table { width: 100%; border-collapse: collapse; margin-top: 20px; }
th { background: #6C5CE7; color: white; padding: 10px; text-align: left; }
td { padding: 10px; border-bottom: 1px solid #ddd; }
.total { font-size: 24px; text-align: right; margin-top: 20px; color: #6C5CE7; font-weight: bold; }
</style></head><body>
<h1>INVOICE #2024-001</h1>
<p>ACME Corp &mdash; January 15, 2024</p>
<table>
  <tr><th>Item</th><th>Qty</th><th>Price</th><th>Total</th></tr>
  <tr><td>Widget Pro</td><td>10</td><td>$29.99</td><td>$299.90</td></tr>
  <tr><td>Gadget Plus</td><td>5</td><td>$49.99</td><td>$249.95</td></tr>
  <tr><td>Service Fee</td><td>1</td><td>$99.00</td><td>$99.00</td></tr>
</table>
<div class=""total"">Total: $648.85</div>
</body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "playground-dashboard",
                Name = "Playground: Flexbox Dashboard",
                Category = "Playground",
                ViewportWidth = 595,
                ViewportHeight = 842,
                Html = @"<!DOCTYPE html>
<html><head><style>
body { font-family: sans-serif; margin: 0; padding: 20px; }
.flex-container { display: flex; gap: 16px; flex-wrap: wrap; }
.card { flex: 1; min-width: 120px; background: white; border-radius: 12px;
  padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); text-align: center; }
.card:nth-child(1) { background: linear-gradient(135deg, #6C5CE7, #a29bfe); color: white; }
.card:nth-child(2) { background: linear-gradient(135deg, #00b894, #55efc4); color: white; }
.card:nth-child(3) { background: linear-gradient(135deg, #e17055, #fab1a0); color: white; }
.card:nth-child(4) { background: linear-gradient(135deg, #0984e3, #74b9ff); color: white; }
.icon { font-size: 32px; }
h3 { margin: 10px 0 5px; }
p { margin: 0; font-size: 13px; opacity: 0.9; }
</style></head><body>
<h2 style=""color:#2d3436"">Dashboard</h2>
<div class=""flex-container"">
  <div class=""card""><div class=""icon"">&#9733;</div><h3>Revenue</h3><p>$12,450</p></div>
  <div class=""card""><div class=""icon"">&#9829;</div><h3>Users</h3><p>1,234</p></div>
  <div class=""card""><div class=""icon"">&#9998;</div><h3>Orders</h3><p>567</p></div>
  <div class=""card""><div class=""icon"">&#9650;</div><h3>Growth</h3><p>+23%</p></div>
</div>
</body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "playground-grid-mosaic",
                Name = "Playground: CSS Grid Mosaic",
                Category = "Playground",
                ViewportWidth = 595,
                ViewportHeight = 842,
                Html = @"<!DOCTYPE html>
<html><head><style>
body { font-family: sans-serif; margin: 0; padding: 20px; background: #f5f5f5; }
.grid { display: grid; grid-template-columns: 2fr 1fr 1fr; grid-template-rows: auto auto;
  gap: 12px; }
.item { background: white; border-radius: 8px; padding: 20px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.1); }
.hero { grid-column: 1 / 2; grid-row: 1 / 3; background: linear-gradient(135deg, #6C5CE7, #a29bfe);
  color: white; display: flex; align-items: center; justify-content: center; font-size: 24px;
  min-height: 150px; }
.item:nth-child(2) { background: #00b894; color: white; }
.item:nth-child(3) { background: #e17055; color: white; }
.item:nth-child(4) { grid-column: 2 / 4; background: #0984e3; color: white; }
</style></head><body>
<h2>CSS Grid Mosaic</h2>
<div class=""grid"">
  <div class=""item hero"">Featured</div>
  <div class=""item"">Widget A</div>
  <div class=""item"">Widget B</div>
  <div class=""item"">Spanning Widget</div>
</div>
</body></html>",
            });
        }
    }
}
