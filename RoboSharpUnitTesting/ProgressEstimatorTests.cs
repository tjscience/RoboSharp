using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp;
using System;
using System.IO;

namespace RoboSharpUnitTesting
{
    [TestClass]
    public class ProgressEstimatorTests_ListOnly : ProgressEstimatorTests
    {
        // Define all testts in the 'ProgressEstimatorTests' class below!
        // This derived class will cause all the tests defined in ProgressEstimatorTests to run twice!
        // ProgressEstimatorTests runs performs the operations, while this override causes the same tests to run using ListOnly = TRUE.
        public override bool ListOnlyMode => true;
    }

    [TestClass]
    public class ProgressEstimatorTests
    {
        public virtual bool ListOnlyMode => false;
        
        //[TestMethod]
        public void SAMPLE_TEST_METHOD()
        {
            //Create the command and base values for the Expected Results
            RoboCommand cmd = Test_Setup.GenerateCommand(false, ListOnlyMode);

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
            RoboCommand cmd = Test_Setup.GenerateCommand(false, ListOnlyMode);

            //Run the test - First Test should just use default values generated from the GenerateCommand method!
            Test_Setup.ClearOutTestDestination();
            RoboSharpTestResults UnitTestResults = Test_Setup.RunTest(cmd).Result;

            //Evaluate the results and pass/Fail the test
            UnitTestResults.AssertTest();
        }

        [TestMethod]
        public void Test_ExcludedFiles()
        {
            //Create the command and base values for the Expected Results
            RoboCommand cmd = Test_Setup.GenerateCommand(false, ListOnlyMode);
            cmd.SelectionOptions.ExcludedFiles.Add("4_Bytes.txt"); // 3 copies of this file exist
            Test_Setup.ClearOutTestDestination();
            RoboSharpTestResults UnitTestResults = Test_Setup.RunTest(cmd).Result;
            UnitTestResults.AssertTest();//Evaluate the results and pass/Fail the test
        }

        [TestMethod]
        public void Test_MinFileSize()
        {
            //Create the command and base values for the Expected Results
            RoboCommand cmd = Test_Setup.GenerateCommand(true, ListOnlyMode);
            cmd.SelectionOptions.MinFileSize = 1500;
            Test_Setup.ClearOutTestDestination();
            RoboSharpTestResults UnitTestResults = Test_Setup.RunTest(cmd).Result;
            UnitTestResults.AssertTest();//Evaluate the results and pass/Fail the test
        }

        [TestMethod]
        public void Test_MaxFileSize()
        {
            //Create the command and base values for the Expected Results
            RoboCommand cmd = Test_Setup.GenerateCommand(true, ListOnlyMode);
            cmd.SelectionOptions.MaxFileSize = 1500;
            Test_Setup.ClearOutTestDestination();
            RoboSharpTestResults UnitTestResults = Test_Setup.RunTest(cmd).Result;
            UnitTestResults.AssertTest();//Evaluate the results and pass/Fail the test
        }

        [TestMethod]
        public void Test_TopLevelFolderOnly_IgnoreAttribReadOnly()
        {
            //Create the command and base values for the Expected Results
            RoboCommand cmd = Test_Setup.GenerateCommand(false, ListOnlyMode);

            //Set file attribute to read only
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "TEST_FILES", "STANDARD", "1024_Bytes.txt");
            File.SetAttributes(filePath, FileAttributes.ReadOnly);

            //Set Up Results
            cmd.SelectionOptions.ExcludeAttributes = "R";

            Test_Setup.ClearOutTestDestination();
            RoboSharpTestResults UnitTestResults = Test_Setup.RunTest(cmd).Result;

            //Evaluate the results and pass/Fail the test
            UnitTestResults.AssertTest();

            //Set file back to normal attributes again
            File.SetAttributes(filePath, FileAttributes.Normal);
        }

        [TestMethod]
        public void Test_FileInUse()
        {
            //Create the command and base values for the Expected Results
            RoboCommand cmd = Test_Setup.GenerateCommand(true, ListOnlyMode);
            Test_Setup.ClearOutTestDestination();
            Directory.CreateDirectory(Test_Setup.TestDestination);
            RoboSharpTestResults UnitTestResults;
            //Create a file in the destination that would normally be copied, then lock it to force an error being generated.
            using (var f = File.CreateText(Path.Combine(Test_Setup.TestDestination, "4_Bytes.txt")))
            {
                f.WriteLine("StartTest!");
                UnitTestResults = Test_Setup.RunTest(cmd).Result;
            }
            //Evaluate the results and pass/Fail the test
            UnitTestResults.AssertTest();
        }
    }
}
