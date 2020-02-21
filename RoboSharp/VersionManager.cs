using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp
{
    public static class VersionManager
    {
        public enum VersionCheckType
        {
            UseRtlGetVersion,
            UseWMI
        }

        public static VersionCheckType VersionCheck { get; set; } = VersionManager.VersionCheckType.UseRtlGetVersion;

        private static double? version;
        public static double Version
        {
            get
            {
                if (version == null)
                {
                    if (VersionCheck == VersionCheckType.UseWMI)
                    {
                        var v = GetOsVersion();
                        version = GetOsVersionNumber(v);
                        return version.Value;
                    }
                    else
                    {
                        var osVersionInfo = new OSVERSIONINFOEX { OSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX)) };
                        RtlGetVersion(ref osVersionInfo);
                        var versionString = $"{osVersionInfo.MajorVersion}.{osVersionInfo.MinorVersion}{osVersionInfo.BuildNumber}";
                        version = Convert.ToDouble(versionString);
                        return version.Value;
                    }
                }
                else
                {
                    return version.Value;
                }
            }
        }

        private static string GetOsVersion()
        {
            using (ManagementObjectSearcher objMOS = new ManagementObjectSearcher("SELECT * FROM  Win32_OperatingSystem"))
            {
                foreach (ManagementObject objManagement in objMOS.Get())
                {
                    var version = objManagement.GetPropertyValue("Version");

                    if (version != null)
                    {
                        return version.ToString();
                    }
                }
            }

            return Environment.OSVersion.Version.ToString();
        }

        private static double GetOsVersionNumber(string version)
        {
            if (version.IsNullOrWhiteSpace())
                return 0;

            var segments = version.Split(new char[] { '.' });
            var major = Convert.ToDouble(segments[0]);
            var otherSegments = segments.Skip(1).ToArray();
            var dec = Convert.ToDouble("." + string.Join("", otherSegments), CultureInfo.InvariantCulture);
            return major + dec;
        }

        /// <summary>
        /// taken from https://stackoverflow.com/a/49641055
        /// </summary>
        /// <param name="versionInfo"></param>
        /// <returns></returns>
        [SecurityCritical]
        [DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int RtlGetVersion(ref OSVERSIONINFOEX versionInfo);
        [StructLayout(LayoutKind.Sequential)]
        private struct OSVERSIONINFOEX
        {
            // The OSVersionInfoSize field must be set to Marshal.SizeOf(typeof(OSVERSIONINFOEX))
            internal int OSVersionInfoSize;
            internal int MajorVersion;
            internal int MinorVersion;
            internal int BuildNumber;
            internal int PlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            internal string CSDVersion;
            internal ushort ServicePackMajor;
            internal ushort ServicePackMinor;
            internal short SuiteMask;
            internal byte ProductType;
            internal byte Reserved;
        }
    }
}
