#if !NET6_0_OR_GREATER

using System.Runtime.InteropServices;

namespace RoboSharp.Extensions.SymbolicLinkSupport
{
    /// <remarks>
    /// Refer to http://msdn.microsoft.com/en-us/library/windows/hardware/ff552012%28v=vs.85%29.aspx
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SymbolicLinkReparseData
    {
        private const int maxUnicodePathLength = 32767 * 2;

        public uint ReparseTag;
        public ushort ReparseDataLength;
        public ushort Reserved;
        public ushort SubstituteNameOffset;
        public ushort SubstituteNameLength;
        public ushort PrintNameOffset;
        public ushort PrintNameLength;
        public uint Flags;
        // PathBuffer needs to be able to contain both SubstituteName and PrintName,
        // so needs to be 2 * maximum of each
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = maxUnicodePathLength * 2)]
        public byte[] PathBuffer;
    }
}

#endif