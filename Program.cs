using iText.IO.Font.Constants;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Layout;
using iText.Layout.Properties;
using iText.Layout.Renderer;

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

        public void ManipulatePdf(String dest)
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
            foreach (Pair<String, Pair<String, int>> entry in _toc)
            {
                Pair<String, int> text = entry.Value;
                p = new Paragraph()
                    .AddTabStops(tabStops)
                    .Add(text.Key)
                    .Add(new Tab())
                    .Add(text.Value.ToString())
                    .SetAction(PdfAction.CreateGoTo(entry.Key));
                document.Add(p);
            }
        }

        public class PageNumberEventHandler : IEventHandler
        {
            private readonly PdfDocument _pdf;

            public bool IsCreatingToc { get; set; } = false;

            public PageNumberEventHandler(PdfDocument pdf)
            {
                _pdf = pdf;
            }

            public void HandleEvent(Event currentEvent)
            {
                PdfDocumentEvent docEvent = (PdfDocumentEvent)currentEvent;
                PdfPage page = docEvent.GetPage();
                int pageNumber = _pdf.GetPageNumber(page);

                if (IsCreatingToc)
                {
                    if (pageNumber == 1) return;

                    pageNumber = pageNumber - 1;
                }

                Rectangle pageSize = page.GetPageSize();
                PdfCanvas canvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), _pdf);
                Canvas footerCanvas = new Canvas(canvas, pageSize);

                Paragraph p = new Paragraph($"{pageNumber}")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.CENTER);

                footerCanvas
                    .ShowTextAligned(p, pageSize.GetWidth() / 2, 20, TextAlignment.CENTER);
                footerCanvas.Close();
            }
        }

        private class UpdatePageRenderer : ParagraphRenderer
        {
            protected Pair<String, int> entry;

            public UpdatePageRenderer(Paragraph modelElement, Pair<String, int> entry) :
                base(modelElement)
            {
                this.entry = entry;
            }

            public override LayoutResult Layout(LayoutContext layoutContext)
            {
                LayoutResult result = base.Layout(layoutContext);
                entry.Value = layoutContext.GetArea().GetPageNumber();
                return result;
            }
        }


        private class Pair<T, U>
        {
            public Pair(T first, U second)
            {
                this.Key = first;
                this.Value = second;
            }

            public T Key { get; set; }

            public U Value { get; set; }
        };
    }
}