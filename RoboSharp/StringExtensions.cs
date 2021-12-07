namespace RoboSharp
{
    internal static class StringExtensions
    {
        /// <returns>Returns <see cref="System.String.IsNullOrWhiteSpace(string)"/></returns>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            if (value == null)
            {
                return true;
            }

            return string.IsNullOrEmpty(value.Trim());
        }

    }
}
