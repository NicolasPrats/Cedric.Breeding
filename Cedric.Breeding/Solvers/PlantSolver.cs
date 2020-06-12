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

        private void Solve(Plant target, Plant fullRecessivePlant)
        {
            //On recherche dans l'ordre, mais on pourrait chercher les permutations
            //pour diminuer les coûts

            Plant? result = fullRecessivePlant;
            for (int i = 0; i < Parameters.NbGenes; i++)
            {
                if (target[i] != fullRecessivePlant[i])
                {
                    result = BitSolver.SetBit(result, i, target[i]);
                    if (result == null)
                        return;
                }
            }
            File.WriteAllText(target.Name + ".txt", result.GenerateTree());
        }

       
    }
}
