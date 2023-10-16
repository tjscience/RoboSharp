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
            };
            string path = Path.Combine(Test_Setup.TestDestination, "XmlSerializerTest.xml");
            serializer.Serialize(commands, path);
            Assert.IsTrue(File.Exists(path), "Failed to create file.");
            Assert.IsTrue(serializer.Deserialize(path).ReadCommands().Count() == commands.Length, "Failed to deserialize");
        }
    }
}
