using System;
using System.Windows.Forms;

namespace accounts2
{
    public partial class UltimateMaster : Form
    {
        private Timer statusTimer;
        private Panel contentPanel;

        public UltimateMaster()
        {
            InitializeComponent();
            contentPanel = this.Controls["ContentPanel"] as Panel;
        }

        private void Homepage_Load(object sender, EventArgs e)
        {
            InitializeStatusBar();
            LoadHomeScreen();
        }

        private void LoadHomeScreen()
        {
            // Simply create HomeScreen and add to ContentPanel
            // HomeScreen will automatically load GatewayofTallypanel in its constructor
            HomeScreen homeScreen = new HomeScreen();
            homeScreen.Dock = DockStyle.Fill;

            contentPanel.Controls.Add(homeScreen);
        }

        private void InitializeStatusBar()
        {
            statusTimer = new Timer();
            statusTimer.Interval = 1000;
            statusTimer.Tick += StatusTimer_Tick;
            statusTimer.Start();

            UpdateDateTime();
        }

        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            DateTime now = DateTime.Now;
            toolStripStatusLabel3.Text = now.ToString("ddd, dd MMMM yyyy");
            toolStripStatusLabel4.Text = now.ToString("HH:mm:ss");
        }

        private void Homepage_FormClosed(object sender, FormClosedEventArgs e)
        {
            statusTimer?.Stop();
            statusTimer?.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Button functionality here
        }
    }
}