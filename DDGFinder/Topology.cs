using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDGFinder
{
    class Topology
    {
        bool[,] matrix = null;

        public Topology(int size, string id)
        {
            matrix = new bool[size, size];
            populate(id);
        }

        public void populate(string id)
        {

        }
    }
}
