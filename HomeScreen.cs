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
        private Stack<UserControl> panelHistory;
        private UserControl currentPanel;

        public HomeScreen()
        {
            InitializeComponent();
            panelHistory = new Stack<UserControl>();
            LoadGatewayPanel();
        }

        private void LoadGatewayPanel()
        {
            // Clear existing panel
            if (currentPanel != null)
            {
                rightContentPanel.Controls.Remove(currentPanel);
                currentPanel.Dispose();
            }

            // Create GatewayofTallypanel instance
            this.gatewayoftallypanel = new GatewayofTallypanel();
            this.gatewayoftallypanel.Dock = DockStyle.None;
            this.gatewayoftallypanel.Anchor = AnchorStyles.None;
            this.gatewayoftallypanel.Name = "gatewayoftallypanel";
            this.gatewayoftallypanel.TabIndex = 0;

            // Subscribe to the AccountsInfo button click event
            var accountsButton = this.gatewayoftallypanel.Controls.Find("btnAccountsInfo", true).FirstOrDefault();
            if (accountsButton != null)
            {
                accountsButton.Click -= AccountsInfoButton_Click;
                accountsButton.Click += AccountsInfoButton_Click;
            }

            // Add to container
            this.rightContentPanel.Controls.Add(this.gatewayoftallypanel);
            currentPanel = this.gatewayoftallypanel;

            // Center the panel
            CenterPanel(this.gatewayoftallypanel);

            // Handle resize to keep centered
            this.rightContentPanel.Resize -= RightContentPanel_Resize;
            this.rightContentPanel.Resize += RightContentPanel_Resize;

            // Clear history when loading gateway panel
            panelHistory.Clear();
        }

        private void AccountsInfoButton_Click(object sender, EventArgs e)
        {
            LoadAccountsInfoPanel();
        }

        private void RightContentPanel_Resize(object sender, EventArgs e)
        {
            if (currentPanel != null && !currentPanel.IsDisposed)
            {
                CenterPanel(currentPanel);
            }
        }

        // Method to load AccountsInfo panel
        private void LoadAccountsInfoPanel()
        {
            // Save current panel to history
            if (currentPanel != null)
            {
                panelHistory.Push(currentPanel);
                rightContentPanel.Controls.Remove(currentPanel);
            }

            // Create and configure AccountsInfoPanel
            var accountsInfoPanel = new AccountsInfoPanel();
            accountsInfoPanel.Dock = DockStyle.None;
            accountsInfoPanel.Anchor = AnchorStyles.None;
            accountsInfoPanel.Name = "accountsInfoPanel";

            // Subscribe to escape key event
            accountsInfoPanel.EscapePressed += (s, e) => GoBack();

            // Add to container
            rightContentPanel.Controls.Add(accountsInfoPanel);
            currentPanel = accountsInfoPanel;

            // Center the panel
            CenterPanel(accountsInfoPanel);

            // Handle resize to keep centered
            rightContentPanel.Resize -= (s, e) => CenterPanel(accountsInfoPanel);
            rightContentPanel.Resize += (s, e) => CenterPanel(accountsInfoPanel);

            // Set focus to enable key events
            accountsInfoPanel.Focus();
        }

        // Method to center panel
        private void CenterPanel(Control panel)
        {
            if (panel != null && !panel.IsDisposed)
            {
                panel.Location = new Point(
                    Math.Max(0, (rightContentPanel.Width - panel.Width) / 2),
                    Math.Max(0, (rightContentPanel.Height - panel.Height) / 2)
                );
            }
        }

        // Method to go back to previous panel
        private void GoBack()
        {
            if (panelHistory.Count > 0)
            {
                // Remove current panel
                if (currentPanel != null)
                {
                    rightContentPanel.Controls.Remove(currentPanel);
                    currentPanel.Dispose();
                }

                // Restore previous panel
                var previousPanel = panelHistory.Pop();

                if (previousPanel is GatewayofTallypanel)
                {
                    this.gatewayoftallypanel = (GatewayofTallypanel)previousPanel;
                    // Re-attach the event handler
                    var accountsButton = this.gatewayoftallypanel.Controls.Find("btnAccountsInfo", true).FirstOrDefault();
                    if (accountsButton != null)
                    {
                        accountsButton.Click -= AccountsInfoButton_Click;
                        accountsButton.Click += AccountsInfoButton_Click;
                    }
                }

                previousPanel.Dock = DockStyle.None;
                previousPanel.Anchor = AnchorStyles.None;
                rightContentPanel.Controls.Add(previousPanel);
                currentPanel = previousPanel;
                CenterPanel(previousPanel);

                // Handle resize to keep centered
                rightContentPanel.Resize -= (s, e) => CenterPanel(previousPanel);
                rightContentPanel.Resize += (s, e) => CenterPanel(previousPanel);
            }
        }

        // Method to load any panel dynamically
        public void LoadPanel(UserControl panel)
        {
            // Save current panel to history
            if (currentPanel != null)
            {
                panelHistory.Push(currentPanel);
                rightContentPanel.Controls.Remove(currentPanel);
            }

            if (panel != null)
            {
                panel.Dock = DockStyle.None;
                panel.Anchor = AnchorStyles.None;
                panel.Name = "dynamicPanel";

                this.rightContentPanel.Controls.Add(panel);
                currentPanel = panel;
                CenterPanel(panel);

                this.rightContentPanel.Resize -= (s, e) => CenterPanel(panel);
                this.rightContentPanel.Resize += (s, e) => CenterPanel(panel);

                // If the panel supports escape handling, subscribe to it
                if (panel is AccountsInfoPanel accountsPanel)
                {
                    accountsPanel.EscapePressed += (s, e) => GoBack();
                }

                panel.Focus();
            }
        }

        // Property to access the gateway panel
        public GatewayofTallypanel GatewayPanel
        {
            get { return gatewayoftallypanel; }
        }

        // IDataExchange Implementation
        public void ReceiveData(object data)
        {
            if (gatewayoftallypanel != null)
            {
                gatewayoftallypanel.ReceiveData(data);
            }
        }

        public object GetData()
        {
            return gatewayoftallypanel?.GetData();
        }
    }
}