using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoboSharp.Interfaces;

namespace RoboSharp
{
    /// <summary>
    /// Serialize IRoboCommands to RCJ files within a directory
    /// </summary>
    public class JobFileSerializer : IRoboQueueSerializer
    {

        /// <inheritdoc cref="Deserialize(string)"/>
        /// <inheritdoc cref="DirectoryInfo.DirectoryInfo(string)"/>
        public IRoboQueueDeserializer Deserialize(string path)
        {
            return Deserialize(new DirectoryInfo(path));
        }


        /// <summary>
        /// Create an <see cref=" IRoboQueueDeserializer"/> that reads all .RCJ files within the specified directory <paramref name="path"/>
        /// </summary>
        /// <param name="path">The directory to read .RCJ files from</param>
        /// <returns>A new <see cref="IRoboQueueDeserializer"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public virtual IRoboQueueDeserializer Deserialize(DirectoryInfo path)
        {
            return new JobFileDeserializer(path ?? throw new ArgumentNullException(nameof(path)));
        }

        /// <inheritdoc cref="Serialize(IEnumerable{IRoboCommand}, DirectoryInfo)"/>
        /// <inheritdoc cref="DirectoryInfo.DirectoryInfo(string)"/>
        public void Serialize(IEnumerable<IRoboCommand> commands, string path)
            => Serialize(commands, new DirectoryInfo(path));

        /// <inheritdoc cref="Serialize(IEnumerable{IRoboCommand}, string)"/>
        public void Serialize(string path, params IRoboCommand[] commands)
            => Serialize(commands, path);

        /// <inheritdoc cref="Serialize(IEnumerable{IRoboCommand}, DirectoryInfo)"/>
        public void Serialize(DirectoryInfo path, params IRoboCommand[] commands)
            => Serialize(commands, path);

        /// <summary>
        /// Process each command and save it as a JobFile
        /// </summary>
        /// <param name="commands">The commands to save as .RCJ files</param>
        /// <param name="path">A path to a Directory to save the RCJ files into</param>
        /// <exception cref="ArgumentNullException"/>
        public void Serialize(IEnumerable<IRoboCommand> commands, DirectoryInfo path)
        {
            DirectoryInfo dest = path ?? throw new ArgumentNullException(nameof(path));
            var cmdArr = commands?.ToList() ?? throw new ArgumentNullException(nameof(commands));
            foreach (var cmd in commands)
            {
                if (cmd is RoboCommand RC)
                {
                    RC.SaveAsJobFile(GetSavePath(RC)).Wait();
                }
                else
                {
                    var rc = new RoboCommand(cmd);
                    rc.SaveAsJobFile(GetSavePath(cmd)).Wait();
                }
            }

            string GetSavePath(IRoboCommand irc)
            {
                string name = string.IsNullOrWhiteSpace(irc.Name) ? "Command_" + cmdArr.IndexOf(irc) : irc.Name;
                name = Path.Combine(dest.FullName, name);
                return Path.ChangeExtension(name, ".rcj");
            }
        }

        private class JobFileDeserializer : IRoboQueueDeserializer
        {
            public JobFileDeserializer(DirectoryInfo dir)
            {
                if (dir is null) throw new ArgumentNullException(nameof(dir));
                enumerator = dir.EnumerateFiles("*.rcj").Select(JobFile.ParseJobFile);
            }

            private readonly IEnumerable<IRoboCommand> enumerator;
            public IEnumerable<IRoboCommand> ReadCommands() => enumerator;
        }
    }
}
