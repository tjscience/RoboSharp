using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp;
using RoboSharp.Interfaces;
using RoboSharp.Results;
using RoboSharp.UnitTests;
using System;
using System.Text;

namespace RoboSharp.UnitTests
{
    [TestClass]
    public class SpeedStatisticTests
    {
        [TestMethod]
        public void ParseTest_CommaThousands() => ParseInputTest("538,252,733", 538252733m, "30,799.068", 30799.068m);

        [TestMethod]
        public void ParseTest_PeriodThousands() => ParseInputTest("538.252.733", 538252733m, "30.799,068", 30799.068m);

        private void ParseInputTest(string sBPS, decimal bps, string sMB, decimal MB)
        {
            Console.Write("Validating SpeedStatistic.Parse()");
            var stat = SpeedStatistic.Parse(sBPS, sMB);
            Assert.AreEqual(bps, stat.BytesPerSec, "\nBytes/second does not match!");
            Assert.AreEqual(MB, stat.MegaBytesPerMin, "\nMegabytes/min does not match!");
            Console.WriteLine(" -- OK");
        }

        [DataRow(" Velocità:           257.555.063 Byte/sec.", " Velocità:             14737,419 MB/min.")]
        [TestMethod]
        public void ParseText(string sBPS, string sMB) 
        {
            var value = SpeedStatistic.Parse(sBPS, sMB);
            Assert.IsNotNull(value);
            Console.WriteLine(value.ToString());
        }
    }
}
