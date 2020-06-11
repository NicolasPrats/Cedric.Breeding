using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cedric.Breeding
{
    public class Plant
    {
        public IReadOnlyList<Allele> Genome { get; }
        public string Name { get; }
        private Allele[] SortedGenes { get; } //Pour faciliter la comparaison avec la cible
        private int HashCode { get; } //Sert de hashcode mais est en fait réellement unique
        public IList<Plant>? Parents { get; private set; }
        public double Probability { get; private set; }
        public double Cost { get; private set; }

        protected event EventHandler? OnCostChanged;

        public Plant(Allele[] genome)
        {
            this.Genome = genome;

            this.SortedGenes = (Allele[])genome.Clone();
            Array.Sort(SortedGenes);
            this.Name = ComputeName();
            this.HashCode = ComputeHashcode();
        }

        public Plant(Allele[] genome, IList<Plant> parents, double probability)
            : this(genome)
        {
            SetParents(parents, probability);
        }

        private void ComputeCost()
        {
            if (Parents != null)
            {
                var cost = Parents.Select(p => p.Cost).Sum() + 1;
                this.Cost = cost / Probability;
            }
            else
            {
                this.Cost = 0;
            }
            OnCostChanged?.Invoke(this, EventArgs.Empty);
        }


        private int ComputeHashcode()
        {
            //Ici on suppose que nombre d'alleles differents ^nombre de genes tient dans un int
            //Ce qui est vrai au moins pour les données du problème original : 5^6
            int hashcode = 0;
            var nbOfAlleles = Enum.GetValues(typeof(Allele)).Length;
            foreach (var gene in Genome)
            {
                hashcode *= nbOfAlleles;
                hashcode += (int)gene;
            }
            return hashcode;
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
            this.ComputeCost();
        }

        private void Parent_OnCostChanged(object? sender, EventArgs e)
        {
            this.ComputeCost();
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

        public override int GetHashCode()
        {
            return HashCode;
        }

        public override bool Equals(object? obj)
        {
            var otherPlant = obj as Plant;
            if (otherPlant == null)
                return false;
            return this.HashCode == otherPlant.HashCode;
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
