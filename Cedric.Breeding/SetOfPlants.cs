using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cedric.Breeding
{
    public class SetOfPlants
        : IEnumerable<Plant>, ICollection<Plant>
    {

        public SetOfPlants()
        {
        }

        private HashSet<Plant> InnerSet = new HashSet<Plant>();
        public double MaximumCost { get; private set; } = double.MaxValue;

        internal void SetMaximumCost(double maximumCost)
        {
            this.MaximumCost = maximumCost;
            HashSet<Plant> newSet = new HashSet<Plant>();
            foreach (var plant in this)
            {
                if (plant.Cost < maximumCost)
                    newSet.Add(plant);
            }
            this.InnerSet = newSet;
        }

        public void Add(Plant plant)
        {
            if (plant.Cost < this.MaximumCost)
            {
                this.InnerSet.Add(plant);
            }
        }

        public void Add(IEnumerable<Plant> plants)
        {
            foreach (var plant in plants)
            {
                this.Add(plant);
            }
        }

        public IEnumerator<Plant> GetEnumerator()
        {
            return ((IEnumerable<Plant>)this.InnerSet).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Plant>)this.InnerSet).GetEnumerator();
        }

        public void Clear()
        {
            ((ICollection<Plant>)this.InnerSet).Clear();
        }

        public bool Contains(Plant item)
        {
            return ((ICollection<Plant>)this.InnerSet).Contains(item);
        }

        public void CopyTo(Plant[] array, int arrayIndex)
        {
            ((ICollection<Plant>)this.InnerSet).CopyTo(array, arrayIndex);
        }

        public bool Remove(Plant item)
        {
            return ((ICollection<Plant>)this.InnerSet).Remove(item);
        }

        public int Count => ((ICollection<Plant>)this.InnerSet).Count;

        public bool IsReadOnly => false;
    }

    public static class SetOfPlantsExtensions
    {
        public static SetOfPlants ToSetOfPlants(this IEnumerable<Plant> plants)
        {
            SetOfPlants set = new SetOfPlants();
            set.Add(plants);
            return set;
        }
    }
}
