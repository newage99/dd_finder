using System;
using Flee.PublicTypes;
using System.Collections.Generic;

namespace DDGFinder
{
    class Topology
    {
        private int size;
        private bool[,] matrix = null;
        public string id;
        private string idToCompile;
        public int degree = 0;
        public int diameter = 0;
        public int disconnected_counter = 0;

        public Topology(int size)
        {
            this.size = size;
            matrix = new bool[size, size];
        }

        public void init()
        {
            disconnected_counter = 0;
        }

        public void setIdAndPopulate(string id)
        {
            this.id = id;
            idToCompile = id.Replace("1", "1M").Replace("2", "2M");
            populate();
        }

        private List<Operation> createOperationsList()
        {
            List<Operation> operations = new List<Operation>();
            for(int i = 0; i < id.Length; i++)
            {

            }
            return operations;
        }

        private void populate()
        {
            if (matrix == null)
                return;
            /*ExpressionContext context = new ExpressionContext();
            context.Imports.AddType(typeof(Math));
            context.Variables["n"] = (decimal)size;
            long d0 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;*/
            for (int x = 0; x < size; x++)
            {
                for(int y = 0; y < size; y++)
                {
                    /*context.Variables["x"] = (decimal)x;
                    context.Variables["y"] = (decimal)y;
                    try
                    {
                        IGenericExpression<decimal> eGeneric = context.CompileGeneric<decimal>("Round(" + idToCompile + ")");
                        decimal resultDecimal = eGeneric.Evaluate();
                        int result = (int)resultDecimal;
                        if (result >= 0 && result < size && result != x && !matrix[x, result])
                        {
                            matrix[x, result] = true;
                            matrix[result, x] = true;
                        }
                    } catch (Exception e)
                    {
                        int a = 0;
                    }*/
                }
            }
            /*long d1 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Console.WriteLine("2: " + (d1 - d0).ToString());*/
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
            return visited.Count != size;
        }

        public void calculateDD()
        {
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
            for (int i = 0; i < size - 1; i++)
            {
                for (int j = i+1; j < size; j++)
                {
                    List<int> toVisitNodes = new List<int>();
                    toVisitNodes.Add(i);
                    int toVisitPos = 0;
                    int actualDiameter = 0;
                    bool notFounded = true;
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
                }
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
