using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions.Helpers
{
    
    /// <summary>
    /// Evaulates Source and Destination paths for the supplied items. 
    /// <br/> Objects are Equal if BOTH Source and Destinations match. 
    /// <br/> If Either source or destination or different, objects are different. 
    /// <br/> <br/> ( X.source == Y.source &amp; X.Dest == Y.dest ) --> TRUE
    /// </summary>
    public sealed class PairEqualityComparer : IEqualityComparer<IFilePair>, IEqualityComparer<IDirectoryPair>
    {
        private readonly static Lazy<PairEqualityComparer> singletonComparer 
            = new Lazy<PairEqualityComparer>(() => new PairEqualityComparer());

        /// <summary> A threadsafe singleton used to compare <see cref="IDirectoryPair"/> and <see cref="IFilePair"/> paths </summary>
        public static PairEqualityComparer Singleton => singletonComparer.Value;

        private static IFilePairEqualityComparer<IFilePair> FileComparer => IFilePairEqualityComparer<IFilePair>.Singleton;
        private static IDirectoryPairEqualityComparer<IDirectoryPair> DirComparer => IDirectoryPairEqualityComparer<IDirectoryPair>.Singleton;

        /// <inheritdoc cref="IFilePairEqualityComparer{T}.Equals(T, T)"/>
        public static bool AreEqual(IFilePair x, IFilePair y)
        {
            return FileComparer.Equals(x, y);
        }

        /// <inheritdoc cref="IDirectoryPairEqualityComparer{T}.Equals(T, T)"/>
        public static bool AreEqual(IDirectoryPair x, IDirectoryPair y)
        {
            return DirComparer.Equals(x, y);
        }

        /// <inheritdoc cref="IFilePairEqualityComparer{T}.Equals(T, T)"/>
        public bool Equals(IFilePair x, IFilePair y)
        {
            return FileComparer.Equals(x, y);
        }

        /// <inheritdoc cref="IDirectoryPairEqualityComparer{T}.Equals(T, T)"/>
        public bool Equals(IDirectoryPair x, IDirectoryPair y)
        {
            return DirComparer.Equals(x, y);
        }

        /// <inheritdoc/>
        public int GetHashCode(IFilePair obj)
        {
            return obj.GetHashCode();
        }

        /// <inheritdoc/>
        public int GetHashCode(IDirectoryPair obj)
        {
            return obj.GetHashCode();
        }
    }

}
