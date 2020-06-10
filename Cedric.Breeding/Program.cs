using System;
using System.Collections.Generic;
using System.Linq;

namespace Cedric.Breeding
{
    class Program
    {
      
        static void Main() {
            var targetPlant = PlantFactory.Instance.GetRandomPlant();
            PlantFactory.Instance.DefineTarget(targetPlant);

            var startingPlants = new Plant[Parameters.NbStartingPlants];         
            for (int i = 0; i < startingPlants.Length; i++)
            {
                startingPlants[i] = PlantFactory.Instance.GetRandomPlant();
            }

            State initialState = new State("Initial Plants", startingPlants, null);
            List<Path> sortedPaths = new List<Path>();
            sortedPaths.Add(new Path(initialState));

            int iterationCount = 0;
            do
            {
                var bestPath = sortedPaths.First();
                iterationCount++;
                Console.WriteLine($"Iteration {iterationCount}");
                Console.WriteLine($"NbPaths = {sortedPaths.Count()}");
                Console.WriteLine($"BestPath estimate = {bestPath.EstimatedTotalDistance}");
                if (bestPath.IsComplete)
                {
                    Console.WriteLine($"Solution found in {bestPath.EstimatedTotalDistance} steps");
                    Console.WriteLine(bestPath.ToString());
                    Console.WriteLine($"Target was: " + targetPlant);
                    return;
                }
                sortedPaths.RemoveAt(0);
                var nextStates = bestPath.GetNextStates();
                foreach (var state in nextStates)
                {
                    var newPath = new Path(state, bestPath);
                    //TODO : on pourrait vérifier si l'état n'a pas déjà été calculé sur un chemin plus court
                    sortedPaths.Add(newPath);
                }
                //TODO : on retrie sans arrêt une partie du tableau déjà triée
                // on pourrait soit faire des inssertions à la bonne place
                // soit trier uniquement les nouveaux arrivants et fusionner les tableaux triés
                sortedPaths.OrderBy(p => p.EstimatedTotalDistance);
            } while (true);
        }

       
    }
}
