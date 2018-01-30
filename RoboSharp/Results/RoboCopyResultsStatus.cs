namespace RoboSharp.Results
{
    public class RoboCopyExitStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public RoboCopyExitStatus(int exitCodeValue)
        {
            ExitCodeValue = exitCodeValue;
        }

        public int ExitCodeValue { get; }

        public RoboCopyExitCodes ExitCode => (RoboCopyExitCodes)ExitCodeValue;

        public bool Successful => ExitCodeValue < 0x10;

        public bool HasWarnings => ExitCodeValue >= 0x4;

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