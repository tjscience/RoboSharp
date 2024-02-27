using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp.Extensions;
using RoboSharp.Extensions.CopyFileEx;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RoboSharp.Extensions.UnitTests
{
    [TestClass()]
    public class CopyFileTests
    {
        [TestMethod]
        public void TestCancellationTokens()
        {
            // Cancelling the linked source does not cancel the input tokens
            var cts_1 = new CancellationTokenSource();
            var cts_2 = new CancellationTokenSource();
            var cts_linked = CancellationTokenSource.CreateLinkedTokenSource(cts_1.Token, cts_2.Token);
            Assert.IsFalse(cts_linked.IsCancellationRequested);
            cts_linked.Cancel();
            Assert.IsTrue(cts_linked.IsCancellationRequested);
            Assert.IsFalse(cts_1.IsCancellationRequested);
            Assert.IsFalse(cts_2.IsCancellationRequested);

            // canceling a token that was an input causes linke to report as cancelled
            cts_linked = CancellationTokenSource.CreateLinkedTokenSource(cts_1.Token, cts_2.Token);
            Assert.IsFalse(cts_linked.IsCancellationRequested);
            cts_1.Cancel();
            Assert.IsTrue(cts_1.IsCancellationRequested);
            Assert.IsFalse(cts_2.IsCancellationRequested);
            Assert.IsTrue(cts_linked.IsCancellationRequested);
        }

        [TestMethod]
        public void CopyFileEx_ToDirectory()
        {
            var source = new FileInfo("TestFile.txt");
            var destination = new DirectoryInfo("\\somedir\\");
            var destFile = new FileInfo(Path.Combine(destination.FullName, source.Name));
            Assert.IsTrue(FileFunctions.CopyFile(source.FullName, destination.FullName));
            source.Delete();
            Assert.IsTrue(File.Exists(destFile.FullName), "File does not exist at destination");
            destFile.Delete();
            destination.Delete();
        }

        [TestMethod]
        public void CopyFileEx_CopyFile()
        {
            string sourceFile = "SomeFIle.txt";
            string destFile = "SomeOtherFile.txt";
            if (File.Exists(sourceFile)) File.Delete(sourceFile);
            Assert.ThrowsException<FileNotFoundException>(() => FileFunctions.CopyFile(sourceFile, destFile, CopyFileExOptions.FAIL_IF_EXISTS));

            File.WriteAllText(sourceFile, "Test Contents");
            File.WriteAllText(destFile, "Content to replace");
            Assert.IsTrue(File.Exists(sourceFile));
            Assert.IsTrue(File.Exists(destFile));
            Assert.IsTrue(FileFunctions.CopyFile(sourceFile, destFile, CopyFileExOptions.NONE));
            Assert.ThrowsException<IOException>(() => FileFunctions.CopyFile(sourceFile, destFile, CopyFileExOptions.FAIL_IF_EXISTS));

            bool callbackHit = false;
            int callbackHitCount = 0;

            // Cancellation
            var cancelCallback = FileFunctions.CreateCallback((long a, long b) =>
            {
                callbackHit = true;
                callbackHitCount++;
                return CopyProgressCallbackResult.CANCEL;
            });
            Assert.IsFalse(callbackHit);
            Assert.ThrowsException<OperationCanceledException>(() => FileFunctions.CopyFile(sourceFile, destFile, default, cancelCallback));
            Assert.IsTrue(callbackHit);
            Assert.AreEqual(1, callbackHitCount);
            callbackHit = false;
            callbackHitCount = 0;

            // Quiet
            var quietCallback = FileFunctions.CreateCallback((long a, long b) =>
            {
                callbackHit = true;
                callbackHitCount++;
                return CopyProgressCallbackResult.QUIET;
            });
            Assert.IsFalse(callbackHit);
            Assert.IsTrue(FileFunctions.CopyFile(sourceFile, destFile, default, quietCallback));
            Assert.IsTrue(callbackHit);
            Assert.AreEqual(1, callbackHitCount);
            callbackHit = false;
            callbackHitCount = 0;

            // Continue
            var continueCallback = FileFunctions.CreateCallback((long a, long b) =>
            {
                callbackHit = true;
                callbackHitCount++;
                return CopyProgressCallbackResult.CONTINUE;
            });
            Assert.IsFalse(callbackHit);
            Assert.IsTrue(FileFunctions.CopyFile(sourceFile, destFile, default, continueCallback));
            Assert.IsTrue(callbackHit);
            Assert.IsTrue(callbackHitCount >= 2);
            callbackHit = false;
            callbackHitCount = 0;
            File.Delete(sourceFile);
            File.Delete(destFile);
        }

        [TestMethod]
        public async Task CopyFileEx_CopyFileAsync()
        {
            string sourceFile = "SomeFIle.txt";
            string destFile = "SomeOtherFile.txt";
            if (File.Exists(sourceFile)) File.Delete(sourceFile);
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile, CopyFileExOptions.NONE));

            File.WriteAllText(sourceFile, "Test Contents");
            File.WriteAllText(destFile, "Content to replace");
            Assert.IsTrue(File.Exists(sourceFile));
            Assert.IsTrue(File.Exists(destFile));
            Assert.IsTrue(await FileFunctions.CopyFileAsync(sourceFile, destFile, CopyFileExOptions.NONE));
            await Assert.ThrowsExceptionAsync<IOException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile, CopyFileExOptions.FAIL_IF_EXISTS));

            bool callbackHit = false;
            int callbackHitCount = 0;

            // Cancellation
            var cancelCallback = FileFunctions.CreateCallback((long a, long b) =>
            {
                callbackHit = true;
                callbackHitCount++;
                return CopyProgressCallbackResult.CANCEL;
            });
            Assert.IsFalse(callbackHit);
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile, CopyFileExOptions.NONE, cancelCallback));
            Assert.IsTrue(callbackHit);
            Assert.AreEqual(1, callbackHitCount);
            callbackHit = false;
            callbackHitCount = 0;

            // Quiet
            var quietCallback = FileFunctions.CreateCallback((long a, long b) =>
            {
                callbackHit = true;
                callbackHitCount++;
                return CopyProgressCallbackResult.QUIET;
            });
            Assert.IsFalse(callbackHit);
            Assert.IsTrue(await FileFunctions.CopyFileAsync(sourceFile, destFile, CopyFileExOptions.NONE, quietCallback));
            Assert.IsTrue(callbackHit);
            Assert.AreEqual(1, callbackHitCount);
            callbackHit = false;
            callbackHitCount = 0;

            // Continue
            var continueCallback = FileFunctions.CreateCallback((long a, long b) =>
            {
                callbackHit = true;
                callbackHitCount++;
                return CopyProgressCallbackResult.CONTINUE;
            });
            Assert.IsFalse(callbackHit);
            Assert.IsTrue(await FileFunctions.CopyFileAsync(sourceFile, destFile, CopyFileExOptions.NONE, continueCallback));
            Assert.IsTrue(callbackHit);
            Assert.IsTrue(callbackHitCount >= 2);
            callbackHit = false;
            callbackHitCount = 0;
            File.Delete(sourceFile);
            File.Delete(destFile);
        }

        [TestMethod()]
        public async Task CopyFileAsyncTest()
        {
            string sourceFile = "SomeFIle.txt";
            string destFile = "SomeOtherFile.txt";

            if (File.Exists(sourceFile)) File.Delete(sourceFile);

            bool progFullUpdated = false;
            var progFull = new Progress<ProgressUpdate>();
            progFull.ProgressChanged += (o, e) => progFullUpdated = true;

            bool progPercentUpdated = false;
            var progPercent = new Progress<double>();
            progPercent.ProgressChanged += (o, e) => progPercentUpdated = true;

            bool progSizeUpdated = false;
            var progSize = new Progress<long>();
            progSize.ProgressChanged += (o, e) => progSizeUpdated = true;


            await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile));
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile, false));
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile, true));
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile, progFull, 100, true));
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile, progPercent, 100, true));
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile, progSize, 100, true));
            Assert.IsFalse(progFullUpdated | progSizeUpdated | progPercentUpdated);

            File.WriteAllText(sourceFile, "Test Contents");
            File.WriteAllText(destFile, "Content to replace");
            Assert.IsTrue(File.Exists(sourceFile));
            Assert.IsTrue(File.Exists(destFile));

            // Prevent Overwrite
            await Assert.ThrowsExceptionAsync<IOException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile));
            await Assert.ThrowsExceptionAsync<IOException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile, false));
            await Assert.ThrowsExceptionAsync<IOException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile, progFull, 100, false));
            await Assert.ThrowsExceptionAsync<IOException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile, progPercent, 100, false));
            await Assert.ThrowsExceptionAsync<IOException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile, progSize, 100, false));

            // Overwrite
            progPercentUpdated = false;
            progSizeUpdated = false;
            progFullUpdated = false;
            Assert.IsTrue(await FileFunctions.CopyFileAsync(sourceFile, destFile, true));
            Assert.IsTrue(await FileFunctions.CopyFileAsync(sourceFile, destFile, progFull, 100, true));
            Assert.IsTrue(await FileFunctions.CopyFileAsync(sourceFile, destFile, progPercent, 100, true));
            Assert.IsTrue(await FileFunctions.CopyFileAsync(sourceFile, destFile, progSize, 100, true));
            Assert.IsTrue(progFullUpdated, "Full Progress object never reported");
            Assert.IsTrue(progSizeUpdated, "Size Progress object never reported");
            Assert.IsTrue(progPercentUpdated, "Percentage Progress object never reported");

            // Cancellation
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();
            CancellationToken GetTimedToken() => tokenSource.Token ;
            File.Delete(destFile);
            await ThrowsExceptionAsync<OperationCanceledException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile, GetTimedToken()));
            await ThrowsExceptionAsync<OperationCanceledException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile, false, GetTimedToken()));
            await ThrowsExceptionAsync<OperationCanceledException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile, progFull, 100, false, GetTimedToken()));
            await ThrowsExceptionAsync<OperationCanceledException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile, progPercent, 100, false, GetTimedToken()));
            await ThrowsExceptionAsync<OperationCanceledException>(async () => await FileFunctions.CopyFileAsync(sourceFile, destFile, progSize, 100, false, GetTimedToken()));
            Assert.IsFalse(File.Exists(destFile));

        }

        // Allows catching derived types - Meant for OperationCancelledException
        static async Task ThrowsExceptionAsync<T>(Func<Task> func) where T : Exception
        {
            try
            {
                await func();
            }catch(T)
            {

            }catch(Exception e)
            {
                Assert.ThrowsException<T>(() => throw e);
            }
        }
    }
}
