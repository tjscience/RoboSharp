#if NET6_0_OR_GREATER

using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace RoboSharp
{
    /// <summary>
    /// Run an action with an impersonated user. Useful to test Source/Destination directory access in .Net >= 6.0
    /// <remarks>See Issue #43 and PR #45 </remarks>
    /// <remarks>This class is not available in Net5.0, NetCoreApp3.1, NetStandard2.0, and NetStandard2.1</remarks>
    /// </summary>
    internal class ImpersonatedRun
    {
        internal static void Execute(string domain, string user, string password, Action action)
        {
            var loggedOn = LogonUser(user, domain, password, LogonType.Interactive, LogonProvider.Default, out var safeAccessTokenHandle);
            if (!loggedOn)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            WindowsIdentity.RunImpersonated(safeAccessTokenHandle, action);
            
            var closed = CloseHandle(safeAccessTokenHandle);
            if (!closed)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            LogonType dwLogonType,
            LogonProvider dwLogonProvider,
            out SafeAccessTokenHandle phToken
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(SafeAccessTokenHandle hHandle);

        enum LogonType
        {
            Interactive = 2,
            Network = 3,
            Batch = 4,
            Service = 5,
            NetworkCleartext = 8,
            NewCredentials = 9,
        }

        enum LogonProvider
        {
            Default = 0,
        }
    }
}

#endif