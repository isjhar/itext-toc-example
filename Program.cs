using iText.IO.Font.Constants;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace TocFirstPageExample
{
    public class TableOfContents
    {
        public static readonly string DEST = "table_of_contents.pdf";

        private List<Pair<string, Pair<string, int>>> _toc = new List<Pair<string, Pair<string, int>>>();

        public static void Main(string[] args)
        {
            FileInfo file = new FileInfo(DEST);

            file.Directory?.Create();

            new TableOfContents().ManipulatePdf(DEST);
        }

        public void ManipulatePdf(string dest)
        {
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            PdfDocument pdfDoc = new PdfDocument(new PdfWriter(dest));
            var pageNumberEventHandler = new PageNumberEventHandler(pdfDoc);
            pdfDoc.AddEventHandler(PdfDocumentEvent.END_PAGE, pageNumberEventHandler);
            Document document = new Document(pdfDoc);
            document
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetFont(font)
                .SetFontSize(11);

            AddPage(pdfDoc, document, _toc, "page1", "Page 1");
            document.Add(new AreaBreak());

            AddPage(pdfDoc, document, _toc, "page2", "Page 2");
            document.Add(new AreaBreak());

            AddPage(pdfDoc, document, _toc, "page3", "Page 3");
            document.Add(new AreaBreak());

            CreateToc(document);

            // Move the table of contents to the first page
            pageNumberEventHandler.IsCreatingToc = true;
            int tocPageNumber = pdfDoc.GetNumberOfPages();
            pdfDoc.MovePage(tocPageNumber, 1);

            document.Close();
        }

        private void AddPage(PdfDocument pdfDoc, Document document, List<Pair<string, Pair<string, int>>> toc, string name, string text)
        {
            Pair<string, int> titlePage = new Pair<string, int>(text, pdfDoc.GetNumberOfPages());

            var p = new Paragraph(text);
            p.SetKeepTogether(true);
            p.SetFontSize(12)
                .SetKeepWithNext(true)
                .SetDestination(name)
                // Add the current page number to the table of contents list
                .SetNextRenderer(new UpdatePageRenderer(p, titlePage));

            document.Add(p);

            toc.Add(new Pair<string, Pair<string, int>>(name, titlePage));
        }

        private void CreateToc(Document document)
        {
            PdfFont bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // Create table of contents
            Paragraph p = new Paragraph("Table of Contents")
                .SetFont(bold)
                .SetDestination("toc");

            document.Add(p);

            List<TabStop> tabStops = new List<TabStop>();
            tabStops.Add(new TabStop(580, TabAlignment.RIGHT, new DottedLine()));
            foreach (Pair<string, Pair<string, int>> entry in _toc)
            {
                Pair<string, int> text = entry.Value;
                p = new Paragraph()
                    .AddTabStops(tabStops)
                    .Add(text.Key)
                    .Add(new Tab())
                    .Add(text.Value.ToString())
                    .SetAction(PdfAction.CreateGoTo(entry.Key));
                document.Add(p);
            }
        }
    }
}