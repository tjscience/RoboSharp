using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboSharp
{
    /// <summary>
    /// 
    /// </summary>
    public class JobOptions : ICloneable
    {
        // For more information, a good resource is here: https://adamtheautomator.com/robocopy/#Robocopy_Jobs

        #region < Constructors >

        /// <summary>
        /// 
        /// </summary>
        public JobOptions() { }

        /// <summary>
        /// Constructor for ICloneable Interface
        /// </summary>
        /// <param name="options">JobOptions object to clone</param>
        public JobOptions(JobOptions options) 
        {
            
        }

        #endregion

        #region < ICloneable >

        /// <summary>
        /// Clone this JobOptions object
        /// </summary>
        /// <returns>New JobOptions object</returns>
        public JobOptions Clone() => new JobOptions(this);

        object ICloneable.Clone() => Clone();

        #endregion

        #region < Constants >

        /// <summary>
        /// Expected File Extension for RoboCopy Job Files
        /// </summary>
        public const string JOB_FileExtension = ".RCJ";

        /// <summary>
        /// Take parameters from the named job file
        /// </summary>
        /// <remarks>
        /// Usage: /JOB:"Path\To\File.RCJ"
        /// </remarks>
        internal const string JOB_LOADNAME = " /JOB:";

        /// <summary>
        /// Save parameters to the named job file
        /// </summary>
        /// <remarks>
        /// Usage: <br/>
        /// /SAVE:"Path\To\File" -> Creates Path\To\File.RCJ <br/>
        /// /SAVE:"Path\To\File.txt" -> Creates Path\To\File.txt.RCJ <br/>
        /// </remarks>
        internal const string JOB_SAVE = " /SAVE:";

        /// <summary>
        /// Quit after processing command line
        /// </summary>
        /// <remarks>
        /// Used when writing JobFile
        /// </remarks>
        internal const string JOB_QUIT = " /QUIT";

        /// <summary>
        /// No source directory is specified
        /// </summary>
        internal const string JOB_NoSourceDirectory = "/NOSD";

        /// <summary>
        /// No destination directory is specified
        /// </summary>
        internal const string JOB_NoDestinationDirectory = "/NODD";

        /// <summary>
        /// Include the following files
        /// </summary>
        internal const string JOB_IncludeFiles = "/IF";

        #endregion

        #region < Properties >

        #endregion

        #region < Methods >

        /// <summary>
        /// Parse the properties and return the string
        /// </summary>
        /// <returns></returns>
        internal string Parse(string SavePath)
        {
            return JOB_SAVE + SavePath.WrapPath() + JOB_QUIT;
        }

        #endregion
    }
}
