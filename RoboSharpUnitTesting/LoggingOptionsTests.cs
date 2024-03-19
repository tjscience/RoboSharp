using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace RoboSharp.UnitTests
{
    [TestClass]
    public class LoggingOptionsTests
    {
        /// <summary>
        /// This test ensures that the destination directory is not created when using the /QUIT function
        /// </summary>
        [TestMethod]
        public void TestListOnlyDestinationCreation() 
        {
            RoboCommand cmd = new RoboCommand(source: Test_Setup.Source_Standard, destination: Path.Combine(Test_Setup.TestDestination, Path.GetRandomFileName()));
            Console.WriteLine("Destination Path: " + cmd.CopyOptions.Destination);
            cmd.CopyOptions.Depth = 1;
            cmd.CopyOptions.FileFilter = new string[] { "*.ABCDEF" };

            cmd.LoggingOptions.ListOnly = true;
            Authentication.AuthenticateDestination(cmd);
            Assert.IsFalse(Directory.Exists(cmd.CopyOptions.Destination), "\nDestination Directory was created during authentication!");

            cmd.LoggingOptions.ListOnly = false;
            cmd.Start_ListOnly().Wait();
            Assert.IsFalse(Directory.Exists(cmd.CopyOptions.Destination), "\nStart_ListOnly() - Destination Directory was created!");

            cmd.LoggingOptions.ListOnly = false;
            cmd.StartAsync_ListOnly().Wait();
            Assert.IsFalse(Directory.Exists(cmd.CopyOptions.Destination), "\nStartAsync_ListOnly() - Destination Directory was created!");

            cmd.LoggingOptions.ListOnly = true;
            cmd.Start().Wait();
            Assert.IsFalse(Directory.Exists(cmd.CopyOptions.Destination), "\nList-Only Setting - Destination Directory was created!");

            cmd.LoggingOptions.ListOnly = false;
            cmd.Start().Wait();
            Assert.IsTrue(Directory.Exists(cmd.CopyOptions.Destination), "\nDestination Directory was not created.");
        }

        [TestMethod]
        public void TestIsLogFileSpecified()
        {
            LoggingOptions options = new LoggingOptions();

            Assert.IsFalse(options.IsLogFileSpecified());

            options.AppendLogPath = "G";
            Assert.IsTrue(options.IsLogFileSpecified());

            options.AppendLogPath = "";
            options.AppendUnicodeLogPath = "G";
            Assert.IsTrue(options.IsLogFileSpecified());

            options.AppendUnicodeLogPath = "";
            options.LogPath = "G";
            Assert.IsTrue(options.IsLogFileSpecified());

            options.LogPath = "";
            options.UnicodeLogPath = "G";
            Assert.IsTrue(options.IsLogFileSpecified());
            
            options.LogPath = null;
            options.UnicodeLogPath = null;
            options.AppendLogPath = null;
            options.AppendUnicodeLogPath = null;
            Assert.IsFalse(options.IsLogFileSpecified());
        }

        [DataRow(true)]
        [DataRow(false)]
        [TestMethod]
        public void TestBytes(bool withBytes)
        {
            RoboCommand cmd = Test_Setup.GenerateCommand(false, true);
            cmd.LoggingOptions.PrintSizesAsBytes = withBytes;
            cmd.Start().Wait();
            var results = cmd.GetResults();
            Assert.IsNotNull(results);
            results.LogLines.ToList().ForEach(Console.WriteLine);
            Console.WriteLine(results.BytesStatistic.ToString());
        }

        [DataRow(true, true)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(false, false)]
        [TestMethod]
        public void ConfigurationLoggingEnabled(bool isEnabled, bool listOnly)
        {
            Test_Setup.ClearOutTestDestination();
            RoboCommand cmd = Test_Setup.GenerateCommand(false, listOnly);
            cmd.Configuration.EnableFileLogging = isEnabled;
            cmd.Start().Wait();
            var results = cmd.GetResults();
            Assert.IsNotNull(results);
            results.LogLines.ToList().ForEach(Console.WriteLine);
        }

        [DataRow(true, true, DisplayName = "Default Functionality")]
        [DataRow(false, true, DisplayName = "No Header")]
        [DataRow(true, false, DisplayName = "No Summary")]
        [DataRow(false, false, DisplayName = "No Header, No Summary")]
        [TestMethod]
        public void TestSummaryAndHeader(bool header, bool summary)
        {
            Test_Setup.ClearOutTestDestination();
            RoboCommand cmd = Test_Setup.GenerateCommand(false, true);
            //cmd.Configuration.EnableFileLogging = true;
            cmd.LoggingOptions.NoJobHeader = !header;
            cmd.LoggingOptions.NoJobSummary= !summary;
            cmd.Start().Wait();
            var results = cmd.GetResults();
            Assert.IsNotNull(results);
            results.LogLines.ToList().ForEach(Console.WriteLine);

            Console.WriteLine("\n\n-------------- Results Object -------------- ");
            Console.WriteLine(results.ToString());

        }
    }
}
