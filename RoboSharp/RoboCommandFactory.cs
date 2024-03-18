using RoboSharp.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp
{
    
    /// <summary>
    /// Object that provides methods to generate new <see cref="IRoboCommand"/> objects.
    /// </summary>
    public class RoboCommandFactory : IRoboCommandFactory
    {
        /// <summary>
        /// The default <see cref="IRoboCommandFactory"/> that will produce <see cref="RoboCommand"/> objects
        /// </summary>
        public static readonly IRoboCommandFactory Default = new DefaultRobotCommandFactory();

        /// <summary>
        /// Create a new RoboCommandFactory using a default configuration
        /// </summary>
        public RoboCommandFactory() : this(new RoboSharpConfiguration()) { }

        /// <summary>
        /// Create a new RoboCommandFactory using the specified configuration
        /// </summary>
        /// <param name="configuration"></param>
        public RoboCommandFactory(RoboSharpConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// The <see cref="RoboSharpConfiguration"/> to apply to all constructed <see cref="IRoboCommand"/> objects.
        /// </summary>
        public RoboSharpConfiguration Configuration { get; }

        /// <summary>
        /// The Default <see cref="CopyActionFlags"/> to apply when generating an <see cref="IRoboCommand"/> using this factory.
        /// </summary>
        public CopyActionFlags DefaultCopyOptions { get; set; } = CopyActionFlags.Default;

        /// <summary>
        /// The Default <see cref="LoggingFlags"/> to apply when generating an <see cref="IRoboCommand"/> using this factory.
        /// </summary>
        public LoggingFlags DefaultLoggingOptions { get; set; } = LoggingFlags.RoboSharpDefault;

        /// <summary>
        /// The Default <see cref="SelectionFlags"/> to apply when generating an <see cref="IRoboCommand"/> using this factory.
        /// </summary>
        public SelectionFlags DefaultSelectionOptions { get; set; } = SelectionFlags.Default;

        /// <summary>
        /// Create a new <see cref="IRoboCommand"/> object using default settings.
        /// </summary>
        /// <remarks>
        /// This method is used by the other methods within the <see cref="RoboCommandFactory"/> to generate the inital <see cref="IRoboCommand"/> object that will be returned. 
        /// <br/>All settings are then applied to the object's options components (such as the source/destination parameters)
        /// <br/>As such, overriding this one method will to provide will provide the other factory methods with the customized default IRobocommand object.
        /// </remarks>
        /// <returns>new <see cref="IRoboCommand"/> object using the parameterless constructor</returns>
        public virtual IRoboCommand GetRoboCommand()
        {
            return new RoboCommand(
                name: "",
                configuration: Configuration,
                copyOptions: new CopyOptions(string.Empty, string.Empty, DefaultCopyOptions),
                selectionOptions: new SelectionOptions(DefaultSelectionOptions),
                retryOptions: new RetryOptions(),
                loggingOptions: new LoggingOptions(DefaultLoggingOptions)
                 );
        }

        /// <summary>
        /// Create a new <see cref="IRoboCommand"/> object with the specified <paramref name="source"/> and <paramref name="destination"/>.
        /// </summary>
        /// <remarks/>
        /// <param name="source"><inheritdoc cref="CopyOptions.Source" path="*"/></param>
        /// <param name="destination"><inheritdoc cref="CopyOptions.Destination" path="*"/></param>
        /// <returns>new <see cref="IRoboCommand"/> object with the specified <paramref name="source"/> and <paramref name="destination"/>.</returns>
        /// <inheritdoc cref="GetRoboCommand()"/>
        public virtual IRoboCommand GetRoboCommand(string source, string destination)
        {
            var cmd = GetRoboCommand();
            cmd.CopyOptions.Source = source;
            cmd.CopyOptions.Destination = destination;
            return cmd;
        }

        /// <summary>
        /// Create a new <see cref="IRoboCommand"/> object with the specified options
        /// </summary>
        /// <remarks/>
        /// <param name="copyActionFlags">The options to apply to the generated <see cref="IRoboCommand"/> object </param>
        /// <param name="selectionFlags">The options to apply to the generated <see cref="IRoboCommand"/> object </param>
        /// <inheritdoc cref="GetRoboCommand(string, string)"/>]
        /// <param name="destination"/><param name="source"/>
        public virtual IRoboCommand GetRoboCommand(string source, string destination, CopyActionFlags copyActionFlags, SelectionFlags selectionFlags)
        {
            var cmd = GetRoboCommand(source, destination);
            cmd.CopyOptions.ApplyActionFlags(copyActionFlags);
            cmd.SelectionOptions.ApplySelectionFlags(selectionFlags);
            return cmd;
        }

        /// <inheritdoc cref="GetRoboCommand(string, string, CopyActionFlags, SelectionFlags)"/>
        public virtual IRoboCommand GetRoboCommand(string source, string destination, CopyActionFlags copyActionFlags)
        {
            return GetRoboCommand(source, destination, copyActionFlags, DefaultSelectionOptions);
        }

        private class DefaultRobotCommandFactory : IRoboCommandFactory
        {
            public IRoboCommand GetRoboCommand() => new RoboCommand(string.Empty, string.Empty, CopyActionFlags.Default, SelectionFlags.Default, LoggingFlags.RoboSharpDefault);
            public IRoboCommand GetRoboCommand(string source, string destination) => new RoboCommand(source, destination, CopyActionFlags.Default, SelectionFlags.Default, LoggingFlags.RoboSharpDefault);
            public IRoboCommand GetRoboCommand(string source, string destination, CopyActionFlags copyActionFlags) => new RoboCommand(source, destination, copyActionFlags, SelectionFlags.Default, LoggingFlags.RoboSharpDefault);
            public IRoboCommand GetRoboCommand(string source, string destination, CopyActionFlags copyActionFlags, SelectionFlags selectionFlags) => new RoboCommand(source, destination, copyActionFlags, selectionFlags, LoggingFlags.RoboSharpDefault);
        }
    }
}
