using SyncfusionPdfTutorial.PdfObjects.Properties;

namespace SyncfusionPdfTutorial.PdfObjects
{
    public abstract class PdfObject
    {
        public Appearance Appearance { get; set; }
        public Border Border { get; set; }
        public Layout Layout { get; set; }
    }
}