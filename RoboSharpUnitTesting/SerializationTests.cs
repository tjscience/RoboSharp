using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp;
using RoboSharp.Interfaces;
using RoboSharp.UnitTests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RoboSharp.UnitTests
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void Test_JobFileSerializer()
        {
            IRoboCommand[] arr1 = new IRoboCommand[]
            {
                new RoboCommand("Job1", true),
                new RoboCommand("Job2", true),
                new RoboCommand("Job3", true)
            };
            string path = Path.Combine(Test_Setup.TestDestination);
            var serializer = new JobFileSerializer();
            serializer.Serialize(arr1, path);
            var obj = serializer.Deserialize(path);
            Assert.IsTrue(obj.Count() == 3);
        }
    }
}
