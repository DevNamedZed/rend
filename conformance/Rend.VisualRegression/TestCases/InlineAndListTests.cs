using Rend.VisualRegression.Infrastructure;

namespace Rend.VisualRegression.TestCases
{
    public static class InlineAndListTests
    {
        static InlineAndListTests()
        {
            // --- Inline Formatting ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "inline-spans-nested",
                Name = "Nested Inline Spans",
                Category = "Inline Formatting",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.6; background:#fff;"">
                    <p style=""margin:0;"">Normal text <span style=""color:red; font-weight:bold;"">bold red</span>
                    then <span style=""background:#e8f4f8; padding:2px 4px; border-radius:3px;"">highlighted</span>
                    and <span style=""text-decoration:underline; color:#3498db;"">underlined blue</span> text.</p>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "inline-badges",
                Name = "Inline Badge Elements",
                Category = "Inline Formatting",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <p style=""margin:0 0 8px;"">Status:
                        <span style=""background:#27ae60; color:#fff; padding:2px 8px; border-radius:10px; font-size:12px;"">Active</span>
                        <span style=""background:#e74c3c; color:#fff; padding:2px 8px; border-radius:10px; font-size:12px;"">Error</span>
                        <span style=""background:#f39c12; color:#fff; padding:2px 8px; border-radius:10px; font-size:12px;"">Warning</span>
                    </p>
                    <p style=""margin:0;"">Priority:
                        <span style=""background:#3498db; color:#fff; padding:1px 6px; border-radius:3px; font-size:11px;"">P1</span>
                        <span style=""background:#95a5a6; color:#fff; padding:1px 6px; border-radius:3px; font-size:11px;"">P2</span>
                    </p>
                </body></html>",
            });

            // --- List Styles ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "list-ordered-types",
                Name = "Ordered List Types",
                Category = "List Styles",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <ol style=""list-style-type:decimal; margin:0 0 8px;""><li>Decimal one</li><li>Decimal two</li></ol>
                    <ol style=""list-style-type:lower-alpha; margin:0 0 8px;""><li>Alpha one</li><li>Alpha two</li></ol>
                    <ol style=""list-style-type:upper-roman; margin:0;""><li>Roman one</li><li>Roman two</li></ol>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "list-unordered-types",
                Name = "Unordered List Types",
                Category = "List Styles",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <ul style=""list-style-type:disc; margin:0 0 8px;""><li>Disc one</li><li>Disc two</li></ul>
                    <ul style=""list-style-type:circle; margin:0 0 8px;""><li>Circle one</li><li>Circle two</li></ul>
                    <ul style=""list-style-type:square; margin:0;""><li>Square one</li><li>Square two</li></ul>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "list-nested",
                Name = "Nested Lists",
                Category = "List Styles",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:13px; background:#fff;"">
                    <ul style=""margin:0;"">
                        <li>Item 1
                            <ul><li>Sub-item A</li><li>Sub-item B</li></ul>
                        </li>
                        <li>Item 2
                            <ol><li>Numbered A</li><li>Numbered B</li></ol>
                        </li>
                    </ul>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "list-position-inside",
                Name = "List Style Position Inside",
                Category = "List Styles",
                Html = @"<html><body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <ul style=""list-style-position:outside; margin:0 0 10px; background:#f0f0f0; padding-left:30px;"">
                        <li style=""background:#ddd;"">Outside position (default)</li>
                        <li style=""background:#ddd;"">Second item outside</li>
                    </ul>
                    <ul style=""list-style-position:inside; margin:0; background:#f0f0f0; padding-left:10px;"">
                        <li style=""background:#ddd;"">Inside position</li>
                        <li style=""background:#ddd;"">Second item inside</li>
                    </ul>
                </body></html>",
            });

            // --- Pseudo-elements ---

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pseudo-before-after",
                Name = "::before and ::after",
                Category = "Pseudo-elements",
                Html = @"<html>
                <head><style>
                    .quote::before { content: '\201C'; color: #3498db; font-size: 20px; }
                    .quote::after { content: '\201D'; color: #3498db; font-size: 20px; }
                    .required::after { content: ' *'; color: red; }
                </style></head>
                <body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <p class=""quote"" style=""margin:0 0 8px;"">This is a quoted paragraph</p>
                    <p style=""margin:0;""><span class=""required"">Email</span>: user@example.com</p>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pseudo-first-letter",
                Name = "::first-letter Drop Cap",
                Category = "Pseudo-elements",
                Html = @"<html>
                <head><style>
                    .dropcap::first-letter {
                        font-size: 2.5em;
                        font-weight: bold;
                        color: #e74c3c;
                        float: left;
                        margin-right: 4px;
                        line-height: 0.8;
                    }
                </style></head>
                <body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; line-height:1.5; background:#fff;"">
                    <p class=""dropcap"" style=""margin:0;"">Once upon a time there was a paragraph with a decorative first letter that floated to the left.</p>
                </body></html>",
            });

            VisualTestCatalog.Register(new VisualTestCase
            {
                Id = "pseudo-counters",
                Name = "CSS Counters",
                Category = "Pseudo-elements",
                Html = @"<html>
                <head><style>
                    .counter-list { counter-reset: section; list-style-type: none; padding-left: 0; margin: 0; }
                    .counter-list li { counter-increment: section; margin-bottom: 4px; }
                    .counter-list li::before { content: counter(section) '. '; font-weight: bold; color: #3498db; }
                </style></head>
                <body style=""margin:0; padding:10px; font-family:sans-serif; font-size:14px; background:#fff;"">
                    <ul class=""counter-list"">
                        <li>First item</li>
                        <li>Second item</li>
                        <li>Third item</li>
                    </ul>
                </body></html>",
            });
        }
    }
}
