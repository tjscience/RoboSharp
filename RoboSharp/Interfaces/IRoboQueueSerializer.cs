using RoboSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RoboSharp.Interfaces
{
    /// <summary>
    /// Interface for objects to serialize / deserialize a collection of <see cref="IRoboCommand"/> objects
    /// </summary>
    public interface IRoboQueueSerializer
    {
        /// <summary>
        /// Serialize the IRoboCommands to a specified <paramref name="path"/>
        /// </summary>
        /// <param name="commands">The commands to serialize</param>
        /// <param name="path">The path to save the serialized commands into</param>
        void Serialize(IEnumerable<IRoboCommand> commands, string path);

        /// <summary>
        /// Deserialize the specified path into a collection of <see cref="IRoboCommand"/> objects
        /// </summary>
        /// <param name="path">The file/folder path to read</param>
        /// <returns>a new collection of <see cref="IRoboCommand"/> objects</returns>
        IEnumerable<IRoboCommand> Deserialize(string path);
    }
}
