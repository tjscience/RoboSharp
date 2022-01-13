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
        /// 
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

        /// <summary>
        /// Parse some Job File
        /// </summary>
        /// <param name="filePath">File Extension should end in *.RCJ</param>
        /// <returns>New <see cref="RoboSharp.RoboCommand"/></returns>
        public static RoboCommand ParseJobFile(string filePath)
        {
            RoboSharp.RoboCommand cmd = new RoboSharp.RoboCommand();
            cmd.JobOptions.ParseJobFile(filePath);
            return cmd;
        }


        /// <summary>
        /// Parse some Job File
        /// </summary>
        /// <param name="filePath">File Extension should end in *.RCJ</param>
        /// <returns>New <see cref="RoboSharp.RoboCommand"/></returns>
        public void ParseJobFile(string filePath)
        {
            RoboSharp.RoboCommand cmd = this;
            if (File.Exists(filePath)
            {
                string[] lines = File.ReadAllLines(filePath));
                string l;
                string[] s;
                bool parsingIF = false;
                bool parsingXD = false;
                bool parsingXF = false;
                foreach (string line in lines)
                {
                    if (line.Trim().StartsWith("::"))
                    { /*comment line - ignore */
                        parsingIF = false;
                        parsingXD = false;
                        parsingXF = false;
                    }
                    //Source
                    else if (line.Trim().StartsWith("/SD"))
                    {
                        s = line.Split(':');
                        cmd.CopyOptions.Source = s[1];
                    }
                    //Destination
                    else if (line.Trim().StartsWith("/DD"))
                    {
                        s = line.Split(':');
                        cmd.CopyOptions.Destination = s[1];
                    }
                    //Include Files
                    else if (parsingIF || line.Trim().StartsWith("/IF"))
                    {
                        if (parsingIF)
                            cmd.CopyOptions.FileFilter += $"{line.RemoveJobComment()}";
                        else
                            parsingIF = true;
                    }
                    //Exclude Directories
                    else if (parsingXD || line.Trim().StartsWith("/XD"))
                    {
                        if (parsingXF)
                            cmd.SelectionOptions.ExcludeDirectories += $"{line.RemoveJobComment()}";
                        else
                            parsingXF = true;
                    }
                    //Exclude Files
                    else if (parsingXF || line.Trim().StartsWith("/XF"))
                    {
                        if (parsingXF)
                            cmd.SelectionOptions.ExcludeFiles += $"{line.RemoveJobComment()}";
                        else
                            parsingXF = true;
                    }

                    #region < Copy Options > 
                    else if (line.Trim().StartsWith("/PURGE"))
                        cmd.CopyOptions.Purge = true;
                    else if (line.Trim().StartsWith("/COPY:"))
                    {
                        //Must parse
                    }
                    else if (line.Trim().StartsWith("/DCOPY:"))
                    {
                        //Must parse
                    }
                    #endregion

                    #region < Retry Options > 
                    else if (line.Trim().StartsWith("/R:"))
                    {
                        l = line.RemoveJobComment().Split(':')[1];
                        cmd.RetryOptions.RetryCount = Convert.ToInt32(l);
                    }
                    else if (line.Trim().StartsWith("/W:"))
                    {
                        l = line.RemoveJobComment().Split(':')[1];
                        cmd.RetryOptions.RetryWaitTime = Convert.ToInt32(l);
                    }
                    #endregion

                    #region < Logging Options > 
                    else if (line.Trim().StartsWith("/R:"))
                    {
                        //To Do
                    }
                    else if (line.Trim().StartsWith("/W:"))
                    {

                    }
                    #endregion
                }
            }

        }
    }
}
