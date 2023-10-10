using System;
using System.Collections.Generic;
using System.Text;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// Provides options specific to the <see cref="RoboMover"/> class
    /// </summary>
    public class RoboMoverOptions
    {
        /// <summary>
        /// When enabled, RoboMover will move the directory as a whole, instead of digging through to move each file individually.
        /// <br/> This setting is only applicable to directories that exist in the source, but not in the destination.
        /// <br/> If a directory exists at source and destination, it will dig through that directory and compare each file per normal operating rules.
        /// </summary>
        public bool QuickMove { get; set; }
    }
}
