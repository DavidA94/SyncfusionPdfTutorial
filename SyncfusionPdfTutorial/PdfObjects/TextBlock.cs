using SyncfusionPdfTutorial.PdfObjects.Properties;

namespace SyncfusionPdfTutorial.PdfObjects
{
    public class TextBlock : PdfObject
    {
        public Font Font { get; set; }

        /// <summary>
        /// If set, this creates a bounding height that the text will not exceed.
        /// If there are too many lines, then the text will be truncated. This is
        /// only valid if AutoHeight is true.
        /// </summary>
        public double? MaxHeight { get; set; }

        /// <summary>
        /// If set, this creates a bounding width that the text will not exceed.
        /// The text will automatically break at this width and create a new line,
        /// if the MaxHeight will not be exceeded by doing so, otherwise, it will
        /// truncate. This is only valid if AutoWidth is true.
        /// </summary>
        public double? MaxWidth { get; set; }
        public string Text { get; set; }
        public bool VerticallyCenter { get; set; }

        /// <summary>
        /// Indicates if base Layout.Height should be ignored
        /// </summary>
        public bool AutoHeight { get; set; }

        /// <summary>
        /// Indicates if base Layout.Width should be ignored
        /// </summary>
        public bool AutoWidth { get; set; }
    }
}