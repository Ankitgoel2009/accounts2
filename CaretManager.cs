using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace CompressedTextBoxControls
{
    /// <summary>
    /// Manages caret positioning, visibility, and rendering for the CompressedTextBox
    /// </summary>
    public class CaretManager : IDisposable
    {
        private Timer _caretTimer;
        private bool _caretVisible = true;
        private int _caretIndex = 0;
        private bool _isActive = false;

        // Events
        public event EventHandler CaretPositionChanged;
        public event EventHandler CaretVisibilityChanged;

        public CaretManager()
        {
            InitializeTimer();
        }

        #region Properties

        public int Position
        {
            get => _caretIndex;
            set
            {
                int newPosition = Math.Max(0, value);
                if (_caretIndex != newPosition)
                {
                    _caretIndex = newPosition;
                    CaretPositionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool IsVisible => _caretVisible && _isActive;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    if (_isActive)
                        StartBlinking();
                    else
                        StopBlinking();
                }
            }
        }

        #endregion

        #region Timer Management

        private void InitializeTimer()
        {
            _caretTimer = new Timer();
            _caretTimer.Interval = 500; // Standard Windows caret blink rate
            _caretTimer.Tick += OnCaretTimerTick;
        }

        private void OnCaretTimerTick(object sender, EventArgs e)
        {
            _caretVisible = !_caretVisible;
            CaretVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        public void StartBlinking()
        {
            _caretVisible = true;
            _caretTimer.Start();
            CaretVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        public void StopBlinking()
        {
            _caretTimer.Stop();
            _caretVisible = false;
            CaretVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Position Calculations

        public int GetPositionFromPoint(Point point, string text, Font font, Rectangle textRect, Graphics graphics)
        {
            if (string.IsNullOrEmpty(text) || point.X <= textRect.Left)
                return 0;

            try
            {
                using (StringFormat format = CreateStringFormat())
                {
                    graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                    // Calculate character sizes and scaling
                    var (charSizes, scaleFactor) = CalculateCharacterMetrics(text, font, textRect.Width, graphics, format);

                    float x = textRect.X;
                    for (int i = 0; i < text.Length; i++)
                    {
                        float scaledWidth = charSizes[i].Width * scaleFactor;

                        if (point.X <= x + scaledWidth / 2)
                            return i;

                        x += scaledWidth;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetPositionFromPoint error: {ex.Message}");
            }

            return text.Length;
        }

        public float GetCaretXPosition(string text, Font font, Rectangle textRect, Graphics graphics)
        {
            float caretX = textRect.X;

            if (_caretIndex > 0 && !string.IsNullOrEmpty(text))
            {
                using (StringFormat format = CreateStringFormat())
                {
                    var (charSizes, scaleFactor) = CalculateCharacterMetrics(text, font, textRect.Width, graphics, format);

                    // Calculate position up to caret index
                    for (int i = 0; i < Math.Min(_caretIndex, text.Length); i++)
                    {
                        float scaledWidth = charSizes[i].Width * scaleFactor;
                        caretX += scaledWidth;
                    }
                }
            }

            return caretX;
        }

        #endregion

        #region Rendering

        public void DrawCaret(Graphics graphics, string text, Font font, Rectangle textRect, Color foreColor, float characterSpacing)
        {
            if (!IsVisible) return;

            float caretX = GetCaretXPosition(text, font, textRect, graphics);

            // Check if caret is clipped
            float maxCaretX = textRect.Right - 10; // More space for thicker caret
            bool caretClipped = caretX > maxCaretX;
            if (caretClipped)
                caretX = maxCaretX;

            // Calculate caret dimensions - DOS style
            float caretY = textRect.Y + 1;
            float caretHeight = Math.Max(textRect.Height - 2, font.Height);
            float caretWidth = CalculateCaretWidth(font, characterSpacing, caretClipped);

            // Choose caret color - white all the time
            Color caretColor = Color.White;

            // Draw the thick DOS-style caret with slight rounding for modern touch
            using (SolidBrush caretBrush = new SolidBrush(caretColor))
            {
                // Main caret body
                graphics.FillRectangle(caretBrush, caretX, caretY, caretWidth, caretHeight);

                // Optional: Add a subtle highlight for 3D DOS effect
                if (caretWidth >= 4)
                {
                    using (SolidBrush highlightBrush = new SolidBrush(Color.FromArgb(100, Color.White)))
                    {
                        graphics.FillRectangle(highlightBrush, caretX, caretY, 1, caretHeight);
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"DOS Caret at index {_caretIndex}, X: {caretX:F1}, Width: {caretWidth:F1}, Clipped: {caretClipped}");
        }

        private float CalculateCaretWidth(Font font, float characterSpacing, bool isClipped)
        {
            // DOS-style thick caret - significantly increased width
            float baseWidth = Math.Max(6, font.Size * 1.2f); // 120% of font size for extra thick DOS look

            // Make even thicker when text is compressed or clipped for better visibility
            if (characterSpacing < -1.0f || isClipped)
            {
                baseWidth = Math.Max(8, font.Size * 1.5f); // 150% font size width when compressed
            }

            // Ensure minimum thickness for prominent DOS appearance
            return Math.Max(6, baseWidth);
        }

        #endregion

        #region Navigation

        public void MoveLeft(int textLength)
        {
            if (_caretIndex > 0)
            {
                Position = _caretIndex - 1;
            }
        }

        public void MoveRight(int textLength)
        {
            if (_caretIndex < textLength)
            {
                Position = _caretIndex + 1;
            }
        }

        public void MoveToStart()
        {
            Position = 0;
        }

        public void MoveToEnd(int textLength)
        {
            Position = textLength;
        }

        public void ClampPosition(int textLength)
        {
            Position = Math.Min(_caretIndex, textLength);
        }

        #endregion

        #region Helper Methods

        private StringFormat CreateStringFormat()
        {
            var format = (StringFormat)StringFormat.GenericTypographic.Clone();
            format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces |
                               StringFormatFlags.NoWrap |
                               StringFormatFlags.FitBlackBox;
            return format;
        }

        private (SizeF[] charSizes, float scaleFactor) CalculateCharacterMetrics(string text, Font font, float availableWidth, Graphics graphics, StringFormat format)
        {
            SizeF[] charSizes = new SizeF[text.Length];
            float totalNaturalWidth = 0f;

            // Measure each character
            for (int i = 0; i < text.Length; i++)
            {
                string charStr = text[i].ToString();
                charSizes[i] = graphics.MeasureString(charStr, font, Point.Empty, format);
                totalNaturalWidth += charSizes[i].Width;
            }

            // Calculate scaling factor
            float scaleFactor = 1.0f;
            if (totalNaturalWidth > availableWidth)
            {
                scaleFactor = availableWidth / totalNaturalWidth;
                scaleFactor = Math.Max(0.4f, scaleFactor); // Minimum 40% scaling
            }

            return (charSizes, scaleFactor);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _caretTimer?.Dispose();
                _caretTimer = null;
            }
        }

        #endregion
    }
}