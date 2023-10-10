using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// Provides caching functionality for <see cref="IEnumerable{T}"/> to reduce cost of iterating over the enumeration multiple times
    /// </summary>
    /// <typeparam name="T">the type to enumerate</typeparam>
    public sealed class CachedEnumerable<T> : IEnumerable<T>, IDisposable, IEnumerable
    {
        private readonly List<T> _cache;
        private readonly IEnumerable<T> _enumerable;
        private IEnumerator<T> _enumerator;
        private bool _enumerated;

        static CachedEnumerable()
        {
            Empty = new CachedEnumerable<T>(Array.Empty<T>());
        }

        /// <summary>
        /// Gets an Empty Array
        /// </summary>
        public static CachedEnumerable<T> Empty { get; }
        
        /// <summary>
        /// Create a new Cached IEnumerable
        /// </summary>
        /// <param name="enumerable"></param>
        public CachedEnumerable(IEnumerable<T> enumerable)
        {
            _enumerable = enumerable ?? throw new ArgumentNullException(nameof(enumerable));
            _cache = new List<T>();
        }

        internal CachedEnumerable(T[] enumerable) 
        {
            if (enumerable is null) throw new ArgumentNullException(nameof(enumerable));
            this._cache = enumerable.ToList();
            this._enumerated = true;
        }

        internal CachedEnumerable(List<T> enumerable) 
        {
            if (enumerable is null) throw new ArgumentNullException(nameof(enumerable));
            this._cache = enumerable.ToList();
            this._enumerated = true;
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
                    //_enumerator.Dispose(); may be used elsewhere - disposal can be handled by the GC
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
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Deconstructor
        /// </summary>
        ~CachedEnumerable() { Dispose(); }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public void CopyTo(System.Array array, int index)
        {
            ((ICollection)_cache).CopyTo(array, index);
        }
    }
}
