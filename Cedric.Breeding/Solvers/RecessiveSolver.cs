using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Cedric.Breeding.Data;
using Cedric.Breeding.Constraints;
using Cedric.Breeding.Utils;

namespace Cedric.Breeding.Solvers
{
    public class RecessiveSolver
    {
        public SetOfPlants PoolOfPlants { get; }

        public RecessiveSolver(SetOfPlants poolOfPlants)
        {
            this.PoolOfPlants = poolOfPlants;
        }

        public Plant Solve(Allele targetAllele)
        {
            var plants = ReplaceAllRecessiveAlleles(targetAllele);
            return SearchFullRecessivePlant(plants);
        }

        private SetOfPlants ReplaceAllRecessiveAlleles(Allele targetAllele)
        {
            SetOfPlants plantsWithOnlyTargetAllele = new SetOfPlants();
            var otherAlleles = new List<Allele>(Parameters.Recessives);
            otherAlleles.Remove(targetAllele);

            var plantsWithOtherAlleles = PoolOfPlants.ToList();
            foreach (var plant in plantsWithOtherAlleles)
            {
                Plant? newPlant = plant;
                for (var i = 0; i < Parameters.NbGenes; i++)
                {
                    if (!otherAlleles.Contains(plant[i]))
                        continue;
                    newPlant = BitSolver.SetBit(newPlant, i, targetAllele);
                    if (newPlant == null)
                        throw new ApplicationException("bug"); 
                }
                this.PoolOfPlants.Add(newPlant);
                plantsWithOnlyTargetAllele.Add(newPlant);
            }
            return plantsWithOnlyTargetAllele;
        }

        private Plant SearchFullRecessivePlant(SetOfPlants plants)
        {
            SetOfPlants recessivePlants = new SetOfPlants();
            do
            {
                var nextPlantWithPotential = plants.Except(recessivePlants)
                                .OrderByDescending(p => p.Genome.Count(g => g.IsRecessive()))
                                .ThenBy(p => p.Cost)
                                .FirstOrDefault();
                int result = nextPlantWithPotential.Genome.Count(g => g.IsDominant());
                if (result == 0)
                    return nextPlantWithPotential;
                if (nextPlantWithPotential == null)
                    throw new ApplicationException("Failure");
                for (int i = 1; i < Parameters.MaxNbPlantsInMerge; i++)
                {
                    foreach (var subSet in recessivePlants.CombinationsWithRepetition(i))
                    {
                        var list = subSet.ToList();
                        list.Add(nextPlantWithPotential);
                        var newPlants = PlantFactory.Instance.MergePlants(list);
                        PoolOfPlants.Add(newPlants);
                        plants.Add(newPlants);
                    }
                }
                recessivePlants.Add(nextPlantWithPotential);
            } while (true);
        }


    }
}
