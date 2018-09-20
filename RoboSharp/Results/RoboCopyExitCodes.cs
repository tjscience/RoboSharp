using System;

namespace RoboSharp.Results
{
    [Flags]
    public enum RoboCopyExitCodes
    {
        NoErrorNoCopy = 0x0,
        FilesCopiedSuccessful = 0x1,
        ExtraFilesOrDirectoriesDetected = 0x2,
        MismatchedDirectoriesDetected = 0x4,
        SomeFilesOrDirectoriesCouldNotBeCopied = 0x8,
        SeriousErrorOccoured = 0x10,
    }
}