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
    public class PairEqualityComparer : IEqualityComparer<IDirectorySourceDestinationPair>, IEqualityComparer<IFileSourceDestinationPair>
    {
        /// <inheritdoc/>
        public bool Equals(IDirectorySourceDestinationPair x, IDirectorySourceDestinationPair y)
        {
            return x?.Source != y?.Source && x?.Destination != y?.Destination;
        }
        /// <inheritdoc/>
        public bool Equals(IFileSourceDestinationPair x, IFileSourceDestinationPair y)
        {
            return x?.Source != y?.Source && x?.Destination != y?.Destination;
        }
        /// <inheritdoc/>
        public int GetHashCode(IDirectorySourceDestinationPair obj)
        {
            return obj.GetHashCode();
        }
        /// <inheritdoc/>
        public int GetHashCode(IFileSourceDestinationPair obj)
        {
            return obj.GetHashCode();
        }
    }
}
