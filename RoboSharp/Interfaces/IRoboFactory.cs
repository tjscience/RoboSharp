using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Interfaces
{
    /// <summary>
    /// Interface for a class factory object to produce <see cref="IRoboCommand"/> objects <br/>
    /// Usable by consumers to specify a factory object their library can rely on to create classes derived from the <see cref="RoboCommand"/> object. <br/>
    /// </summary>
    public interface IRoboFactory
    {
        /// <summary>
        /// Create a new <see cref="IRoboCommand"/> object using the parameterless constructor
        /// </summary>
        /// <returns>
        /// new <see cref="IRoboCommand"/> object
        /// </returns>
        /// <inheritdoc cref="RoboCommand.RoboCommand()"/>
        IRoboCommand GetRoboCommand();


        /// <summary>
        /// Create a new <see cref="IRoboCommand"/> with the specified source and destination
        /// </summary>
        /// <inheritdoc cref="RoboCommand.RoboCommand(string, string, bool)"/>
        IRoboCommand GetRoboCommand(string source, string destination);

        /*
         * The constructors within the region below have been intentionally left out of the interface. 
         * This is because these constructors are for more advanced usage of the object, and the base interface should only enforce the 
         *   what will most likely be the two most commonly used constructors. 
         *
         * Should consumers require the interface to be expanded, they can produce their own interface that is derived from this one.
         */
        #region

        ///// <summary>
        ///// Create a new <see cref="IRoboCommand"/> with the specified name
        ///// </summary>
        ///// <inheritdoc cref="RoboCommand.RoboCommand(string, bool)"/>
        //IRoboCommand GetRoboCommand(string name, bool stopIfDisposing = true);

        ///// <summary>
        ///// Create a new <see cref="IRoboCommand"/> with the specified source and destination
        ///// </summary>
        ///// <inheritdoc cref="RoboCommand.RoboCommand(string, string, bool)"/>
        //IRoboCommand GetRoboCommand(string source, string destination, bool stopIfDisposing = true);

        ///// <summary>
        ///// Create a new <see cref="IRoboCommand"/> with the specified source, destination, and name
        ///// </summary>
        ///// <inheritdoc cref="RoboCommand.RoboCommand(string, string, string, bool)"/>
        //IRoboCommand GetRoboCommand(string source, string destination, string name, bool stopIfDisposing = true);


        ///// <inheritdoc cref="RoboCommand.RoboCommand(string, string, string, bool, RoboSharpConfiguration, CopyOptions, SelectionOptions, RetryOptions, LoggingOptions, JobOptions)"/>
        //IRoboCommand GetRoboCommand(string name, string source = null, string destination = null, bool stopIfDisposing = true, RoboSharpConfiguration configuration = null, CopyOptions copyOptions = null, SelectionOptions selectionOptions = null, RetryOptions retryOptions = null, LoggingOptions loggingOptions = null, JobOptions jobOptions = null);


        ///// <inheritdoc cref="RoboCommand.RoboCommand(RoboCommand, string, string, bool, bool, bool, bool, bool)"/>
        //IRoboCommand GetRoboCommand(RoboCommand command, string NewSource = null, string NewDestination = null, bool LinkConfiguration = true, bool LinkRetryOptions = true, bool LinkSelectionOptions = false, bool LinkLoggingOptions = false, bool LinkJobOptions = false);

        #endregion
    }
}
