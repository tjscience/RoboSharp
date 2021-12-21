using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
        public int ExitCodeValue { get; protected set; }

        /// <summary>ExitCode reported by RoboCopy converted into the Enum</summary>
        public RoboCopyExitCodes ExitCode => (RoboCopyExitCodes)ExitCodeValue;

        /// <inheritdoc cref="RoboCopyExitCodes.FilesCopiedSuccessful"/>
        public bool Successful => ExitCodeValue < 0x10;

        /// <inheritdoc cref="RoboCopyExitCodes.MismatchedDirectoriesDetected"/>
        public bool HasWarnings => ExitCodeValue >= 0x4;

        /// <inheritdoc cref="RoboCopyExitCodes.SeriousErrorOccoured"/>
        public bool HasErrors => ExitCodeValue >= 0x10;

        /// <inheritdoc cref="RoboCopyExitCodes.Cancelled"/>
        public virtual bool WasCancelled => ExitCodeValue < 0x0;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return $"ExitCode: {ExitCodeValue} ({ExitCode})";
        }

    }

    /// <summary>
    /// Represents the combination of multiple Exit Statuses
    /// </summary>
    public class RoboCopyCombinedExitStatus : RoboCopyExitStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoboCopyCombinedExitStatus"/> class.
        /// </summary>
        public RoboCopyCombinedExitStatus() : base(0) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoboCopyCombinedExitStatus"/> class.
        /// </summary>
        public RoboCopyCombinedExitStatus(int exitCodeValue) : base(exitCodeValue) { }

        //Private bools for the Combine methods
        private bool wascancelled;
        private bool noCopyNoError;

        /// <summary>Overides <see cref="RoboCopyExitStatus.WasCancelled"/></summary>
        /// <returns> <see cref="AnyWasCancelled"/></returns>
        public override bool WasCancelled => AnyWasCancelled;

        /// <summary>
        /// Atleast one <see cref="RoboCopyExitStatus"/> objects combined into this result resulted in no errors and no files/directories copied.
        /// </summary>
        public bool AnyNoCopyNoError => noCopyNoError || ExitCodeValue == 0x0;

        /// <summary>
        /// Atleast one <see cref="RoboCopyExitStatus"/> object combined into this result had been cancelled / exited prior to completion.
        /// </summary>
        public bool AnyWasCancelled => wascancelled || ExitCodeValue < 0x0;

        /// <summary>
        /// All jobs completed without errors or warnings.
        /// </summary>
        public bool AllSuccessful => !WasCancelled && (ExitCodeValue == 0x0 || ExitCodeValue == 0x1);

        /// <summary>
        /// All jobs completed without errors or warnings, but Extra Files/Folders were detected.
        /// </summary>
        public bool AllSuccessful_WithWarnings => !WasCancelled && Successful && ExitCodeValue > 0x1;

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
                wascancelled = true;
            }
            else
            {
                if (status.ExitCode == 0x0) this.noCopyNoError = true; //0x0 is lost if any other values have been added, so this logs the state
                RoboCopyExitCodes code = this.ExitCode | status.ExitCode;
                this.ExitCodeValue = (int)code;
            }
        }

        /// <summary>
        /// Combine all the RoboCopyExitStatuses together.
        /// </summary>
        /// <param name="status">Array or List of ExitStatuses to combine.</param>
        public void CombineStatus(IEnumerable<RoboCopyExitStatus> status)
        {
            foreach (RoboCopyExitStatus s in status)
            {
                CombineStatus(s);
            }
        }

        /// <summary>
        /// Combine all the RoboCopyExitStatuses together.
        /// </summary>
        /// <param name="statuses">Array or List of ExitStatuses to combine.</param>
        /// <returns> new RoboCopyExitStatus object </returns>
        public static RoboCopyCombinedExitStatus CombineStatuses(IEnumerable<RoboCopyExitStatus> statuses)
        {
            RoboCopyCombinedExitStatus ret = new RoboCopyCombinedExitStatus(0);
            ret.CombineStatus(statuses);
            return ret;
        }

        /// <summary>
        /// Reset the value of the object
        /// </summary>
        public void Reset()
        {
            this.wascancelled = false;
            this.noCopyNoError = false;
            this.ExitCodeValue = 0;
        }


    }
}
