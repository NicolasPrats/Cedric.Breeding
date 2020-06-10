using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cedric.Breeding
{
    public class PlantFactory
    {
        private static readonly Random Rng = new Random();

        public static PlantFactory Instance { get; } = new PlantFactory();
        private Plant? TargetPlant { get; set; }
        private Dictionary<Allele, int>? TargetOccurences { get; set; }

        private PlantFactory()
        {
        }

        public void DefineTarget(Plant targetPlant)
        {
            if (this.TargetPlant != null)
                throw new ApplicationException("Target already defined");
            this.TargetPlant = targetPlant;
            Dictionary<Allele, int> occurences = CountOccurencesOfAlleles(targetPlant.Genes);
            TargetOccurences = occurences;
        }

        private static Dictionary<Allele, int> CountOccurencesOfAlleles(Allele[] genes)
        {
            var occurences = new Dictionary<Allele, int>();
            foreach (var allele in genes)
            {
                occurences.TryGetValue(allele, out int count);
                count++;
                occurences[allele] = count;
            }

            return occurences;
        }

        public Plant GetRandomPlant()
        {
            var values = Enum.GetValues(typeof(Allele));
            var genes = new Allele[Parameters.NbGenes];
            for (int i = 0; i < genes.Length; i++)
            {
                var alleleIndex = Rng.Next(0, values.Length);
                var value = values.GetValue(alleleIndex);
                if (value == null)
                    throw new ApplicationException("Unexpected null value!");
                genes[i] = (Allele)(int)value;
            }
            if (TargetPlant == null)
            {
                return new Plant(genes);
            }
            else
            {
                return new Plant(genes, CalculateDistanceToTarget(genes));
            }
        }

        private int CalculateDistanceToTarget(Allele[] genesToEvaluate)
        {
            if (TargetOccurences == null)
                throw new ApplicationException("Target must be initialized");
            var occurencesToEvaluate = CountOccurencesOfAlleles(genesToEvaluate);
            int distance = 0;
            foreach (var kvp in TargetOccurences)
            {
                var targetAllele = kvp.Key;
                var targetCount = kvp.Value;
                occurencesToEvaluate.TryGetValue(targetAllele, out int count);
                if (count < targetCount)
                {
                    distance += targetCount - count;
                }
            }
            //TODO : si les genes manquants sont récessifs et que les genes en trop sont dominants,
            // la distance peut être augmentée
            return distance;

        }

        internal Plant MergePlants(IEnumerable<Plant> subSet)
        {
            if (TargetPlant == null)
                throw new ApplicationException("Target must be defined before merging plants");
            List<Allele>[] pool = new List<Allele>[Parameters.NbGenes];
            for (var i = 0; i < pool.Length; i++)
            {
                pool[i] = new List<Allele>();
            }
            foreach (var plant in subSet)
            {
                for (var i = 0; i < plant.Genes.Length; i++)
                {
                    pool[i].Add(plant.Genes[i]);
                }
            }
            Allele[] genes = new Allele[Parameters.NbGenes];
            for (var i = 0; i < genes.Length; i++)
            {
                var alleles = pool[i].GroupBy(a => a).OrderByDescending(g => g.Count());
                var count = alleles.First().Count();
                var mostPresentAlleles = alleles.Where(g => g.Count() == count).Select(g => g.Key);

                var dominantPresentAlleles = mostPresentAlleles.Intersect(Parameters.Dominants);
                if (dominantPresentAlleles.Any())
                {
                    mostPresentAlleles = dominantPresentAlleles;
                }
                var index = Rng.Next(0, mostPresentAlleles.Count());
                genes[i] = mostPresentAlleles.ElementAt(index);
            }
            return new Plant(genes, CalculateDistanceToTarget(genes));
        }
    }
}
