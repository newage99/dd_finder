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
        public int degree = 0;
        public int diameter = 0;

        public Topology(int size)
        {
            this.size = size;
            matrix = new bool[size, size];
        }

        public void init(string id)
        {
            this.id = id.Replace("1", "1M").Replace("2", "2M");
            populate();
        }

        private void populate()
        {
            if (matrix == null)
                return;
            ExpressionContext context = new ExpressionContext();
            context.Imports.AddType(typeof(Math));
            context.Variables["n"] = (decimal)size;
            for(int x = 0; x < size; x++)
            {
                for(int y = 0; y < size; y++)
                {
                    context.Variables["x"] = (decimal)x;
                    context.Variables["y"] = (decimal)y;
                    try
                    {
                        IGenericExpression<decimal> eGeneric = context.CompileGeneric<decimal>("Round(" + id + ")");
                        decimal resultDecimal = eGeneric.Evaluate();
                        int result = (int)resultDecimal;
                        if (result >= 0 && result < size && result != x && !matrix[x, result])
                        {
                            matrix[x, result] = true;
                            matrix[result, x] = true;
                        }
                    } catch (Exception e)
                    {
                        Console.WriteLine("populate (id=" + id + ", x=" + x.ToString() + ", y=" + y.ToString() + "): " + e.ToString());
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
            return visited.Count != size;
        }

        public void calculateDD()
        {
            // TODO: Calculate diameter
            int actualDegree;
            for (int i = 0; i < size; i++)
            {
                actualDegree = 0;
                for(int j = 0; j < size; j++)
                {
                    if (matrix[i, j])
                    {
                        actualDegree += 1;
                    }
                }
                if (actualDegree > degree)
                    degree = actualDegree;
            }
        }
    }
}
