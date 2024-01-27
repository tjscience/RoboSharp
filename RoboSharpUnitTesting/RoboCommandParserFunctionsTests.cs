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
            Assert.AreEqual(expectedResult, actionPerformed, $"/n Function {(expectedResult ? "Not Performed" : "Performed Unexpectedly") }");
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
    }
}