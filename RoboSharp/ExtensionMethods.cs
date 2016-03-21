namespace RoboSharp
{
    public static class ExtensionMethods
    {
        public static string CleanOptionInput(this string option)
        {
            // Get rid of forward slashes
            option = option.Replace("/", "");
            // Get rid of padding
            option = option.Trim();

            return option;
        }
    }
}
