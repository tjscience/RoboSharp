using RoboSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp
{
    /// <summary>
    /// Object that provides methods to generate new <see cref="RoboCommand"/> objects using the public constructors. 
    /// <br/> This class can not be inherited
    /// </summary>
    public sealed class RoboCommandFactory : IRoboCommandFactoryBase
    {
        /// <inheritdoc cref="RoboCommandFactory"/>
        public static RoboCommandFactory Factory { get; } = new RoboCommandFactory();

        #region < RoboCommand Generation >

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public RoboCommand GetRoboCommand()

        {
            return new RoboCommand();
        }

        public RoboCommand GetRoboCommand(string name, bool stopIfDisposing = true)
        {
            return new RoboCommand(name, stopIfDisposing);
        }

        public RoboCommand GetRoboCommand(string source, string destination, bool stopIfDisposing = true)
        {
            return new RoboCommand(source, destination, stopIfDisposing);
        }

        public RoboCommand GetRoboCommand(string source, string destination, string name, bool stopIfDisposing = true)
        {
            return new RoboCommand(source, destination, name, stopIfDisposing);
        }

        public RoboCommand GetRoboCommand(string name, string source = null, string destination = null, bool stopIfDisposing = true,
            RoboSharpConfiguration configuration = null,
            CopyOptions copyOptions = null,
            SelectionOptions selectionOptions = null,
            RetryOptions retryOptions = null,
            LoggingOptions loggingOptions = null,
            JobOptions jobOptions = null)
        {
            return new RoboCommand(name, source, destination, stopIfDisposing, configuration, copyOptions, selectionOptions, retryOptions, loggingOptions, jobOptions);
        }

        public RoboCommand GetRoboCommand(
            RoboCommand command,
            string NewSource = null,
            string NewDestination = null,
            bool LinkConfiguration = true,
            bool LinkRetryOptions = true,
            bool LinkSelectionOptions = false,
            bool LinkLoggingOptions = false,
            bool LinkJobOptions = false)
        {
            return new RoboCommand(command, NewSource, NewDestination, LinkConfiguration, LinkRetryOptions, LinkSelectionOptions, LinkLoggingOptions, LinkJobOptions);
        }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion

        #region < Interface Implementation >

        IRoboCommand IRoboCommandFactoryBase.GetRoboCommand()
            => Factory.GetRoboCommand();


        IRoboCommand IRoboCommandFactoryBase.GetRoboCommand(string source, string destination)
            => Factory.GetRoboCommand(source, destination, true);

        //IRoboCommand IRoboFactory.GetRoboCommand(string name, bool stopIfDisposing)
        //    => ((IRoboFactory)Factory).GetRoboCommand(name, stopIfDisposing);

        //IRoboCommand IRoboFactory.GetRoboCommand(string source, string destination, bool stopIfDisposing)
        //    => ((IRoboFactory)Factory).GetRoboCommand(source, destination, stopIfDisposing);

        //IRoboCommand IRoboFactory.GetRoboCommand(string source, string destination, string name, bool stopIfDisposing)
        //    => ((IRoboFactory)Factory).GetRoboCommand(source, destination, name, stopIfDisposing);


        //IRoboCommand IRoboFactory.GetRoboCommand(string name, string source, string destination, bool stopIfDisposing, RoboSharpConfiguration configuration, CopyOptions copyOptions, SelectionOptions selectionOptions, RetryOptions retryOptions, LoggingOptions loggingOptions, JobOptions jobOptions)
        //    => ((IRoboFactory)Factory).GetRoboCommand(name, source, destination, stopIfDisposing, configuration, copyOptions, selectionOptions, retryOptions, loggingOptions, jobOptions);


        //IRoboCommand IRoboFactory.GetRoboCommand(RoboCommand command, string NewSource, string NewDestination, bool LinkConfiguration, bool LinkRetryOptions, bool LinkSelectionOptions, bool LinkLoggingOptions, bool LinkJobOptions)
        //    => ((IRoboFactory)Factory).GetRoboCommand(command, NewSource, NewDestination, LinkConfiguration, LinkRetryOptions, LinkSelectionOptions, LinkLoggingOptions, LinkJobOptions);


        #endregion

    }
}
