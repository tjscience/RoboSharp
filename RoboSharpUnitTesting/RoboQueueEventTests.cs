using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using RoboSharp;
using System.Threading;
using System.Threading.Tasks;

namespace RoboSharpUnitTesting
{
    [TestClass]
    public class RoboQueueEventTests
    {
        private static RoboQueue GenerateRQ(out RoboCommand cmd)
        {
            cmd = Test_Setup.GenerateCommand(false, false);
            return new RoboQueue(cmd);
        }

        private static void RunTestThenAssert(RoboQueue Q, ref bool testPassed)
        {
            Q.StartAll().Wait();
            if (Q.RunResults.Count > 0) Test_Setup.WriteLogLines(Q.RunResults[0], true);
            if (!testPassed) throw new AssertFailedException("Subscribed Event was not Raised!");
        }

        [TestMethod]
        public void RoboQueue_OnCommandCompleted()
        {
            var RQ = GenerateRQ(out RoboCommand cmd);
            bool TestPassed = false;
            RQ.OnCommandCompleted += (o, e) => TestPassed = true;
            RunTestThenAssert(RQ, ref TestPassed);
        }

        [TestMethod]
        public void RoboQueue_OnCommandError()
        {
            var RQ = GenerateRQ(out RoboCommand cmd);
            cmd.CopyOptions.Source += "FolderDoesNotExist";
            bool TestPassed = false;
            RQ.OnCommandError += (o, e) => TestPassed = true;
            RunTestThenAssert(RQ, ref TestPassed);
        }

        [TestMethod]
        public void RoboQueue_OnCopyProgressChanged()
        {
            var RQ = GenerateRQ(out RoboCommand cmd);
            Test_Setup.ClearOutTestDestination();
            bool TestPassed = false;
            RQ.OnCopyProgressChanged += (o, e) => TestPassed = true;
            RunTestThenAssert(RQ, ref TestPassed);
        }

        [TestMethod]
        public void RoboQueue_OnError()
        {
            //Create a file in the destination that would normally be copied, then lock it to force an error being generated.
            var RQ = GenerateRQ(out RoboCommand cmd);
            bool TestPassed = false;
            RQ.OnError += (o, e) => TestPassed = true;
            Test_Setup.ClearOutTestDestination();
            Directory.CreateDirectory(Test_Setup.TestDestination);
            using (var f = File.CreateText(Path.Combine(Test_Setup.TestDestination, "4_Bytes.txt")))
            {
                f.WriteLine("StartTest!");
                Console.WriteLine("Expecting 1 File Failed!\n\n");
                RunTestThenAssert(RQ, ref TestPassed);
            }
        }

        [TestMethod]
        public void RoboQueue_OnFileProcessed()
        {
            var RQ = GenerateRQ(out RoboCommand cmd);
            Test_Setup.ClearOutTestDestination();
            bool TestPassed = false;
            RQ.OnFileProcessed += (o, e) => TestPassed = true;
            RunTestThenAssert(RQ, ref TestPassed);
        }


        [TestMethod]
        public void RoboQueue_ProgressEstimatorCreated()
        {
            var RQ = GenerateRQ(out RoboCommand cmd);
            bool TestPassed = false;
            RQ.OnProgressEstimatorCreated += (o, e) => TestPassed = true;
            RunTestThenAssert(RQ, ref TestPassed);
        }

        [TestMethod]
        public void RoboQueue_OnCommandStarted()
        {
            var RQ = GenerateRQ(out RoboCommand cmd);
            bool TestPassed = false;
            RQ.OnCommandStarted += (o, e) => TestPassed = true;
            RunTestThenAssert(RQ, ref TestPassed);
        }

        [TestMethod]
        public void RoboQueue_RunCompleted()
        {
            var RQ = GenerateRQ(out RoboCommand cmd);
            bool TestPassed = false;
            RQ.RunCompleted += (o, e) => TestPassed = true;
            RunTestThenAssert(RQ, ref TestPassed);
        }

        [TestMethod]
        public void RoboQueue_RunResultsUpdated()
        {
            var RQ = GenerateRQ(out RoboCommand cmd);
            bool TestPassed = false;
            RQ.RunResultsUpdated += (o, e) => TestPassed = true;
            RunTestThenAssert(RQ, ref TestPassed);
        }

        [TestMethod]
        public void RoboQueue_ListResultsUpdated()
        {
            var RQ = GenerateRQ(out RoboCommand cmd);
            bool TestPassed = false;
            RQ.ListResultsUpdated += (o, e) => TestPassed = true;
            RQ.StartAll_ListOnly().Wait();
            if (!TestPassed) throw new AssertFailedException("ListResultsUpdated Event was not Raised!");
        }

        [TestMethod]
        public void RoboQueue_CommandAdded()
        {
            var RQ = GenerateRQ(out RoboCommand cmd);
            bool TestPassed = false;
            RQ.CollectionChanged += (o, e) => TestPassed = true;
            RQ.AddCommand(new RoboCommand());
            if (!TestPassed) throw new AssertFailedException("CollectionChanged Event was not Raised!");
        }

        [TestMethod]
        public void RoboQueue_CommandRemoved()
        {
            var RQ = GenerateRQ(out RoboCommand cmd);
            bool TestPassed = false;
            RQ.CollectionChanged += (o, e) => TestPassed = true;
            RQ.RemoveCommand(cmd);
            if (!TestPassed) throw new AssertFailedException("CollectionChanged Event was not Raised!");
        }
        
        [TestMethod]
        public void RoboQueue_ReplaceCommand()
        {
            var RQ = GenerateRQ(out RoboCommand cmd);
            bool TestPassed = false;
            RQ.CollectionChanged += (o, e) => TestPassed = true;
            RQ.ReplaceCommand(new RoboCommand(), 0);
            if (!TestPassed) throw new AssertFailedException("CollectionChanged Event was not Raised!");
        }

        // Property Change would have to be tested for every time the property is changed, which can get odd to test.
        // ListCount and How many ran for example require running it to trigger the event.
        //[TestMethod]
        //public void RoboQueue_PropertyChanged()
        //{
        //    var RQ = GenerateRQ(out RoboCommand cmd);
        //    bool TestPassed = false;
        //    RQ.PropertyChanged += (o, e) => TestPassed = true;
        //    RQ.RemoveCommand(cmd);
        //    Assert.IsTrue(TestPassed);
        //}

        //[TestMethod] //TODO: Unsure how to force the TaskFaulted Unit test, as it should never actually occurr.....
        //public void RoboQueue_TaskFaulted()
        //{
        //    var RQ = GenerateRQ(out RoboCommand cmd);
        //    bool TestPassed = false;
        //    RQ.TaskFaulted += (o, e) => TestPassed = true;
        //    RunTestThenAssert(RQ, ref TestPassed);
        //    Assert.IsTrue(TestPassed);
        //}

    }
}
