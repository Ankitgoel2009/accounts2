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
        public AccountsInfoPanel()
        {
            InitializeComponent();
        }
        public void ReceiveData(object data) { }
        public object GetData() { return null; }

     
    }
}
