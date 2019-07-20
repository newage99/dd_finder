using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDGFinder
{
    public class GridItemClass
    {
        private string idValue = "";

        public string Id
        {
            get { return idValue; }
        }

        private int degreeValue = 0;
        private int diameterValue = 0;

        public string StateOrResult
        {
            get { return degreeValue.ToString() + "  " + diameterValue.ToString(); }
        }

        public void setId(string id)
        {
            idValue = id;
        }
        public void setDegree(int degree)
        {
            degreeValue = degree;
        }
        public void setDiameter(int diameter)
        {
            diameterValue = diameter;
        }
    }
}
