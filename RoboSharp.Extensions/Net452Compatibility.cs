
namespace RoboSharp.Extensions
{
#if NET452
    internal static class Array
    {
        public static T[] Empty<T>() => EmptyArr<T>.Empty;

        private static class EmptyArr<T>
        {
            public static readonly T[] Empty = new T[] { };
        }
    }
#endif
}
