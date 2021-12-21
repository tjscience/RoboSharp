using System.Collections.Generic;

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
            IsCombinedStatus = false;
        }

        /// <summary>ExitCode as reported by RoboCopy</summary>
        public int ExitCodeValue { get; private set; }

        /// <summary>ExitCode reported by RoboCopy converted into the Enum</summary>
        public RoboCopyExitCodes ExitCode => (RoboCopyExitCodes)ExitCodeValue;

        /// <inheritdoc cref="RoboCopyExitCodes.FilesCopiedSuccessful"/>
        public bool Successful => ExitCodeValue < 0x10;

        /// <inheritdoc cref="RoboCopyExitCodes.MismatchedDirectoriesDetected"/>
        public bool HasWarnings => ExitCodeValue >= 0x4;

        /// <inheritdoc cref="RoboCopyExitCodes.SeriousErrorOccoured"/>
        public bool HasErrors => ExitCodeValue >= 0x10;

        /// <inheritdoc cref="RoboCopyExitCodes.Cancelled"/>
        public bool WasCancelled => wascancelled || ExitCodeValue < 0x0;

        /// <summary> 
        /// If this object is the result of two RoboCopyExitStatus objects being combined, this will be TRUE. <br/>
        /// Otherwise this will be false. 
        /// </summary>
        public bool IsCombinedStatus { get; private set; }

        /// <summary> This is purely to facilitate the CombineStatus method </summary>
        private bool wascancelled;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return $"ExitCode: {ExitCodeValue} ({ExitCode})";
        }

        /// <summary>
        /// Combine the RoboCopyExitCodes of the supplied ExitStatus with this ExitStatus.
        /// </summary>
        /// <remarks>If any were Cancelled, set the WasCancelled property to TRUE. Otherwise combine the exit codes.</remarks>
        /// <param name="status">ExitStatus to combine with</param>
#if !NET40
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
#endif
        public void CombineStatus(RoboCopyExitStatus status)
        {
            if (status == null) return;
            if (status.WasCancelled)
            {
                this.wascancelled = true;
            }
            else
            {
                RoboCopyExitCodes code = this.ExitCode & status.ExitCode;
                this.ExitCodeValue = (int)code;
            }
            this.IsCombinedStatus = true;
        }

        /// <summary>
        /// Combine all the RoboCopyExitStatuses together.
        /// </summary>
        /// <param name="status">Array or List of ExitStatuses to combine.</param>
        public void CombineStatus(IEnumerable<RoboCopyExitStatus> status)
        {
            foreach (RoboCopyExitStatus s in status)
            {
                this.CombineStatus(s);
            }
        }

        /// <summary>
        /// Combine all the RoboCopyExitStatuses together.
        /// </summary>
        /// <param name="statuses">Array or List of ExitStatuses to combine.</param>
        /// <returns> new RoboCopyExitStatus object </returns>
        public static RoboCopyExitStatus CombineStatuses(IEnumerable<RoboCopyExitStatus> statuses)
        {
            RoboCopyExitStatus ret = new RoboCopyExitStatus(0);
            ret.CombineStatus(statuses);
            return ret;
        }

    }
}
