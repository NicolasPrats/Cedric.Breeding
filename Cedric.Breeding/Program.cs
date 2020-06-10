using System;
using System.Collections.Generic;
using System.Linq;

namespace Cedric.Breeding
{
    class Program
    {

        static void Main()
        {
            //var targetPlant = PlantFactory.Instance.GetRandomPlant();
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
                Console.WriteLine("Plantes analysées : " + analyzedPlants.Count());
                Console.WriteLine("Plantes à analyser : " + nextPlantsToAnalyze.Count());
                Console.WriteLine("Pool de plantes : " + poolOfPlantsToAnalyze.Count());
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
                            Console.WriteLine("Target=" + targetPlant);
                            Console.WriteLine("Plant=" + plant);
                            Console.WriteLine("Cout=" + plant.ComputeCost());

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
                    Console.WriteLine("Nouveau décompte suite à meilleure plante trouvée: ");
                    Console.WriteLine("Plantes analysées : " + analyzedPlants.Count());
                    Console.WriteLine("Plantes à analyser : " + nextPlantsToAnalyze.Count());
                    Console.WriteLine("Pool de plantes : " + poolOfPlantsToAnalyze.Count());
                }


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
            SetOfPlants nextPlantsToAnalyze = new SetOfPlants();
            //TODO : trier par distance ?
            while (poolOfPlantsToAnalyze.Count() > 0 && nextPlantsToAnalyze.Count < Parameters.BatchSize)
            {
                var plant = poolOfPlantsToAnalyze.First();
                poolOfPlantsToAnalyze.Remove(plant);
                if (!analyzedPlants.Contains(plant))
                {
                    nextPlantsToAnalyze.Add(plant);
                }
            }

            return nextPlantsToAnalyze;
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
            SetOfPlants discoveredPlants = new SetOfPlants();
            foreach (var subSetCurrentGen in currentGenerationPlants.Combinations(nbOfCurrentGenPlants))
            {
                foreach (var subSetAvailable in previousGenerationPlants.Combinations(nbOfPreviousGenPlants))
                {
                    var subSet = subSetAvailable.Union(subSetCurrentGen);
                    var newPlants = PlantFactory.Instance.MergePlants(subSet);
                    discoveredPlants.UnionWith(newPlants);
                }
            }
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
