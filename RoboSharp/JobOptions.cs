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
    /// <remarks>
    /// <see href="https://github.com/tjscience/RoboSharp/wiki/JobOptions"/>
    /// </remarks>
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
            FilePath = options.FilePath;
            NoDestinationDirectory = options.NoDestinationDirectory;
            NoSourceDirectory = options.NoSourceDirectory;
            PreventCopyOperation = options.PreventCopyOperation;
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
        internal const string JOB_NoSourceDirectory = " /NOSD";

        /// <summary>
        /// No destination directory is specified
        /// </summary>
        internal const string JOB_NoDestinationDirectory = " /NODD";

        #endregion

        #region < Properties >

        /// <summary>
        /// FilePath to save the Job Options (.RCJ) file to. <br/>
        /// /SAVE:{FilePath}
        /// </summary>
        /// <remarks>
        /// This causes RoboCopy to generate an RCJ file where the command options are stored to so it can be used later.<br/>
        /// <see cref="NoSourceDirectory"/> and <see cref="NoDestinationDirectory"/> options are only evaluated if this is set. <br/>
        /// </remarks>
        public virtual string FilePath { get; set; } = "";

        /// <summary>
        /// RoboCopy will validate the command, then exit before performing any Move/Copy/List operations. <br/>
        /// /QUIT
        /// </summary>
        /// <remarks>
        /// This option is typically used when generating JobFiles. RoboCopy will exit after saving the Job FIle to the specified <see cref="FilePath"/>
        /// </remarks>
        public virtual bool PreventCopyOperation { get; set; }

        /// <summary>
        /// <see cref="CopyOptions.Source"/> path will not be saved to the JobFile. <br/>
        /// /NOSD
        /// </summary>
        /// <remarks>
        /// Default value is False, meaning if <see cref="CopyOptions.Source"/> is set, it will be saved to the JobFile RoboCopy generates.
        /// </remarks>
        public virtual bool NoSourceDirectory { get; set; }

        /// <summary>
        /// <see cref="CopyOptions.Destination"/> path will not be saved to the JobFile. <br/>
        /// /NODD
        /// </summary>
        /// <remarks>
        /// Default value is False, meaning if <see cref="CopyOptions.Destination"/> is set, it will be saved to the JobFile RoboCopy generates.
        /// </remarks>
        public virtual bool NoDestinationDirectory { get; set; }
        #endregion

        #region < Methods >

        /// <summary>
        /// Parse the properties and return the string
        /// </summary>
        /// <returns></returns>
        internal string Parse()
        {
            string options = "";
            if (!FilePath.IsNullOrWhiteSpace())
            {
                options += $"{JOB_SAVE}{FilePath.WrapPath()}";
                if (NoSourceDirectory) options += JOB_NoSourceDirectory;
                if (NoDestinationDirectory) options += JOB_NoDestinationDirectory;
            }
            if (PreventCopyOperation) options += JOB_QUIT;
            return options;
        }

        /// <summary>
        /// Adds the 'NAME' and other properties into the JobFile
        /// </summary>
        internal void RunPostProcessing(RoboCommand cmd)
        {
            if (FilePath.IsNullOrWhiteSpace()) return;
            if (!File.Exists(FilePath)) return;
            var txt = File.ReadAllLines(FilePath).ToList();
            //Write Options to JobFile
            txt.InsertRange(6, new string[]
            {
                "",
                "::",
                ":: Options for RoboSharp.JobFile",
                "::",
                JobFileBuilder.JOBFILE_JobName + " " + cmd.Name,
                JobFileBuilder.JOBFILE_StopIfDisposing + " " + cmd.StopIfDisposing.ToString().ToUpper()
            });
            File.WriteAllLines(FilePath, txt);
        }

        /// <summary>
        /// Combine this object with another RetryOptions object. <br/>
        /// <see cref="FilePath"/> not not be modified.
        /// </summary>
        /// <param name="options"></param>
        public void Merge(JobOptions options)
        {
            NoSourceDirectory |= options.NoSourceDirectory;
            NoDestinationDirectory |= options.NoDestinationDirectory;
            PreventCopyOperation |= options.PreventCopyOperation;
        }

        #endregion
    }
}
