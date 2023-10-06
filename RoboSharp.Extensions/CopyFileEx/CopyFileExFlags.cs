﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions.CopyFileEx
{
    ///// <summary>
    ///// 
    ///// </summary>
    //public enum SymbolicLinkFlags : uint
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    SYMBLOC_LINK_FLAG_FILE = 0x0,
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    SYMBLOC_LINK_FLAG_DIRECTORY = 0x1
    //}

    /// <summary>
    /// Flags to pass into CopyFileEx to determine how it should run
    /// </summary>
    [Flags]
    public enum CopyFileExFlags : uint
    {
        /// <summary>
        /// An attempt to copy an encrypted file will succeed even if the destination copy cannot be encrypted. 
        /// </summary>
        COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x00000008,

        /// <summary>
        /// If the source file is a symbolic link, the destination file is also a symbolic link pointing to the same file that the source symbolic link is pointing to.
        /// <para/>Windows Server 2003 and Windows XP:  This value is not supported
        /// </summary>
        COPY_FILE_COPY_SYMLINK = 0x00000800,

        /// <summary>
        /// The copy operation fails immediately if the target file already exists. 
        /// </summary>
        COPY_FILE_FAIL_IF_EXISTS = 0x00000001,

        /// <summary>
        /// The copy operation is performed using unbuffered I/O, bypassing system I/O cache resources. Recommended for very large file transfers.
        /// <para/>Windows Server 2003 and Windows XP:  This value is not supported
        /// </summary>
        COPY_FILE_NO_BUFFERING = 0x00001000,

        /// <summary>
        /// The file is copied and the original file is opened for write access. 
        /// </summary>
        COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x00000004,


        /// <summary>
        /// Progress of the copy is tracked in the target file in case the copy fails. ( Required for Progress Reporting )
        /// <br/> - The failed copy can be restarted at a later time by specifying the same values for lpExistingFileName and lpNewFileName as those used in the call that failed. 
        /// <br/> - This can significantly slow down the copy operation as the new file may be flushed multiple times during the copy operation. 
        /// </summary>
        COPY_FILE_RESTARTABLE = 0x00000002,

        /// <summary>
        /// Request the underlying transfer channel compress the data during the copy operation. The request may not be supported for all mediums, in which case it is ignored. The compression attributes and parameters (computational complexity, memory usage) are not configurable through this API, and are subject to change between different OS releases.
        /// <para/>This flag was introduced in Windows 10, version 1903 and Windows Server 2022. On Windows 10, the flag is supported for files residing on SMB shares, where the negotiated SMB protocol version is SMB v3.1.1 or greater.
        /// </summary>
        COPY_FILE_REQUEST_COMPRESSED_TRAFFIC = 0x10000000,
        
    }
}
