using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// Static Class for creating <see cref="CachedEnumerable{T}"/> objects
    /// </summary>
    public static class CachedEnumerable
    {
        /// <summary>
        /// Create a new <see cref="CachedEnumerable{T}"/> object to cache the results of the enumerable
        /// </summary>
        /// <typeparam name="T">the type to enumerate</typeparam>
        /// <param name="enumerable">the underlying <see cref="IEnumerable{T}"/> to enumerate</param>
        /// <returns>new <see cref="CachedEnumerable{T}"/></returns>
        public static CachedEnumerable<T> Create<T>(IEnumerable<T> enumerable)
        {
            return new CachedEnumerable<T>(enumerable);
        }

        /// <summary>
        /// Extension method to convert an IEnumerable to a thread-safe CachedIEnumerable to reduce cost of multiple iterations
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns>new <see cref="CachedEnumerable{T}"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public static CachedEnumerable<T> AsCachedEnumerable<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable is null) throw new ArgumentNullException(nameof(enumerable));
            if (enumerable is CachedEnumerable<T> obj)
                return obj;
            else if (enumerable is T[] arr)
                return new CachedEnumerable<T>(arr);
            else if (enumerable is List<T> list)
                return new CachedEnumerable<T>(list);
            else
                return new CachedEnumerable<T>(enumerable);
        }

        /// <summary>
        /// Wrap the enumerable into a <see cref="CachedEnumerable"/> by using <see cref="System.Linq.Enumerable.Cast{TResult}(IEnumerable)"/> (object)
        /// </summary>
        /// <param name="enumerable"></param>
        /// <inheritdoc cref="AsCachedEnumerable{T}(IEnumerable{T})"/>
        public static CachedEnumerable<object> AsCachedEnumerableObjects(this IEnumerable enumerable)
        {
            return new CachedEnumerable<object>(enumerable.Cast<object>());
        }

        /// <summary>
        /// Check if any of the items in the collection is a <paramref name="match"/>
        /// </summary>
        /// <remarks>
        /// Only Positive verification should be used here.
        /// <br/>Arr = { 1, 2, 3, 4 }   
        /// <br/> - Arr.None( x => x == 5) -- Returns TRUE since none equal 5 (checks all items)
        /// <br/> - Arr.None( x => x == 3) -- Returns FALSE since 3 exists (Stops checking after the match is found)
        /// <br/> - Arr.None( x => x != 3) -- Returns FALSE since 1 != 3 ( never checked if 3 exists, because 1 passed the check )
        /// </remarks>
        /// <returns>TRUE if no matches found, FALSE if any matches found</returns>
        /// <inheritdoc cref="System.Linq.Enumerable.Any{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
        public static bool None<T>(this IEnumerable<T> collection, Func<T, bool> match) => collection is null || !collection.Any(match);

        /// <summary>
        /// Check if the collection is empty
        /// </summary>
        /// <returns>TRUE if the collection is empty, otherwise false</returns>
        /// <inheritdoc cref="System.Linq.Enumerable.Any{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
        public static bool None<T>(this IEnumerable<T> collection) => collection is null || !collection.Any();

        /// <summary>
        /// Check if there are atleast 2 elements in a collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns>TRUE if atleast 2 elements exist in the sequence, otherwise false.</returns>
        public static bool HasMultiple<T>(this IEnumerable<T> collection)
        {
            if (None(collection)) return false;
            try
            {
                _ = collection.Single();
                return false;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Iterate across a <paramref name="collection"/> and return any unique values, as determined by the <paramref name="comparer"/>
        /// </summary>
        /// <typeparam name="T">The type of object in the collectionm</typeparam>
        /// <param name="collection">The collection to iterate against</param>
        /// <param name="comparer">A comparer that compares each item in the collection with the other items in the collection to determine if the items is unique.</param>
        /// <returns>All items considered unique within the collection.</returns>
        public static IEnumerable<T> WhereUnique<T>(this IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            if (collection is null) throw new ArgumentNullException(nameof(collection));
            if (comparer is null) throw new ArgumentNullException(nameof(comparer));
            List<T> yielded = new List<T>();
            bool firstOK = false;
            foreach(T obj in collection)
            {
                if (firstOK)
                {
                    if (!yielded.Any(previous => comparer.Equals(previous, obj)))
                    {
                        yielded.Add(obj);
                        yield return obj;
                    }
                }
                else
                {
                    yielded.Add(obj);
                    firstOK = true;
                    yield return obj;
                }
            }
            yield break;
        }
    }
}
