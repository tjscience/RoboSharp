using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions.Tests
{
    [TestClass()]
    public class PairExtensionTests
    {
        public static DirectoryPair DirPair = new DirectoryPair(new DirectoryInfo(RoboSharp.Tests.Test_Setup.Source_Standard), new DirectoryInfo(RoboSharp.Tests.Test_Setup.TestDestination));
        public static FilePair FilePair = new FilePair(new FileInfo(RoboSharp.Tests.Test_Setup.Source_Standard + @"\1024_Bytes.txt"), new FileInfo(RoboSharp.Tests.Test_Setup.TestDestination + @"\1024_Bytes.txt"));

        public static DirectoryPair DiffDriveDirPair = new DirectoryPair(new DirectoryInfo(RoboSharp.Tests.Test_Setup.Source_Standard), new DirectoryInfo(@"D:\"));
        public static FilePair DiffDriveFilePair = new FilePair(new FileInfo(RoboSharp.Tests.Test_Setup.Source_Standard + @"\1024_Bytes.txt"), new FileInfo(@"D:\1024_Bytes.txt"));

        public static FilePair SameFile = new FilePair(FilePair.Source, FilePair.Source);
        public static DirectoryPair SameDir = new DirectoryPair(DirPair.Source, DirPair.Source);

        [TestInitialize]
        public void TestPrep()
        {
            RoboSharp.Tests.Test_Setup.ClearOutTestDestination();
        }

        [TestMethod()]
        public void IsLocatedOnSameDriveTest()
        {
            Assert.IsTrue(DirPair.IsLocatedOnSameDrive());
            Assert.IsFalse(DiffDriveDirPair.IsLocatedOnSameDrive());
        }

        [TestMethod()]
        public void IsLocatedOnSameDriveTest1()
        {
            Assert.IsTrue(FilePair.IsLocatedOnSameDrive());
            Assert.IsFalse(DiffDriveFilePair.IsLocatedOnSameDrive());
        }

        [TestMethod()]
        public void IsSourceNewerTest()
        {
            Assert.IsTrue(FilePair.IsSourceNewer());
            Assert.IsFalse(SameFile.IsSourceNewer());
        }

        [TestMethod()]
        public void IsDestinationNewerTest()
        {
            Assert.IsFalse(FilePair.IsDestinationNewer());
            Assert.IsFalse(SameFile.IsDestinationNewer());
            Assert.IsTrue(new FilePair(FilePair.Destination, FilePair.Source).IsDestinationNewer());
        }

        [TestMethod()]
        public void IsSameDateTest()
        {
            Assert.IsTrue(SameFile.IsSameDate());
        }

        [TestMethod()]
        public void CreateSourceChildTest_Directory()
        {
            var child = DirPair.CreateSourceChild(new DirectoryInfo(Path.Combine(DirPair.Source.FullName, "Test")), (O, E) => new DirectoryPair(O, E));
            Assert.IsNotNull(child);
            Assert.IsTrue(child.Source.FullName.StartsWith(DirPair.Source.FullName));
            Assert.IsTrue(child.Destination.FullName.StartsWith(DirPair.Destination.FullName));
        }

        [TestMethod()]
        public void CreateDestinationChildTest_Directory()
        {
            var child = DirPair.CreateDestinationChild(new DirectoryInfo(Path.Combine(DirPair.Destination.FullName, "Test")), (O, E) => new DirectoryPair(O, E));
            Assert.IsNotNull(child);
            Assert.IsTrue(child.Source.FullName.StartsWith(DirPair.Source.FullName));
            Assert.IsTrue(child.Destination.FullName.StartsWith(DirPair.Destination.FullName));
        }

        [TestMethod()]
        public void CreateSourceChildTest_File()
        {
            var child = DirPair.CreateSourceChild(new FileInfo(Path.Combine(DirPair.Source.FullName, "Test.TXT")), (O, E) => new FilePair(O, E));
            Assert.IsNotNull(child);
            Assert.IsTrue(child.Source.FullName.StartsWith(DirPair.Source.FullName));
            Assert.IsTrue(child.Destination.FullName.StartsWith(DirPair.Destination.FullName));
        }

        [TestMethod()]
        public void CreateDestinationChild_File()
        {
            var child = DirPair.CreateDestinationChild(new FileInfo(Path.Combine(DirPair.Destination.FullName, "Test.TXT")), (O, E) => new FilePair(O, E));
            Assert.IsNotNull(child);
            Assert.IsTrue(child.Source.FullName.StartsWith(DirPair.Source.FullName));
            Assert.IsTrue(child.Destination.FullName.StartsWith(DirPair.Destination.FullName));
        }

        [TestMethod()]
        public void GetFilePairsTest()
        {
            Assert.AreEqual(4, DirPair.GetFilePairs().Count());
        }

        [TestMethod()]
        public void GetFilePairsEnumerableTest()
        {
            Assert.AreEqual(4, DirPair.EnumerateFilePairs().Count());
        }

        [TestMethod()]
        public void GetDirectoryPairsTest()
        {
            Assert.AreEqual(2, DirPair.GetDirectoryPairs().Count());
            Assert.AreEqual(2, SameDir.GetDirectoryPairs().Count());
            Assert.IsTrue(SameDir.EnumerateDirectoryPairs().Distinct(new PairEqualityComparer()).Count() == 2);
        }


        [TestMethod()]
        public void GetDirectoryPairsEnumerableTest()
        {
            Assert.AreEqual(2, DirPair.EnumerateDirectoryPairs().Count());
            Assert.AreEqual(2, SameDir.EnumerateDirectoryPairs().Count());
        }

        [TestMethod()]
        public void CachedEnumerableDigTest()
        {
            int SubDirs = 0;
            Dig(DirPair).Wait();
            Assert.AreEqual(4, SubDirs);
            async Task Dig(DirectoryPair dir)
            {
                foreach (var d in dir.EnumerateDirectoryPairs())
                {
                    SubDirs++;
                    await Dig(d);
                }
            }
        }

        [TestMethod()]
        public void NoneTest()
        {
            var collection = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Assert.IsFalse(collection.None());
            Assert.IsTrue(collection.Where(i => i > 10).None());

            Assert.IsFalse(collection.None(i => i == 8));   // Succeeds on 8
            Assert.IsFalse(collection.None(i => i != 8));   // Succeeds on 1
            Assert.IsTrue(collection.None(i => i > 10));    // Checks all
            Assert.IsTrue(collection.None(i => i == 10));   // Checks all


            //Validate that all file pairs have source and destinations with valid parents
            var FilePairs = DirPair.GetFilePairs();
            var FileEnum = DirPair.EnumerateFilePairs();
            Assert.IsTrue(FilePairs.None(o => o.Source.Directory.FullName != DirPair.Source.FullName));
            Assert.IsTrue(FilePairs.None(o => o.Destination.Directory.FullName != DirPair.Destination.FullName));
            Assert.IsTrue(FileEnum.None(o => o.Source.Directory.FullName != DirPair.Source.FullName));
            Assert.IsTrue(FileEnum.None(o => o.Destination.Directory.FullName != DirPair.Destination.FullName));

            //Validate that all directory pairs have source and destinations with valid parents
            var DirPairs = DirPair.GetDirectoryPairs();
            var DirEnum = DirPair.EnumerateDirectoryPairs();
            Assert.IsTrue(DirPairs.None(o => o.Source.Parent.FullName != DirPair.Source.FullName));
            Assert.IsTrue(DirPairs.None(o => o.Destination.Parent.FullName != DirPair.Destination.FullName));
            Assert.IsTrue(DirEnum.None(o => o.Source.Parent.FullName != DirPair.Source.FullName));
            Assert.IsTrue(DirEnum.None(o => o.Destination.Parent.FullName != DirPair.Destination.FullName));
        }
    }
}