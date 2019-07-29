using System;
using Flee.PublicTypes;

namespace DDGFinder
{
    class Topology
    {
        private int size;
        private string id;
        private bool[,] matrix = null;
        public int degree = 0;
        public int diameter = 0;

        public Topology(int size, string id)
        {
            this.size = size;
            this.id = id.Replace("1", "1M").Replace("2", "2M");
            matrix = new bool[size, size];
            populate();
            calculateDD();
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

        private void calculateDD()
        {
            
        }
    }
}
