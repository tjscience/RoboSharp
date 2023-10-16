using RoboSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RoboSharp.Interfaces
{
    /// <summary>
    /// An object that represents a collection of deserialized IRoboCommands
    /// </summary>
    public interface IRoboQueueDeserializer
    {
        /// <summary>
        /// Enumerate the IRoboCommands
        /// </summary>
        /// <returns>An IEnumerable that returns deserialized IRoboCommand objects</returns>
        IEnumerable<IRoboCommand> ReadCommands();
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IRoboQueueSerializer
    {
        /// <summary>
        /// Serialize the IRoboCommands into a <see cref="IRoboQueueDeserializer"/>
        /// </summary>
        /// <param name="commands">The commands to serialize</param>
        /// <param name="path">The path to save the serialized commands into</param>
        /// <returns>a new <see cref="IRoboQueueDeserializer"/></returns>
        void Serialize(IEnumerable<IRoboCommand> commands, string path);

        /// <summary>
        /// Read the file path and produce an <see cref="IRoboQueueDeserializer"/>
        /// </summary>
        /// <param name="path">The file path to read</param>
        /// <returns>a new <see cref="IRoboQueueDeserializer"/></returns>
        IRoboQueueDeserializer Deserialize(string path);
    }
}
