using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private int numberOfNodesValue = 1;
        private int minLengthValue = 8;
        private int maxLengthValue = 20;
        private bool[,,,] topologies;
        private static string characters = "xyn+-*/%()12";
        private static char[] numbers_array = new char[] { 'x', 'y', 'n', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private static char[] xyn_array = new char[] { 'x', 'y', 'n', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '(' };
        private static char[] plus_minus_array = new char[] { '+', '-', '*', '/', '%', ')' };
        private static char[] operators_array = new char[] { '+', '*', '/', '%', ')' };
        private static Dictionary<char, char[]> forbidden_right_chars = new Dictionary<char, char[]>()
        {
            { 'x', xyn_array },
            { 'y', xyn_array },
            { 'n', xyn_array },
            { '+', plus_minus_array },
            { '-', plus_minus_array },
            { '*', operators_array },
            { '/', operators_array },
            { '%', operators_array },
            { '(', new char[] { '+', '*', '/', '%', ')' } },
            { ')', numbers_array },
            { '0', numbers_array },
            { '1', numbers_array },
            { '2', numbers_array },
            { '3', numbers_array },
            { '4', numbers_array },
            { '5', numbers_array },
            { '6', numbers_array },
            { '7', numbers_array },
            { '8', numbers_array },
            { '9', numbers_array }
        };
        private static char[] array_left = new char[] { 'x', 'y', 'n', ')', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private static char[] operators_and_close_parenthesis_array_left = new char[] { '+', '-', '*', '/', '%', '(' };
        private static Dictionary<char, char[]> forbidden_left_chars = new Dictionary<char, char[]>()
        {
            { 'x', array_left },
            { 'y', array_left },
            { 'n', array_left },
            { '+', operators_and_close_parenthesis_array_left },
            { '-', new char[] { '+', '-' } },
            { '*', operators_and_close_parenthesis_array_left },
            { '/', operators_and_close_parenthesis_array_left },
            { '%', operators_and_close_parenthesis_array_left },
            { '(', array_left },
            { ')', operators_and_close_parenthesis_array_left },
            { '0', array_left },
            { '1', array_left },
            { '2', array_left },
            { '3', array_left },
            { '4', array_left },
            { '5', array_left },
            { '6', array_left },
            { '7', array_left },
            { '8', array_left },
            { '9', array_left }
        };

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
        public bool StateNotLoadingOrDoingIterations
        {
            get { return appState != AppState.LOADING_TOPOLOGIES || appState != AppState.DOING_ITERATIONS; }
        }
        public string NumberOfNodes
        {
            get { return numberOfNodesValue.ToString(); }
            set
            {
                if (!int.TryParse(value, out numberOfNodesValue))
                    numberOfNodesValue = 0;
            }
        }
        public string MinLength
        {
            get { return minLengthValue.ToString(); }
            set
            {
                if (!int.TryParse(value, out minLengthValue))
                    minLengthValue = 0;
            }
        }
        public string MaxLength
        {
            get { return maxLengthValue.ToString(); }
            set
            {
                if (!int.TryParse(value, out maxLengthValue))
                    maxLengthValue = 0;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
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
            topologies = new bool[10, 10, numberOfNodesValue, numberOfNodesValue];
            string aaa = "";
            while (true)
            {
                aaa = getValidRandomId();
            }
        }

        private static char randomChar(Random r)
        {
            return characters[r.Next(0, characters.Length - 1)];
        }

        private bool validToPlaceCloseParenthesis(string result, int length)
        {
            return false;
        }

        private string getValidRandomId()
        {
            Random r = new Random();
            int length = r.Next(minLengthValue, maxLengthValue);
            char aux = randomChar(r);
            while (aux == '+' || aux == '*' || aux == '/' || aux == '%' || aux == ')')
                aux = randomChar(r);
            string result = aux.ToString();
            int parenCounter = aux != '(' ? 0 : 1;
            int placeOfTheLastOpenParen = -10;
            string charsRemoved = "";
            for (int i = 1; i < length; i++)
            {
                if (parenCounter > 0 && i+1 > length-parenCounter)
                {
                    while (result.EndsWith("+") || result.EndsWith("-") || result.EndsWith("*") || result.EndsWith("/") || result.EndsWith("%"))
                    {
                        charsRemoved += result[result.Length - 1];
                        result = result.Substring(0, result.Length - 1);
                    }
                    while (parenCounter > 0)
                    {
                        result += ')';
                        parenCounter -= 1;
                    }
                    break;
                } else
                {
                    bool notDone = true;
                    while (notDone)
                    {
                        aux = randomChar(r);
                        if (!forbidden_left_chars[aux].Contains(result[i - 1])
                            && (aux != ')' || (aux == ')' && (parenCounter > 0 && i-3 > placeOfTheLastOpenParen)))
                            && (aux != '(' || (aux == '(' && i+4 < length)))
                        {
                            result += aux;
                            if (aux == '(')
                            {
                                parenCounter += 1;
                                placeOfTheLastOpenParen = i;
                            }
                            else if (aux == ')')
                                parenCounter -= 1;
                            notDone = false;
                        }
                    }
                }
            }
            string almost_result = result;
            while(result.EndsWith("+") || result.EndsWith("-") || result.EndsWith("*") || result.EndsWith("/") || result.EndsWith("%"))
            {
                result = result.Substring(0, result.Length - 1);
            }
            return result;
        }
    }
}
