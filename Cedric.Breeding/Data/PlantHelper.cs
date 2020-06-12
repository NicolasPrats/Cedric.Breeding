using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cedric.Breeding.Constraints;

namespace Cedric.Breeding.Data
{
    public  static class PlantHelper
    {
        public static double ComputeCost(IList<Plant>? parents, double probability)
        {
            if (parents != null)
            {
                var cost = parents.Select(p => p.Cost).Sum() + 1;
                return cost / probability;
            }
            else
            {
                return 0;
            }
        }

       
    }
}
