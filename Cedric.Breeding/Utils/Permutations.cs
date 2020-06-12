using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Cedric.Breeding.Utils
{
    public static class PermutationsExtensions
    {

        public static IEnumerable<T[]> Permutations<T>(this IList<T> list)
        {
            return Permutations(list, list.Count).Select(l => l.ToArray());
        }

        private static IEnumerable<T[]> Permutations<T>(IList<T> list, int length)
        {
            if (length == 1)
            {
                foreach (var item in list)
                {
                    yield return new T[] { item };
                }
            }
            else
            {
                var permutations = Permutations(list, length - 1);
                foreach (var item in list)
                {
                    var array = new T[] { item };
                    foreach (var permutation in permutations)
                    {
                        if (!permutation.Contains(item))
                        {
                            yield return permutation.Concat(array).ToArray();
                        }
                    }
                }
            }
        }
    }
}
