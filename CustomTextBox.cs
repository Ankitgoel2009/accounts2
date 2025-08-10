using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using CompressedTextBoxControls;

namespace CompressedTextBoxControls
{
    [ToolboxItem(true)]
    [Description("TextBox with automatic character spacing compression")]
    [DisplayName("Compressed TextBox")]
    public class CompressedTextBox : Control
    {
        #region Private Fields

        private string _text = string.Empty;
        private int _maxLength = 50;
        private bool _readOnly = false;
        private Color _borderColor = SystemColors.ControlDark;
        private BorderStyle _borderStyle = BorderStyle.Fixed3D;
        private Color _originalBackColor;
        private bool _isInitialized = false;
        private bool _focused = false;

        // Managers
        private CaretManager _caretManager;
        private TextCompressionManager _compressionManager;
        private KeyboardHandler _keyboardHandler;

        #endregion

        #region Events

        public new event EventHandler TextChanged;

        #endregion

        #region Properties

        [Category("Appearance")]
        [Description("The text contained in the control")]
        [DefaultValue("")]
        public override string Text
        {
            get { return _text; }
            set
            {
                string newText = value ?? string.Empty;
                if (newText.Length > _maxLength)
                    newText = newText.Substring(0, _maxLength);

                UpdateText(newText);
            }
        }

        [Category("Behavior")]
        [Description("Maximum number of characters that can be entered")]
        [DefaultValue(50)]
        public int MaxLength
        {
            get { return _maxLength; }
            set
            {
                _maxLength = Math.Max(1, value);
                if (_keyboardHandler != null)
                    _keyboardHandler.MaxLength = _maxLength;

                if (_text.Length > _maxLength)
                {
                    Text = _text.Substring(0, _maxLength);
                }
            }
        }

        [Category("Behavior")]
        [Description("Controls whether the text can be changed")]
        [DefaultValue(false)]
        public bool ReadOnly
        {
            get { return _readOnly; }
            set
            {
                _readOnly = value;
                if (_keyboardHandler != null)
                    _keyboardHandler.ReadOnly = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("The border style of the control")]
        [DefaultValue(BorderStyle.Fixed3D)]
        public BorderStyle BorderStyle
        {
            get { return _borderStyle; }
            set
            {
                _borderStyle = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("The color of the border")]
        [DefaultValue(typeof(Color), "ControlDark")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Automatically inherit parent form's background color")]
        [DefaultValue(true)]
        public bool InheritFormBackColor { get; set; } = true;

        [Category("Appearance")]
        [Description("Enable or disable text compression")]
        [DefaultValue(true)]
        public bool CompressionEnabled
        {
            get { return _compressionManager?.CompressionEnabled ?? true; }
            set
            {
                if (_compressionManager != null)
                {
                    _compressionManager.CompressionEnabled = value;
                    RecalculateLayout();
                }
            }
        }

        [Category("Appearance")]
        [Description("Minimum scale factor for compressed text (0.1 to 1.0)")]
        [DefaultValue(0.4f)]
        public float MinimumScaleFactor
        {
            get { return _compressionManager?.MinimumScaleFactor ?? 0.4f; }
            set
            {
                if (_compressionManager != null)
                {
                    _compressionManager.MinimumScaleFactor = value;
                    RecalculateLayout();
                }
            }
        }

        [Browsable(false)]
        public int SelectionStart
        {
            get { return _caretManager?.Position ?? 0; }
            set
            {
                if (_caretManager != null)
                {
                    _caretManager.Position = Math.Max(0, Math.Min(value, _text.Length));
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        public float CharacterSpacing => _compressionManager?.CharacterSpacing ?? 0f;

        [Browsable(false)]
        public bool IsTextCompressed => _compressionManager?.IsTextCompressed ?? false;

        #endregion

        #region Constructor

        public CompressedTextBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.Selectable |
                     ControlStyles.StandardClick, true);

            InitializeManagers();
            InitializeControl();
        }

        #endregion

        #region Initialization

        private void InitializeManagers()
        {
            // Initialize caret manager
            _caretManager = new CaretManager();
            _caretManager.CaretPositionChanged += (s, e) => Invalidate();
            _caretManager.CaretVisibilityChanged += (s, e) => Invalidate();

            // Initialize compression manager
            _compressionManager = new TextCompressionManager();

            // Initialize keyboard handler
            _keyboardHandler = new KeyboardHandler(_caretManager);
            _keyboardHandler.MaxLength = _maxLength;
            _keyboardHandler.ReadOnly = _readOnly;
            _keyboardHandler.TextChangeRequested += OnTextChangeRequested;
            _keyboardHandler.NavigationRequested += OnNavigationRequested;
            _keyboardHandler.EnterPressed += OnEnterPressed;
        }

        private void InitializeControl()
        {
            // Set default properties
            _originalBackColor = SystemColors.Control;
            this.BackColor = _originalBackColor;
            this.ForeColor = Color.Black;
            this.Size = new Size(200, 23);
            this.TabStop = true;
            this.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold);
        }

        #endregion

        #region Event Handlers

        private void OnTextChangeRequested(object sender, TextChangeEventArgs e)
        {
            UpdateText(e.NewText, e.NewCaretPosition);
        }

        private void OnNavigationRequested(object sender, NavigationEventArgs e)
        {
            // Navigation is already handled by the caret manager
            // This event can be used for additional logic if needed
            Invalidate();
        }

        private void OnEnterPressed(object sender, EventArgs e)
        {
            // Move to next control in tab order
            if (this.Parent != null)
            {
                this.Parent.SelectNextControl(this, true, true, true, true);
            }
        }

        #endregion

        #region Text Management

        private void UpdateText(string newText, int? newCaretPosition = null)
        {
            if (_text == newText) return;

            _text = newText;

            if (newCaretPosition.HasValue && _caretManager != null)
            {
                _caretManager.Position = newCaretPosition.Value;
            }
            else if (_caretManager != null)
            {
                _caretManager.ClampPosition(_text.Length);
            }

            RecalculateLayout();
            TextChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RecalculateLayout()
        {
            if (this.IsHandleCreated && this.Font != null && _compressionManager != null)
            {
                try
                {
                    using (Graphics g = CreateGraphics())
                    {
                        Rectangle textRect = GetTextRectangle();
                        _compressionManager.CalculateCompression(_text, this.Font, textRect, g);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"RecalculateLayout error: {ex.Message}");
                }
            }
            Invalidate();
        }

        #endregion

        #region Layout and Appearance

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (InheritFormBackColor && !_isInitialized)
            {
                SetFormBackColor();
                _isInitialized = true;
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (this.Parent != null)
            {
                SetFormBackColor();
            }
        }

        private void SetFormBackColor()
        {
            Form parentForm = this.FindForm();
            if (parentForm != null)
            {
                _originalBackColor = parentForm.BackColor;

                if (!_focused)
                {
                    this.BackColor = _originalBackColor;
                    this.ForeColor = Color.Black;
                }
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            RecalculateLayout();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            RecalculateLayout();
        }

        #endregion

        #region Focus Management

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            _focused = true;

            // Change to focus colors
            this.BackColor = Color.Black;
            this.ForeColor = Color.White;

            if (!_readOnly && _caretManager != null)
            {
                _caretManager.IsActive = true;
            }

            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            _focused = false;

            // Revert to original colors
            this.BackColor = _originalBackColor;
            this.ForeColor = Color.Black;

            if (_caretManager != null)
            {
                _caretManager.IsActive = false;
            }
            Invalidate();
        }

        #endregion

        #region Input Handling

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (_keyboardHandler != null)
            {
                bool handled = _keyboardHandler.ProcessKeyPress(e, _text);
                e.Handled = handled;

                if (!handled)
                {
                    base.OnKeyPress(e);
                }
            }
            else
            {
                base.OnKeyPress(e);
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return (_keyboardHandler?.IsInputKey(keyData) ?? false) || base.IsInputKey(keyData);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_keyboardHandler != null)
            {
                bool handled = _keyboardHandler.ProcessCommandKey(keyData, _text);

                if (handled)
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (!_focused)
            {
                Focus();
                return;
            }

            // Calculate cursor position from mouse click
            if (_caretManager != null)
            {
                try
                {
                    using (Graphics g = CreateGraphics())
                    {
                        Rectangle textRect = GetTextRectangle();
                        int newPosition = _caretManager.GetPositionFromPoint(e.Location, _text, this.Font, textRect, g);
                        _caretManager.Position = newPosition;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"OnMouseClick error: {ex.Message}");
                }

                Invalidate();
            }
        }

        #endregion

        #region Painting

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            // Draw background
            using (SolidBrush bgBrush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(bgBrush, this.ClientRectangle);
            }

            // Draw border
            DrawBorder(g);

            Rectangle textRect = GetTextRectangle();

            // Set clipping region
            Region originalClip = g.Clip;
            g.SetClip(textRect);

            // Draw text
            if (!string.IsNullOrEmpty(_text) && _compressionManager != null)
            {
                _compressionManager.DrawCompressedText(g, _text, this.Font, textRect, this.ForeColor);
            }

            // Restore clipping
            g.Clip = originalClip;

            // Draw caret (outside clipping)
            if (_focused && !_readOnly && _caretManager != null && _compressionManager != null)
            {
                _caretManager.DrawCaret(g, _text, this.Font, textRect, this.ForeColor, _compressionManager.CharacterSpacing);
            }
        }

        private void DrawBorder(Graphics g)
        {
            Rectangle rect = this.ClientRectangle;

            switch (_borderStyle)
            {
                case BorderStyle.FixedSingle:
                    using (Pen pen = new Pen(_borderColor))
                    {
                        g.DrawRectangle(pen, 0, 0, rect.Width - 1, rect.Height - 1);
                    }
                    break;

                case BorderStyle.Fixed3D:
                    ControlPaint.DrawBorder3D(g, rect, Border3DStyle.Sunken);
                    break;
            }
        }

        private Rectangle GetTextRectangle()
        {
            Rectangle rect = this.ClientRectangle;

            // Adjust for border
            switch (_borderStyle)
            {
                case BorderStyle.Fixed3D:
                    rect.Inflate(-2, -2);
                    break;
                case BorderStyle.FixedSingle:
                    rect.Inflate(-1, -1);
                    break;
            }

            // Add padding
            rect.Inflate(-3, -2);

            return rect;
        }

        // Prevent flicker
        protected override void OnPaintBackground(PaintEventArgs pevent) { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Clears all text from the control
        /// </summary>
        public void Clear()
        {
            UpdateText(string.Empty, 0);
        }

        /// <summary>
        /// Selects all text (moves cursor to end)
        /// </summary>
        public void SelectAll()
        {
            if (_caretManager != null)
            {
                _caretManager.Position = _text.Length;
                Invalidate();
            }
        }

        /// <summary>
        /// Appends text to the end of the current text
        /// </summary>
        public void AppendText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            int available = _maxLength - _text.Length;
            if (available <= 0) return;

            string toAppend = text.Length <= available ? text : text.Substring(0, available);
            Text += toAppend;
        }

        /// <summary>
        /// Inserts text at the current caret position
        /// </summary>
        public void InsertText(string text)
        {
            if (string.IsNullOrEmpty(text) || _readOnly) return;

            int available = _maxLength - _text.Length;
            if (available <= 0) return;

            string toInsert = text.Length <= available ? text : text.Substring(0, available);
            int caretPos = _caretManager?.Position ?? 0;
            string newText = _text.Insert(caretPos, toInsert);
            int newCaretPosition = caretPos + toInsert.Length;

            UpdateText(newText, newCaretPosition);
        }

        /// <summary>
        /// Gets the current compression ratio (1.0 = no compression, 0.4 = maximum compression)
        /// </summary>
        public float GetCompressionRatio()
        {
            if (!this.IsHandleCreated || string.IsNullOrEmpty(_text) || _compressionManager == null)
                return 1.0f;

            try
            {
                using (Graphics g = CreateGraphics())
                {
                    Rectangle textRect = GetTextRectangle();
                    return _compressionManager.GetScaleFactorForWidth(_text, this.Font, textRect.Width, g);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCompressionRatio error: {ex.Message}");
                return 1.0f;
            }
        }

        #endregion

        #region Cleanup

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Unsubscribe from events to prevent memory leaks
                if (_caretManager != null)
                {
                    _caretManager.CaretPositionChanged -= (s, e) => Invalidate();
                    _caretManager.CaretVisibilityChanged -= (s, e) => Invalidate();
                    _caretManager.Dispose();
                    _caretManager = null;
                }

                if (_keyboardHandler != null)
                {
                    _keyboardHandler.TextChangeRequested -= OnTextChangeRequested;
                    _keyboardHandler.NavigationRequested -= OnNavigationRequested;
                    _keyboardHandler.EnterPressed -= OnEnterPressed;
                    _keyboardHandler = null;
                }

                _compressionManager = null;
            }
            base.Dispose(disposing);
        }

        #endregion

    } // End of CompressedTextBox class
} // End of namespace