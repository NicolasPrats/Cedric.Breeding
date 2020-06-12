using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Cedric.Breeding.Constraints
{
    public enum Allele
    {
        G,
        Y,
        H,
        W,
        X
    }

    public static class AlleleExtensions
    {
        public static bool IsDominant(this Allele allele)
        {
            return Parameters.Dominants.Contains(allele);
        }

        public static bool IsRecessive(this Allele allele)
        {
            return Parameters.Recessives.Contains(allele);
        }
    }
}
