using System;
using System.Linq;
using System.Text.RegularExpressions;

// Do Not change NameSpace here! -> Must be RoboSharp due to prior releases
namespace RoboSharp
{
    /// <summary>
    /// Information about an Error reported by the RoboCopy process
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// The full text of the error code
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Error Description provided by RoboSharp (based on the Error Code)
        /// </summary>
        public string ErrorDescription { get; }
        
        /// <summary>
        /// Error Code
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// Signed Error Code
        /// </summary>
        public string SignedErrorCode { get; }

        /// <summary>
        /// The File or Directory Path the Error refers to
        /// </summary>
        public string ErrorPath { get; }

        /// <summary>
        /// DateTime the error occurred
        /// </summary>
        public DateTime DateTime { get; }

        /// <summary>
        /// Exception Data - Can only be used by custom implementations. RoboCommand will not provide exception data.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Concatenate the <see cref="Error"/> and <see cref="ErrorDescription"/> into a string seperated by an <see cref="Environment.NewLine"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (ErrorDescription.IsNullOrWhiteSpace())
                return Error;
            else
                return String.Format("{0}{1}{2}", Error, Environment.NewLine, ErrorDescription);
        }

        /// <summary>
        /// Generate a new set of ErrorEventArgs
        /// </summary>
        /// <param name="errorMessage"><inheritdoc cref="Error" path="*"/></param>
        /// <param name="errorDescrip"><inheritdoc cref="ErrorDescription" path="*"/></param>
        /// <param name="errorCode"><inheritdoc cref="ErrorCode" path="*"/></param>
        /// <param name="signedErrorCode"><inheritdoc cref="SignedErrorCode" path="*"/></param>
        /// <param name="errorPath"><inheritdoc cref="ErrorPath" path="*"/></param>
        /// <param name="time"><inheritdoc cref="DateTime" path="*"/></param>
        public ErrorEventArgs(string errorMessage, string errorDescrip, int errorCode, string signedErrorCode, string errorPath, DateTime time)
        {
            Error = errorMessage;
            ErrorDescription = errorDescrip;
            ErrorCode = errorCode;
            SignedErrorCode = signedErrorCode;
            ErrorPath = errorPath;
            DateTime = time;
        }

        /// <summary>
        /// Generate a new set of ErrorEventArgs.
        /// </summary>
        /// <param name="exception">The exception that occurred</param>
        /// <param name="errorPath"><inheritdoc cref="ErrorPath" path="*"/></param>
        /// <param name="time"><inheritdoc cref="DateTime" path="*"/></param>
        public ErrorEventArgs(Exception exception, string errorPath, DateTime time)
        {
            Error = exception?.Message;
            ErrorDescription = exception?.StackTrace;
            ErrorCode = exception?.HResult ?? 0;
            SignedErrorCode = (exception?.HResult ?? 0).ToString() ;
            ErrorPath = errorPath;
            DateTime = time;
            Exception = exception;
        }

        /// <summary>
        /// <inheritdoc cref="ErrorEventArgs"/>
        /// </summary>
        /// <param name="errorData"><inheritdoc cref="Error"/></param>
        /// <param name="descripData"><inheritdoc cref="ErrorCode"/></param>
        /// <param name="errTokenRegex">
        /// Regex used to split the Error Code into its various parts. <br/>
        /// Must have the following groups: Date, ErrCode, SignedErrCode, Descrip, Path
        /// </param>
        internal ErrorEventArgs(string errorData, string descripData, Regex errTokenRegex)
        {
            var match = errTokenRegex.Match(errorData);
            var groups = match.Groups;

            //Date
            string dateStr = groups["Date"].Value;
            if (DateTime.TryParse(dateStr, out var DT))
                this.DateTime = DT;
            else
                this.DateTime = DateTime.Now;

            //Path
            ErrorPath = groups["Path"].Value;

            //Error Code
            ErrorCode = Convert.ToInt32(groups["ErrCode"].Value);
            SignedErrorCode = groups["SignedErrCode"].Value;

            //Error String
            Error = errorData;
            ErrorDescription = descripData;
            
        }
    }
}
