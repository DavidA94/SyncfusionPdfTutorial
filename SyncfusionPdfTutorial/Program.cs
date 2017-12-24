using Syncfusion.Pdf.Graphics;
using SyncfusionPdfTutorial.PdfObjects;
using SyncfusionPdfTutorial.PdfObjects.Properties;
using System.Collections.Generic;
using System.IO;

namespace SyncfusionPdfTutorial
{
    class Program
    {
        static void Main(string[] args)
        {
            var document = new Document
            {
                Title = "Syncfusion Tutorial!",
                Shapes = new List<PdfObject>
                {
                    new Box
                    {
                        Appearance = new Appearance
                        {
                            Background = "#0000FF", // Blue fill
                        },
                        Border = new Border
                        {
                            Color = "#FF0000", // Red
                            Top = 1,
                            Right = 1,
                            Bottom = 1,
                            Left = 1
                        },
                        Layout = new Layout
                        {
                            Height = 90,
                            Width = 160,
                            X = 0,
                            Y = 50,
                        }
                    },
                    new CheckBox
                    {
                        IsChecked = true,
                        Layout = new Layout
                        {
                            Height = 20,
                            Width = 20,
                            X = 0,
                            Y = 150
                        }
                    },
                    new CheckBox
                    {
                        IsChecked = false,
                        Layout = new Layout
                        {
                            Height = 20,
                            Width = 20,
                            X = 30,
                            Y = 150
                        }
                    },
                    new TextBlock
                    {
                        Text = "This is some text that will add new lines, because it is longer than its MaxWidth property.",
                        AutoWidth = true,
                        AutoHeight = true,
                        MaxWidth = 4 * PdfBuilder.PPI,
                        Layout = new Layout
                        {
                            X = 0,
                            Y = 180,
                        },
                        Font = new Font
                        {
                            Alignment = new PdfStringFormat(PdfTextAlignment.Right),
                            Bold = true,
                            Color = "#FF00FF",
                            Family = "Arial",
                            Size = 16
                        }
                    },

                    new Image
                    {
                        ImagePath = Path.Combine(Directory.GetCurrentDirectory(), "image.jpeg"),
                        PreserveRatio = true,
                        Layout = new Layout
                        {
                            Width = 150,
                            Height = 150,
                            X = 0,
                            Y = 220
                        }
                    },

                    new TextBlock
                    {
                        Text = "This text will be on the second page because it was pushed down",
                        AutoWidth = true,
                        AutoHeight = true,
                        MaxWidth = 7 * PdfBuilder.PPI,
                        Layout = new Layout
                        {
                            X = 0,
                            Y = 9.5f * PdfBuilder.PPI,
                        },
                        Font = new Font
                        {
                            Alignment = new PdfStringFormat(PdfTextAlignment.Left),
                            Family = "Arial",
                            Size = 20
                        }
                    },

                    new Image
                    {
                        ImagePath = Path.Combine(Directory.GetCurrentDirectory(), "image.jpeg"),
                        PreserveRatio = false,
                        Layout = new Layout
                        {
                            Width = 150,
                            Height = 150,
                            X = 0,
                            Y = 9.6f * PdfBuilder.PPI
                        }
                    },
                }
            };

            var builder = new PdfBuilder();
            builder.Generate(document);
        }
    }
}