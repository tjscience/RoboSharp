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
        public static CachedEnumerable<T> AsCachedEnumerable<T>(this IEnumerable<T> enumerable)
        {
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
    }

    /// <summary>
    /// Provides caching functionality for <see cref="IEnumerable{T}"/> to reduce cost of iterating over the enumeration multiple times
    /// </summary>
    /// <typeparam name="T">the type to enumerate</typeparam>
    public sealed class CachedEnumerable<T> : IEnumerable<T>, IDisposable, IEnumerable
    {
        private readonly List<T> _cache = new List<T>();
        private readonly IEnumerable<T> _enumerable;
        private IEnumerator<T> _enumerator;
        private bool _enumerated = false;

        /// <summary>
        /// Create a new Cached IEnumerable
        /// </summary>
        /// <param name="enumerable"></param>
        public CachedEnumerable(IEnumerable<T> enumerable)
        {
            _enumerable = enumerable ?? throw new ArgumentNullException(nameof(enumerable));
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            var index = 0;
            while (true)
            {
                if (TryGetItem(index, out var result))
                {
                    yield return result;
                    index++;
                }
                else
                {
                    // There are no more items
                    yield break;
                }
            }
        }

        private bool TryGetItem(int index, out T result)
        {
            // if the item is in the cache, use it
            if (index < _cache.Count)
            {
                result = _cache[index];
                return true;
            }

            lock (_cache)
            {
                if (_enumerator == null && !_enumerated)
                {
                    _enumerator = _enumerable.GetEnumerator();
                }

                // Another thread may have get the item while we were acquiring the lock
                if (index < _cache.Count)
                {
                    result = _cache[index];
                    return true;
                }

                // If we have already enumerate the whole stream, there is nothing else to do
                if (_enumerated)
                {
                    result = default;
                    return false;
                }

                // Get the next item and store it to the cache
                if (_enumerator.MoveNext())
                {
                    result = _enumerator.Current;
                    _cache.Add(result);
                    return true;
                }
                else
                {
                    // There are no more items, we can dispose the underlying enumerator
                    _enumerator.Dispose();
                    _enumerator = null;
                    _enumerated = true;
                    result = default;
                    return false;
                }
            }
        }

        /// <inheritdoc cref="System.Linq.Enumerable.Count{TSource}(IEnumerable{TSource})"/>
        public int Count()
        {
            if (_enumerated) return _cache.Count;
            return System.Linq.Enumerable.Count(this);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_enumerator != null)
            {
                _enumerator.Dispose();
                _enumerator = null;
            }
        }

        /// <summary>
        /// Deconstructor
        /// </summary>
        ~CachedEnumerable() { Dispose(); }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_cache).CopyTo(array, index);
        }
    }
}
