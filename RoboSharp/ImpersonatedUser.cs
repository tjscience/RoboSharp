#if NET40_OR_GREATER

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace RoboSharp
{

    /// <summary>
    /// Create an authenticated user to test Source/Destination directory access
    /// <remarks>See Issue #43 and PR #45 </remarks>
    /// <remarks>This class is not available in NetCoreApp3.1, NetStandard2.0, and NetStandard2.1</remarks>
    /// </summary>
    internal class ImpersonatedUser : IDisposable
    {
        IntPtr userHandle;

        WindowsImpersonationContext impersonationContext;

        /// <inheritdoc cref="ImpersonatedUser"/>
        /// <inheritdoc cref="RoboCommand.Start(string, string, string)"></inheritdoc>
        internal ImpersonatedUser(string user, string domain, string password)
        {
            userHandle = IntPtr.Zero;

            bool loggedOn = LogonUser(
                user,
                domain,
                password,
                LogonType.Interactive,
                LogonProvider.Default,
                out userHandle);

            if (!loggedOn)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            // Begin impersonating the user
            impersonationContext = WindowsIdentity.Impersonate(userHandle);
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (userHandle != IntPtr.Zero)
            {
                CloseHandle(userHandle);

                userHandle = IntPtr.Zero;

                impersonationContext.Undo();
            }
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool LogonUser(

            string lpszUsername,

            string lpszDomain,

            string lpszPassword,

            LogonType dwLogonType,

            LogonProvider dwLogonProvider,

            out IntPtr phToken

            );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hHandle);

        enum LogonType : int
        {
            Interactive = 2,
            Network = 3,
            Batch = 4,
            Service = 5,
            NetworkCleartext = 8,
            NewCredentials = 9,
        }

        enum LogonProvider : int
        {
            Default = 0,
        }

    }

}

#endif