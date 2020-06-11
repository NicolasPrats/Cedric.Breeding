using System;
using System.Collections.Generic;
using System.Text;

namespace Cedric.Breeding.Constraints
{
    public static class Parameters
    {
        public const int NbGenes = 6;
        public const int NbStartingPlants = 30;
        public const int MaxNbPlantsInMerge = 4;
        public static readonly IReadOnlyCollection<Allele> Dominants = new Allele[] { Allele.W, Allele.X };
        internal static readonly int BatchSize = 10; //batchsize ^ maxnbplantsinmerge doit rester "raisonnable"
    }
}
