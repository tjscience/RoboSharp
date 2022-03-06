using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp;
using System;

namespace RoboSharpUnitTesting
{
    [TestClass]
    public class ProgressEstimatorTests
    {

        //[TestMethod]
        public void SAMPLE_TEST_METHOD()
        {
            //Create the command and base values for the Expected Results
            RoboCommand cmd = Test_Setup.GenerateCommand(false);

            //Apply command options

            //Run the test
            RoboSharpTestResults UnitTestResults = Test_Setup.RunTest(cmd).Result;

            //Evaluate the results and pass/Fail the test
            UnitTestResults.AssertTest();
        }

        [TestMethod]
        public void Test_NoCopyOptions()
        {
            //Create the command and base values for the Expected Results
            RoboCommand cmd = Test_Setup.GenerateCommand(false);

            //Run the test - First Test should just use default values generated from the GenerateCommand method!
            Test_Setup.ClearOutTestDestination();
            RoboSharpTestResults UnitTestResults = Test_Setup.RunTest(cmd).Result;

            //Evaluate the results and pass/Fail the test
            UnitTestResults.AssertTest();
        }

        [TestMethod]
        public void Test_TopLevelFolderOnly_Ignore1()
        {
            //Create the command and base values for the Expected Results
            RoboCommand cmd = Test_Setup.GenerateCommand(false);

            //Set Up Results
            cmd.SelectionOptions.ExcludedFiles.Add("4_Bytes.txt"); // 3 copies of this file exist

            Test_Setup.ClearOutTestDestination();
            RoboSharpTestResults UnitTestResults = Test_Setup.RunTest(cmd).Result;

            //Evaluate the results and pass/Fail the test
            UnitTestResults.AssertTest();
        }

        [TestMethod]
        public void Test_TopLevelFolderOnly_IgnoreLarger()
        {
            //Create the command and base values for the Expected Results
            RoboCommand cmd = Test_Setup.GenerateCommand(true);

            //Set Up Results
            cmd.SelectionOptions.MaxFileSize = 1500;

            Test_Setup.ClearOutTestDestination();
            RoboSharpTestResults UnitTestResults = Test_Setup.RunTest(cmd).Result;

            //Evaluate the results and pass/Fail the test
            UnitTestResults.AssertTest();
        }

    }
}
