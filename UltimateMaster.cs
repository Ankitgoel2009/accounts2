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
    public partial class UltimateMaster : Form
    {
        private Timer statusTimer;
        private Panel contentPanel; // Your main panel (docked as Fill)
        private Dictionary<Type, UserControl> loadedForms; // For memory efficiency

        public UltimateMaster()
        {
            InitializeComponent();
            loadedForms = new Dictionary<Type, UserControl>();

            // Get reference to your content panel
            // Assuming you named it "contentPanel" in designer
            contentPanel = this.Controls["ContentPanel"] as Panel;
        }

        private void Homepage_Load(object sender, EventArgs e)
        {
            InitializeStatusBar();
            ShowGatewayOfTally();
        }

        // Method to load GatewayOfTally
        public void ShowGatewayOfTally()
        {
            // Check if already loaded (memory efficiency)
            if (!loadedForms.ContainsKey(typeof(HomeScreen)))
            {
                loadedForms[typeof(HomeScreen)] = new HomeScreen();
            }

            var gatewayForm = loadedForms[typeof(HomeScreen)];

            // Clear current content
            contentPanel.Controls.Clear();

            // Add GatewayOfTally to panel
            gatewayForm.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(gatewayForm);

            // Set focus for key events (like Escape)
            gatewayForm.Focus();
        }

        // Generic method for loading any UserControl (following your architecture)
        public void ShowUserControl<T>() where T : UserControl, IDataExchange, new()
        {
            if (!loadedForms.ContainsKey(typeof(T)))
            {
                loadedForms[typeof(T)] = new T();
            }

            var userControl = loadedForms[typeof(T)];
            contentPanel.Controls.Clear();
            userControl.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(userControl);
            userControl.Focus();
        }

        private void InitializeStatusBar()
        {
            // Initialize timer for date/time updates
            statusTimer = new Timer();
            statusTimer.Interval = 1000; // Update every second
            statusTimer.Tick += StatusTimer_Tick;
            statusTimer.Start();

            // Set initial path and trademark (assuming toolStripStatusLabel1 and toolStripStatusLabel2)
            // toolStripStatusLabel1.Text = Application.StartupPath;
            // toolStripStatusLabel2.Text = "© Your Company 2025";

            // Initial date/time update
            UpdateDateTime();
        }

        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            DateTime now = DateTime.Now;

            // Date format: Mon, 29 July 2025
            toolStripStatusLabel3.Text = now.ToString("ddd, dd MMMM yyyy");

            // Time format: HH:mm:ss (20:09:30)
            toolStripStatusLabel4.Text = now.ToString("HH:mm:ss");
        }

        // Method to update path when needed
        public void UpdatePath(string path)
        {
            // toolStripStatusLabel1.Text = path;
        }

        // Form closing event to clean up timer
        private void Homepage_FormClosed(object sender, FormClosedEventArgs e)
        {
            statusTimer?.Stop();
            statusTimer?.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}