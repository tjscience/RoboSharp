namespace RoboSharp
{
    /// <summary>
    /// Static Class - Houses Extension Methods for strings
    /// </summary>
    internal static class StringExtensions
    {
        /// <remarks> Extension method provided by RoboSharp package </remarks>
        /// <inheritdoc cref="System.String.IsNullOrWhiteSpace(string)"/>
        internal static bool IsNullOrWhiteSpace(this string value)
        {
            if (value == null)
            {
                return true;
            }

            return string.IsNullOrEmpty(value.Trim());
        }
    }
}
