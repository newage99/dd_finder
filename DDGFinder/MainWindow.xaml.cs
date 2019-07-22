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
            idsValues[0, 0] = getValidRandomId();
        }

        /*private int validateId(string id)
        {
            int length = id.Length;
            char start = id[0];
            if (start == '+' || start == '*' || start == '/' || start == '%' || start == ')')
                return length - 1;
            char prev = ' ';
            char actual = id[0];
            char next;
            for(int i = 1; i < length; i++)
            {
                if (i < id.Length)
                    next = id[i];
                else next = ' ';
                if (forbidden_left_chars[actual].Contains(prev) || forbidden_right_chars[actual].Contains(next))
                    return i;
                prev = actual;
                actual = next;
            }
            char last = id.Last();
            if (last == '+' || last == '-' || last == '*' || last == '/' || last == '%' || last == '(')
                return length - 1;
            if (!id.Contains('x') || !id.Contains('y'))
                return length - 1;
            if ((id.Contains('y') && !id.Contains('x')) || (id.Contains('z') && (!id.Contains('x') || !id.Contains('y'))))
                return length - 1;
            bool contains_variables = false;
            bool wrong_parenthesis = false;
            int parenthesis_counter = 0;
            bool parenthesis_with_one_space_only = false;
            int vars_since_last_opening_parenthesis = 0;
            last = ' ';
            bool double_parenthesis_starts = false;
            bool there_is_a_double_parenthesis = false;
            for(int i = 0; i < length; i++)
            {
                char c = id[i];
                if ("xyzn".Contains(c))
                    contains_variables = true;
                if (c == '(')
                {
                    parenthesis_counter += 1;
                    vars_since_last_opening_parenthesis = 0;
                    if (last == '(')
                        double_parenthesis_starts = true;
                }
                else
                {
                    if (c == ')')
                    {
                        if (last == ')' && double_parenthesis_starts)
                        {
                            there_is_a_double_parenthesis = true;
                            break;
                        }
                        parenthesis_counter -= 1;
                        if (vars_since_last_opening_parenthesis < 2)
                        {
                            parenthesis_with_one_space_only = true;
                            break;
                        }
                    }
                    else
                    {
                        if (last == ')')
                        {
                            double_parenthesis_starts = false;
                            break;
                        }
                        vars_since_last_opening_parenthesis += 1;
                    }
                }
                if (parenthesis_counter < 0)
                {
                    wrong_parenthesis = true;
                    break;
                }
                last = c;
            }
            if (!contains_variables || wrong_parenthesis || parenthesis_counter != 0
                || parenthesis_with_one_space_only || there_is_a_double_parenthesis)
                return length - 1;
            if (id.StartsWith("(") && id.EndsWith(")"))
                return length - 1;
            return 0;
        }

        private bool nextId(ref string id, int pointer)
        {
            int length = id.Length;
            int model_pos = -1;
            if (pointer < length - 1)
            {
                if (pointer > 0)
                {
                    string aux = "";
                    for (int i = pointer + 1; i < length; i++)
                    {
                        aux += characters[0];
                    }
                    id = id.Substring(0, pointer + 1) + aux;
                }
                else pointer = length - 1;
            }
            else pointer = length - 1;
            while (pointer >= 0)
            {
                char char_to_look_at = id[pointer];
                int characters_length = characters.Length;
                for(int i = 0; i < characters_length; i++)
                {
                    if (characters[i] == char_to_look_at)
                    {
                        model_pos = i;
                        break;
                    }
                }
                if (model_pos < characters_length - 1)
                {
                    id = id.Substring(0, pointer) + characters[model_pos + 1];
                    for (int j = id.Length; j < length; j++)
                    {
                        id += characters[0];
                    }
                    return true;
                }
                else pointer -= 1;
            }
            return false;
        }*/

        private static char randomChar()
        {
            return characters[new Random().Next(0, characters.Length - 1)];
        }

        private string createRandomId()
        {
            string result = "";
            int length = new Random().Next(minLengthValue, maxLengthValue);
            char aux = randomChar();
            while (aux == '+' || aux == '*' || aux == '/' || aux == '%' || aux == ')')
                aux = randomChar();
            result += aux;
            for (int i = 1; i < length; i++)
            {
                result += randomChar();
            }
            return result;
        }

        private string getValidRandomId()
        {
            string result = createRandomId();
            int length = result.Length;
            for (int i = 0; i < length; i++)
            {
                // TODO
            }
            return result;
        }
    }
}

/*int pointer = validateId(result);
bool not_arrived_to_end = true;
while (pointer != 0)
{
    if (!nextId(ref result, pointer))
    {
        not_arrived_to_end = false;
        break;
    }
    pointer = validateId(result);
}
if (not_arrived_to_end)
    random_string_not_done = false;*/