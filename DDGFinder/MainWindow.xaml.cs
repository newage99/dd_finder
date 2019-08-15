using System.Windows;
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
        private ObservableCollection<string> idsValues = new ObservableCollection<string>();
        private string[] idsValuesAux = new string[100];
        private ObservableCollection<string> stateOrResultsValues = new ObservableCollection<string>();
        private int numberOfNodesValue = 1;
        private static int minLengthValue = 30;
        private static int maxLengthValue = 32;
        private static readonly string characters = "xyn+-*/%^L()";
        private static readonly char[] numbers_and_operators_array = new char[] { 'x', 'y', 'n', '2', '+', '-', '*', '/', '%', '^', 'L' };
        private static readonly char[] numbers_array = new char[] { 'x', 'y', 'n' };
        private static readonly int numbers_array_length = numbers_array.Length;
        private static readonly char[] operators_array = new char[] { '+', '-', '*', '/', '%', '^', 'L' };
        private static readonly int operators_array_length = operators_array.Length;
        private static readonly char[] array_left = new char[] { 'x', 'y', 'n', ')', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private static readonly char[] operators_and_close_parenthesis_array_left = new char[] { '+', '-', '*', '/', '%', '^', 'L', '(' };
        private static readonly Dictionary<char, char[]> forbidden_left_chars = new Dictionary<char, char[]>()
        {
            { 'x', array_left },
            { 'y', array_left },
            { 'n', array_left },
            { '+', operators_and_close_parenthesis_array_left },
            { '-', operators_and_close_parenthesis_array_left },
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
        private static volatile ObservableCollection<Topology> topologies = new ObservableCollection<Topology>();
        private int numberOfIterationsValue = 0;
        private int iterationsToDoValue = 1;
        private int leftIterations = 0;
        private int amountOfEqualFirstsSecondPuntuations = 1;
        private int numberOfTopologiesToMutate = 48;

        public string IterationsToDo
        {
            get { return iterationsToDoValue.ToString(); }
            set {
                bool isNumeric = int.TryParse(value, out int n);
                if (isNumeric && n > 0)
                    iterationsToDoValue = n;
            }
        }
        private double iterationTimeAvgValue = 0D;
        public string IterationTimeAvg
        {
            get
            { return iterationTimeAvgValue.ToString(); }
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
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            DataContext = this;
            for (int i = 0; i < 100; i++)
            {
                topologies.Add(null);
                idsValues.Add(null);
                stateOrResultsValues.Add(null);
            }
            NumberOfNodesTextBox.Focus();
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
            idsValues.CopyTo(idsValuesAux, 0);
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(InitTopology(i));
            }
            Task.WaitAll(tasks.ToArray());
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
                    if (idsValuesAux[i] != null && idsValuesAux[i].Equals(id))
                        return false;
                }
                idsValuesAux[pos] = id;
            }
            return true;
        }

        private static int Next(int max)
        {
            lock (r) return r.Next(max);
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
                DateTime dateTimeInit = DateTime.Now;
                tasks.Clear();
                int end = numberOfTopologiesToMutate * 2;
                int mutationsPerThread = 1;
                idsValues.CopyTo(idsValuesAux, 0);
                for (int i = numberOfTopologiesToMutate; i < end; i+= mutationsPerThread)
                    tasks.Add(MutateIdsAndCompute(i, mutationsPerThread));
                for (int i = end; i < 100; i++)
                    tasks.Add(CreateRandomIdAndCompute(i));
                Task.WaitAll(tasks.ToArray());
                orderTopologiesByPuntuation();                
                leftIterations -= 1;
                SetField(ref numberOfIterationsValue, numberOfIterationsValue + 1, "NumberOfIterations");
                TimeSpan timeSpan = DateTime.Now - dateTimeInit;
                if (numberOfIterationsValue > 1)
                {
                    iterationTimeAvgValue = ((iterationTimeAvgValue * (numberOfIterationsValue - 1)) / numberOfIterationsValue) + (timeSpan.TotalMilliseconds / numberOfIterationsValue);
                }
                else
                    iterationTimeAvgValue = (timeSpan.TotalMilliseconds / numberOfIterationsValue);


                OnPropertyChanged("IterationTimeAvg");
            }
            SetField(ref appState, AppState.IDLE, "StateIsIdle");
        }

        private Task MutateIdsAndCompute(int i, int numberOfMutations)
        {
            return Task.Run(() => MutateRandomIdsAndComputeAsync(i, numberOfMutations));
        }

        private Task MutateIdAndCompute(int i)
        {
            return Task.Run(() => MutateOrCreateRandomIdAndComputeAsync(i, topologies[i - numberOfTopologiesToMutate].Id));
        }
        
        private void MutateRandomIdsAndComputeAsync(int j, int numberOfMutations)
        {
            int end = j + numberOfMutations;
            for (int i = j; i < end; i++)
            {
                try
                {
                    string idToMutate = topologies[i - numberOfTopologiesToMutate].Id;
                    string oldId = topologies[i].Id;
                    bool idNotSetted = true;
                    DateTime init = DateTime.Now;
                    int tries = 0;
                    while (idNotSetted)
                    {
                        string createdId = mutateId(idToMutate, j);
                        topologies[i].setIdAndPopulate(createdId);
                        if (!topologies[i].isDisconnected())
                        {
                            idNotSetted = !setId(i, topologies[i].Id);
                            if (!idNotSetted)
                            {
                                //stateOrResultsValues[i] = "Waiting to start";
                                topologies[i].calculateDD();
                                //stateOrResultsValues[i] = topologies[i].DegreeDiameterAndSecondPuntuation;
                            }
                        }
                        else
                        {
                            idNotSetted = false;
                            topologies[i].Id = oldId;
                        }
                        tries++;
                    }
                    topologies[i].tries = tries;
                    topologies[i].populateAndIdSettedTime = Math.Round((DateTime.Now - init).TotalMilliseconds);
                }
                catch (Exception e)
                {
                    Console.WriteLine("CreateRandomIdAndComputeAsync: " + e.ToString());
                }
            }
        }

        private void MutateOrCreateRandomIdAndComputeAsync(int i, string idToMutate)
        {
            try {
                bool disconnected;
                bool idNotSetted = true;
                while (idNotSetted)
                {
                    disconnected = true;
                    bool mutationReturnedDisconnectedGraph = false;
                    string createdId = "";
                    DateTime init = DateTime.Now;
                    int tries = 0;
                    while (disconnected)
                    {
                        if (idToMutate == null)
                            createdId = getValidRandomId();
                        else if (mutationReturnedDisconnectedGraph)
                           createdId = mutateIdInner(createdId, i);
                        else
                            createdId = mutateId(idToMutate, i);
                        topologies[i].setIdAndPopulate(createdId);
                        disconnected = topologies[i].isDisconnected();
                        if (disconnected)
                            mutationReturnedDisconnectedGraph = true;
                        tries++;
                    }
                    topologies[i].tries = tries;
                    topologies[i].populateAndIdSettedTime = Math.Round((DateTime.Now - init).TotalMilliseconds);
                    idNotSetted = !setId(i, topologies[i].Id);
                    if (!idNotSetted)
                    {
                        //stateOrResultsValues[i] = "Waiting to start";
                        topologies[i].calculateDD();
                        //stateOrResultsValues[i] = topologies[i].DegreeDiameterAndSecondPuntuation;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("CreateRandomIdAndComputeAsync: " + e.ToString());
            }
        }

        private string mutateIdInner(string id, int j)
        {
            int posToMutate = 0;
            char charToMutate = ' ';
            try
            {
                posToMutate = Next(id.Length);
                charToMutate = id[posToMutate];
                if (charToMutate == '-' && (posToMutate == 0 || (posToMutate > 0 && (id[posToMutate - 1] == '(' || id[posToMutate - 1] == '*'))))
                    return id.Remove(posToMutate, 1);
            } catch (Exception e)
            {
                int a = 0;
            }
            string newId = "";
            if (numbers_and_operators_array.Contains(charToMutate))
            {
                try
                {
                    bool trueNumberFalseOperation = numbers_array.Contains(charToMutate);
                    bool allowedToInsertOpen = allowedToInsertOpenParenthesisAtLeft(id, posToMutate, trueNumberFalseOperation);
                    bool allowedToInsertClose = allowedToInsertCloseParenthesisAtRight(id, posToMutate, trueNumberFalseOperation);
                    int idLengthAverage = (maxLengthValue + minLengthValue) / 2;
                    int probToInsertParenthesis = (Math.Abs((idLengthAverage) - id.Length) * 2) + idLengthAverage;
                    if ((allowedToInsertOpen || allowedToInsertClose) && Next(probToInsertParenthesis) == 0)
                    {
                        if (!allowedToInsertClose)
                            newId = insertCloseParenthesis(id, posToMutate, trueNumberFalseOperation);
                        else if (!allowedToInsertOpen)
                            newId = insertOpenParenthesis(id, posToMutate, trueNumberFalseOperation);
                        else if (Next(2) == 0)
                            newId = insertOpenParenthesis(id, posToMutate, trueNumberFalseOperation);
                        else
                            newId = insertCloseParenthesis(id, posToMutate, trueNumberFalseOperation);
                        checkNewIdHasCorrectParenthesis(newId);
                    } else if (posToMutate > 0 && id.Length > maxLengthValue + 6 && (id[posToMutate - 1] != ')' || id[posToMutate + 1] != '('))
                    {
                        try
                        {
                            if (numbers_and_operators_array.Contains(id[posToMutate - 1]))
                                newId = id.Remove(posToMutate - 1, 2);
                            else if (numbers_and_operators_array.Contains(id[posToMutate + 1]))
                                newId = id.Remove(posToMutate, 2);
                            else
                            {
                                int a = 0;
                            }
                            bool founded = true;
                            while (founded)
                            {
                                founded = false;
                                int positionOfTheLastOpenParenthesis = -1;
                                int length = newId.Length;
                                for (int i = 0; i < length; i++)
                                {
                                    if (newId[i] == '(')
                                        positionOfTheLastOpenParenthesis = i;
                                    else if (newId[i] == ')' && i - 2 == positionOfTheLastOpenParenthesis)
                                    {
                                        newId = newId.Remove(positionOfTheLastOpenParenthesis + 2, 1);
                                        newId = newId.Remove(positionOfTheLastOpenParenthesis, 1);
                                        founded = true;
                                        break;
                                    }
                                }
                            }
                        } catch (Exception e)
                        {
                            int a = 0;
                        }
                    } else
                    {
                        char[] arrayToUse;
                        int arrayToUseLength;
                        if (trueNumberFalseOperation)
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
                            newChar = arrayToUse[Next(arrayToUseLength)];

                        if (newChar == '-' && (posToMutate == 0 || (posToMutate + 1 < id.Length && id[posToMutate + 1] == '-')))
                            newId = id.Remove(posToMutate, 1);
                        else
                            newId = id.Substring(0, posToMutate) + newChar + id.Substring(posToMutate + 1, id.Length - (posToMutate + 1));
                    }
                }
                catch (Exception e)
                {
                    int a = 0;
                }
            }
            else if (charToMutate == '(' || charToMutate == ')')
            {
                int option = Next(4);
                if (option < 2)
                {
                    try
                    {
                        int offset = option == MUTATION_ADD_RIGHT ? 1 : 0;
                        newId = id.Substring(0, posToMutate + offset);
                        if (charToMutate == '(')
                            newId += numbers_array[Next(numbers_array_length)].ToString() + operators_array[Next(operators_array_length)].ToString();
                        else
                            newId += operators_array[Next(operators_array_length)].ToString() + numbers_array[Next(numbers_array_length)].ToString();
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
                        bool directionToRemoveChar = true;
                        if (charToMutate == '(')
                        {
                            newId += numbers_array[Next(numbers_array_length)];
                            if (posToMutate < id.Length - 1 && id[posToMutate + 1] != '-')
                                newId += operators_array[Next(operators_array_length)];
                            directionToRemoveChar = false;
                        } else
                            newId += operators_array[Next(operators_array_length)].ToString() + numbers_array[Next(numbers_array_length)].ToString();
                        newId += id.Substring(posToMutate + 1, id.Length - (posToMutate + 1));
                        newId = removeRandomParenthesisChar(newId, posToMutate, directionToRemoveChar);
                    }
                    catch (Exception e)
                    {
                        int a = 0;
                    }
                }
                checkNewIdHasCorrectParenthesis(newId);
            }
            else
            {
                int a = 0;
            }
            int lastTimeOpenParen = 0;
            for (int i = 0; i < newId.Length; i++)
            {
                lastTimeOpenParen++;
                if (newId[i] == '(')
                    lastTimeOpenParen = 0;
                else if (newId[i] == ')' && lastTimeOpenParen <= 3)
                {
                    int a = 0;
                }
            }
            return newId;
        }

        private string insertOpenParenthesis(string value, int startingPos, bool trueNumberFalseOperation)
        {
            int offset = 1;
            if (trueNumberFalseOperation)
                offset--;
            string newId = value.Insert(startingPos + offset, "(");
            List<int> positions = new List<int>();
            int length = newId.Length;
            int init = startingPos + offset + 1;
            for (int i = init; i < length; i++)
            {
                if (newId[i] == '(')
                    break;
                else if (i > init + 1 && operators_array.Contains(newId[i]) || newId[i] == ')')
                    positions.Add(i);
            }
            int count = positions.Count;
            if (count > 0)
            {
                int pos = positions[Next(count)];
                newId = newId.Insert(pos, ")");
            }
            else if (startingPos + (trueNumberFalseOperation ? 4 : 5) == newId.Length)
                newId += ")";
            else
            {
                int a = 0;
            }
            int lastTimeOpenParen = 0;
            for (int i = 0; i < newId.Length; i++)
            {
                lastTimeOpenParen++;
                if (newId[i] == '(')
                    lastTimeOpenParen = 0;
                else if (newId[i] == ')' && lastTimeOpenParen <= 3)
                {
                    int a = 0;
                }
            }
            return newId;
        }

        private string insertCloseParenthesis(string value, int startingPos, bool trueNumberFalseOperation)
        {
            int offset = 0;
            if (trueNumberFalseOperation)
                offset++;
            string newId = value.Insert(startingPos + offset, ")");
            List<int> positions = new List<int>();
            for (int i = startingPos - 2; i > 0; i--)
            {
                if (newId[i] == ')')
                    break;
                else if (numbers_array.Contains(newId[i]) || newId[i] == '(')
                    positions.Add(i);
            }
            int count = positions.Count;
            if (count > 0)
                newId = newId.Insert(positions[Next(count)], "(");
            else
            {
                int a = 0;
            }
            int lastTimeOpenParen = 0;
            for (int i = 0; i < newId.Length; i++)
            {
                lastTimeOpenParen++;
                if (newId[i] == '(')
                    lastTimeOpenParen = 0;
                else if (newId[i] == ')' && lastTimeOpenParen <= 3)
                {
                    int a = 0;
                }
            }
            return newId;
        }

        private void checkNewIdHasCorrectParenthesis(string newId)
        {
            int parCounter = 0;
            for (int i = 0; i < newId.Length; i++)
            {
                if (newId[i] == '(')
                    parCounter++;
                else if (newId[i] == ')')
                {
                    parCounter--;
                }
                if (parCounter < 0)
                {
                    int a = 0;
                }
            }
            if (parCounter != 0)
            {
                int a = 0;
            }
        }

        private bool allowedToInsertOpenParenthesisAtLeft(string value, int startingPos, bool trueNumberFalseOperation)
        {
            int threshold = 3;
            if (trueNumberFalseOperation)
                threshold--;
            if (startingPos <= threshold + 1)
                return false;
            int end = startingPos - threshold;
            for (int i = startingPos - 1; i >= end; i--)
            {
                if (value[i] == ')' || value[i] == '(')
                    return false;
            }
            return true;
        }

        private bool allowedToInsertCloseParenthesisAtRight(string value, int startingPos, bool trueNumberFalseOperation)
        {
            int threshold = 3;
            if (trueNumberFalseOperation)
                threshold--;
            if (startingPos >= value.Length - threshold)
                return false;
            int end = startingPos + threshold;
            for (int i = startingPos + 1; i <= end; i++)
            {
                if (value[i] == '(' || value[i] == ')')
                    return false;
            }
            return true;
        }

        private string removeRandomParenthesisChar(string value, int startingPos, bool directionTrueLeftFalseRight)
        {
            List<int> positions = new List<int>();
            int length = value.Length;
            int additionOrSubstraction = directionTrueLeftFalseRight ? -1 : 1;
            startingPos += additionOrSubstraction;
            char charToRemove = directionTrueLeftFalseRight ? '(' : ')';
            char charThatIndicatesUsThatWeHaveToExitFor = directionTrueLeftFalseRight ? ')' : '(';
            bool positionsNotEmpty = false;
            for (int i = startingPos; directionTrueLeftFalseRight ? i >= 0 : i < length; i += additionOrSubstraction)
            {
                if (value[i] == charToRemove)
                {
                    positions.Add(i);
                    positionsNotEmpty = true;
                }
                else if (value[i] == charThatIndicatesUsThatWeHaveToExitFor && positionsNotEmpty)
                    break;
            }
            int count = positions.Count;
            if (count > 0)
                value = value.Remove(positions[Next(count)], 1);
            else
            {
                int a = 0;
            }
            return value;
        }

        private string mutateId(string id, int j)
        {
            int numberOfTimesToMutate = Next(4) + (amountOfEqualFirstsSecondPuntuations / 6);
            string mutatedId = id;
            for (int i = 0; i < numberOfTimesToMutate; i++)
                mutatedId = mutateIdInner(mutatedId, j);
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
            amountOfEqualFirstsSecondPuntuations = 0;
            int actualSecondPuntuation = topologies[0].SecondPuntuation;
            bool notDone = true;
            ObservableCollection<string> newIdsValues = new ObservableCollection<string>(idsValuesAux);
            idsValues = newIdsValues;
            OnPropertyChanged("Ids");
            ObservableCollection<string> newResults = new ObservableCollection<string>();
            for (int i = 0; i < 100; i++)
            {
                newResults.Add(topologies[i].DegreeDiameterAndSecondPuntuation);
                if (notDone && topologies[i].SecondPuntuation == actualSecondPuntuation)
                    amountOfEqualFirstsSecondPuntuations++;
                else
                    notDone = false;
            }
            stateOrResultsValues = newResults;
            OnPropertyChanged("StateOrResults");
        }

        private static char randomChar()
        {
            if (Next(maxLengthValue) == 0)
                return Next(2) == 0 ? '(' : ')';
            return characters[Next(characters.Length - 1)];
        }

        private string getValidRandomId()
        {
            int length = Next(maxLengthValue - minLengthValue) + minLengthValue;
            char aux = randomChar();
            while (!numbers_array.Contains(aux))
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
