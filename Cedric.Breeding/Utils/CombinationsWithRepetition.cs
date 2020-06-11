using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cedric.Breeding.Utils
{
    static class Extensions
    {

        private static IEnumerable<T> Combination<T>(T first, IEnumerable<T> items)
        {
            yield return first;
            foreach (var item in items)
                yield return item;
        }

        public static IEnumerable<IEnumerable<T>> CombinationsWithRepetition<T>(this IEnumerable<T> items, int length)
        {
            if (length <= 0)
            {
                yield break;
            }
            else if (length == 1)
            {
                var first = items.FirstOrDefault();
                if (first == null)
                {
                    yield break;
                }
                else
                {
                    yield return Combination(items.First(), Enumerable.Empty<T>());
                }
            }
            else
            {
                foreach (var item in items)
                {
                    foreach (var c in CombinationsWithRepetition(items, length - 1))
                        yield return Combination(item, c);
                }
            }
        }
    }


}