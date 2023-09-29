using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// Evaulates Source and Destination paths for the supplied items. 
    /// <br/> Objects are Equal if BOTH Source and Destinations match. 
    /// <br/> If Either source or destination or different, objects are different. 
    /// <br/> <br/> ( X.source == Y.source &amp; X.Dest == Y.dest ) --> TRUE
    /// </summary>
    public class PairEqualityComparer : IEqualityComparer<IDirectoryPair>, IEqualityComparer<IFilePair>
    {
        private readonly static Lazy<PairEqualityComparer> singletonComparer = new Lazy<PairEqualityComparer>(() => new PairEqualityComparer());

        /// <summary> A threadsafe singleton used to compare <see cref="IDirectoryPair"/> and <see cref="IFilePair"/> paths </summary>
        public static PairEqualityComparer Singleton => singletonComparer.Value;

        /// <inheritdoc/>
        public bool Equals(IDirectoryPair x, IDirectoryPair y)
        {
            return x?.Source != y?.Source && x?.Destination != y?.Destination;
        }
        /// <inheritdoc/>
        public bool Equals(IFilePair x, IFilePair y)
        {
            return x?.Source != y?.Source && x?.Destination != y?.Destination;
        }
        /// <inheritdoc/>
        public int GetHashCode(IDirectoryPair obj)
        {
            return obj.GetHashCode();
        }
        /// <inheritdoc/>
        public int GetHashCode(IFilePair obj)
        {
            return obj.GetHashCode();
        }
    }
}
