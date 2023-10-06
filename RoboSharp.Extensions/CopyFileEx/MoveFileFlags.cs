using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions.CopyFileEx
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks><see href="https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-movefilewithprogressa"/></remarks>
    [Flags]
    public enum MoveFileFlags : uint
    {
        /// <summary>
        /// Default Recommended options to ensure it works across drives and doesn't return until file move completed.
        /// </summary>
        Default = MoveFileFlags.MOVE_FILE_COPY_ALLOWED | MOVE_FILE_WRITE_THROUGH,

        /// <summary>
        /// If a file named lpNewFileName exists, the function replaces its contents with the contents of the lpExistingFileName file.
        /// <br/> - This value cannot be used if lpNewFileName or lpExistingFileName names a directory.
        /// </summary>
        MOVE_FILE_REPLACE_EXISTSING = 0x00000001,

        /// <summary>
        /// If the file is to be moved to a different volume, the function simulates the move by using the CopyFile and DeleteFile functions.
        /// <br/> - If the file is successfully copied to a different volume and the original file is unable to be deleted, the function succeeds leaving the source file intact.
        /// <br/> - This value cannot be used with MOVEFILE_DELAY_UNTIL_REBOOT.
        /// </summary>
        MOVE_FILE_COPY_ALLOWED = 0x00000002,

        /// <summary>
        /// The system does not move the file until the operating system is restarted. The system moves the file immediately after AUTOCHK is executed, but before creating any paging files. 
        /// Consequently, this parameter enables the function to delete paging files from previous startups.
        /// <br/> - This value can only be used if the process is in the context of a user who belongs to the administrators group or the LocalSystem account.
        /// <br/> - This value cannot be used with MOVEFILE_COPY_ALLOWED.
        /// </summary>
        MOVE_FILE_DELAY_UNTIL_REBOOT = 0x00000004,

        /// <summary>
        /// The function does not return until the file has actually been moved on the disk. 
        /// <br/> - Setting this value guarantees that a move performed as a copy and delete operation is flushed to disk before the function returns. The flush occurs at the end of the copy operation. 
        /// <br/> - This value has no effect if MOVEFILE_DELAY_UNTIL_REBOOT is set.
        /// </summary>
        MOVE_FILE_WRITE_THROUGH = 0x00000008,

        /// <summary>
        /// Reserved for future use. 
        /// </summary>
        MOVE_FILE_CREATE_HARDLINK = 0x00000010,

        /// <summary>
        /// The function fails if the source file is a link source, but the file cannot be tracked after the move. 
        /// This situation can occur if the destination is a volume formatted with the FAT file system.
        /// </summary>
        MOVE_FILE_FAIL_IF_NOT_TRACKABLE = 0x00000020
    }
}
