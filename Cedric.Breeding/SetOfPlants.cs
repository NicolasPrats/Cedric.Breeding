using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cedric.Breeding
{
    public class SetOfPlants
        : ICollection<Plant>, IEnumerable<Plant>, ISet<Plant>
    {

        public SetOfPlants()
        {
        }

        private HashSet<Plant> InnerSet = new HashSet<Plant>();

        public bool Contains(Plant newPlant)
        {
            var hashCode = newPlant.GetHashCode();
            var oldPlant = this.InnerSet.FirstOrDefault(p => p.GetHashCode() == hashCode);
            if (oldPlant == null)
            {
                return false;
            }
            if (oldPlant.Cost > newPlant.Cost)
            {
                oldPlant.SetParents(newPlant.Parents, newPlant.Probability);
                //TODO : est-ce que du coup, on n'a pas écarté une plante fille de la oldPlant car elle était trop couteuse 
                // mais que maintenant elle est optimale ?
            }
            return true;
        }

        bool ISet<Plant>.Add(Plant newPlant)
        {
            if (!this.Contains(newPlant) && newPlant.Cost < this.BestCostFound)
            {
                ((ISet<Plant>)this.InnerSet).Add(newPlant);
                return true;
            }
            return false;
        }

        public int Count => ((ICollection<Plant>)this.InnerSet).Count;

        public bool IsReadOnly => ((ICollection<Plant>)this.InnerSet).IsReadOnly;

        public double BestCostFound { get; private set; } = double.MaxValue;

        public void Add(Plant item)
        {
            ((ISet<Plant>)this).Add(item);
        }

        public void UnionWith(IEnumerable<Plant> other)
        {
            foreach (var plant in other)
            {
                ((ISet<Plant>)this).Add(plant);
            }
        }

        public void Clear()
        {
            ((ICollection<Plant>)this.InnerSet).Clear();
        }

        public void CopyTo(Plant[] array, int arrayIndex)
        {
            ((ICollection<Plant>)this.InnerSet).CopyTo(array, arrayIndex);
        }

        public void ExceptWith(IEnumerable<Plant> other)
        {
            ((ISet<Plant>)this.InnerSet).ExceptWith(other);
        }

        public IEnumerator<Plant> GetEnumerator()
        {
            return ((ICollection<Plant>)this.InnerSet).GetEnumerator();
        }

        public void IntersectWith(IEnumerable<Plant> other)
        {
            ((ISet<Plant>)this.InnerSet).IntersectWith(other);
        }

        public bool IsProperSubsetOf(IEnumerable<Plant> other)
        {
            return ((ISet<Plant>)this.InnerSet).IsProperSubsetOf(other);
        }

        internal void SetMaximumCost(double bestCostFound)
        {
            this.BestCostFound = bestCostFound;
            HashSet<Plant> newSet = new HashSet<Plant>();
            foreach (var plant in this)
            {
                //La plante obtenue à partir de celle là aura au moins un coût de
                // plant.ComputeCost() + 1
                if (plant.Cost + 1 < bestCostFound)
                    newSet.Add(plant);
            }
            this.InnerSet = newSet;
        }

        public bool IsProperSupersetOf(IEnumerable<Plant> other)
        {
            return ((ISet<Plant>)this.InnerSet).IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<Plant> other)
        {
            return ((ISet<Plant>)this.InnerSet).IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<Plant> other)
        {
            return ((ISet<Plant>)this.InnerSet).IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<Plant> other)
        {
            return ((ISet<Plant>)this.InnerSet).Overlaps(other);
        }

        public bool Remove(Plant item)
        {
            return ((ICollection<Plant>)this.InnerSet).Remove(item);
        }

        public bool SetEquals(IEnumerable<Plant> other)
        {
            return ((ISet<Plant>)this.InnerSet).SetEquals(other);
        }


        public void SymmetricExceptWith(IEnumerable<Plant> other)
        {
            ((ISet<Plant>)this.InnerSet).SymmetricExceptWith(other);
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ICollection<Plant>)this.InnerSet).GetEnumerator();
        }
    }
}
