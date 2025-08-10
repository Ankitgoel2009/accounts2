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
        private GatewayofTallypanel gatewayoftallypanel; 
            
        public HomeScreen()
        {
            InitializeComponent();
            this.gatewayoftallypanel = new GatewayofTallypanel();
            this.gatewayoftallypanel.Dock = DockStyle.Fill;
            this.gatewayoftallypanel.Name = "gatewayoftallypanel";
            this.gatewayoftallypanel.TabIndex = 0;
            this.rightCenteringLayout.Controls.Add(this.gatewayoftallypanel, 1, 1);

        }
        // IDataExchange Implementation (required by your architecture)
        public void ReceiveData(object data)
        {
            // Handle incoming data from other forms
            // Example: populate left panel content based on data
        }

        public object GetData()
        {
            // Return data to be passed to other forms
            return null; // Return relevant data when needed
        }
    }
}
