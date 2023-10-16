using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp;
using RoboSharp.Interfaces;
using RoboSharp.UnitTests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RoboSharp.Extensions.UnitTests
{
    [TestClass]
    public class XmlSerializer
    {
        [TestMethod]
        public void Test_Serializer()
        {
            var serializer = new RoboSharp.Extensions.RoboCommandXmlSerializer();
            var commands = new IRoboCommand[]
            {
                new RoboCommand("Test1", "C:\\Test", "C:\\TestDest", true),
                new RoboCommand(null, null, copyActionFlags: CopyActionFlags.Purge, loggingFlags: LoggingFlags.IncludeFullPathNames),
                new RoboCommand(null, null, CopyActionFlags.MoveFilesAndDirectories, SelectionFlags.ExcludeLonely, LoggingFlags.NoDirectoryList)
            };
            commands[0].CopyOptions.AddFileFilter("*.pdf", "*.txt");
            commands[1].SelectionOptions.ExcludedDirectories.AddRange(new string[] { "Archive", "SomeDirectory" });
            commands[2].SelectionOptions.ExcludedFiles.AddRange(new string[] { "SomeFile.txt", "SomeOtherFile.pdf" });
            string path = Path.Combine(Test_Setup.TestDestination, "XmlSerializerTest.xml");
            serializer.Serialize(commands, path);
            Assert.IsTrue(File.Exists(path), "Failed to create file.");
            var readCommands = serializer.Deserialize(path).ReadCommands().ToArray();
            Assert.AreEqual(commands.Length, readCommands.Length, "\nParsed count != Input Count");

            for(int i = 0; i < commands.Length; i++)
            {
                Console.WriteLine("\nCommand Index " + i);
                Console.WriteLine("Input  : " + commands[i].CommandOptions);
                Console.WriteLine("Output : " + readCommands[i].CommandOptions);
                Assert.AreEqual(commands[i].CommandOptions, readCommands[i].CommandOptions, $"\n Command index {i} Input != Output");
            }
        }
    }
}
