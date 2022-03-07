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
        /// Error Code
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Error Description
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
