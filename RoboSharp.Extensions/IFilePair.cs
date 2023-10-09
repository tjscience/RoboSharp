using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// Interface used for extension methods for RoboSharp custom implementations that has Source/Destination FileInfo 
    /// </summary>
    public interface IFilePair
    {   
        /// <summary>
        /// Source File Information
        /// </summary>
        FileInfo Source { get; }

        /// <summary>
        /// Destination File Information
        /// </summary>
        FileInfo Destination { get; }
    }
}
