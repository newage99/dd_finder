﻿using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace DDGFinder
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        enum AppState
        {
            INITIAL = 0,
            LOADING_TOPOLOGIES = 1,
            IDLE= 2,
            DOING_ITERATIONS = 3
        };
        private AppState appState = AppState.INITIAL;
        private static readonly Random r = new Random();
        private bool requestedToStopIterating = false;
        /*private BindableTwoDArray<string> idsValues =
            new BindableTwoDArray<string>(10, 10);
        private BindableTwoDArray<string> stateOrResultsValues =
            new BindableTwoDArray<string>(10, 10);*/
        private ObservableCollection<string> idsValues = new ObservableCollection<string>();
        private ObservableCollection<string> stateOrResultsValues = new ObservableCollection<string>();
        private int numberOfNodesValue = 1;
        private int minLengthValue = 10;
        private int maxLengthValue = 20;
        private static string characters = "xyn+-*/%^L()";
        private static char[] numbers_and_operators_array = new char[] { 'x', 'y', 'n', '2', '+', '-', '*', '/', '%', '^', 'L' };
        private static char[] numbers_array = new char[] { 'x', 'y', 'n' };
        private static int numbers_array_length = numbers_array.Length;
        private static char[] operators_array = new char[] { '+', '-', '*', '/', '%', '^', 'L' };
        private static int operators_array_length = operators_array.Length;
        private static char[] array_left = new char[] { 'x', 'y', 'n', ')', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private static char[] operators_and_close_parenthesis_array_left = new char[] { '+', '-', '*', '/', '%', '^', 'L', '(' };
        private static Dictionary<char, char[]> forbidden_left_chars = new Dictionary<char, char[]>()
        {
            { 'x', array_left },
            { 'y', array_left },
            { 'n', array_left },
            { '+', operators_and_close_parenthesis_array_left },
            { '-', new char[] { '+', '-', '/', '%', 'L' } },
            { '*', operators_and_close_parenthesis_array_left },
            { '/', operators_and_close_parenthesis_array_left },
            { '%', operators_and_close_parenthesis_array_left },
            { '^', operators_and_close_parenthesis_array_left },
            { 'L', operators_and_close_parenthesis_array_left },
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
        private static int MUTATION_ADD_LEFT = 0;
        private static int MUTATION_ADD_RIGHT = 1;
        private static int MUTATION_DELETE = 2;
        private ObservableCollection<Topology> topologies = new ObservableCollection<Topology>();
        private int numberOfIterationsValue = 0;
        private int iterationsToDoValue = 1;
        private int leftIterations = 0;

        public string IterationsToDo
        {
            get { return iterationsToDoValue.ToString(); }
            set {
                bool isNumeric = int.TryParse(value, out int n);
                if (isNumeric && n > 0)
                    iterationsToDoValue = n;
            }
        }
        public string NumberOfIterations
        {
            get { return numberOfIterationsValue.ToString(); }
            set { int.TryParse(value, out numberOfIterationsValue); }
        }
        public ObservableCollection<string> Ids
        {
            get { return idsValues; }
        }
        public ObservableCollection<string> StateOrResults
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
            for (int i = 0; i < 100; i++)
            {
                topologies.Add(null);
                idsValues.Add(null);
                stateOrResultsValues.Add(null);
            }
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
            SetField(ref appState, AppState.INITIAL, "StateIsIdle");
            OnPropertyChanged("StopIteratingIsEnabled");
            Task.Run(() => Init());
        }

        private async void Init()
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(InitTopology(i));
            }
            Task.WaitAll(tasks.ToArray());
            OnPropertyChanged("Ids");
            orderTopologiesByPuntuation();
            SetField(ref appState, AppState.IDLE, "StateIsIdle");
            SetField(ref numberOfIterationsValue, 0, "NumberOfIterations");
        }

        private async Task InitTopology(int i)
        {
            idsValues[i] = "";
            stateOrResultsValues[i] = "Allocating and creating...";
            topologies[i] = new Topology(numberOfNodesValue);
            await Task.Run(() => MutateOrCreateRandomIdAndComputeAsync(i, null));
        }

        private Task CreateRandomIdAndCompute(int i)
        {
            return Task.Run(() => MutateOrCreateRandomIdAndComputeAsync(i, null));
        }

        private bool setId(int pos, string id)
        {
            lock (idsValues)
            {
                for (int i = 0; i < 100; i++)
                {
                    if (idsValues[i].Equals(id))
                        return false;
                }
                idsValues[pos] = id;
            }
            return true;
        }

        private void DoIterations_Click(object sender, RoutedEventArgs e)
        {
            SetField(ref appState, AppState.DOING_ITERATIONS, "StateIsIdle");
            OnPropertyChanged("StopIteratingIsEnabled");
            leftIterations = iterationsToDoValue;
            Task.Run(() => Iterate());
        }

        private async Task Iterate()
        {
            List<Task> tasks = new List<Task>();
            while (leftIterations > 0)
            {
                tasks.Clear();
                for (int i = 48; i < 96; i++)
                    tasks.Add(MutateIdAndCompute(i));
                for (int i = 96; i < 100; i++)
                    tasks.Add(CreateRandomIdAndCompute(i));
                Task.WaitAll(tasks.ToArray());
                orderTopologiesByPuntuation();
                leftIterations -= 1;
                SetField(ref numberOfIterationsValue, numberOfIterationsValue + 1, "NumberOfIterations");
            }
            SetField(ref appState, AppState.IDLE, "StateIsIdle");
        }

        private Task MutateIdAndCompute(int i)
        {
            return Task.Run(() => MutateOrCreateRandomIdAndComputeAsync(i, topologies[i - 45].Id));
        }

        private void MutateOrCreateRandomIdAndComputeAsync(int i, string idToMutate)
        {
            try {
                bool disconnected;
                bool idNotSetted = true;
                while (idNotSetted)
                {
                    disconnected = true;
                    while (disconnected)
                    {
                        string createdId;
                        if (idToMutate == null)
                            createdId = getValidRandomId();
                        else
                            createdId = mutateId(idToMutate);
                        topologies[i].setIdAndPopulate(createdId);
                        disconnected = topologies[i].isDisconnected();
                    }
                    idNotSetted = !setId(i, topologies[i].Id);
                    if (!idNotSetted)
                    {
                        stateOrResultsValues[i] = "Waiting to start";
                        topologies[i].calculateDD();
                        stateOrResultsValues[i] = topologies[i].DegreeDiameterAndSecondPuntuation;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("CreateRandomIdAndComputeAsync: " + e.ToString());
            }
        }

        private string mutateIdInner(string id)
        {
            int posToMutate = 0;
            char charToMutate = ' ';
            try
            {
                posToMutate = r.Next(id.Length);
                charToMutate = id[posToMutate];
            } catch (Exception e)
            {
                int a = 0;
            }
            string newId = "";
            if (numbers_and_operators_array.Contains(charToMutate))
            {
                try
                {
                    char[] arrayToUse;
                    int arrayToUseLength;
                    if (numbers_array.Contains(charToMutate))
                    {
                        arrayToUse = numbers_array;
                        arrayToUseLength = numbers_array_length;
                    }
                    else
                    {
                        arrayToUse = operators_array;
                        arrayToUseLength = operators_array_length;
                    }
                    char newChar = charToMutate;
                    while (newChar == charToMutate)
                        newChar = arrayToUse[r.Next(arrayToUseLength)];
                    if (newChar == '-' && posToMutate == 0)
                        newId = id.Remove(0, 1);
                    else
                        newId = id.Substring(0, posToMutate) + newChar + id.Substring(posToMutate + 1, id.Length - (posToMutate + 1));
                } catch (Exception e)
                {
                    int a = 0;
                }
                if (newId.Contains('1') || newId.Contains('3') || newId.Contains('4') || newId.Contains('5') || newId.Contains('6') || newId.Contains('7') || newId.Contains('8') || newId.Contains('9'))
                {
                    int a = 0;
                }
            }
            else if (charToMutate == '(' || charToMutate == ')')
            {
                int option = r.Next(4);
                if (option < 2)
                {
                    try
                    {
                        int offset = 0;
                        if (option == MUTATION_ADD_RIGHT)
                            offset += 1;
                        newId = id.Substring(0, posToMutate + offset);
                        if (charToMutate == '(')
                            newId += numbers_array[r.Next(numbers_array_length)].ToString() + operators_array[r.Next(operators_array_length)].ToString();
                        else
                            newId += operators_array[r.Next(operators_array_length)].ToString() + numbers_array[r.Next(numbers_array_length)].ToString();
                        newId += id.Substring(posToMutate + offset, id.Length - (posToMutate + offset));
                    }
                    catch (Exception e)
                    {
                        int a = 0;
                    }
                } else
                {
                    try
                    {
                        newId = id.Substring(0, posToMutate);
                        if (charToMutate == '(')
                        {
                            newId += numbers_array[r.Next(numbers_array_length)];
                            if (posToMutate < id.Length - 1 && id[posToMutate + 1] != '-')
                                newId += operators_array[r.Next(operators_array_length)];
                        } else
                            newId += operators_array[r.Next(operators_array_length)].ToString() + numbers_array[r.Next(numbers_array_length)].ToString();
                        newId += id.Substring(posToMutate + 1, id.Length - (posToMutate + 1));
                    }
                    catch (Exception e)
                    {
                        int a = 0;
                    }
                }
            }
            else
            {
                int a = 0;
            }
            return newId;
        }

        private string mutateId(string id)
        {
            int numberOfTimesToMutate = r.Next(4) + 1;
            string mutatedId = id;
            for (int i = 0; i < numberOfTimesToMutate; i++)
                mutatedId = mutateIdInner(mutatedId);
            return mutatedId;
        }

        private void orderTopologiesByPuntuation()
        {
            ObservableCollection<Topology> newTopologies = new ObservableCollection<Topology>();
            for (int i = 0; i < 100; i++)
            {
                bool notInserted = true;
                for (int j = 0; j < newTopologies.Count; j++)
                {
                    if ((topologies[i].Puntuation < newTopologies[j].Puntuation) ||
                        (topologies[i].Puntuation == newTopologies[j].Puntuation &&
                        topologies[i].SecondPuntuation < newTopologies[j].SecondPuntuation))
                    {
                        newTopologies.Insert(j, topologies[i]);
                        notInserted = false;
                        break;
                    }
                }
                if (notInserted)
                    newTopologies.Add(topologies[i]);
            }
            topologies = newTopologies;
            for (int i = 0; i < 100; i++)
            {
                idsValues[i] = topologies[i].Id;
                stateOrResultsValues[i] = topologies[i].DegreeDiameterAndSecondPuntuation;
            }
        }

        private static char randomChar()
        {
            if (r.Next(32) == 0)
            {
                return r.Next(1) == 0 ? '(' : ')';
            }
            return characters[r.Next(0, characters.Length - 1)];
        }

        private string getValidRandomId()
        {
            int length = r.Next(minLengthValue, maxLengthValue);
            char aux = randomChar();
            while (aux == '+' || aux == '*' || aux == '/' || aux == '%' || aux == '^' || aux == 'L' || aux == ')')
                aux = randomChar();
            string result = aux.ToString();
            int parenCounter = aux != '(' ? 0 : 1;
            int placeOfTheLastOpenParen = aux != '(' ? -10 : 0;
            string charsRemoved = "";
            for (int i = 1; i < length; i++)
            {
                try
                {
                    if (parenCounter > 0 && i + 1 > length - parenCounter)
                    {
                        while (result.EndsWith("+") ||
                            result.EndsWith("-") ||
                            result.EndsWith("*") ||
                            result.EndsWith("/") ||
                            result.EndsWith("%") ||
                            result.EndsWith("^") ||
                            result.EndsWith("L")
                            )
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
                    }
                    else
                    {
                        bool notDone = true;
                        while (notDone)
                        {
                            aux = randomChar();
                            if (!forbidden_left_chars[aux].Contains(result[i - 1])
                                && (aux != ')' || (aux == ')' && (parenCounter > 0 && i - 3 > placeOfTheLastOpenParen)))
                                && (aux != '(' || (aux == '(' && i + 4 + parenCounter < length)))
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
                } catch (Exception e)
                {
                    int a = 0;
                }
            }
            while (result.EndsWith("+") || result.EndsWith("-") || result.EndsWith("*") || result.EndsWith("/") || result.EndsWith("%") || result.EndsWith("^") || result.EndsWith("L"))
            {
                result = result.Substring(0, result.Length - 1);
            }
            List<char> notContains = new List<char>();
            if (!result.Contains("x")) notContains.Add('x');
            if (!result.Contains("y")) notContains.Add('y');
            if (!result.Contains("n")) notContains.Add('n');
            if (notContains.Count > 0)
            {
                try
                {
                    // We ensure 'x' and 'y' variables are in the id
                    int newLength = result.Length;
                    char[] order = new char[5] { 'x', 'y', 'n', '1', '2' };
                    Dictionary<char, int> orderCharToPos = new Dictionary<char, int>()
                    {
                        { 'x', 0 },
                        { 'y', 1 },
                        { 'n', 2 },
                        { '1', 3 },
                        { '2', 4 }
                    };
                    List<int>[] positions = new List<int>[5];
                    for (int i = 0; i < 5; i++)
                    {
                        positions[i] = new List<int>();
                    }
                    for (int i = 0; i < newLength; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            if (result[i] == order[j])
                                positions[j].Add(i);
                        }
                    }
                    for (int i = 0; i < notContains.Count; i++)
                    {
                        char notContainChar = notContains[i];
                        int pos = orderCharToPos[notContainChar];
                        bool notAchieved = true;
                        for (int j = 0; j < 5; j++)
                        {
                            if (j != pos && positions[j].Count > 1)
                            {
                                int actualPos = positions[j][0];
                                positions[j].RemoveAt(0);
                                result = result.Substring(0, actualPos) + notContainChar +
                                    result.Substring(actualPos + 1, result.Length - (actualPos + 1));
                                notAchieved = false;
                                break;
                            }
                        }
                        if (notAchieved)
                            result += "%" + notContainChar.ToString();
                    }
                } catch (Exception e)
                {
                    int a = 0;
                }
            }
            return result;
        }
    }
}
