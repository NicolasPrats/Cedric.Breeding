using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Cedric.Breeding.Constraints;
using Cedric.Breeding.Data;
using Cedric.Breeding.Utils;
using System.IO;

namespace Cedric.Breeding.Solvers
{
    /// <summary>
    /// Nécessite que le bitsolver ait tourné
    /// A partir d'une plante donné, on peut changer n'importe lequel de ses gènes récessifs.
    /// Exemple si on a ?r????
    /// On combine les 4 plantes :
    /// ?r????
    /// ?r????
    /// XgXXXX
    /// WgWWWW
    /// Dans le lot, on obtient:
    /// ?g????
    /// </summary>
    public class PlantSolver
    {
        public SetOfPlants PoolOfPlants { get; }

        public PlantSolver(SetOfPlants poolOfPlants)
        {
            this.PoolOfPlants = poolOfPlants;
        }

        public void Solve(SetOfPlants targets, Plant fullRecessivePlant)
        {
            foreach (var target in targets)
            {
                Solve(target, fullRecessivePlant);
            }
        }

        private IList<int[]> PossibleOrders = (new int[] { 0, 1, 2, 3, 4, 5 }).Permutations().ToList();
        private void Solve(Plant target, Plant fullRecessivePlant)
        {
            var evaluatedVariants = new HashSet<Plant>();
            Allele[] genome = target.Genome.ToArray();
            Plant? bestVariant = null;
            Allele[] genomeVariant = new Allele[Parameters.NbGenes];
            foreach (var order in PossibleOrders)
            {
                for (int i = 0; i < Parameters.NbGenes; i++)
                {
                    genomeVariant[i] = genome[order[i]];
                }
                var plantVariant = PlantFactory.Instance.GetPlant(genomeVariant, double.MaxValue);
                if (evaluatedVariants.Contains(plantVariant))
                    continue;
                evaluatedVariants.Add(plantVariant);
                if (SolveRespectingOrder(plantVariant, fullRecessivePlant))
                {
                    if (bestVariant == null || bestVariant.Cost > plantVariant.Cost)
                    {
                        bestVariant = plantVariant;
                    }
                }
            }
            if (bestVariant == null)
            {
                File.WriteAllText(target.Name + ".txt", "Not found");
            }
            else
            {
                File.WriteAllText(target.Name + ".txt", bestVariant.GenerateTree());
            }
        }

        private bool SolveRespectingOrder(Plant target, Plant fullRecessivePlant)
        {
            Plant? result = fullRecessivePlant;
            for (int i = 0; i < Parameters.NbGenes; i++)
            {
                if (target[i] != fullRecessivePlant[i])
                {
                    result = BitSolver.SetBit(result, i, target[i]);
                    if (result == null)
                        return false;
                }
            }
            return true;
        }

       
    }
}
