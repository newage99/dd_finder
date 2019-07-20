using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DDGFinder
{
    public partial class GridItem : UserControl
    {
        public GridItem()
        {
            InitializeComponent();
        }

        private string idValue = "";

        public string Id
        {
            get { return idValue; }
            set { idValue = value; }
        }

        private string stateOrResultValue = "";

        public string StateOrResult
        {
            get { return stateOrResultValue; }
            set { stateOrResultValue = value; }
        }
    }
}
