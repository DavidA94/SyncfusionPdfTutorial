using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using SyncfusionPdfTutorial.PdfObjects;
using SyncfusionPdfTutorial.PdfObjects.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SyncfusionPdfTutorial
{
    public class PdfBuilder
    {
        public const int PPI = 70;
        public const double LINE_HEIGHT_RATIO = 1.4;

        private double m_globalShift = 0;

        public string Generate(Document document)
        {
            const float height = 11f * PPI;
            const float width = 8.5f * PPI;
            const float margin = 0.25f * PPI;

            var doc = new PdfDocument();
            doc.DocumentInformation.Title = document.Title;
            doc.PageSettings.SetMargins(margin);
            doc.PageSettings.Size = new SizeF(width, height);

            PdfPage page = doc.Pages.Add();

            var shapes = document.Shapes.OrderBy(s => s.Layout.Y).ToList();
            foreach (dynamic shape in shapes)
            {
                // If we've passed the bottom of the page, due to shifting from the global offset
                var pdfObject = (shape as PdfObject);
                var realLayout = GetRealLayout(pdfObject);
                if (realLayout.Y + realLayout.Height + m_globalShift > height - (2 * margin))
                {
                    page = doc.Pages.Add();

                    // Need to shift everything up now
                    m_globalShift = -pdfObject.Layout.Y;
                    page.Graphics.TranslateTransform(0f, (float)m_globalShift);
                }
                Draw(page.Graphics, shape);
                DrawMain(page.Graphics, shape);
            }

            string fileName = Path.Combine(Directory.GetCurrentDirectory(),
                                           $"{document.Title}.pdf");
            using (MemoryStream stream = new MemoryStream())
            {
                doc.Save(stream);
                File.WriteAllBytes(fileName, stream.ToArray());
            }

            return fileName;
        }

        private void Draw(PdfGraphics g, Box box)
        {
            var boxX = (float)box.Layout.X;
            var boxY = (float)box.Layout.Y;
            var boxH = (float)box.Layout.Height;
            var boxW = (float)box.Layout.Width;

            var color = new PdfColor(GetColor(box.Appearance.Background));
            var background = new PdfSolidBrush(color);
            g.DrawRectangle(background, boxX, boxY, boxW, boxH);
        }

        private void Draw(PdfGraphics g, CheckBox box)
        {
            var lineWidth = Math.Min(box.Layout.Width, box.Layout.Height) / 10.0;

            var x = box.Layout.X;
            var y = box.Layout.Y;
            var width = box.Layout.Width - lineWidth;
            var height = box.Layout.Height - lineWidth;

            var shiftX = 0.0;
            var shiftY = 0.0;

            if (width > height)
            {
                shiftX = (width - height) / 2;
                width = height;
            }

            if (width < height)
            {
                shiftY = (height - width) / 2.0;
                height = width;
            }

            shiftX += x + (lineWidth / 2);
            shiftY += y + (lineWidth / 2);

            var pen = new PdfPen(PdfBrushes.Black, (float)lineWidth);
            g.DrawRectangle(pen, (float)x, (float)y, (float)width, (float)height);

            if (box.IsChecked)
            {
                pen = new PdfPen(PdfBrushes.Black, (float)lineWidth * 1.5f);
                g.DrawLine(pen, (float)(x + width * 0.2), (float)(y + height * 0.57),
                                (float)(x + width * 0.4), (float)(y + height * 0.77));
                g.DrawLine(pen, (float)(x + width * 0.4), (float)(y + height * 0.77),
                                (float)(x + width * 0.8), (float)(y + height * 0.27));
            }
        }

        private void Draw(PdfGraphics g, PdfObjects.Image image)
        {
            using (var stream = new FileStream(image.ImagePath, FileMode.Open))
            {
                using (var bitmap = new PdfBitmap(stream))
                {
                    float x = (float)image.Layout.X;
                    float y = (float)image.Layout.Y;
                    float width = (float)image.Layout.Width;
                    float height = (float)image.Layout.Height;

                    if (image.PreserveRatio)
                    {
                        height = bitmap.Height * (width / bitmap.Width);

                        // If we went the wrong way
                        if (height > image.Layout.Height)
                        {
                            height = (float)image.Layout.Height;
                            width = bitmap.Width * (height / bitmap.Height);
                        }
                    }

                    g.DrawImage(bitmap, new PointF(x, y), new SizeF(width, height));
                }
            }
        }

        private void Draw(PdfGraphics g, TextBlock block)
        {
            var brush = new PdfSolidBrush(new PdfColor(GetColor(block.Font.Color)));
            string fontStyle = "";
            if (block.Font.Bold) fontStyle += "b";
            if (block.Font.Italic) fontStyle += "i";

            string fontFile = Path.Combine(Directory.GetCurrentDirectory(), "Fonts", block.Font.Family, fontStyle + ".ttf");
            if (!File.Exists(fontFile)) fontFile = Path.Combine(Directory.GetCurrentDirectory(), "Fonts", block.Font.Family + ".ttf");

            Stream fontStream = new FileStream(fontFile, FileMode.Open, FileAccess.Read);
            PdfFont pdfFont = new PdfTrueTypeFont(fontStream, (float)block.Font.Size);

            double? width = block.AutoWidth ? (block.MaxWidth > 0 ? block.MaxWidth : null) : block.Layout.Width;
            double? height = block.AutoHeight ? (block.MaxHeight > 0 ? block.MaxHeight : null) : block.Layout.Height;

            var textProps = GetTextProperties(block.Text, width, height, pdfFont);

            double shiftAmt = 0;
            if (block.AutoHeight)
            {
                shiftAmt = textProps.Height - block.Layout.Height;
                m_globalShift += shiftAmt;
                block.Layout.Height = textProps.Height;
            }
            if (block.AutoWidth)
            {
                block.Layout.Width = textProps.Width;
            }

            double lineHeight = block.Font.Size * LINE_HEIGHT_RATIO;
            double yShiftAmt = 0;

            if (block.VerticallyCenter)
            {
                yShiftAmt += (block.Layout.Height - textProps.Height) / 2;
            }

            PdfStringFormat textSettings = block.Font.Alignment;

            var xPos = (float)block.Layout.X;
            if (textSettings.Alignment == PdfTextAlignment.Center) xPos = (float)(block.Layout.X + (block.Layout.Width / 2));
            else if (textSettings.Alignment == PdfTextAlignment.Right) xPos = (float)(block.Layout.X + block.Layout.Width);

            for (var lineIdx = 0; lineIdx < textProps.TextLines.Count; ++lineIdx)
            {
                var line = textProps.TextLines[lineIdx];
                float yPos = (float)((lineIdx * lineHeight) + yShiftAmt + block.Layout.Y);
                g.DrawString(line, pdfFont, brush, new PointF(xPos, yPos), textSettings);
            }

            if (block.AutoHeight)
            {
                g.TranslateTransform(0f, (float)shiftAmt);
            }
        }


        private void DrawMain(PdfGraphics g, PdfObject obj)
        {
            DrawBorder(g, obj);
        }

        private void DrawBorder(PdfGraphics g, PdfObject obj)
        {
            if (obj.Border == null) return;

            // Store the needed properties in local variables for easy access
            var topThickness = obj.Border.Top;
            var rightThickness = obj.Border.Right;
            var bottomThickness = obj.Border.Bottom;
            var leftThickness = obj.Border.Left;
            var x = obj.Layout.X;
            var y = obj.Layout.Y;
            var height = obj.Layout.Height;
            var width = obj.Layout.Width;

            // Create a brush with the right color
            PdfBrush fill = new PdfSolidBrush(new PdfColor(GetColor(obj.Border.Color)));

            /**
            ** The following four if statements work with the following logic:
            ** If the border has a size, figure out the size it should be
            ** The size includes the size of neighboring border. Such that,
            ** if there is a top and right border, the top's width will be
            ** extended so that it goes all the way to the right of the right
            ** border, and the right's height and y position will be adjusted
            ** so that the right border will go to the top of the top border.
            ** This is a little redundant, But it keeps all the logic the same.
            ** Might change it someday.
            ** Then a rectangle is drawn to represent the border
            */

            if (topThickness > 0)
            {
                var bY = (float)(y - topThickness);
                var bX = (float)(x - leftThickness);
                var bW = (float)(width + leftThickness + rightThickness);

                g.DrawRectangle(fill, bX, bY, bW, (float)topThickness);
            }

            if (rightThickness > 0)
            {
                var bX = (float)(x + width);
                var bY = (float)(y - topThickness);
                var bH = (float)(height + topThickness + bottomThickness);

                g.DrawRectangle(fill, bX, bY, (float)rightThickness, bH);
            }

            if (bottomThickness > 0)
            {
                var bY = (float)(y + height);
                var bX = (float)(x - leftThickness);
                var bW = (float)(width + leftThickness + rightThickness);

                g.DrawRectangle(fill, bX, bY, bW, (float)bottomThickness);
            }

            if (leftThickness > 0)
            {
                var bX = (float)(x - leftThickness);
                var bY = (float)(y - topThickness);
                var bH = (float)(height + topThickness + bottomThickness);

                g.DrawRectangle(fill, bX, bY, (float)leftThickness, bH);
            }
        }


        private Color GetColor(string hex)
        {
            hex = hex.Trim('#');

            var parts = Enumerable.Range(0, hex.Length)
                                  .Where(x => x % 2 == 0)
                                  .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                                  .ToArray();

            if (parts.Length != 3) throw new ArgumentException("Invalid HEX code");

            return Color.FromArgb(parts[0], parts[1], parts[2]);
        }

        private Layout GetRealLayout(PdfObject obj)
        {
            if (obj is TextBlock block)
            {
                var brush = new PdfSolidBrush(new PdfColor(GetColor(block.Font.Color)));
                string fontStyle = "";
                if (block.Font.Bold) fontStyle += "b";
                if (block.Font.Italic) fontStyle += "i";

                string fontFile = Path.Combine(Directory.GetCurrentDirectory(), "Fonts", block.Font.Family, fontStyle + ".ttf");
                if (!File.Exists(fontFile)) fontFile = Path.Combine(Directory.GetCurrentDirectory(), "Fonts", block.Font.Family + ".ttf");

                Stream fontStream = new FileStream(fontFile, FileMode.Open, FileAccess.Read);
                PdfFont pdfFont = new PdfTrueTypeFont(fontStream, (float)block.Font.Size);

                double? width = block.AutoWidth ? (block.MaxWidth > 0 ? block.MaxWidth : null) : block.Layout.Width;
                double? height = block.AutoHeight ? (block.MaxHeight > 0 ? block.MaxHeight : null) : block.Layout.Height;

                var textProps = GetTextProperties(block.Text, width, height, pdfFont);

                return new Layout
                {
                    Height = textProps.Height,
                    Width = textProps.Width,
                    X = obj.Layout.X,
                    Y = obj.Layout.Y
                };
            }

            return obj.Layout;
        }

        private TextProperties GetTextProperties(string text, double? width, double? height, PdfFont font)
        {
            float calcWidth = 0;  // Holds what we calculated the width to be for a given line
            float calcHeight = 0; // Holds what we calculated the height to be
            float maxWidth = 0;   // Holds the maximum line width we find

            var outputText = new List<string>(); // Holds the lines of text to be returned
            string tempLine = "";   // Holds the line being measured

            // Hold a dash as a [255] character
            string dash255 = "-" + Convert.ToChar(255);

            // Hold the regex to find either a [space] or [255] character, globally
            Regex space255reg = new Regex($"[ {Convert.ToChar(255)}]");

            // Replace - with [255], and split by [space] and [space] and [255] to preserve -'s.
            // This makes us be able to keep words together, and break on the dashes.
            var lines = text.Split('\n');

            // The current word we're on
            var wordStartIdx = 0;
            var lineIdx = 0;

            // Start with the first line as the words
            var words = space255reg.Split(Regex.Replace(lines[lineIdx], "-", dash255));

            // While we either don't have a height, or while the number of lines we have has not exceeded the height
            while (height == null || calcHeight < height)
            {

                calcWidth = 0; // Start width a width of zero
                tempLine = ""; // And no text in the line
                var wordEndIdx = wordStartIdx; // Adjust the end index so when we ++ it will be the word after the start

                // If no width restriction
                if (width == null)
                {
                    // Push all but the last line back
                    for (var i = 0; i < lines.Length - 1; ++i) outputText.Add(lines[i]);

                    // Use the last line as the line to be pushed later
                    tempLine = lines[lines.Length - 1];
                    wordEndIdx = words.Length;
                    lineIdx = lines.Length;
                }
                else
                {
                    // While we haven't reached the end of the words
                    while (wordEndIdx < words.Length)
                    {
                        // Get the [startWord] to [endWord], and join them with spaces, then
                        // remove spaces after hyphens, since the hyphen is what we originally
                        // split on
                        var wordConcat = string.Join(" ", words, wordStartIdx, ++wordEndIdx - wordStartIdx);
                        wordConcat = Regex.Replace(wordConcat, "- ", "-");

                        // Measure how long the string of words is
                        calcWidth = font.MeasureString(wordConcat).Width;

                        // If we didn't exceed the width, then, remember what we have so far
                        if (calcWidth <= width)
                        {
                            tempLine = wordConcat;
                        }
                        // Otherwise, back up the last word (so it will be the starting word next time) and stop processing
                        else
                        {
                            --wordEndIdx;
                            break;
                        }
                    }
                }

                // If we didn't get any text back, then there wasn't enough width for one word, so stop processing
                if (tempLine == "") break;

                // Determine if this line is longer than the last max
                maxWidth = Math.Max(maxWidth, font.MeasureString(tempLine).Width);

                // Add the line to the array of lines
                outputText.Add(tempLine);

                // Set the starting word for next time to be the word after the one we ended width
                // (No, it shouldn't have a +1, it's how the slice method works)
                wordStartIdx = wordEndIdx;

                // Calculate how high we are now
                calcHeight = (float)(font.Size + ((font.Size * LINE_HEIGHT_RATIO) * (outputText.Count - 1)));

                // If we've gotten too tall, remove the last element we added, and stop processing
                if (calcHeight > height)
                {
                    outputText.RemoveAt(outputText.Count - 1);

                    // Recalculate the height
                    calcHeight = (float)(font.Size + ((font.Size * LINE_HEIGHT_RATIO) * (outputText.Count - 1)));

                    break;
                }
                // Otherwise, If we've reached the end
                else if (wordEndIdx >= words.Length)
                {
                    // If there's another line, go to that, otherwise stop processing
                    if (lineIdx < lines.Length - 1)
                    {
                        ++lineIdx;
                        words = space255reg.Split(Regex.Replace(lines[lineIdx], "-", dash255));
                        wordStartIdx = wordEndIdx = 0;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Ensure not a decimal; Go up so not too small
            calcHeight = (float)Math.Ceiling(calcHeight);

            // Return what we got
            return new TextProperties
            {
                Width = maxWidth,
                Height = calcHeight,
                TextLines = outputText
            };
        }
    }

    class TextProperties
    {
        public float Height { get; set; }
        public float Width { get; set; }

        public List<string> TextLines { get; set; }
    }
}