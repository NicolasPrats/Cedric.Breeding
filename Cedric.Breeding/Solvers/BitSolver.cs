using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Cedric.Breeding.Constraints;
using Cedric.Breeding.Data;
using Cedric.Breeding.Utils;

namespace Cedric.Breeding.Solvers
{
    public class BitSolver
    {
    
        // Objectif : pour chaque récessif r, obtenir toutes les plantes :
        // rXXXXX, XrXXXX, XXrXXXX, ...
        // rWWWWW, WrWWWW, WWrWWWW, ...
        

        public SetOfPlants PoolOfPlants { get; }
        private static Dictionary<Allele, Plant[]> BitsX { get; } = new Dictionary<Allele, Plant[]>();
        private static Dictionary<Allele, Plant[]> BitsW { get; } = new Dictionary<Allele, Plant[]>();
        public Allele? AlleleWithFullBits { get; set; }

        public BitSolver(SetOfPlants poolOfPlants)
        {
            this.PoolOfPlants = poolOfPlants;
        }


        public void Solve(SetOfPlants targets)
        {
            //TODO : à chaque fois qu'on a le choix entre plusieurs plantes,
            // prendre la moins chère
            foreach (var recessif in Parameters.Recessives)
            {
                int nbBits = 0;
                BitsX[recessif] = new Plant[Parameters.NbGenes];
                BitsW[recessif] = new Plant[Parameters.NbGenes];
                for (int pos = 0; pos < Parameters.NbGenes; pos++)
                {
                    var bitX = Solve(pos, recessif, Allele.X);
                    if (bitX != null)
                        BitsX[recessif][pos] = bitX;
                    var bitW = Solve(pos, recessif, Allele.W);
                    if (bitW != null)
                        BitsW[recessif][pos] = bitW;
                    if (bitX != null && bitW != null)
                    {
                        nbBits++;
                    }
                }
                foreach (var target in targets.ToList())
                {
                    var required = target.Genome.Where(g => g == recessif).Count();
                    if (required > nbBits)
                    {
                        Console.WriteLine("Retrait de la target " + target);
                        targets.Remove(target);
                    }
                }
                if (nbBits == Parameters.NbGenes)
                {
                    AlleleWithFullBits = recessif;
                }
            }
        }

        internal static Plant? SetBit(Plant plant, int i, Allele targetAllele)
        {
            var plantX = BitsX[targetAllele][i];
            var plantW = BitsW[targetAllele][i];
            if (plantX == null || plantW == null)
                return null;
            var newPlants = PlantFactory.Instance.MergePlants(plant, plant, plantX, plantW);
            var result = newPlants.Where(plant => plant[i] == targetAllele).First();
            return result;
        }

        private Plant? Solve(int pos, Allele recessif, Allele dominant)
        {
            var candidates = this.PoolOfPlants.Where(plant => plant[pos] == recessif)
                                            .OrderBy(plant => plant.Genome.Select(g => g == dominant).Count());
            foreach (var candidate in candidates)
            {
                var enhancedCandidate = EnhanceCandidate(pos, recessif, dominant, candidate);
                if (enhancedCandidate != null)
                {
                    return enhancedCandidate;
                }
            }
            return null;
        }

        private Plant? EnhanceCandidate(int pos, Allele recessif, Allele dominant, Plant candidate)
        {
            for (int i = 0; i < Parameters.NbGenes; i++)
            {
                if (i == pos || candidate[i] == dominant)
                    continue;
                // On a quelque chose comme XX??Y? (pos =4, i =2)
                // On veut obtenir XXX?Y?

                //TODO : revoir les paramètres de ces méthodes
                var newCandidate = TryMethod1(pos, recessif, dominant, candidate, i) ??
                                   TryMethod2(pos, recessif, dominant, candidate, i) ??
                                   TryMethod3(pos, recessif, dominant, candidate, i) ??
                                   TryMethod4(pos, recessif, dominant, candidate, i);
                if (newCandidate == null)
                    return null;
                candidate = newCandidate;
            }
            return candidate;
        }

        private Plant? TryMethod1(int pos, Allele recessif, Allele dominant, Plant candidate, int i)
        {
            // On a       XX??Y?
            // On cherche ??X?r? (r récessif)
            // et on combine, parmi les possibilités il y a 
            // XXX?Y?
            var otherPlant = this.PoolOfPlants.Where(plant => plant[pos].IsRecessive() && plant[i] == dominant)
                            .OrderBy(plant => plant.Cost)
                            .FirstOrDefault();
            if (otherPlant == null)
                return null;
            var newPlants = PlantFactory.Instance.MergePlants(candidate, otherPlant);
            this.PoolOfPlants.Add(newPlants);
            return SearchNewCandidate(pos, recessif, dominant, i, newPlants);
        }

        private Plant? TryMethod2(int pos, Allele recessif, Allele dominant, Plant candidate, int i)
        {
            // On a       XX??Y?
            // On cherche ??X??? (comme la méthode 1 a échoué, on sait qu'en fait on aura ??X??d?)
            // On cherche ????Y?  de tel sorte que le 3 ème gène soit différent de celui du candidat
            // et que les 2 premiers gènes soient différent de la plante précédente
            // et on combine, parmi les possibilités il y a 
            // XXX?Y?
            var firstPlants = this.PoolOfPlants.Where(plant => plant[i] == dominant).OrderBy(plant => plant.Cost);
            var secondPlants = this.PoolOfPlants.Where(plant => plant[pos] == recessif && plant[i] != candidate[i]).OrderBy(plant => plant.Cost);

            foreach (var firstPlant in firstPlants)
            {
                foreach (var secondPlant in secondPlants)
                {
                    var isPairOk = true;
                    for (var j = 0; j < i; j++)
                    {
                        if (j == pos)
                            continue;
                        if (firstPlant[j] == secondPlant[j])
                        {
                            isPairOk = false;
                            break;
                        }
                    }
                    if (!isPairOk)
                        continue;
                    var newPlants = PlantFactory.Instance.MergePlants(candidate, firstPlant, secondPlant);
                    this.PoolOfPlants.Add(newPlants);
                    return SearchNewCandidate(pos, recessif, dominant, i, newPlants);
                }
            }
            return null;
        }


        private Plant? TryMethod3(int pos, Allele recessif, Allele dominant, Plant candidate, int i)
        {
            // On a       XX??Y?
            // On cherche ??X?X? 
            // On cherche ??X?W?  
            // et on combine en prenant 2 fois le candidat, parmi les possibilités il y a 
            // XXX?Y?
            var firstPlant = this.PoolOfPlants.Where(plant => plant[i] == dominant && plant[pos] == Allele.X).OrderBy(plant => plant.Cost).FirstOrDefault();
            var secondPlant = this.PoolOfPlants.Where(plant => plant[i] == dominant && plant[pos] == Allele.Y).OrderBy(plant => plant.Cost).FirstOrDefault();
            if (firstPlant == null || secondPlant == null)
                return null;
            var newPlants = PlantFactory.Instance.MergePlants(candidate, candidate, firstPlant, secondPlant);
            this.PoolOfPlants.Add(newPlants);
            return SearchNewCandidate(pos, recessif, dominant, i, newPlants);
        }

        private Plant? TryMethod4(int pos, Allele recessif, Allele dominant, Plant candidate, int i)
        {
            // On a       XX??Y?
            // On cherche ??r?r?  (avec le 3ème gène différent de celui du candidat)
            // On combine, parmi les possibilités il y a 
            //            XXr?Y?
            // On cherche ??X???
            // On combine les 2 derniers + candidat, parmi les possibilités il y a 
            // XXX?Y?
            var intermediatePlant = this.PoolOfPlants.Where(plant => plant[i].IsRecessive() && plant[i] != candidate[i] && plant[pos].IsRecessive()).OrderBy(plant => plant.Cost).FirstOrDefault();
            if (intermediatePlant == null)
                return null;
            var newPlants = PlantFactory.Instance.MergePlants(candidate, intermediatePlant);
            this.PoolOfPlants.Add(newPlants);
            var firstPlant = newPlants.Where(plant => plant[i] == intermediatePlant[i] && plant[pos] == candidate[pos]).OrderBy(plant => plant.Cost).FirstOrDefault();
            var secondPlant = this.PoolOfPlants.Where(plant => plant[i] == dominant).FirstOrDefault();
            if (firstPlant == null || secondPlant == null)
                return null;
            newPlants = PlantFactory.Instance.MergePlants(candidate, firstPlant, secondPlant);
            this.PoolOfPlants.Add(newPlants);
            return SearchNewCandidate(pos, recessif, dominant, i, newPlants);
        }

        private static Plant SearchNewCandidate(int pos, Allele recessif, Allele dominant, int i, IEnumerable<Plant> newPlants)
        {
            Plant? newCandidate = null;
            foreach (var plant in newPlants.Where(plant => plant[pos] == recessif).OrderBy(plant => plant.Cost))
            {
                bool plantOk = true;
                for (int j = 0; j <= i; j++)
                {
                    if (j != pos && plant[j] != dominant)
                    {
                        plantOk = false;
                        break;
                    }
                }
                if (plantOk)
                {
                    newCandidate = plant;
                    break;
                }
            }
            if (newCandidate == null)
                throw new ApplicationException("bug");
            return newCandidate;
        }
    }
}
