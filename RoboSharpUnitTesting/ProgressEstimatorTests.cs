using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp;
using RoboSharp.Results;
using System;
using System.Collections.Generic;
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
            // Create the Command
            RoboCommand cmd = Test_Setup.GenerateCommand(false, ListOnlyMode);
            

            //Run the test and Evaluate the results and pass/Fail the test
            RoboSharpTestResults UnitTestResults = Test_Setup.RunTest(cmd).Result;
            UnitTestResults.AssertTest();
        }


        [TestMethod]
        public void Test_NoCopyOptions()
        {
            // Create the Command
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
            // Create the Command
            RoboCommand cmd = Test_Setup.GenerateCommand(false, ListOnlyMode);
            cmd.SelectionOptions.ExcludedFiles.Add("4_Bytes.txt"); // 3 copies of this file exist
            Test_Setup.ClearOutTestDestination();
            RoboSharpTestResults UnitTestResults = Test_Setup.RunTest(cmd).Result;
            UnitTestResults.AssertTest();//Evaluate the results and pass/Fail the test
        }

        [TestMethod]
        public void Test_MinFileSize()
        {
            // Create the Command
            RoboCommand cmd = Test_Setup.GenerateCommand(true, ListOnlyMode);
            cmd.SelectionOptions.MinFileSize = 1500;
            Test_Setup.ClearOutTestDestination();
            RoboSharpTestResults UnitTestResults = Test_Setup.RunTest(cmd).Result;
            UnitTestResults.AssertTest();//Evaluate the results and pass/Fail the test
        }

        [TestMethod]
        public void Test_MaxFileSize()
        {
            // Create the Command
            RoboCommand cmd = Test_Setup.GenerateCommand(true, ListOnlyMode);
            cmd.SelectionOptions.MaxFileSize = 1500;
            Test_Setup.ClearOutTestDestination();
            RoboSharpTestResults UnitTestResults = Test_Setup.RunTest(cmd).Result;
            UnitTestResults.AssertTest();//Evaluate the results and pass/Fail the test
        }

        [TestMethod]
        public void Test_FileInUse()
        {
            //Create the command and base values for the Expected Results
            List<string> CommandErrorData = new List<string>();
            RoboCommand cmd = Test_Setup.GenerateCommand(true, ListOnlyMode);
            cmd.OnCommandError += (o, e) =>
            {
                CommandErrorData.Add(e.Error);
                if (e.Exception != null)
                {
                    CommandErrorData.Add("ExceptionData: " + e.Exception.Message);
                }
            };
            
            Test_Setup.ClearOutTestDestination();
            Directory.CreateDirectory(Test_Setup.TestDestination);
            RoboSharpTestResults UnitTestResults;
            //Create a file in the destination that would normally be copied, then lock it to force an error being generated.
            string fPath = Path.Combine(Test_Setup.TestDestination, "4_Bytes.txt");
            Console.WriteLine("Configuration File Error Token: " + cmd.Configuration.ErrorToken);
            Console.WriteLine("Error Token Regex: " + cmd.Configuration.ErrorTokenRegex);
            Console.WriteLine("Creating and locking file: " + fPath);
            var f = File.Open(fPath, FileMode.Create);    
                Console.WriteLine("Running Test");
                UnitTestResults = Test_Setup.RunTest(cmd).Result;
                Console.WriteLine("Test Complete");
            Console.WriteLine("Releasing File: " + fPath);
            f.Close();
            if (CommandErrorData.Count > 0)
            {
                Console.WriteLine("\nCommand Error Data Received:");
                foreach (string s in CommandErrorData)
                    Console.WriteLine(s);
            }else
                Console.WriteLine("\nCommand Error Data Received: None");

            //Evaluate the results and pass/Fail the test
            UnitTestResults.AssertTest();
        }

        [TestMethod]
        public void Test_ExcludeLastAccessDate()
        {
            //Create the command and base values for the Expected Results
            RoboCommand cmd = Test_Setup.GenerateCommand(false, ListOnlyMode);

            //Set last access time to date in the past for two files
            string filePath1 = Path.Combine(Directory.GetCurrentDirectory(), "TEST_FILES", "STANDARD", "1024_Bytes.txt");
            string filePath2 = Path.Combine(Directory.GetCurrentDirectory(), "TEST_FILES", "STANDARD", "4_Bytes.txt");
            File.SetLastAccessTime(filePath1, new DateTime(1980, 1, 1));
            File.SetLastAccessTime(filePath2, new DateTime(1980, 1, 1));

            //Set Up Results
            cmd.SelectionOptions.MaxLastAccessDate = "19900101";

            Test_Setup.ClearOutTestDestination();
            RoboSharpTestResults UnitTestResults = Test_Setup.RunTest(cmd).Result;

            //Evaluate the results and pass/Fail the test
            UnitTestResults.AssertTest();

            //Set last access time back to today for two files
            File.SetLastAccessTime(filePath1, DateTime.Now);
            File.SetLastAccessTime(filePath2, DateTime.Now);
        }


        #region < Attribute Testing >

        /*TODO: While these all report identical values from RoboCopy and progressEstimator, they aren't working as expected.
         * Some flags are set and work as expected ( Setting Read-Only for example works fine for Include and Exclude (1/12 copied or 11/12 copied respectively)
         * but other flags (Compressed, Ecnrypted) are showing 12/12 copied or ignored, when 1/12 and 11/12 are expected. 
         * 
         * ToDo: Compressed and Encrypted flags appear to be unable to be set programmatically? -> Tests Currently Commented Out!
         */

        //INCLUDE
        [TestMethod] public void Test_IncludeAttribReadOnly() => Test_Attributes(FileAttributes.ReadOnly, true);
        [TestMethod] public void Test_IncludeAttribArchive() => Test_Attributes(FileAttributes.Archive, true);
        [TestMethod] public void Test_IncludeAttribSystem() => Test_Attributes(FileAttributes.System, true);
        [TestMethod] public void Test_IncludeAttribHidden() => Test_Attributes(FileAttributes.Hidden, true);
        //[TestMethod] public void Test_IncludeAttribCompressed() => Test_Attributes(FileAttributes.Compressed, true);
        [TestMethod] public void Test_IncludeAttribNotContentIndexed() => Test_Attributes(FileAttributes.NotContentIndexed, true);
        //[TestMethod] public void Test_IncludeAttribEncrypted() => Test_Attributes(FileAttributes.Encrypted, true);
        [TestMethod] public void Test_IncludeAttribTemporary() => Test_Attributes(FileAttributes.Temporary, true);
        [TestMethod] public void Test_IncludeAttribOffline() => Test_Attributes(FileAttributes.Offline, true);
        

        //EXCLUDE
        [TestMethod] public void Test_ExcludeAttribReadOnly() => Test_Attributes(FileAttributes.ReadOnly, false);
        [TestMethod] public void Test_ExcludeAttribArchive() => Test_Attributes(FileAttributes.Archive, false);
        [TestMethod] public void Test_ExcludeAttribSystem() => Test_Attributes(FileAttributes.System, false);
        [TestMethod] public void Test_ExcludeAttribHidden() => Test_Attributes(FileAttributes.Hidden, false);
        //[TestMethod] public void Test_ExcludeAttribCompressed() => Test_Attributes(FileAttributes.Compressed, false);
        [TestMethod] public void Test_ExcludeAttribNotContentIndexed() => Test_Attributes(FileAttributes.NotContentIndexed, false);
        //[TestMethod] public void Test_ExcludeAttribEncrypted() => Test_Attributes(FileAttributes.Encrypted, false);
        [TestMethod] public void Test_ExcludeAttribTemporary() => Test_Attributes(FileAttributes.Temporary, false);
        [TestMethod] public void Test_ExcludeAttribOffline() => Test_Attributes(FileAttributes.Offline, false);
        

        /// <param name="attributes"><inheritdoc cref="SelectionOptions.ConvertFileAttrToString(FileAttributes?)" path="*"/></param>
        /// <param name="Include">TRUE if setting to INCLUDE, False to EXCLUDE</param>
        private void Test_Attributes(FileAttributes attributes, bool Include)
        {
            // Create the Command
            RoboCommand cmd = Test_Setup.GenerateCommand(false, ListOnlyMode);

            //Set all files in source as normal
            var sourcePath = Test_Setup.Source_Standard;
            var files = new DirectoryInfo(sourcePath).GetFiles("*", SearchOption.AllDirectories);
            FileAttributes sourceAttr = attributes.HasFlag(FileAttributes.Normal) ? FileAttributes.Temporary : FileAttributes.Normal;
            Console.WriteLine($"Setting all Source Files to the following attribute: FileAttributes.{sourceAttr}");
            foreach (var f in files)
            {
                File.SetAttributes(f.FullName, sourceAttr);
                f.Refresh();
                var attr = f.Attributes;    // For Debugging to evaluate the attributes
            }

            //Set file attribute to read only
            string fileName = "1024_Bytes.txt";
            string filePath = Path.Combine(sourcePath, fileName);
            File.SetAttributes(filePath, attributes);   // Always mark the flag as normal since it wipes out all other flags
            var attr2 = File.GetAttributes(filePath);   // For Debugging to evaluate the attributes
            Console.WriteLine($"Setting Attribute: FileAttributes.{attributes} on {filePath}");
            Console.WriteLine($"Running in List-Only Mode: {ListOnlyMode}");
            Console.WriteLine($"Expected Outcome: 1 File {(Include ? "Copied" : "Skipped")}\n\n");

            //Set Up Results
            Statistic expectedFileCounts = new Statistic(Statistic.StatType.Files);
            if (Include)
            {
                cmd.SelectionOptions.SetIncludedAttributes(attributes);
                expectedFileCounts.SetValues(12, 1, 0, 0, 0, 11);
            }
            else
            {
                cmd.SelectionOptions.SetExcludedAttributes(attributes);
                expectedFileCounts.SetValues(12, 11, 0, 0, 0, 1);
            }

            Test_Setup.ClearOutTestDestination();
            RoboSharpTestResults UnitTestResults = Test_Setup.RunTest(cmd).Result;
            
            //Revert all modified files to their normal state
            File.SetAttributes(filePath, FileAttributes.Normal);    //Source File
            filePath = Path.Combine(cmd.CopyOptions.Destination, fileName); // Destination
            if (File.Exists(filePath)) File.SetAttributes(filePath, FileAttributes.Normal);

            //Evaluate the results and pass/Fail the test
            UnitTestResults.AssertTest();
            Assert.AreEqual(expectedFileCounts.Copied, UnitTestResults.Results.FilesStatistic.Copied);
        }

        #endregion

    }
}
