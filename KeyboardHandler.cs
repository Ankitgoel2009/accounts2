using System;
using System.Windows.Forms;

namespace CompressedTextBoxControls
{
    /// <summary>
    /// Handles keyboard input and navigation for the CompressedTextBox
    /// </summary>
    public class KeyboardHandler
    {
        private readonly CaretManager _caretManager;
        private bool _readOnly = false;
        private int _maxLength = 50;

        // Events
        public event EventHandler<TextChangeEventArgs> TextChangeRequested;
        public event EventHandler<NavigationEventArgs> NavigationRequested;
        public event EventHandler EnterPressed;

        public KeyboardHandler(CaretManager caretManager)
        {
            _caretManager = caretManager ?? throw new ArgumentNullException(nameof(caretManager));
        }

        #region Properties

        public bool ReadOnly
        {
            get => _readOnly;
            set => _readOnly = value;
        }

        public int MaxLength
        {
            get => _maxLength;
            set => _maxLength = Math.Max(1, value);
        }

        #endregion

        #region Key Processing

        public bool ProcessKeyPress(KeyPressEventArgs e, string currentText)
        {
            if (_readOnly)
            {
                e.Handled = true;
                return true;
            }

            // Handle control characters
            if (char.IsControl(e.KeyChar))
            {
                return HandleControlCharacter(e.KeyChar, currentText);
            }

            // Handle printable characters
            return HandlePrintableCharacter(e.KeyChar, currentText);
        }

        public bool ProcessCommandKey(Keys keyData, string currentText)
        {
            switch (keyData)
            {
                case Keys.Left:
                    HandleLeftArrow();
                    return true;

                case Keys.Right:
                    HandleRightArrow(currentText);
                    return true;

                case Keys.Home:
                    HandleHome();
                    return true;

                case Keys.End:
                    HandleEnd(currentText);
                    return true;

                case Keys.Delete:
                    HandleDelete(currentText);
                    return true;

                case Keys.Enter:
                    HandleEnter();
                    return true;

                default:
                    return false;
            }
        }

        public bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.Home:
                case Keys.End:
                case Keys.Delete:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region Control Character Handling

        private bool HandleControlCharacter(char keyChar, string currentText)
        {
            switch (keyChar)
            {
                case (char)Keys.Back:
                    HandleBackspace(currentText);
                    return true;

                case (char)Keys.Tab:
                    // Let the control handle tab navigation
                    return false;

                case (char)Keys.Enter:
                    HandleEnter();
                    return true;

                default:
                    // Ignore other control characters
                    return true;
            }
        }

        private void HandleBackspace(string currentText)
        {
            if (_caretManager.Position > 0)
            {
                int removeIndex = _caretManager.Position - 1;
                string newText = currentText.Remove(removeIndex, 1);

                var args = new TextChangeEventArgs
                {
                    NewText = newText,
                    NewCaretPosition = removeIndex,
                    ChangeType = TextChangeType.Delete
                };

                TextChangeRequested?.Invoke(this, args);
            }
        }

        #endregion

        #region Printable Character Handling

        private bool HandlePrintableCharacter(char keyChar, string currentText)
        {
            // Check if we can add more characters
            if (currentText.Length >= _maxLength)
            {
                System.Diagnostics.Debug.WriteLine($"Maximum length reached: {_maxLength}");
                return true; // Handled, but not processed
            }

            // Insert character at caret position
            string newText = currentText.Insert(_caretManager.Position, keyChar.ToString());
            int newCaretPosition = _caretManager.Position + 1;

            var args = new TextChangeEventArgs
            {
                NewText = newText,
                NewCaretPosition = newCaretPosition,
                ChangeType = TextChangeType.Insert,
                InsertedCharacter = keyChar
            };

            TextChangeRequested?.Invoke(this, args);
            return true;
        }

        #endregion

        #region Navigation Handling

        private void HandleLeftArrow()
        {
            _caretManager.MoveLeft(int.MaxValue); // Pass max value since we don't have text length here

            var args = new NavigationEventArgs
            {
                Direction = NavigationDirection.Left,
                NewPosition = _caretManager.Position
            };

            NavigationRequested?.Invoke(this, args);
        }

        private void HandleRightArrow(string currentText)
        {
            _caretManager.MoveRight(currentText.Length);

            var args = new NavigationEventArgs
            {
                Direction = NavigationDirection.Right,
                NewPosition = _caretManager.Position
            };

            NavigationRequested?.Invoke(this, args);
        }

        private void HandleHome()
        {
            _caretManager.MoveToStart();

            var args = new NavigationEventArgs
            {
                Direction = NavigationDirection.Home,
                NewPosition = _caretManager.Position
            };

            NavigationRequested?.Invoke(this, args);
        }

        private void HandleEnd(string currentText)
        {
            _caretManager.MoveToEnd(currentText.Length);

            var args = new NavigationEventArgs
            {
                Direction = NavigationDirection.End,
                NewPosition = _caretManager.Position
            };

            NavigationRequested?.Invoke(this, args);
        }

        private void HandleDelete(string currentText)
        {
            if (_readOnly || _caretManager.Position >= currentText.Length)
                return;

            string newText = currentText.Remove(_caretManager.Position, 1);

            var args = new TextChangeEventArgs
            {
                NewText = newText,
                NewCaretPosition = _caretManager.Position, // Caret stays in same position
                ChangeType = TextChangeType.Delete
            };

            TextChangeRequested?.Invoke(this, args);
        }

        private void HandleEnter()
        {
            EnterPressed?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Updates the caret position after text changes
        /// </summary>
        public void UpdateCaretAfterTextChange(string newText, int? requestedPosition = null)
        {
            if (requestedPosition.HasValue)
            {
                _caretManager.Position = Math.Min(requestedPosition.Value, newText.Length);
            }
            else
            {
                _caretManager.ClampPosition(newText.Length);
            }
        }

        /// <summary>
        /// Validates if a character can be inserted
        /// </summary>
        public bool CanInsertCharacter(char character, string currentText)
        {
            if (_readOnly) return false;
            if (currentText.Length >= _maxLength) return false;
            if (char.IsControl(character) && character != '\b') return false;

            return true;
        }

        /// <summary>
        /// Gets the text that would result from a key press
        /// </summary>
        public string GetResultingText(char keyChar, string currentText)
        {
            if (char.IsControl(keyChar))
            {
                if (keyChar == (char)Keys.Back && _caretManager.Position > 0)
                {
                    return currentText.Remove(_caretManager.Position - 1, 1);
                }
                return currentText;
            }

            if (currentText.Length >= _maxLength)
                return currentText;

            return currentText.Insert(_caretManager.Position, keyChar.ToString());
        }

        #endregion
    }

    #region Event Args Classes

    public class TextChangeEventArgs : EventArgs
    {
        public string NewText { get; set; }
        public int NewCaretPosition { get; set; }
        public TextChangeType ChangeType { get; set; }
        public char? InsertedCharacter { get; set; }
    }

    public class NavigationEventArgs : EventArgs
    {
        public NavigationDirection Direction { get; set; }
        public int NewPosition { get; set; }
    }

    public enum TextChangeType
    {
        Insert,
        Delete,
        Replace
    }

    public enum NavigationDirection
    {
        Left,
        Right,
        Home,
        End,
        Up,
        Down
    }

    #endregion
}