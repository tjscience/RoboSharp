using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RoboSharp.JobFileRegex;
using JobRegex = RoboSharp.JobFileRegex;

namespace RoboSharp
{
    /// <summary>
    /// Represents a single RoboCopy Job File
    /// </summary>
    /// <remarks>
    /// For more information, a good resource is here: <see href="https://adamtheautomator.com/robocopy/#Robocopy_Jobs"/>
    /// </remarks>
    public class JobFile : ICloneable
    {
        // 

        #region < Constructor >
        
        private JobFile() { }

        public JobFile(RoboCommand cmd, string filePath)
        {
            RoboCmd = cmd;
            FilePath = filePath;
        }


        public JobFile(RoboCommand cmd, string filePath, bool ParseImmediately)
        {
            RoboCmd = cmd;
            FilePath = filePath;
            if (ParseImmediately) this.Parse();
        }

        /// <summary>
        /// Constructor for ICloneable Interface
        /// </summary>
        /// <param name="jobFile"></param>
        public JobFile(JobFile jobFile)
        {
        }


        #endregion

        #region < ICLONEABLE >

        /// <summary>
        /// Create a clone of this JobFile
        /// </summary>
        public JobFile Clone() => new JobFile(this);

        object ICloneable.Clone() => Clone();

        #endregion

        #region < Constants >

        /// <summary>
        /// Any comments within the job file lines will start with this string
        /// </summary>
        public const string JOBFILE_CommentPrefix = ":: ";

        /// <inheritdoc cref="JobOptions.JOB_FileExtension"/>
        public const string JOBFILE_Extension = JobOptions.JOB_FileExtension;

        /// <inheritdoc cref="JobOptions.JOB_FileExtension"/>
        internal const string JOBFILE_JobName = ":: JOB_NAME: ";

        /// <summary>
        /// Regex to check if a string is a comment
        /// </summary>
        internal static Regex REGEX_IsComment = new Regex("^\\s*(?<COMMENT>::.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Regex to check if the string is a flag for RoboCopy
        /// </summary>
        internal static Regex REGEX_IsSwitch = new Regex("^\\s*(?<SWITCH>\\/[A-Za-z])(?<DELIMITER>:)(?<VALUE>.*)(?<COMMENT>::.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// JobName for ROboCommand is not valid parameter for RoboCopy, so we save it into a comment within the file
        /// </summary>
        internal static Regex REGEX_JOB_NAME = new Regex("^\\s*(?<COMMENT>.*::JOB_NAME:\\s*)(?<Name>.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);


        #endregion

        #region < Fields >

        /// <summary>
        /// Options are stored in a RoboCommand object for simplicity.
        /// </summary>
        private RoboCommand roboCommand;

        #endregion

        #region < Properties >

        /// <summary>The RoboCommand object this JobFile is assigned to </summary>
        public RoboCommand RoboCmd { get; private set; }

        /// <summary>Log if this JobFile has been parsed yet </summary>
        public bool HasBeenParsed { get; private set; }

        /// <summary>FilePath of the Job File </summary>
        public string FilePath { get; set; }

        #endregion

        #region < Methods >

        /// <summary> Reset the HasBeenParsed flag </summary>
        public void ResetHasBeenParsed() => HasBeenParsed = false;

        //Put the Parser method I wrote above here - Make sure to update HasBeenParsed=true once complete !

        #endregion


    }
}
