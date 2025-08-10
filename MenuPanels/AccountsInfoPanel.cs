using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace accounts2.MenuPanels
{
    public partial class AccountsInfoPanel : UserControl, IDataExchange
    {
        // Add this event
        public event EventHandler EscapePressed;

        public AccountsInfoPanel()
        {
            InitializeComponent();
            // Subscribe to key events
            this.KeyDown += AccountsInfoPanel_KeyDown;
            this.PreviewKeyDown += AccountsInfoPanel_PreviewKeyDown;

            // Subscribe to key events for all child controls
            SubscribeToChildKeyEvents(this);
        }

        private void AccountsInfoPanel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.IsInputKey = true;
            }
        }

        private void AccountsInfoPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                EscapePressed?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        // Subscribe to key events for all child controls
        private void SubscribeToChildKeyEvents(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                control.KeyDown += (s, e) => {
                    if (e.KeyCode == Keys.Escape)
                    {
                        EscapePressed?.Invoke(this, EventArgs.Empty);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                };

                // Recursively subscribe to child controls
                if (control.HasChildren)
                {
                    SubscribeToChildKeyEvents(control);
                }
            }
        }

        public void ReceiveData(object data) { }
        public object GetData() { return null; }
    }
}