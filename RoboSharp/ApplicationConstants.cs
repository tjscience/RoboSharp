using System.Collections.Generic;
using System.Text;

namespace RoboSharp
{
    internal static class ApplicationConstants
    {

        /// <summary>
        /// The static constructor for the class to take care of any setup / fixes required before running any operations.
        /// </summary>
        static ApplicationConstants()
        {
#if NETCOREAPP // Ensure that encoding 437 is supported
            CodePagesEncodingProvider.Instance.GetEncoding(437);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
        }

        /// <summary> Request this null object used to ensure the static constructor executes </summary>
        internal static object Initializer => null;

        internal static Dictionary<string, string> ErrorCodes = new Dictionary<string, string>()
        {
            { "ERROR 33 (0x00000021)", "The process cannot access the file because another process has locked a portion of the file." },
            { "ERROR 32 (0x00000020)", "The process cannot access the file because it is being used by another process." },
            { "ERROR 5 (0x00000005)", "Access is denied." }
        };
    }
}
