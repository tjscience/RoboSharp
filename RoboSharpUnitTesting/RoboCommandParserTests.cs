using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp;
using RoboSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.UnitTests
{
    [TestClass()]
    public class RoboCommandParserTests
    {
        [DataRow("robocopy C:\\source D:\\destination\\ \"*.*\" /copyall", DisplayName = "Accept All Files")]
        [DataRow("robocopy C:\\source D:\\destination /copyall", DisplayName = "No Quotes")]
        [DataRow("robocopy C:\\source \"D:\\destination\" /copyall", DisplayName = "Destination Quotes")]
        [DataRow("robocopy \"C:\\source\" D:\\destination /copyall", DisplayName = "Source Quoted")]
        [DataRow("robocopy \"C:\\source\" \"D:\\destination\" /copyall", DisplayName = "Both Quoted")]
        [TestMethod()]
        public void ParseSourceDestinationTest(string command)
        {
            IRoboCommand cmd = RoboCommandParser.Parse(command);
            Assert.AreEqual("C:\\source", cmd.CopyOptions.Source, "\n\nSource is not expected value");
            Assert.AreEqual("D:\\destination", cmd.CopyOptions.Destination, "\n\nDestination is not expected value");
            Assert.IsTrue(cmd.CopyOptions.CopyAll, "\nCopyAll flag was not detected");
        }

        [DataRow( (SelectionFlags)4097,  (CopyActionFlags)255, (LoggingFlags)65535, "C:\\SomeSourcePath\\My Source Folder", "D:\\SomeDestination\\My Dest Folder", DisplayName = "Test_All_Flags")]
        [DataRow(SelectionFlags.Default,  CopyActionFlags.Default, LoggingFlags.None, "C:\\SomeSourcePath\\", "D:\\SomeDestination\\",  DisplayName = "Test_Defaults")]
        [TestMethod]
        public void TestOptionFlags(SelectionFlags selectionFlags, CopyActionFlags copyFlags, LoggingFlags loggingFlags, string source, string destination)
        {
            // Transform the selection flags to a robocommand, generate the command, parse it, then test that both have the same flags. 
            // ( What the library generates should be able to be reparsed back into the library )
            IRoboCommand cmdSource = new RoboCommand(source, destination, copyFlags, selectionFlags, loggingFlags);
            string text = cmdSource.ToString();
            IRoboCommand cmdResult = RoboCommandParser.Parse(text);
            Assert.AreEqual(cmdSource.CopyOptions.Source, cmdResult.CopyOptions.Source, "\nCopyOptions.Source is not equal!");
            Assert.AreEqual(cmdSource.CopyOptions.Destination, cmdResult.CopyOptions.Destination, "\nCopyOptions.Destination is not equal!");
            Assert.AreEqual(cmdSource.CopyOptions.GetCopyActionFlags(), cmdResult.CopyOptions.GetCopyActionFlags(), $"\n\nCopy Flags are not the same!\n\nExpected:{cmdSource.CopyOptions.GetCopyActionFlags()}\nResult:{cmdResult.CopyOptions.GetCopyActionFlags()}");
            Assert.AreEqual(cmdSource.SelectionOptions.GetSelectionFlags(), cmdResult.SelectionOptions.GetSelectionFlags(), $"\n\nSelection Flags are not the same!\n\nExpected:{cmdSource.SelectionOptions.GetSelectionFlags()}\nResult:{cmdResult.SelectionOptions.GetSelectionFlags()}");
            Assert.AreEqual(cmdSource.LoggingOptions.GetLoggingActionFlags(), cmdResult.LoggingOptions.GetLoggingActionFlags(), $"\n\nLogging Flags are not the same!\n\nExpected:{cmdSource.LoggingOptions.GetLoggingActionFlags()}\nResult:{cmdResult.LoggingOptions.GetLoggingActionFlags()}");

            // Final test : both should produce the same ToString()
            Assert.AreEqual(cmdSource.ToString(), cmdResult.ToString(), $"\n\nProduced Command is not equal!\nExpected:\t{cmdSource}\n  Result:\t{cmdResult}");
        }

        [TestMethod]
        public void TestFileSize()
        {
            // Transform the selection flags to a robocommand, generate the command, parse it, then test that both have the same flags. 
            // ( What the library generates should be able to be reparsed back into the library )
            IRoboCommand cmdSource = new RoboCommand();
            cmdSource.SelectionOptions.MinFileSize = 1234567890;
            cmdSource.SelectionOptions.MaxFileSize= 0987654321;
            IRoboCommand cmdResult = RoboCommandParser.Parse(cmdSource.ToString());

            Assert.AreEqual(cmdSource.SelectionOptions.MinFileSize, cmdResult.SelectionOptions.MinFileSize, "\n\nMinFileSize does not match!");
            Assert.AreEqual(cmdSource.SelectionOptions.MaxFileSize, cmdResult.SelectionOptions.MaxFileSize, "\n\nMaxFileSize does not match!");
            Assert.AreEqual(cmdSource.ToString(), cmdResult.ToString(), $"\n\nProduced Command is not equal!\nExpected:\t{cmdSource}\n  Result:\t{cmdResult}"); // Final test : both should produce the same ToString()
        }
    }
}