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
        public void ParseTest()
        {
            string bps_string = " 538,252,733";
            decimal bps = 538252733;

            string mbpm_string = "30,799.068";
            decimal mbpm = 30799.068m;

            Console.Write("Validating Decimal.Parse()");
            Assert.AreEqual(bps, decimal.Parse(bps_string), "\ndecimal parsing fails - bps");
            Assert.AreEqual(mbpm, decimal.Parse(mbpm_string), "\ndecimal parsing fails - mbpm");
            Console.WriteLine(" -- OK");

            Console.Write("Validating SpeedStatistic.Parse()");
            var stat = SpeedStatistic.Parse(bps_string, mbpm_string);
            Assert.AreEqual(bps, stat.BytesPerSec, "\nBytes/second does not match!");
            Assert.AreEqual(mbpm, stat.MegaBytesPerMin, "\nMegabytes/min does not match!");
            Console.WriteLine(" -- OK");
        }
    }
}
