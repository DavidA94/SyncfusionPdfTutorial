using Syncfusion.Pdf.Graphics;

namespace SyncfusionPdfTutorial.PdfObjects.Properties
{
    public class Font
    {
        public PdfStringFormat Alignment { get; set; }
        public bool Bold { get; set; }
        public string Color { get; set; } = "#000000"; // Black
        public string Family { get; set; }
        public bool Italic { get; set; }
        public double Size { get; set; }
    }
}