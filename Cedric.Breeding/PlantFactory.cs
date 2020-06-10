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

        private PlantFactory()
        {
        }


        public Plant GetRandomPlant()
        {
            var values = Enum.GetValues(typeof(Allele));
            var genome = new Allele[Parameters.NbGenes];
            for (int i = 0; i < genome.Length; i++)
            {
                var alleleIndex = Rng.Next(0, values.Length);
                var value = values.GetValue(alleleIndex);
                if (value == null)
                    throw new ApplicationException("Unexpected null value!");
                genome[i] = (Allele)(int)value;
            }

            return new Plant(genome);

        }

        internal IEnumerable<Plant> MergePlants(IEnumerable<Plant> subSet)
        {
            List<Allele>[] pool = GetPoolOfAllelles(subSet);
            KeepPredominantAlleles(pool);
            return GeneratePlants(subSet, pool);
        }

        private IEnumerable<Plant> GeneratePlants(IEnumerable<Plant> parents, List<Allele>[] pool)
        {
            List<Plant> plants = new List<Plant>();
            var genomes = GenerateGenomes(parents, pool, 0);
            foreach (var result in genomes)
            {
                var plant = new Plant(result.Genome, parents.ToList(), result.Probability);
                plants.Add(plant);
            }
            return plants;
        }

        private List<(Allele[] Genome, double Probability)> GenerateGenomes(IEnumerable<Plant> parents, List<Allele>[] pool, int index)
        {
            var genomes = new List<(Allele[] genome, double probability)>();
            var probability = 1.0 / pool[index].Count;
            if (index == Parameters.NbGenes - 1)
            {
                foreach (var allele in pool[index])
                {
                    Allele[] genome = new Allele[Parameters.NbGenes];
                    genome[index] = allele;
                    genomes.Add((genome, probability));
                }
                return genomes;
            }
            var nextGenomes = GenerateGenomes(parents, pool, index + 1);
            foreach (var allele in pool[index])
            {
                foreach (var next in nextGenomes)
                {
                    Allele[] genome = (Allele[])next.Genome.Clone();
                    genome[index] = allele;
                    genomes.Add((genome, next.Probability * probability));
                }
            }
            return genomes;
        }

        private static void KeepPredominantAlleles(List<Allele>[] pool)
        {
            for (var i = 0; i < Parameters.NbGenes; i++)
            {
                var alleles = pool[i].GroupBy(a => a).OrderByDescending(g => g.Count());
                var count = alleles.First().Count();
                var mostPresentAlleles = alleles.Where(g => g.Count() == count).Select(g => g.Key);

                var dominantPresentAlleles = mostPresentAlleles.Intersect(Parameters.Dominants);
                if (dominantPresentAlleles.Any())
                {
                    mostPresentAlleles = dominantPresentAlleles;
                }
                pool[i] = mostPresentAlleles.ToList();
            }
        }

        private static List<Allele>[] GetPoolOfAllelles(IEnumerable<Plant> subSet)
        {
            List<Allele>[] pool = new List<Allele>[Parameters.NbGenes];
            for (var i = 0; i < pool.Length; i++)
            {
                pool[i] = new List<Allele>();
            }
            foreach (var plant in subSet)
            {
                for (var i = 0; i < Parameters.NbGenes; i++)
                {
                    pool[i].Add(plant.Genome[i]);
                }
            }

            return pool;
        }
    }
}
