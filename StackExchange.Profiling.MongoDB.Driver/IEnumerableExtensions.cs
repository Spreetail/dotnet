using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackExchange.Profiling.MongoDB.Driver
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// http://stackoverflow.com/a/30441479
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="childSelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> FlattenTree<TSource>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> childSelector)
        {
            var stack = new Stack<IEnumerator<TSource>>();
            var enumerator = source.GetEnumerator();

            try
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        TSource element = enumerator.Current;
                        yield return element;

                        stack.Push(enumerator);
                        var nextChild = childSelector(element);
                        if(nextChild != null)
                            enumerator = nextChild.GetEnumerator();
                    }
                    else if (stack.Count > 0)
                    {
                        enumerator.Dispose();
                        enumerator = stack.Pop();
                    }
                    else
                    {
                        yield break;
                    }
                }
            }
            finally
            {
                enumerator.Dispose();

                while (stack.Count > 0) // Clean up in case of an exception.
                {
                    enumerator = stack.Pop();
                    enumerator.Dispose();
                }
            }
        }
    }
}
