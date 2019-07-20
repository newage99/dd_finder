using System.Windows;
using System.Windows.Controls;

namespace DDGFinder
{
    public partial class GridItem : UserControl
    {
        public GridItem()
        {
            InitializeComponent();
        }

        public static DependencyProperty IdProperty = DependencyProperty.Register("Id", typeof(string), typeof(GridItem));
        public string Id
        {
            get { return (string)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }
        /*private string idValue = "";
        public string Id
        {
            get { return idValue; }
            set { idValue = value; }
        }*/
    }
}
