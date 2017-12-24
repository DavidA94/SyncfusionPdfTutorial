using SyncfusionPdfTutorial.PdfObjects;
using System.Collections.Generic;

namespace SyncfusionPdfTutorial
{
    public class Document
    {
        public string Title { get; set; }
        public List<PdfObject> Shapes { get; set; }
    }
}