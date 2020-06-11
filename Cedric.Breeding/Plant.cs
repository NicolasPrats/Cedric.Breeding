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
            this.Parents = parents;
            this.Probability = probability;
        }

        public double ComputeCost()
        {
            //TODO mettre le coût en cache et l'invalider ?
            if (Parents == null)
                return 0;
            var cost = Parents.Select(p => p.ComputeCost()).Sum() + 1;
            return cost / Probability;
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

        internal void ReplaceParents(IList<Plant>? parents, double probability)
        {
            this.Parents = parents;
            this.Probability = probability;
        }

        public bool IsSimilar(Plant plant)
        {
            for (int i = 0; i < Parameters.NbGenes; i++)
            {
                if (this.SortedGenes[i] != plant.SortedGenes[i])
                    return false;
            }
            return true;
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
                builder.AppendLine(this.Name + " (" + this.ComputeCost() + ")");
            }
            else
            {
                builder.AppendLine(indent + "\\- " + this.Name + " (" + this.ComputeCost() + ")");
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
