using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Cedric.Breeding
{
    class Program
    {
        static string[] Sample = new string[] {
"YHGWGX",
"WHWXYH",
"XYWWGX",
"WGYGXX",
"YYYWYH",
"XXYWYW",
"WHXWHY",
"WGHWHH",
"YYYWYW",
"WGYWHW",
"XGGWGH",
"WYYXYX",
"XGHHGW",
"XYYWGH",
"WHYWHW",
"WYYWGG",
"YYWYHX",
"WXYXGH",
"XHHWWX",
"XWYXGX",
"XGHXGW",
"HHXWHX"
};
        static void Main()
        {
            //var targetPlant = PlantFactory.Instance.GetRandomPlant();
            var targetPlants = new Plant[] {
                new Plant(new Allele[] { Allele.Y, Allele.Y, Allele.Y, Allele.Y, Allele.G, Allele.G })
             };


            var analyzedPlants = new SetOfPlants();

            foreach (var genome in Sample)
            {
                Plant item = PlantFactory.Instance.ParsePlant(genome);
                if (CalculateScore(item) != int.MinValue)
                {
                    analyzedPlants.Add(item);
                }
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
                    var targetPlant = plant.IsSimilarToAny(targetPlants);
                    if (targetPlant != null)
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
                if (!analyzedPlants.Contains(plant) && CalculateScore(plant) != int.MinValue)
                {
                    nextPlantsToAnalyze.Add(plant);
                }
            }
            return nextPlantsToAnalyze;
        }

        private static int CalculateScore(Plant plant)
        {
            //TODO ne pas recalculer le score pour chaque plante
            //int score = 0;
            //int nbG = 0;
            //int nbY = 0;
            int nbDominants = 0;
            foreach (var gene in plant.Genome)
            {
                //if (gene == Allele.G)
                //{
                //    nbG++;
                //}
                //else if (gene == Allele.Y)
                //{
                //    nbY++;
                //}
                //else 
                if (Parameters.Dominants.Any(d => d == gene))
                {
                    nbDominants++;
                }
            }
            return 0 - nbDominants - (int)plant.ComputeCost();
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
            foreach (var subSetCurrentGen in currentGenerationPlants.CombinationsWithRepetition(nbOfCurrentGenPlants))
            {
                foreach (var subSetAvailable in previousGenerationPlants.CombinationsWithRepetition(nbOfPreviousGenPlants))
                {
                    var subSet = subSetAvailable.Union(subSetCurrentGen);
                    var newPlants = PlantFactory.Instance.MergePlants(subSet).Except(previousGenerationPlants).Except(currentGenerationPlants);
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
                foreach (var subSet in availablePlants.CombinationsWithRepetition(nbPlants))
                {
                    var newPlants = PlantFactory.Instance.MergePlants(subSet);
                    discoveredPlants.UnionWith(newPlants.Except(availablePlants));
                }
            }
            return discoveredPlants;
        }


    }
}
