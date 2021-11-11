namespace RoboSharp.Results
{
    /// <summary>
    /// Object that evaluates the ExitCode reported after RoboCopy finishes executing.
    /// </summary>
    public class RoboCopyExitStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public RoboCopyExitStatus(int exitCodeValue)
        {
            ExitCodeValue = exitCodeValue;
        }

        /// <summary>ExitCode as reported by RoboCopy</summary>
        public int ExitCodeValue { get; }

        /// <summary>ExitCode reported by RoboCopy converted into the Enum</summary>
        public RoboCopyExitCodes ExitCode => (RoboCopyExitCodes)ExitCodeValue;
        
        /// <inheritdoc cref="RoboCopyExitCodes.FilesCopiedSuccessful"/>
        public bool Successful => ExitCodeValue < 0x10;

        /// <inheritdoc cref="RoboCopyExitCodes.MismatchedDirectoriesDetected"/>
        public bool HasWarnings => ExitCodeValue >= 0x4;
        
        /// <inheritdoc cref="RoboCopyExitCodes.SeriousErrorOccoured"/>
        public bool HasErrors => ExitCodeValue >= 0x10;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return $"ExitCode: {ExitCodeValue} ({ExitCode})";
        }
    }
}