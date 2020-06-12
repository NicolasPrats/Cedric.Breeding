using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cedric.Breeding.Constraints;
using Cedric.Breeding.Data;

namespace Cedric.Breeding.Solvers
{
   public class RecessiveSolver
    {
        public SetOfPlants PoolOfPlants { get; }

        public RecessiveSolver(SetOfPlants poolOfPlants)
        {
            this.PoolOfPlants = poolOfPlants;
        }

        public void Solve()
        {
            for (int level = 1; level <= Parameters.NbGenes; level++)
            {
                EnsureRecessivePlantExistsAtLevel(level);
            }
        }

        /* XXXXrX */

        private void EnsureRecessivePlantExistsAtLevel(int level)
        {
            // Plante récessive de level X = plante dont le genome commence par au moins X alleles recessifs
            //if (this.PoolOfPlants.Any(p => IsRecessiveUntilLevel(p, level)))
            //    return;
            //if (level == 1)
            //{
            //    throw new ApplicationException("No solution exist");
            //}
            /* abcdefg sont tous récessifs
             * On cherche 2 plantes : abcX?? et defW??
             * on fusionne, dans le lot il y a abcW.
             * On cherche une ???g??. On fusionne les 3 plantes
             * abcX
             * abcW
             * ???g
             * ???g
             * 
             * yyyX
             * yyyW
             * 
             * ???y
             * */
        }

        private static bool IsRecessiveUntilLevel(Plant plant, int level)
        {
            for (int i = 0; i < level; i++)
            {
                if (Parameters.Dominants.Contains(plant.Genome[i]))
                    return false;
            }
            return true;
        }

    }
}
