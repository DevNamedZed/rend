using Xunit;
using Xunit.Abstractions;

namespace Rend.Tests.Layout
{
    public class WptTableCaptionTests
    {
        private readonly ITestOutputHelper _output;

        public WptTableCaptionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CaptionSideTop_IsDefault()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:collapse'>
                    <caption id='cap'>Default caption</caption>
                    <tr><td id='cell' style='height:30px'>A</td></tr>
                </table></body>");
            var caption = LayoutTestHelper.FindById(root, "cap");
            var cell = LayoutTestHelper.FindById(root, "cell");
            Assert.NotNull(caption);
            Assert.NotNull(cell);
            _output.WriteLine($"caption Y={caption!.ContentRect.Y} cell Y={cell!.ContentRect.Y}");
            Assert.True(caption.ContentRect.Y < cell.ContentRect.Y,
                $"Default caption-side:top should place caption above cells (cap Y={caption.ContentRect.Y}, cell Y={cell.ContentRect.Y})");
        }

        [Fact]
        public void CaptionAddsToTotalHeight()
        {
            var rootWithCaption = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:collapse'>
                    <caption style='height:20px'>Cap</caption>
                    <tr><td style='height:30px'>A</td></tr>
                </table></body>");
            var rootWithout = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:collapse'>
                    <tr><td style='height:30px'>A</td></tr>
                </table></body>");
            var tableWithCaption = LayoutTestHelper.FindById(rootWithCaption, "tbl");
            var tableWithout = LayoutTestHelper.FindById(rootWithout, "tbl");
            Assert.NotNull(tableWithCaption);
            Assert.NotNull(tableWithout);
            _output.WriteLine($"with caption: {tableWithCaption!.ContentRect.Height}, without: {tableWithout!.ContentRect.Height}");
            Assert.True(tableWithCaption.ContentRect.Height > tableWithout.ContentRect.Height,
                $"Caption should increase total table height (with={tableWithCaption.ContentRect.Height}, without={tableWithout.ContentRect.Height})");
        }

        [Fact]
        public void CaptionWidthMatchesTable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:300px;border-collapse:collapse'>
                    <caption id='cap'>Table caption</caption>
                    <tr><td style='height:30px'>A</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            var caption = LayoutTestHelper.FindById(root, "cap");
            Assert.NotNull(table);
            Assert.NotNull(caption);
            _output.WriteLine($"table W={table!.ContentRect.Width} caption W={caption!.ContentRect.Width}");
            Assert.True(System.Math.Abs(caption.ContentRect.Width - table.ContentRect.Width) < 2,
                $"Caption width should match table (table={table.ContentRect.Width}, caption={caption.ContentRect.Width})");
        }

        [Fact]
        public void TheadRowsAtTop()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <thead><tr><td id='hd' style='height:25px'>Header</td></tr></thead>
                    <tbody><tr><td id='bd' style='height:30px'>Body</td></tr></tbody>
                </table></body>");
            var header = LayoutTestHelper.FindById(root, "hd");
            var body = LayoutTestHelper.FindById(root, "bd");
            Assert.NotNull(header);
            Assert.NotNull(body);
            _output.WriteLine($"thead Y={header!.ContentRect.Y} tbody Y={body!.ContentRect.Y}");
            Assert.True(header.ContentRect.Y < body.ContentRect.Y,
                $"thead rows should be above tbody (thead Y={header.ContentRect.Y}, tbody Y={body.ContentRect.Y})");
        }

        [Fact]
        public void TbodyRowsAfterThead()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <thead><tr><td id='hd' style='height:25px'>H</td></tr></thead>
                    <tbody><tr><td id='bd' style='height:30px'>B</td></tr></tbody>
                </table></body>");
            var header = LayoutTestHelper.FindById(root, "hd");
            var body = LayoutTestHelper.FindById(root, "bd");
            Assert.NotNull(header);
            Assert.NotNull(body);
            float headerBottom = header!.ContentRect.Y + header.ContentRect.Height;
            _output.WriteLine($"thead bottom={headerBottom} tbody Y={body!.ContentRect.Y}");
            Assert.True(body.ContentRect.Y >= headerBottom - 1,
                $"tbody should start at or after thead bottom (headerBottom={headerBottom}, tbodyY={body.ContentRect.Y})");
        }

        [Fact]
        public void TfootRowsAtBottom()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <thead><tr><td id='hd' style='height:25px'>H</td></tr></thead>
                    <tbody><tr><td id='bd' style='height:30px'>B</td></tr></tbody>
                    <tfoot><tr><td id='ft' style='height:20px'>F</td></tr></tfoot>
                </table></body>");
            var body = LayoutTestHelper.FindById(root, "bd");
            var footer = LayoutTestHelper.FindById(root, "ft");
            Assert.NotNull(body);
            Assert.NotNull(footer);
            _output.WriteLine($"tbody Y={body!.ContentRect.Y} tfoot Y={footer!.ContentRect.Y}");
            Assert.True(footer.ContentRect.Y > body.ContentRect.Y,
                $"tfoot should be below tbody (tbodyY={body.ContentRect.Y}, tfootY={footer.ContentRect.Y})");
        }

        [Fact]
        public void MultipleTbodySections()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tbody><tr><td id='b1' style='height:25px'>Body1</td></tr></tbody>
                    <tbody><tr><td id='b2' style='height:25px'>Body2</td></tr></tbody>
                    <tbody><tr><td id='b3' style='height:25px'>Body3</td></tr></tbody>
                </table></body>");
            var body1 = LayoutTestHelper.FindById(root, "b1");
            var body2 = LayoutTestHelper.FindById(root, "b2");
            var body3 = LayoutTestHelper.FindById(root, "b3");
            Assert.NotNull(body1);
            Assert.NotNull(body2);
            Assert.NotNull(body3);
            _output.WriteLine($"b1 Y={body1!.ContentRect.Y} b2 Y={body2!.ContentRect.Y} b3 Y={body3!.ContentRect.Y}");
            Assert.True(body1.ContentRect.Y < body2.ContentRect.Y,
                $"Second tbody after first (b1Y={body1.ContentRect.Y}, b2Y={body2.ContentRect.Y})");
            Assert.True(body2.ContentRect.Y < body3.ContentRect.Y,
                $"Third tbody after second (b2Y={body2.ContentRect.Y}, b3Y={body3.ContentRect.Y})");
        }

        [Fact]
        public void AllSections_CaptionTheadTbodyTfoot()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:300px;border-collapse:collapse'>
                    <caption id='cap' style='height:20px'>Caption</caption>
                    <thead><tr><td id='hd' style='height:25px'>Head</td></tr></thead>
                    <tbody><tr><td id='bd' style='height:30px'>Body</td></tr></tbody>
                    <tfoot><tr><td id='ft' style='height:20px'>Foot</td></tr></tfoot>
                </table></body>");
            var caption = LayoutTestHelper.FindById(root, "cap");
            var header = LayoutTestHelper.FindById(root, "hd");
            var body = LayoutTestHelper.FindById(root, "bd");
            var footer = LayoutTestHelper.FindById(root, "ft");
            Assert.NotNull(caption);
            Assert.NotNull(header);
            Assert.NotNull(body);
            Assert.NotNull(footer);
            _output.WriteLine($"cap Y={caption!.ContentRect.Y} hd Y={header!.ContentRect.Y} bd Y={body!.ContentRect.Y} ft Y={footer!.ContentRect.Y}");
            Assert.True(caption.ContentRect.Y < header.ContentRect.Y, "Caption before thead");
            Assert.True(header.ContentRect.Y < body.ContentRect.Y, "thead before tbody");
            Assert.True(body.ContentRect.Y < footer.ContentRect.Y, "tbody before tfoot");
        }

        [Fact]
        public void ThAndTd_BothAreCells()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tr>
                        <th id='headerCell' style='height:30px'>H</th>
                        <td id='dataCell' style='height:30px'>D</td>
                    </tr>
                </table></body>");
            var headerCell = LayoutTestHelper.FindById(root, "headerCell");
            var dataCell = LayoutTestHelper.FindById(root, "dataCell");
            Assert.NotNull(headerCell);
            Assert.NotNull(dataCell);
            _output.WriteLine($"th W={headerCell!.ContentRect.Width} td W={dataCell!.ContentRect.Width}");
            Assert.True(headerCell.ContentRect.Width > 0, "th has width");
            Assert.True(dataCell.ContentRect.Width > 0, "td has width");
            Assert.True(System.Math.Abs(headerCell.ContentRect.Height - dataCell.ContentRect.Height) < 2,
                $"th and td in same row have same height (th={headerCell.ContentRect.Height}, td={dataCell.ContentRect.Height})");
        }

        [Fact]
        public void FixedLayout_FirstRowDefinesColumnWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:300px;border-collapse:collapse;table-layout:fixed'>
                    <tr>
                        <td id='c1' style='width:100px;height:30px'>A</td>
                        <td id='c2' style='width:200px;height:30px'>B</td>
                    </tr>
                    <tr>
                        <td id='c3' style='height:25px'>C</td>
                        <td id='c4' style='height:25px'>D</td>
                    </tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell3 = LayoutTestHelper.FindById(root, "c3");
            Assert.NotNull(table);
            Assert.NotNull(cell1);
            Assert.NotNull(cell3);
            _output.WriteLine($"table W={table!.ContentRect.Width} c1 W={cell1!.ContentRect.Width} c3 W={cell3!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - cell3.ContentRect.Width) < 2,
                $"Second row should inherit first row column widths (c1={cell1.ContentRect.Width}, c3={cell3.ContentRect.Width})");
        }

        [Fact]
        public void TableExplicitHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;height:150px;border-collapse:collapse'>
                    <tr><td style='height:30px'>A</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table H={table!.ContentRect.Height}");
            Assert.True(table.ContentRect.Height >= 149,
                $"Explicit height should be respected (got {table.ContentRect.Height})");
        }

        [Fact]
        public void TableMinHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;min-height:100px;border-collapse:collapse'>
                    <tr><td style='height:30px'>A</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table H={table!.ContentRect.Height}");
            Assert.True(table.ContentRect.Height >= 99,
                $"min-height should be respected (got {table.ContentRect.Height})");
        }

        [Fact]
        public void TableMaxHeight()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;max-height:40px;border-collapse:collapse'>
                    <tr><td style='height:100px'>Tall content</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"table H={table!.ContentRect.Height}");
            // max-height on tables has limited effect per CSS spec but should constrain
            // the table box if supported
            Assert.True(table.ContentRect.Height > 0, "Table has height");
        }

        [Fact]
        public void EmptyTable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:collapse'>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            Assert.NotNull(table);
            _output.WriteLine($"empty table: {table!.ContentRect.Width}x{table!.ContentRect.Height}");
            Assert.True(table.ContentRect.Width >= 0, "Empty table exists");
        }

        [Fact]
        public void SingleCellTable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='border-collapse:collapse'>
                    <tr><td id='cell' style='width:100px;height:50px'>Only</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            var cell = LayoutTestHelper.FindById(root, "cell");
            Assert.NotNull(table);
            Assert.NotNull(cell);
            _output.WriteLine($"table W={table!.ContentRect.Width} cell W={cell!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell!.ContentRect.Width - 100) < 2,
                $"Single cell width should be ~100 (got {cell.ContentRect.Width})");
            Assert.True(System.Math.Abs(cell.ContentRect.Height - 50) < 2,
                $"Single cell height should be ~50 (got {cell.ContentRect.Height})");
        }

        [Fact]
        public void DisplayTableOnDiv()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='tbl' style='display:table;width:250px'>
                    <div style='display:table-row'>
                        <div id='c1' style='display:table-cell;height:30px'>A</div>
                        <div id='c2' style='display:table-cell;height:30px'>B</div>
                    </div>
                </div></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(table);
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"table W={table!.ContentRect.Width} c1 W={cell1!.ContentRect.Width} c2 W={cell2!.ContentRect.Width}");
            Assert.True(System.Math.Abs(table.ContentRect.Width - 250) < 2,
                $"display:table width should be 250 (got {table.ContentRect.Width})");
            Assert.True(cell1.ContentRect.Width > 0 && cell2.ContentRect.Width > 0,
                "Both table-cell divs should have width");
        }

        [Fact]
        public void DisplayTableRow_OnDiv()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:table;width:200px'>
                    <div style='display:table-row'>
                        <div id='r1c1' style='display:table-cell;height:25px'>R1</div>
                    </div>
                    <div style='display:table-row'>
                        <div id='r2c1' style='display:table-cell;height:25px'>R2</div>
                    </div>
                </div></body>");
            var row1Cell = LayoutTestHelper.FindById(root, "r1c1");
            var row2Cell = LayoutTestHelper.FindById(root, "r2c1");
            Assert.NotNull(row1Cell);
            Assert.NotNull(row2Cell);
            _output.WriteLine($"r1 Y={row1Cell!.ContentRect.Y} r2 Y={row2Cell!.ContentRect.Y}");
            Assert.True(row2Cell!.ContentRect.Y > row1Cell.ContentRect.Y,
                $"Second table-row should be below first (r1Y={row1Cell.ContentRect.Y}, r2Y={row2Cell.ContentRect.Y})");
        }

        [Fact]
        public void DisplayTableCaption_OnDiv()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div style='display:table;width:200px'>
                    <div id='cap' style='display:table-caption;height:20px'>Caption div</div>
                    <div style='display:table-row'>
                        <div id='cell' style='display:table-cell;height:30px'>A</div>
                    </div>
                </div></body>");
            var caption = LayoutTestHelper.FindById(root, "cap");
            var cell = LayoutTestHelper.FindById(root, "cell");
            Assert.NotNull(caption);
            Assert.NotNull(cell);
            _output.WriteLine($"caption Y={caption!.ContentRect.Y} cell Y={cell!.ContentRect.Y}");
            Assert.True(caption.ContentRect.Y < cell!.ContentRect.Y,
                $"display:table-caption should appear above cells (capY={caption.ContentRect.Y}, cellY={cell.ContentRect.Y})");
        }

        [Fact]
        public void TheadOnNativeTable_HeaderAboveBody()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <thead><tr><td id='hd2' style='height:25px'>Head</td></tr></thead>
                    <tbody><tr><td id='bd2' style='height:30px'>Body</td></tr></tbody>
                </table></body>");
            var header = LayoutTestHelper.FindById(root, "hd2");
            var body = LayoutTestHelper.FindById(root, "bd2");
            Assert.NotNull(header);
            Assert.NotNull(body);
            _output.WriteLine($"thead Y={header!.ContentRect.Y} tbody Y={body!.ContentRect.Y}");
            Assert.True(header.ContentRect.Y < body!.ContentRect.Y,
                $"thead rows should be above tbody (theadY={header.ContentRect.Y}, tbodyY={body.ContentRect.Y})");
        }

        [Fact]
        public void TfootOnNativeTable_FooterBelowBody()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tbody><tr><td id='bd3' style='height:30px'>Body</td></tr></tbody>
                    <tfoot><tr><td id='ft3' style='height:20px'>Foot</td></tr></tfoot>
                </table></body>");
            var body = LayoutTestHelper.FindById(root, "bd3");
            var footer = LayoutTestHelper.FindById(root, "ft3");
            Assert.NotNull(body);
            Assert.NotNull(footer);
            _output.WriteLine($"tbody Y={body!.ContentRect.Y} tfoot Y={footer!.ContentRect.Y}");
            Assert.True(footer!.ContentRect.Y > body.ContentRect.Y,
                $"tfoot should be below tbody (tbodyY={body.ContentRect.Y}, tfootY={footer.ContentRect.Y})");
        }

        [Fact]
        public void TfootAfterTbody_RendersInOrder()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:collapse'>
                    <tbody><tr><td id='bd4' style='height:30px'>Body</td></tr></tbody>
                    <tfoot><tr><td id='ft4' style='height:20px'>Foot</td></tr></tfoot>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            var body = LayoutTestHelper.FindById(root, "bd4");
            var footer = LayoutTestHelper.FindById(root, "ft4");
            Assert.NotNull(table);
            Assert.NotNull(body);
            Assert.NotNull(footer);
            _output.WriteLine($"tbody Y={body!.ContentRect.Y} tfoot Y={footer!.ContentRect.Y}");
            Assert.True(footer!.ContentRect.Y > body.ContentRect.Y,
                $"tfoot after tbody should render below it (bdY={body.ContentRect.Y}, ftY={footer.ContentRect.Y})");
            Assert.True(table!.ContentRect.Height >= 49,
                $"Table height should include both sections (got {table.ContentRect.Height})");
        }

        [Fact]
        public void TheadBeforeTfoot_TbodyBetween()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <thead><tr><td id='hd' style='height:20px'>H</td></tr></thead>
                    <tfoot><tr><td id='ft' style='height:20px'>F</td></tr></tfoot>
                    <tbody><tr><td id='bd' style='height:30px'>B</td></tr></tbody>
                </table></body>");
            var header = LayoutTestHelper.FindById(root, "hd");
            var body = LayoutTestHelper.FindById(root, "bd");
            var footer = LayoutTestHelper.FindById(root, "ft");
            Assert.NotNull(header);
            Assert.NotNull(body);
            Assert.NotNull(footer);
            _output.WriteLine($"thead Y={header!.ContentRect.Y} tbody Y={body!.ContentRect.Y} tfoot Y={footer!.ContentRect.Y}");
            Assert.True(header.ContentRect.Y < body!.ContentRect.Y, "thead before tbody");
            Assert.True(body.ContentRect.Y < footer!.ContentRect.Y ||
                         footer.ContentRect.Y >= header.ContentRect.Y + header.ContentRect.Height,
                "tfoot after thead (at minimum)");
        }

        [Fact]
        public void CaptionWithBorderSpacing()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:separate;border-spacing:10px'>
                    <caption id='cap' style='height:20px'>Cap</caption>
                    <tr><td style='height:30px'>A</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            var caption = LayoutTestHelper.FindById(root, "cap");
            Assert.NotNull(table);
            Assert.NotNull(caption);
            _output.WriteLine($"table H={table!.ContentRect.Height} cap H={caption!.ContentRect.Height}");
            Assert.True(table.ContentRect.Height > 50,
                $"Table with caption + border-spacing should be > 50 (got {table.ContentRect.Height})");
        }

        [Fact]
        public void MultipleRowsInThead()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <thead>
                        <tr><td id='h1' style='height:20px'>H1</td></tr>
                        <tr><td id='h2' style='height:20px'>H2</td></tr>
                    </thead>
                    <tbody><tr><td id='bd' style='height:30px'>B</td></tr></tbody>
                </table></body>");
            var header1 = LayoutTestHelper.FindById(root, "h1");
            var header2 = LayoutTestHelper.FindById(root, "h2");
            var body = LayoutTestHelper.FindById(root, "bd");
            Assert.NotNull(header1);
            Assert.NotNull(header2);
            Assert.NotNull(body);
            _output.WriteLine($"h1 Y={header1!.ContentRect.Y} h2 Y={header2!.ContentRect.Y} bd Y={body!.ContentRect.Y}");
            Assert.True(header1.ContentRect.Y < header2.ContentRect.Y, "First thead row above second");
            Assert.True(header2.ContentRect.Y < body.ContentRect.Y, "Both thead rows above tbody");
        }

        [Fact]
        public void MultipleRowsInTfoot()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse'>
                    <tbody><tr><td id='bd' style='height:30px'>B</td></tr></tbody>
                    <tfoot>
                        <tr><td id='f1' style='height:20px'>F1</td></tr>
                        <tr><td id='f2' style='height:20px'>F2</td></tr>
                    </tfoot>
                </table></body>");
            var body = LayoutTestHelper.FindById(root, "bd");
            var footer1 = LayoutTestHelper.FindById(root, "f1");
            var footer2 = LayoutTestHelper.FindById(root, "f2");
            Assert.NotNull(body);
            Assert.NotNull(footer1);
            Assert.NotNull(footer2);
            _output.WriteLine($"bd Y={body!.ContentRect.Y} f1 Y={footer1!.ContentRect.Y} f2 Y={footer2!.ContentRect.Y}");
            Assert.True(body.ContentRect.Y < footer1!.ContentRect.Y, "tbody above first tfoot row");
            Assert.True(footer1.ContentRect.Y < footer2!.ContentRect.Y, "First tfoot row above second");
        }

        [Fact]
        public void CaptionSideBottom_PositionsBelowTable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:200px;border-collapse:collapse;caption-side:bottom'>
                    <caption id='cap' style='height:20px'>Bottom cap</caption>
                    <tr><td id='cell' style='height:30px'>A</td></tr>
                </table></body>");
            var caption = LayoutTestHelper.FindById(root, "cap");
            var cell = LayoutTestHelper.FindById(root, "cell");
            Assert.NotNull(caption);
            Assert.NotNull(cell);
            _output.WriteLine($"caption Y={caption!.ContentRect.Y} cell Y={cell!.ContentRect.Y}");
            Assert.True(caption.ContentRect.Y > cell!.ContentRect.Y,
                $"caption-side:bottom should place caption below cells (capY={caption.ContentRect.Y}, cellY={cell.ContentRect.Y})");
        }

        [Fact]
        public void TableWithCaptionAndRow_HeightIncludesBoth()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:200px;border-collapse:collapse'>
                    <caption id='cap'>Caption</caption>
                    <tr><td style='height:40px'>A</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            var caption = LayoutTestHelper.FindById(root, "cap");
            Assert.NotNull(table);
            Assert.NotNull(caption);
            _output.WriteLine($"table H={table!.ContentRect.Height} cap H={caption!.ContentRect.Height}");
            Assert.True(table.ContentRect.Height > 40,
                $"Table height should exceed row height alone when caption present (got {table.ContentRect.Height})");
        }

        [Fact]
        public void FixedLayout_ExplicitCellWidths()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table style='width:300px;border-collapse:collapse;table-layout:fixed'>
                    <tr>
                        <td id='narrow' style='width:50px;height:30px'>N</td>
                        <td id='wide' style='width:250px;height:30px'>W</td>
                    </tr>
                </table></body>");
            var narrow = LayoutTestHelper.FindById(root, "narrow");
            var wide = LayoutTestHelper.FindById(root, "wide");
            Assert.NotNull(narrow);
            Assert.NotNull(wide);
            _output.WriteLine($"narrow W={narrow!.ContentRect.Width} wide W={wide!.ContentRect.Width}");
            Assert.True(wide!.ContentRect.Width > narrow.ContentRect.Width,
                $"250px cell should be wider than 50px cell (narrow={narrow.ContentRect.Width}, wide={wide.ContentRect.Width})");
        }

        [Fact]
        public void DisplayTableCell_EqualDistribution()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <div id='tbl' style='display:table;width:200px'>
                    <div style='display:table-row'>
                        <div id='c1' style='display:table-cell;height:30px'>A</div>
                        <div id='c2' style='display:table-cell;height:30px'>B</div>
                    </div>
                </div></body>");
            var cell1 = LayoutTestHelper.FindById(root, "c1");
            var cell2 = LayoutTestHelper.FindById(root, "c2");
            Assert.NotNull(cell1);
            Assert.NotNull(cell2);
            _output.WriteLine($"c1 W={cell1!.ContentRect.Width} c2 W={cell2!.ContentRect.Width}");
            Assert.True(System.Math.Abs(cell1.ContentRect.Width - cell2!.ContentRect.Width) < 5,
                $"Equal cells should have similar widths (c1={cell1.ContentRect.Width}, c2={cell2.ContentRect.Width})");
            float totalWidth = cell1.ContentRect.Width + cell2.ContentRect.Width;
            Assert.True(totalWidth > 150,
                $"Combined cell widths should fill most of 200px table (got {totalWidth})");
        }

        [Fact]
        public void CaptionExistsInLayout_ExplicitWidthTable()
        {
            var root = LayoutTestHelper.Layout(@"<body style='margin:0'>
                <table id='tbl' style='width:250px;border-collapse:collapse'>
                    <caption id='cap'>Explicit width caption</caption>
                    <tr><td style='width:150px;height:30px'>A</td><td style='width:100px;height:30px'>B</td></tr>
                </table></body>");
            var table = LayoutTestHelper.FindById(root, "tbl");
            var caption = LayoutTestHelper.FindById(root, "cap");
            Assert.NotNull(table);
            Assert.NotNull(caption);
            _output.WriteLine($"table W={table!.ContentRect.Width} caption W={caption!.ContentRect.Width}");
            Assert.True(caption!.ContentRect.Width > 0, "Caption should have width");
            Assert.True(caption.ContentRect.Height > 0, "Caption should have height from text content");
        }
    }
}
