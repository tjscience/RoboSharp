using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RoboSharpUnitTesting
{
    [TestClass]
    public class RoboCommandEventTests
    {
        private void RunTestThenAssert(IRoboCommand cmd, ref bool EventBool)
        {
            var results = cmd.StartAsync().Result;
            Test_Setup.WriteLogLines(results);
            if (!EventBool) throw new AssertFailedException("Subscribed Event was not Raised!");
        }

        [TestMethod]
        public void RoboCommand_OnCommandCompleted()
        {
            var cmd = Test_Setup.GenerateCommand(false, true);
            bool TestPassed = false;
            cmd.OnCommandCompleted += (o, e) => TestPassed = true;
            RunTestThenAssert(cmd, ref TestPassed);
        }

        [TestMethod]
        public void RoboCommand_OnCommandError()
        {
            var cmd = Test_Setup.GenerateCommand(false, true);
            cmd.CopyOptions.Source += "FolderDoesNotExist";
            bool TestPassed = false;
            cmd.OnCommandError += (o, e) => TestPassed = true;
            RunTestThenAssert(cmd, ref TestPassed);
        }

        [TestMethod]
        public void RoboCommand_OnCopyProgressChanged()
        {
            Test_Setup.ClearOutTestDestination();
            var cmd = Test_Setup.GenerateCommand(false, false);
            bool TestPassed = false;
            cmd.OnCopyProgressChanged += (o, e) => TestPassed = true;
            RunTestThenAssert(cmd, ref TestPassed);
        }

        [TestMethod]
        public void RoboCommand_OnError()
        {
            //Create a file in the destination that would normally be copied, then lock it to force an error being generated.
            var cmd = Test_Setup.GenerateCommand(false, false);
            bool TestPassed = false;
            cmd.OnError += (o, e) => TestPassed = true;
            Test_Setup.ClearOutTestDestination();
            Directory.CreateDirectory(Test_Setup.TestDestination);
            using (var f = File.CreateText(Path.Combine(Test_Setup.TestDestination, "4_Bytes.txt")))
            {
                f.WriteLine("StartTest!");
                Console.WriteLine("Expecting 1 File Failed!\n\n");
                RunTestThenAssert(cmd, ref TestPassed);
            }
        }

        [TestMethod]
        public void RoboCommand_OnFileProcessed()
        {
            Test_Setup.ClearOutTestDestination();
            var cmd = Test_Setup.GenerateCommand(false, true);
            bool TestPassed = false;
            cmd.OnFileProcessed += (o, e) => TestPassed = true;
            RunTestThenAssert(cmd, ref TestPassed);
        }

        [TestMethod]
        public void RoboCommand_ProgressEstimatorCreated()
        {
            var cmd = Test_Setup.GenerateCommand(false, true);
            bool TestPassed = false;
            cmd.OnProgressEstimatorCreated += (o, e) => TestPassed = true;
            RunTestThenAssert(cmd, ref TestPassed);
        }

        //[TestMethod] //TODO: Unsure how to force the TaskFaulted Unit test, as it should never actually occurr.....
        public void RoboCommand_TaskFaulted()
        {
            var cmd = Test_Setup.GenerateCommand(false, true);
            bool TestPassed = false;
            cmd.TaskFaulted += (o, e) => TestPassed = true;
            RunTestThenAssert(cmd, ref TestPassed);
        }

    }
}
