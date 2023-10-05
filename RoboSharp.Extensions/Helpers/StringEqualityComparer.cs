using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions.Helpers
{
    public sealed class StringEqualityComparer : IEqualityComparer<string>
    {
        /// <summary> Create a new Comparer </summary>
        private StringEqualityComparer(StringComparison type)
        {
            ComparisonType = type;
        }

        public static readonly StringEqualityComparer Ordinal = new StringEqualityComparer(StringComparison.Ordinal);
        public static readonly StringEqualityComparer OrdinalIgnoreCase = new StringEqualityComparer(StringComparison.OrdinalIgnoreCase);

        public static readonly StringEqualityComparer InvariantCulture = new StringEqualityComparer(StringComparison.InvariantCulture);
        public static readonly StringEqualityComparer InvariantCultureIgnoreCase  = new StringEqualityComparer(StringComparison.InvariantCultureIgnoreCase);
        
        public static readonly StringEqualityComparer CurrentCulture = new StringEqualityComparer(StringComparison.CurrentCulture);
        public static readonly StringEqualityComparer CurrentCultureIgnoreCase = new StringEqualityComparer(StringComparison.CurrentCultureIgnoreCase);

        /// <summary>
        /// The comparison enum to use within <see cref="String.Equals(string?, StringComparison)"/>
        /// </summary>
        public StringComparison ComparisonType { get; }

        /// <inheritdoc/>
        public bool Equals(string x, string y)
        {
            return x.Equals(y, ComparisonType);
        }

        /// <inheritdoc/>
        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }
}
