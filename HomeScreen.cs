
using accounts2.MenuPanels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace accounts2
{
    public partial class HomeScreen : UserControl, IDataExchange
    {
        // 🔴 MOVED TO DESIGNER: private GatewayofTallypanel gatewayoftallypanel;

        public HomeScreen()
        {
            InitializeComponent();
            LoadGatewayPanel();
        }

        private void LoadGatewayPanel()
        {
            // Create GatewayofTallypanel instance
            this.gatewayoftallypanel = new GatewayofTallypanel();
            // 🔴 CHANGED: Don't dock fill, center instead
            this.gatewayoftallypanel.Dock = DockStyle.None; // 🔴 CHANGED
            this.gatewayoftallypanel.Anchor = AnchorStyles.None; // 🔴 ADD: Center it
            this.gatewayoftallypanel.Name = "gatewayoftallypanel";
            this.gatewayoftallypanel.TabIndex = 0;

            // 🔴 CHANGED: Add directly to rightContentPanel, not to table cell
            this.rightContentPanel.Controls.Add(this.gatewayoftallypanel);

            // 🔴 ADD: Center the panel
            CenterPanel(this.gatewayoftallypanel);

            // 🔴 ADD: Handle resize to keep centered
            this.rightContentPanel.Resize += (s, e) => CenterPanel(this.gatewayoftallypanel);
        }

        // 🔴 ADD: Method to center panel
        private void CenterPanel(Control panel)
        {
            if (panel != null)
            {
                panel.Location = new Point(
                    Math.Max(0, (rightContentPanel.Width - panel.Width) / 2),
                    Math.Max(0, (rightContentPanel.Height - panel.Height) / 2)
                );
            }
        }

        // 🔴 ADD: Method to load any panel dynamically
        public void LoadPanel(UserControl panel)
        {
            // Remove existing panel
            if (this.gatewayoftallypanel != null)
            {
                this.rightContentPanel.Controls.Remove(this.gatewayoftallypanel);
                this.gatewayoftallypanel.Dispose();
            }

            // Set new panel
            this.gatewayoftallypanel = panel as GatewayofTallypanel; // Adjust type as needed

            if (panel != null)
            {
                panel.Dock = DockStyle.None;
                panel.Anchor = AnchorStyles.None;
                panel.Name = "dynamicPanel";

                this.rightContentPanel.Controls.Add(panel);
                CenterPanel(panel);

                this.rightContentPanel.Resize -= (s, e) => CenterPanel(panel);
                this.rightContentPanel.Resize += (s, e) => CenterPanel(panel);
            }
        }

        // Property to access the gateway panel
        public GatewayofTallypanel GatewayPanel
        {
            get { return gatewayoftallypanel; }
        }

        // IDataExchange Implementation (required by your architecture)
        public void ReceiveData(object data)
        {
            // Handle incoming data from other forms
            // Pass data to gateway panel if needed
            if (gatewayoftallypanel != null)
            {
                gatewayoftallypanel.ReceiveData(data);
            }
        }

        public object GetData()
        {
            // Return data from gateway panel
            return gatewayoftallypanel?.GetData();
        }
    }
}
