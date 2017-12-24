namespace SyncfusionPdfTutorial.PdfObjects
{
    public class Image : PdfObject
    {
        /// <summary>
        /// Path to where we'll find the image to draw
        /// </summary>
        public string ImagePath { get; set; }

        /// <summary>
        /// Indicates if ratio of the image should be preserved. For instance,
        /// if the image is 300x500, and the surrounding box is 500x500, if 
        /// this is true, then the image will be drawn as 300x500, however,
        /// if it is false, then it will be stretched horizontally to fit the
        /// width of 500.
        /// </summary>
        public bool PreserveRatio { get; set; }
    }
}