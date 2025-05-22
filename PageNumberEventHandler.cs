using iText.IO.Font.Constants;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace TocFirstPageExample
{
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
}
