using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cedric.Breeding.Constraints;
using Cedric.Breeding.Data;
using Cedric.Breeding.Utils;

namespace Cedric.Breeding.Solvers
{
    /// <summary>
    /// Objectif, on veut que pour chaque position (P1 et P2, P1 < P2), on ait 2 plantes telles que
    ///   * A P2 on ait un allèle récessif commun
    ///   * A P1 on ait :
    ///             1) soit 2 allèles récessifs différents (Paire de type 1)
    ///             2) soit 2 allèles dominants différents (Paire de type 2)
    ///  On va donc vérifier pour chaque position on trouve des plantes de type 1
    ///  Sinon on va essayer de construire des plantes de type 2: (si on n'y arrive pas, l'algo échoue. On continue quand même en espérant qu'on n'ait pas besoin de ce cas de figure plus tard)
    ///     Si il nous manque par exemple ?X??R?
    ///     On cherche les plantes ?X??X?, ?X??W? et ????R?. 
    ///     On les combine en utilisant deux fois la dernière. Dans le lot, on est assuré d'avoir ?X??R? (au pire 1 chance sur 2)
    /// </summary>
    class DominantsSolver
    {
        public SetOfPlants PoolOfPlants { get; }
        

        public DominantsSolver(SetOfPlants poolOfPlants)
        {
            this.PoolOfPlants = poolOfPlants;
        }

        public void Solve()
        {
            for (int p1 = 0; p1 < Parameters.NbGenes; p1++)
            {
                for (int p2 = p1 + 2; p2 < Parameters.NbGenes; p2++)
                {
                    if (PairOfType1Exists(p1, p2))
                    {
                        continue;
                    }
                    CreatePairOfType2(p1, p2);
                }
            }
        }       

        private bool PairOfType1Exists(int p1, int p2)
        {
            var groupOfPlants = this.PoolOfPlants.Where(plant => Parameters.Recessives.Contains(plant[p1]) && Parameters.Recessives.Contains(plant[p2]))
                                .GroupBy(plant => plant.Genome[p2])
                                .Where(g => g.Count() > 1);
            foreach (var group in groupOfPlants)
            {
                foreach (var pair in group.Combinations(2))
                {
                    var list = pair.ToList();
                    var plant1 = list[0];
                    var plant2 = list[1];
                    if (plant1[p1] != plant2[p1])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void CreatePairOfType2(int p1, int p2)
        {
            // perte de généricité , car on prend en dur les 2 allèles dominants            
            CreatePlantType2(p1, p2, Allele.X);
            CreatePlantType2(p1, p2, Allele.W);
        }

        private void CreatePlantType2(int p1, int p2, Allele dominant)
        {
            // ?D??R?
            // ?D??X?
            // ?D??W?
            if (this.PoolOfPlants.Any(plant => plant[p1] == dominant && Parameters.Recessives.Contains(plant[p2])))
            {
                // La plante existe déjà
                return;
            }
            var recessivePlant = this.PoolOfPlants.FirstOrDefault(plant => Parameters.Recessives.Contains(plant[p2]));
            if (recessivePlant == null)
            {
                throw new ApplicationException("Pas de solution: aucun gène récessif en position " + p2);
            }
            var xPlant = this.PoolOfPlants.FirstOrDefault(plant => plant[p1] == dominant && plant[p2] == Allele.X);
            var yPlant = this.PoolOfPlants.FirstOrDefault(plant => plant[p1] == dominant && plant[p2] == Allele.Y);
            if (xPlant == null || yPlant == null)
            {
                //Echec de l'algo.
                // Ca ne veut pas dire qu'il n'y a pas de solution, juste que l'algo ne marche pas
                // On croise les doigts pour ne pas avoir besoin du couple (P1, P2) plus tard
                Console.WriteLine($"({p1},{p2}) KO");
                return;
            }
            var plants = PlantFactory.Instance.MergePlants(xPlant, yPlant, recessivePlant, recessivePlant);
            this.PoolOfPlants.Add(plants);
            if (!this.PoolOfPlants.Any(plant => plant[p1] == dominant && Parameters.Recessives.Contains(plant[p2])))
            {
                throw new ApplicationException("bug");
            }
        }

        private void Solve(Allele dominant, int position)
        {
            //On cherche à trouver des plantes avec des dominants à chque gene sauf sur le gene "position" où on veut un récessit
            Plant[] plants = new Plant[Parameters.NbGenes];
            for (int i = 0; i <= position; i++)
            {
                if (i != position)
                {
                    var plant = this.PoolOfPlants.FirstOrDefault(p => p.Genome[i] == dominant && Parameters.Recessives.Contains(p.Genome[position]));
                    if (plant == null)
                    {
                        var plantWithDominant = this.PoolOfPlants.FirstOrDefault(p => p.Genome[i] == dominant);
                        var plantsWithRecessive = this.PoolOfPlants.Where(p => Parameters.Recessives.Contains(p.Genome[position]));
                        foreach (var pair in plantsWithRecessive.CombinationsWithRepetition(2))
                        {
                            var plant1 = pair.First();
                            var plant2 = pair.Skip(1).First();
                            if (plant1.Genome[position] != plant2.Genome[position])
                                continue;
                            if (plant1.Genome[i] == plant2.Genome[i])
                                continue;
                            var newPlants = PlantFactory.Instance.MergePlants(plant1, plant2, plantWithDominant);
                            this.PoolOfPlants.Add(newPlants);
                             plant = this.PoolOfPlants.First(p => p.Genome[i] == dominant && Parameters.Recessives.Contains(p.Genome[position]));

                        }
                        if (plant ==null)
                            throw new ApplicationException("This algorithm cannot be applied in this situation!"); 
                        //A priori pas grave, on pourrait mettre un recessif à la place du dominant
                    }
                    plants[i] = plant;
                }
                else
                {
                    var plant = this.PoolOfPlants.FirstOrDefault(p => Parameters.Recessives.Contains(p.Genome[position]));
                    if (plant == null)
                        throw new ApplicationException("No solution can be found");
                    plants[i] = plant;
                }
            }
        }
    }
}
