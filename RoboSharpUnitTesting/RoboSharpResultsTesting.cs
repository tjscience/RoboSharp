using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp;
using System;

namespace RoboSharpUnitTesting
{
    [TestClass]
    public class ProgressEstimatorTests
    {
        private TestContext testContextInstance;

        public TestContext TestContext { get => testContextInstance; set => testContextInstance = value; }

        //[TestMethod]
        public void SAMPLE_TEST_METHOD()
        {
            //Create the command and base values for the Expected Results
            RoboCommand cmd = StaticMethods.GenerateCommand();

            //Apply command options

            //Run the test
            RoboSharpTestResults UnitTestResults = StaticMethods.RunTest(cmd).Result;

            //Evaluate the results and pass/Fail the test
            UnitTestResults.AssertTest();
        }

        [TestMethod]
        public void Test_NoCopyOptions()
        {
            //Create the command and base values for the Expected Results
            RoboCommand cmd = StaticMethods.GenerateCommand();

            //Run the test - First Test should just use default values generated from the GenerateCommand method!
            StaticMethods.ClearOutTestDestination();
            RoboSharpTestResults UnitTestResults = StaticMethods.RunTest(cmd).Result;

            //Evaluate the results and pass/Fail the test
            UnitTestResults.AssertTest();
        }

        [TestMethod]
        public void Test_TopLevelFolderOnly_Ignore1()
        {
            //Create the command and base values for the Expected Results
            RoboCommand cmd = StaticMethods.GenerateCommand();

            //Set Up Results
            cmd.SelectionOptions.ExcludedFiles.Add("4_Bytes.txt"); // 3 copies of this file exist

            StaticMethods.ClearOutTestDestination();
            RoboSharpTestResults UnitTestResults = StaticMethods.RunTest(cmd).Result;

            //Evaluate the results and pass/Fail the test
            UnitTestResults.AssertTest();
        }

        [TestMethod]
        public void Test_TopLevelFolderOnly_IgnoreLarger()
        {
            //Create the command and base values for the Expected Results
            RoboCommand cmd = StaticMethods.GenerateCommand();

            //Set Up Results
            cmd.CopyOptions.Source = StaticMethods.TestSource2; //1024_Bytes is actually 2048 bytes for the larger file test!
            cmd.SelectionOptions.MaxFileSize = 1500;

            StaticMethods.ClearOutTestDestination();
            RoboSharpTestResults UnitTestResults = StaticMethods.RunTest(cmd).Result;

            //Evaluate the results and pass/Fail the test
            UnitTestResults.AssertTest();
        }

    }
}
