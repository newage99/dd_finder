using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace DDGFinder
{
    public partial class MainWindow : Window
    {
        enum AppState
        {
            INITIAL = 0,
            LOADING_TOPOLOGIES = 1,
            IDLE= 2,
            DOING_ITERATIONS = 3
        };
        private AppState appState = AppState.INITIAL;
        private bool requestedToStopIterating = false;
        private BindableTwoDArray<string> idsValues =
            new BindableTwoDArray<string>(10, 10);
        private BindableTwoDArray<string> stateOrResultsValues =
            new BindableTwoDArray<string>(10, 10);
        public BindableTwoDArray<string> Ids
        {
            get { return idsValues; }
        }
        public BindableTwoDArray<string> StateOrResults
        {
            get { return stateOrResultsValues; }
        }
        public bool StateIsIdle
        {
            get { return appState == AppState.IDLE; }
        }
        public bool StopIteratingIsEnabled
        {
            get { return appState == AppState.DOING_ITERATIONS && !requestedToStopIterating; }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            idsValues[0, 0] = "asdasd";
            idsValues[0, 1] = "asdasd";
        }

        private static readonly Regex _regex = new Regex("[^0-9.-]+");
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void Init_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
