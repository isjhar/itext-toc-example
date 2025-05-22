using iText.Commons.Utils;
using iText.Layout.Element;
using iText.Layout.Layout;
using iText.Layout.Renderer;

namespace TocFirstPageExample
{
    public class UpdatePageRenderer : ParagraphRenderer
    {
        protected Pair<string, int> entry;

        public UpdatePageRenderer(Paragraph modelElement, Pair<string, int> entry) :
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
}
