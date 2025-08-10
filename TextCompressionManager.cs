using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace CompressedTextBoxControls
{
    /// <summary>
    /// Manages text compression and rendering for the CompressedTextBox
    /// </summary>
    public class TextCompressionManager
    {
        private float _characterSpacing = 0f;
        private bool _compressionEnabled = true;
        private float _minimumScaleFactor = 0.4f;
        private float _maximumCompressionPerGap = 3.0f;

        #region Properties

        public float CharacterSpacing => _characterSpacing;

        public bool CompressionEnabled
        {
            get => _compressionEnabled;
            set => _compressionEnabled = value;
        }

        public float MinimumScaleFactor
        {
            get => _minimumScaleFactor;
            set => _minimumScaleFactor = Math.Max(0.1f, Math.Min(1.0f, value));
        }

        public float MaximumCompressionPerGap
        {
            get => _maximumCompressionPerGap;
            set => _maximumCompressionPerGap = Math.Max(0.5f, value);
        }

        public bool IsTextCompressed => Math.Abs(_characterSpacing) > 0.01f;

        #endregion

        #region Compression Calculation

        public void CalculateCompression(string text, Font font, Rectangle textRect, Graphics graphics)
        {
            _characterSpacing = 0f;

            if (!_compressionEnabled ||
                string.IsNullOrEmpty(text) ||
                textRect.Width <= 0 ||
                font == null)
                return;

            try
            {
                using (StringFormat format = CreateStringFormat())
                {
                    graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                    float availableWidth = textRect.Width;
                    if (availableWidth <= 10) return;

                    // Calculate total natural width
                    float totalNaturalWidth = CalculateTotalTextWidth(text, font, graphics, format);

                    // Only compress if text is actually too wide
                    if (totalNaturalWidth > availableWidth && text.Length > 1)
                    {
                        CalculateCompressionSpacing(totalNaturalWidth, availableWidth, text.Length);
                    }

                    LogCompressionInfo(text, totalNaturalWidth, availableWidth);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CalculateCompression error: {ex.Message}");
                _characterSpacing = 0f;
            }
        }

        private void CalculateCompressionSpacing(float totalNaturalWidth, float availableWidth, int textLength)
        {
            float excessWidth = totalNaturalWidth - availableWidth;
            int gaps = textLength - 1; // Number of gaps between characters

            if (gaps > 0 && excessWidth > 0)
            {
                // Calculate required compression per gap to fit exactly
                _characterSpacing = -excessWidth / gaps;

                // Apply compression limits
                float minSpacing = -Math.Min(_maximumCompressionPerGap, availableWidth / textLength * 0.3f);
                if (_characterSpacing < minSpacing)
                    _characterSpacing = minSpacing;
            }
        }

        private float CalculateTotalTextWidth(string text, Font font, Graphics graphics, StringFormat format)
        {
            float totalWidth = 0f;

            for (int i = 0; i < text.Length; i++)
            {
                string character = text[i].ToString();
                SizeF charSize = graphics.MeasureString(character, font, Point.Empty, format);
                totalWidth += charSize.Width;
            }

            return totalWidth;
        }

        #endregion

        #region Text Rendering

        public void DrawCompressedText(Graphics graphics, string text, Font font, Rectangle textRect, Color foreColor)
        {
            if (string.IsNullOrEmpty(text)) return;

            using (SolidBrush textBrush = new SolidBrush(foreColor))
            using (StringFormat format = CreateStringFormat())
            {
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                var renderInfo = CalculateRenderingInfo(text, font, textRect, graphics, format);
                DrawCharacters(graphics, text, font, textRect, textBrush, format, renderInfo);
            }
        }

        private RenderingInfo CalculateRenderingInfo(string text, Font font, Rectangle textRect, Graphics graphics, StringFormat format)
        {
            float availableWidth = textRect.Width;
            float totalNaturalWidth = 0f;
            SizeF[] charSizes = new SizeF[text.Length];

            // Measure each character
            for (int i = 0; i < text.Length; i++)
            {
                string charStr = text[i].ToString();
                charSizes[i] = graphics.MeasureString(charStr, font, Point.Empty, format);
                totalNaturalWidth += charSizes[i].Width;
            }

            // Calculate scaling factor if compression is needed
            float scaleFactor = 1.0f;
            if (totalNaturalWidth > availableWidth)
            {
                scaleFactor = availableWidth / totalNaturalWidth;
                scaleFactor = Math.Max(_minimumScaleFactor, scaleFactor);
            }

            return new RenderingInfo
            {
                CharSizes = charSizes,
                ScaleFactor = scaleFactor,
                TotalNaturalWidth = totalNaturalWidth,
                Y = textRect.Y + (textRect.Height - font.Height) / 2f
            };
        }

        private void DrawCharacters(Graphics graphics, string text, Font font, Rectangle textRect,
            SolidBrush textBrush, StringFormat format, RenderingInfo renderInfo)
        {
            float x = textRect.X;

            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];
                string charStr = character.ToString();

                // Calculate scaled character width
                float scaledWidth = renderInfo.CharSizes[i].Width * renderInfo.ScaleFactor;

                // Draw character if it starts within bounds
                if (x <= textRect.Right)
                {
                    graphics.DrawString(charStr, font, textBrush, x, renderInfo.Y, format);
                }

                // Move to next position
                x += scaledWidth;
            }

            LogRenderingInfo(renderInfo, x - textRect.X);
        }

        #endregion

        #region Helper Methods and Classes

        private StringFormat CreateStringFormat()
        {
            var format = (StringFormat)StringFormat.GenericTypographic.Clone();
            format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces |
                               StringFormatFlags.NoWrap |
                               StringFormatFlags.FitBlackBox;
            return format;
        }

        private void LogCompressionInfo(string text, float totalNaturalWidth, float availableWidth)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Compression - Text: '{text}' ({text.Length} chars), " +
                $"NaturalWidth: {totalNaturalWidth:F1}, " +
                $"AvailableWidth: {availableWidth:F1}, " +
                $"Spacing: {_characterSpacing:F2}");
        }

        private void LogRenderingInfo(RenderingInfo renderInfo, float finalWidth)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Rendering - NaturalWidth: {renderInfo.TotalNaturalWidth:F1}, " +
                $"ScaleFactor: {renderInfo.ScaleFactor:F2}, " +
                $"FinalWidth: {finalWidth:F1}");
        }

        #endregion

        #region Measurement Methods

        /// <summary>
        /// Gets the total width the text would occupy when rendered
        /// </summary>
        public float GetRenderedTextWidth(string text, Font font, Graphics graphics)
        {
            if (string.IsNullOrEmpty(text)) return 0f;

            try
            {
                using (StringFormat format = CreateStringFormat())
                {
                    float totalWidth = 0f;

                    for (int i = 0; i < text.Length; i++)
                    {
                        string charStr = text[i].ToString();
                        SizeF charSize = graphics.MeasureString(charStr, font, Point.Empty, format);
                        totalWidth += charSize.Width;
                    }

                    return totalWidth;
                }
            }
            catch
            {
                return 0f;
            }
        }

        /// <summary>
        /// Gets the scale factor that would be applied to fit text in the given width
        /// </summary>
        public float GetScaleFactorForWidth(string text, Font font, float availableWidth, Graphics graphics)
        {
            float naturalWidth = GetRenderedTextWidth(text, font, graphics);

            if (naturalWidth <= availableWidth || naturalWidth <= 0)
                return 1.0f;

            float scaleFactor = availableWidth / naturalWidth;
            return Math.Max(_minimumScaleFactor, scaleFactor);
        }

        #endregion

        private class RenderingInfo
        {
            public SizeF[] CharSizes { get; set; }
            public float ScaleFactor { get; set; }
            public float TotalNaturalWidth { get; set; }
            public float Y { get; set; }
        }
    }
}