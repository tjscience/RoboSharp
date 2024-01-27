using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.UnitTests
{
    [TestClass()]
    public class RoboCommandParserFunctionsTests
    {
        [DataRow("Test_1 /E /XD", "/PURGE ", false, "Test_1 /E /XD")]
        [DataRow("Test_1 /E /XD", "/E ", true, "Test_1 /XD")]
        [TestMethod()]
        public void ExtractFlagTest(string input, string flag, bool expectedResult, string expectedOutput)
        {
            var result = RoboCommandParserFunctions.ExtractFlag(input, flag, out string outputText);
            Assert.AreEqual(expectedResult, result, "\n Function Result Incorrect");
            Assert.AreEqual(expectedOutput, outputText.Trim(), "\n Sanitized Output Mismatch");

            bool actionPerformed = false;
            result = RoboCommandParserFunctions.ExtractFlag(input, flag, out outputText, () => actionPerformed = true);
            Assert.AreEqual(expectedResult, result, "\n Function Result Incorrect");
            Assert.AreEqual(expectedOutput, outputText.Trim(), "\n Sanitized Output Mismatch");
            Assert.AreEqual(expectedResult, actionPerformed, $"/n Function {(expectedResult ? "Not Performed" : "Performed Unexpectedly")}");
        }

        [DataRow("Test_1.txt *.pdf *.txt *_SomeFile*.jpg", "Test_1.txt", "*.pdf", "*.txt", "*_SomeFile*.jpg")]
        [TestMethod()]
        public void ParseFiltersTest(params string[] itemsWhereFirstIsInputText)
        {
            string input = itemsWhereFirstIsInputText[0];
            var expected = itemsWhereFirstIsInputText.Skip(1);
            Debugger.Instance.DebugMessageEvent += RoboCommandParserTests.DebuggerWriteLine;
            var result = RoboCommandParserFunctions.ParseFilters(input, "{0}").ToArray();
            Debugger.Instance.DebugMessageEvent -= RoboCommandParserTests.DebuggerWriteLine;

            Assert.AreEqual(expected.Count(), result.Count(), "\n Number of items differs.");
            int i = 0;
            foreach (string item in expected)
            {
                Assert.AreEqual(item, result[i], "\n Parsed Item does not match!");
                i++;
            }
        }

        [DataRow("C:\\MySource \"D:\\My Destination\" /XF", @"C:\MySource", @"D:\My Destination", "/XF", DisplayName = "Quoted Destination")]
        [DataRow("\"C:\\My Source\" D:\\MyDestination /XF", @"C:\My Source", @"D:\MyDestination", "/XF", DisplayName = "Quoted Source")]
        [DataRow("robocopy \"C:\\My Source\" \"D:\\My Destination\" /XF", @"C:\My Source", @"D:\My Destination", "/XF", DisplayName = "Quotes + Spaces")]
        [DataRow("robocopy \"C:\\MySource\" \"D:\\MyDestination\" /XF", @"C:\MySource", @"D:\MyDestination", "/XF", DisplayName = "Quotes")]
        [DataRow(@"robocopy C:\MySource D:\MyDestination /XF", @"C:\MySource", @"D:\MyDestination", "/XF", DisplayName = "No Quotes")]
        [TestMethod()]
        public void ParseSourceAndDestinationTest(string input, string expectedSource, string expectedDestination, string expectedSanitizedValue)
        {
            var result = RoboCommandParserFunctions.ParseSourceAndDestination(input);
            Assert.AreEqual(input, result.InputString, "\n Input Value Incorrect");
            Assert.AreEqual(expectedSource, result.Source, "\n Source Value Incorrect");
            Assert.AreEqual(expectedDestination, result.Dest, "\n Destination Value Incorrect");
            Assert.AreEqual(expectedSanitizedValue, result.SanitizedString.Trim(), "\n Sanitized Value Incorrect");
        }

        [DataRow("Test_1 /Data:5", "/Data:{0}", "5", "Test_1", true)]
        [TestMethod()]
        public void TryExtractParameterTest(string input, string parameter, string expectedvalue, string expectedOutput, bool expectedResult)
        {
            var result = RoboCommandParserFunctions.TryExtractParameter(input, parameter, out string value, out string outputText);
            Assert.AreEqual(expectedResult, result, "/n Function Result Mismatch");
            Assert.AreEqual(expectedvalue, value, "/n Expected Value Mismatch");
            Assert.AreEqual(expectedOutput, outputText.Trim(), "/n Sanitized Output Mismatch");
        }

        [DataRow(@"*.* *.pdf", @"", 2, DisplayName = "Test 1")]
        [DataRow(@" *.* *.pdf ", @"", 2, DisplayName = "Test 2")]
        [DataRow(@" *.* /PURGE ", @"/PURGE ", 1, DisplayName = "Test 3")]
        [DataRow(@"""Some File.txt"" *.* *.pdf ", @"", 3, DisplayName = "Test 4")]
        [DataRow(@"*.* ""Some File.txt"" *.pdf /s", @"/s", 3, DisplayName = "Test 5")]
        [TestMethod]
        public void FileFilterParsingTest(string input, string expectedoutput, int expectedCount)
        {
            Debugger.Instance.DebugMessageEvent += RoboCommandParserTests.DebuggerWriteLine;
            var result = RoboCommandParserFunctions.ExtractFileFilters(input, out string modified);
            Debugger.Instance.DebugMessageEvent -= RoboCommandParserTests.DebuggerWriteLine;
            Assert.AreEqual(expectedCount, result.Count(), "Did not receive expected count!");
            Assert.AreEqual(expectedoutput.Trim(), modified.Trim(), "Extracted Text does not match!");
        }

        [DataRow(@"/XF C:\someDir\someFile.pdf *some_Other-File* /XD SomeDir", @"/XD SomeDir", 2, DisplayName = "Test 1")]
        [DataRow(@"/XF some-File.?df /XD *SomeDir* /XF SomeFile.*", @"/XD *SomeDir*", 2, DisplayName = "Test 2")]
        [DataRow(@"/XF some_File.*df /COPYALL /XF ""*some Other-File*"" /XD *SomeDir* ", @"/COPYALL  /XD *SomeDir*", 2, DisplayName = "Test 3")]
        [DataRow(@"/PURGE /XF ""C:\some File.pdf"" *someOtherFile* /XD SomeDir", @"/PURGE  /XD SomeDir", 2, DisplayName = "Test 4")]
        [TestMethod]
        public void ExtractExclusionFilesTest(string input, string expectedoutput, int expectedCount)
        {
            Debugger.Instance.DebugMessageEvent += RoboCommandParserTests.DebuggerWriteLine;
            var result = RoboCommandParserFunctions.ExtractExclusionFiles(input, out string modified);
            Debugger.Instance.DebugMessageEvent -= RoboCommandParserTests.DebuggerWriteLine;
            Assert.AreEqual(expectedCount, result.Count(), "Did not receive expected count of excluded Files!");
            Assert.AreEqual(expectedoutput.Trim(), modified.Trim(), "Extracted Text does not match!");
        }

        [DataRow(@"/XD C:\someDir *someOtherDir* /XF SomeFile.*", @"/XF SomeFile.*", 2, DisplayName = "Test 1")]
        [DataRow(@"/XD C:\someDir /XD *someOtherDir* /XF SomeFile.*", @"/XF SomeFile.*", 2, DisplayName = "Test 2")]
        [DataRow(@"/XD C:\someDir /XF SomeFile.* /XD *someOtherDir* ", @"/XF SomeFile.*", 2, DisplayName = "Test 3")]
        [DataRow(@"/XD ""C:\some Dir"" *someOtherDir* /XF SomeFile.*", @"/XF SomeFile.*", 2, DisplayName = "Test 4")]
        [TestMethod]
        public void ExtractExclusionDirectoriesTest(string input, string expectedoutput, int expectedCount)
        {
            Debugger.Instance.DebugMessageEvent += RoboCommandParserTests.DebuggerWriteLine;
            var result = RoboCommandParserFunctions.ExtractExclusionDirectories(input, out string modified);
            Debugger.Instance.DebugMessageEvent -= RoboCommandParserTests.DebuggerWriteLine;
            Assert.AreEqual(expectedCount, result.Count(), "Did not receive expected count of excluded directories!");
            Assert.AreEqual(expectedoutput.Trim(), modified.Trim(), "Extracted Text does not match!");
        }
    }
}