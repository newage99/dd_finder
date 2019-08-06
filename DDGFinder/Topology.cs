using System;
using Flee.PublicTypes;
using System.Collections.Generic;

namespace DDGFinder
{
    class Topology
    {
        private int size;
        private bool[,] matrix = null;
        private string id;
        private string idToCompile;
        public int degree = 0;
        public int diameter = 0;
        public int second_puntuation = 0;
        public int disconnected_counter = 0;
        public bool correctly_populated = false;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        public int Puntuation
        {
            get { return degree + diameter; }
        }

        public int SecondPuntuation
        {
            get { return second_puntuation; }
        }

        public string DisconnectedCounterDegreeDiameterAndSecondPuntuation
        {
            get { return disconnected_counter.ToString() + ": " + degree.ToString() + " " + diameter.ToString() + " " + SecondPuntuation.ToString(); }
        }

        public Topology(int size)
        {
            this.size = size;
            matrix = new bool[size, size];
        }

        public void setIdAndPopulate(string id)
        {
            if (id == null || id.Length <= 0)
            {
                int a = 0;
            }
            disconnected_counter = 0;
            correctly_populated = false;
            this.id = id;
            populate();
        }

        private void clear()
        {
            for(int i = 0; i < size; i++)
            {
                for(int j = 0; j < size; j++)
                {
                    matrix[i, j] = false;
                }
            }
        }

        private void populate()
        {
            if (matrix == null)
                return;
            clear();
            ExpressionInterpreter.Result result;
            for (int x = 0; x < size; x++)
            {
                for(int y = 0; y < size; y++)
                {
                    try
                    {
                        result = ExpressionInterpreter.Result.OK;
                        string input = id.Replace("x", x.ToString()).Replace("y", y.ToString())
                            .Replace("n", size.ToString()).Replace("*-", "*0-").Replace("(-", "(0-");
                        if (input.StartsWith("-"))
                            input = "0" + input;
                        bool connect = new ExpressionInterpreter().Compute(input, out result);
                        if (result == ExpressionInterpreter.Result.OK && connect)
                        {
                            matrix[x, y] = true;
                            matrix[y, x] = true;
                        }
                    } catch (Exception e)
                    {
                        Console.WriteLine(id + ": " + e.ToString());
                    }
                }
            }
        }

        public bool isDisconnected()
        {
            List<int> visited = new List<int>();
            visited.Add(0);
            int pos = 0;
            int toVisit;
            while(pos < visited.Count && visited.Count < size)
            {
                toVisit = visited[pos];
                for (int i = 0; i < size; i++)
                {
                    if (i != toVisit && matrix[toVisit, i])
                    {
                        bool notVisited = true;
                        for(int j = 0; j < visited.Count; j++)
                        {
                            if(visited[j] == i)
                            {
                                notVisited = false;
                                break;
                            }
                        }
                        if (notVisited)
                            visited.Add(i);
                    }
                }
                pos += 1;
            }
            disconnected_counter += 1;
            correctly_populated = visited.Count == size;
            return !correctly_populated;
        }

        public void calculateDD()
        {
            degree = 0;
            diameter = 0;
            second_puntuation = 0;
            int actualDegree;
            for (int i = 0; i < size; i++)
            {
                actualDegree = 0;
                for (int j = 0; j < size; j++)
                {
                    if (matrix[i, j])
                        actualDegree += 1;
                }
                if (actualDegree > degree)
                    degree = actualDegree;
            }
            for (int i = 0; i < size; i++)
            {
                actualDegree = 0;
                for (int j = 0; j < size; j++)
                {
                    if (matrix[i, j])
                        actualDegree += 1;
                    if (i < size - 1 && j > i)
                    {
                        List<int> toVisitNodes = new List<int>();
                        toVisitNodes.Add(i);
                        int toVisitPos = 0;
                        int actualDiameter = 0;
                        bool notFounded = true;
                        try
                        {
                            while (toVisitNodes.Count < size && notFounded)
                            {
                                int toVisitNodesCount = toVisitNodes.Count;
                                for (int x = toVisitPos; x < toVisitNodesCount && toVisitNodes.Count < size && notFounded; x++)
                                {
                                    int toVisit = toVisitNodes[toVisitPos];
                                    for (int y = 0; y < size && notFounded; y++)
                                    {
                                        if (matrix[toVisit, y])
                                        {
                                            if (y == j)
                                                notFounded = false;
                                            if (listNotContainsValue(toVisitNodes, y))
                                                toVisitNodes.Add(y);
                                        }
                                    }
                                    toVisitPos += 1;
                                }
                                actualDiameter += 1;
                            }
                            if (actualDiameter > diameter)
                                diameter = actualDiameter;
                            second_puntuation += actualDiameter;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("calculateDD: " + e.ToString());
                        }
                    }
                }
                second_puntuation += actualDegree * (size - i);
            }
        }

        private bool listNotContainsValue(List<int> list, int value)
        {
            for(int i = 0; i < list.Count; i++)
            {
                if (list[i] == value)
                    return false;
            }
            return true;
        }
    }
}
