using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions
{
#if NET452
    internal static class Array
    {
        public static T[] Empty<T>() => new T[] { };
    }
#endif
}
