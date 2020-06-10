using System;
using System.Collections.Generic;
using System.Text;

namespace Cedric.Breeding
{
    public class Plant
    {
        
        public Allele[] Genes { get; }
        public string Name { get; }
        public int EstimatedDistance { get; }
       
        public Plant(Allele[] genes)
        {
            this.Genes = genes;
            this.Name = CalculateName();
        }

        public Plant(Allele[] genes, int estimatedDistance)
            : this(genes)
        {
            this.EstimatedDistance = estimatedDistance;
        }

        public override string ToString()
        {
            return Name;
        }

        private string CalculateName()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var allele in Genes)
            {
                builder.Append(allele.ToString());
            }
            return builder.ToString();
        }
    }
}
