using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cedric.Breeding
{
    class Program
    {

        static void Main()
        {
            //var targetPlant = PlantFactory.Instance.GetRandomPlant();
            //Attention la fonction score est désormais codée en dur vis à vis de cette plante
            var targetPlant = new Plant(new Allele[] { Allele.Y, Allele.Y, Allele.Y, Allele.Y, Allele.G, Allele.G });

            var analyzedPlants = new SetOfPlants();
            for (int i = 0; i < Parameters.NbStartingPlants; i++)
            {
                Plant plant;
                do
                {
                    plant = PlantFactory.Instance.GetRandomPlant();
                } while (plant.IsSimilar(targetPlant));
                analyzedPlants.Add(plant);
            }

            Plant? bestPlantFound = null;
            double bestCostFound = int.MaxValue;
            var poolOfPlantsToAnalyze = GenerateNewPlants(analyzedPlants);
            var nextPlantsToAnalyze = TakeSomePlantsToAnalyze(analyzedPlants, poolOfPlantsToAnalyze);
            while (nextPlantsToAnalyze.Count() != 0)
            {
                bool newBestPlantFound = false;
                if (bestPlantFound != null && bestPlantFound.ComputeCost() < bestCostFound)
                {
                    //Ce cas est faisable si on a amélioré les parents de bestplant
                    bestCostFound = bestPlantFound.ComputeCost();
                    newBestPlantFound = true;
                }
                //TODO on recherche plusieurs fois dans poolofplantstoanalyze
                foreach (var plant in nextPlantsToAnalyze.Union(poolOfPlantsToAnalyze))
                {
                    if (plant.IsSimilar(targetPlant))
                    {

                        if (bestPlantFound == null || plant.ComputeCost() < bestCostFound)
                        {
                            Console.WriteLine("Nouvelle meilleure plante trouvée pour correspondre à la cible: " + targetPlant);
                            Console.WriteLine(plant.GenerateTree());

                            bestPlantFound = plant;
                            bestCostFound = plant.ComputeCost();
                            newBestPlantFound = true;
                        }
                    }
                }
                if (newBestPlantFound)
                {
                    nextPlantsToAnalyze.SetMaximumCost(bestCostFound);
                    analyzedPlants.SetMaximumCost(bestCostFound);
                    poolOfPlantsToAnalyze.SetMaximumCost(bestCostFound);
                }


                Console.WriteLine("Plantes analysées : " + analyzedPlants.Count());
                Console.WriteLine("Plantes à analyser : " + nextPlantsToAnalyze.Count());
                Console.WriteLine("Pool de plantes : " + poolOfPlantsToAnalyze.Count());

                poolOfPlantsToAnalyze.UnionWith(GenerateNewPlants(analyzedPlants, nextPlantsToAnalyze));
                analyzedPlants.UnionWith(nextPlantsToAnalyze);
                nextPlantsToAnalyze = TakeSomePlantsToAnalyze(analyzedPlants, poolOfPlantsToAnalyze);
            }
            Console.WriteLine();
            Console.WriteLine("Plus de nouvelles plantes!");

            Console.WriteLine("Meilleure plante=" + bestPlantFound);
            Console.WriteLine("Cout=" + bestCostFound);
            Console.WriteLine(bestPlantFound?.GenerateTree());
        }

        private static SetOfPlants TakeSomePlantsToAnalyze(SetOfPlants analyzedPlants, SetOfPlants poolOfPlantsToAnalyze)
        {
            var availablePlants = poolOfPlantsToAnalyze.OrderBy(p => CalculateScore(p)).ToList();
            SetOfPlants nextPlantsToAnalyze = new SetOfPlants();
            while (poolOfPlantsToAnalyze.Count() > 0 && nextPlantsToAnalyze.Count < Parameters.BatchSize)
            {
                var plant = poolOfPlantsToAnalyze.Last();
                poolOfPlantsToAnalyze.Remove(plant);
                availablePlants.RemoveAt(availablePlants.Count - 1);
                if (!analyzedPlants.Contains(plant))
                {
                    nextPlantsToAnalyze.Add(plant);
                }
            }
            return nextPlantsToAnalyze;
        }

        private static int CalculateScore(Plant plant)
        {
            //TODO ne pas recalculer le score pour chaque plante
            int score = 0;
            int nbG = 0;
            int nbY = 0;
            int nbDominants = 0;
            foreach (var gene in plant.Genome)
            {
                if (gene == Allele.G)
                {
                    nbG++;
                }
                else if (gene == Allele.Y)
                {
                    nbY++;
                }
                else if (Parameters.Dominants.Any(d => d == gene))
                {
                    nbDominants++;
                }
            }
            score = Math.Min(nbG, 4) + Math.Min(nbY, 2) - nbDominants;
            return score * 10 - (int) plant.ComputeCost();
        }

        private static SetOfPlants GenerateNewPlants(SetOfPlants previousGenerationPlants, SetOfPlants currentGenerationPlants)
        {
            var discoveredPlants = new SetOfPlants();
            for (var nbOfPlantsToMerge = 2; nbOfPlantsToMerge <= Parameters.MaxNbPlantsInMerge; nbOfPlantsToMerge++)
            {
                for (var nbOfPlantsInCurrentGen = 1; nbOfPlantsInCurrentGen <= nbOfPlantsToMerge; nbOfPlantsInCurrentGen++)
                {
                    var plants = GenerateNewPlants(previousGenerationPlants, currentGenerationPlants, nbOfPlantsToMerge - nbOfPlantsInCurrentGen, nbOfPlantsInCurrentGen);
                    discoveredPlants.UnionWith(plants);
                }
            }
            return discoveredPlants;
        }

        private static SetOfPlants GenerateNewPlants(SetOfPlants previousGenerationPlants, SetOfPlants currentGenerationPlants, int nbOfPreviousGenPlants, int nbOfCurrentGenPlants)
        {
            ConcurrentBag<Plant> bag = new ConcurrentBag<Plant>();
            //foreach (var subSetCurrentGen in currentGenerationPlants.Combinations(nbOfCurrentGenPlants))
            Parallel.ForEach(currentGenerationPlants.Combinations(nbOfCurrentGenPlants), subSetCurrentGen => 
            {
                //foreach (var subSetAvailable in previousGenerationPlants.Combinations(nbOfPreviousGenPlants))
                Parallel.ForEach(previousGenerationPlants.Combinations(nbOfPreviousGenPlants), subSetAvailable => 
                {
                    var subSet = subSetAvailable.Union(subSetCurrentGen);
                    var newPlants = PlantFactory.Instance.MergePlants(subSet);
                    foreach (var plant in newPlants)
                    {
                        bag.Add(plant);
                    }
                });
            });
            SetOfPlants discoveredPlants = new SetOfPlants();
            discoveredPlants.UnionWith(bag);
            return discoveredPlants;
        }


        private static SetOfPlants GenerateNewPlants(SetOfPlants availablePlants)
        {
            var discoveredPlants = new SetOfPlants();
            for (int nbPlants = 2; nbPlants <= Parameters.MaxNbPlantsInMerge; nbPlants++)
            {
                foreach (var subSet in availablePlants.Combinations(nbPlants))
                {
                    var newPlants = PlantFactory.Instance.MergePlants(subSet);
                    discoveredPlants.UnionWith(newPlants);
                }
            }
            return discoveredPlants;
        }


    }
}
