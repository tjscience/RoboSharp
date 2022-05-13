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
        /// Create a new <see cref="IRoboCommand"/> object using default settings.
        /// </summary>
        /// <remarks>
        /// This method is used by the other methods within the <see cref="RoboCommandFactory"/> to generate the inital <see cref="IRoboCommand"/> object that will be returned. 
        /// All settings are then applied to the object's options components.
        /// </remarks>
        /// <returns>new <see cref="IRoboCommand"/> object using the parameterless constructor</returns>
        public virtual IRoboCommand GetRoboCommand() => new RoboCommand();

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
        public virtual IRoboCommand GetRoboCommand(string source, string destination, CopyOptions.CopyActionFlags copyActionFlags, SelectionOptions.SelectionFlags selectionFlags)
        {
            var cmd = GetRoboCommand(source, destination);
            cmd.CopyOptions.ApplyActionFlags(copyActionFlags);
            cmd.SelectionOptions.ApplySelectionFlags(selectionFlags);
            return cmd;
        }

        /// <inheritdoc cref="GetRoboCommand(string, string, CopyOptions.CopyActionFlags, SelectionOptions.SelectionFlags)"/>
        public virtual IRoboCommand GetRoboCommand(string source, string destination, CopyOptions.CopyActionFlags copyActionFlags)
        {
            return GetRoboCommand(source, destination, copyActionFlags, SelectionOptions.SelectionFlags.Default);
        }
    }
}
