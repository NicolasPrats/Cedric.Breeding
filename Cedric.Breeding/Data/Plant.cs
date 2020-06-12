using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cedric.Breeding.Constraints;

namespace Cedric.Breeding.Data
{
    public class Plant
    {
        public IReadOnlyList<Allele> Genome { get; }
        public string Name { get; }
        private Allele[] SortedGenes { get; } //Pour faciliter la comparaison avec la cible
        public IList<Plant>? Parents { get; private set; }
        public double Probability { get; private set; }
        public double Cost { get; set; }

        protected event EventHandler? OnCostChanged;

        public Plant(Allele[] genome)
        {
            this.Genome = genome;

            this.SortedGenes = (Allele[])genome.Clone();
            Array.Sort(SortedGenes);
            this.Name = ComputeName();
        }

        private void ComputeCost()
        {
            this.Cost = PlantHelper.ComputeCost(this.Parents, this.Probability);
            OnCostChanged?.Invoke(this, EventArgs.Empty);
        }

        public Allele this[int index]
        {
            get
            {
                return this.Genome[index];
            }
        }

        public override string ToString()
        {
            return Name;
        }

        private string ComputeName()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var gene in Genome)
            {
                builder.Append(gene.ToString());
            }
            return builder.ToString();
        }

        internal void SetParents(IList<Plant>? parents, double probability)
        {
            if (this.Parents != null)
            {
                foreach (var parent in this.Parents)
                {
                    parent.OnCostChanged -= Parent_OnCostChanged;
                }
            }

            this.Parents = parents;
            if (parents != null)
            {
                foreach (var parent in parents)
                {
                    parent.OnCostChanged += Parent_OnCostChanged;
                }
            }
            this.Probability = probability;
            ComputeCost();
        }

        private void Parent_OnCostChanged(object? sender, EventArgs e)
        {
            ComputeCost();
        }

        public Plant? IsSimilarToAny(Plant[] plants)
        {
            foreach (var plant in plants)
            {
                bool isSimilar = true;
                for (int i = 0; i < Parameters.NbGenes; i++)
                {
                    if (this.SortedGenes[i] != plant.SortedGenes[i])
                    {
                        isSimilar = false;
                        break;
                    }
                }
                if (isSimilar)
                {
                    return plant;
                }
            }
            return null;
        }

        public string GenerateTree()
        {
            StringBuilder builder = new StringBuilder();
            GenerateTree(builder, "", true, true);
            return builder.ToString();
        }

        protected void GenerateTree(StringBuilder builder, string indent, bool root, bool last)
        {
            if (root)
            {
                builder.AppendLine(this.Name + " (" + this.Cost + ")");
            }
            else
            {
                builder.AppendLine(indent + "\\- " + this.Name + " (" + this.Cost + ")");
            }
            if (this.Parents != null)
            {
                indent += last ? "   " : "|  ";
                for (int i = 0; i < this.Parents.Count(); i++)
                {
                    this.Parents[i].GenerateTree(builder, indent, false, i == this.Parents.Count() - 1);
                }
            }
        }
    }
}
