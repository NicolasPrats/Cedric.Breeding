using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Cedric.Breeding.Data;
using Cedric.Breeding.Utils;
using Cedric.Breeding.Constraints;

namespace Cedric.Breeding.Solvers
{
    public class BruteForceSolver
    {
        public SetOfPlants PoolOfPlants { get; }

        public BruteForceSolver(SetOfPlants poolOfPlants)
        {
            this.PoolOfPlants = poolOfPlants;
        }

        public void Solve(int nbRounds)
        {
            var alreadyAnalyzedPlants = new SetOfPlants();
            var plantsToAnalyze = new SetOfPlants();
            var newPlants = new SetOfPlants();

            plantsToAnalyze.Add(this.PoolOfPlants);
            do
            {
                nbRounds--;
                for (int nbPlants = 2; nbPlants < Parameters.MaxNbPlantsInMerge; nbPlants++)
                {
                    for (int nbNewPlants = 1; nbNewPlants <= nbPlants; nbNewPlants++)
                    {
                        int nbOldPlants = nbPlants - nbNewPlants;
                        foreach (var newCombinations in plantsToAnalyze.Combinations(nbNewPlants))
                        {
                            foreach (var oldCombinations in alreadyAnalyzedPlants.Combinations(nbOldPlants))
                            {
                                var subSet = newCombinations.Union(oldCombinations);
                                var result = PlantFactory.Instance.MergePlants(subSet)
                                    .Except(this.PoolOfPlants);
                                newPlants.Add(result);
                                this.PoolOfPlants.Add(result);
                            }
                        }
                    }
                }
                alreadyAnalyzedPlants.Add(plantsToAnalyze);
                var extract = newPlants.OrderBy(p=> p.Cost).Take(20);
                plantsToAnalyze.Clear();
                plantsToAnalyze.Add(extract);
                foreach (var plant in extract)
                {
                    newPlants.Remove(plant);
                }
            } while (nbRounds > 0);
        }
    }
}
