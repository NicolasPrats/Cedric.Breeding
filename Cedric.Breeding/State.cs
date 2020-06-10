using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cedric.Breeding
{
    public class State
    {
        private Plant? LastCreatedPlant { get; }
        public string Origin { get; }
        private Plant[] AvailablePlants { get; } 
        public int EstimatedRemainingDistance { get; }

        public State(string origin, Plant[] availablePlants, Plant? lastCreatedPlant)
        {
            this.LastCreatedPlant = lastCreatedPlant;
            this.Origin = origin;
            this.AvailablePlants = availablePlants;
            this.EstimatedRemainingDistance = availablePlants.Select(p => p.EstimatedDistance).Min();
            //TODO : retirer la contrainte ci-dessous, ça permettra de trier les plantes
            // et du coup de vérifier plus rapidement si 2 états sont identiques
            // à date la contrainte est utilisée pour faire un var oldPlants = this.AvailablePlants.Take(this.AvailablePlants.Count() - 1);
            if (lastCreatedPlant != null && lastCreatedPlant != availablePlants.Last())
            {
                throw new ApplicationException("Last created plant expected to be at the end");
            }
        }

        internal IEnumerable<State> GetNextStates()
        {
            List<State> states = new List<State>();
            for (int i = 2; i <= Parameters.MaxNbPlantsInMerge; i++)
            {
                
                AddNextStates(states,  i);
            }
            return states;
        }

        private void AddNextStates(List<State> states, int nbPlantsInMerge)
        {
            if (this.AvailablePlants.Count() < nbPlantsInMerge)
                return;
            if (LastCreatedPlant == null)
            {
                foreach (var combination in this.AvailablePlants.Combinations(nbPlantsInMerge))
                {
                    IEnumerable<Plant> subSet = combination.subSet;
                    IEnumerable<Plant> remainingSet = combination.remainingSet;
                    AddNewStates(states, subSet, remainingSet);
                }
            }
            else
            {
                // On vient d'un état existant
                // On ne va générer que les combinaisons utilisant la nouvelle plante
                // les autres combinaisons peuvent être générées depuis l'état précédents.
                
                var oldPlants = this.AvailablePlants.Take(this.AvailablePlants.Count() - 1);
                var newPlants = new Plant[] { LastCreatedPlant};
                foreach (var combination in oldPlants.Combinations(nbPlantsInMerge - 1))
                {
                    IEnumerable<Plant> subSet = combination.subSet.Union(newPlants);
                    IEnumerable<Plant> remainingSet = combination.remainingSet;
                    AddNewStates(states, subSet, remainingSet);
                }
            }
        }

        private static void AddNewStates(List<State> states, IEnumerable<Plant> subSet, IEnumerable<Plant> remainingSet)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var plant in subSet)
            {
                if (builder.Length != 0)
                    builder.Append("+");
                builder.Append(plant);
            }
            var newPlant = PlantFactory.Instance.MergePlants(subSet);

            var availablePlants = remainingSet.ToList();
            availablePlants.Add(newPlant);
            builder.Append("=");
            builder.Append(newPlant);
            var newState = new State(builder.ToString(), availablePlants.ToArray(), newPlant);
            states.Add(newState);
        }
    }
}
