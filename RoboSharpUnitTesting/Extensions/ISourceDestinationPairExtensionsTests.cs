using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SO = RoboSharp.Extensions.ISourceDestinationPairExtensions;

namespace RoboSharp.Extensions.Tests
{
    [TestClass()]
    public class ISourceDestinationPairExtensionsTests
    {
        public static DirectorySourceDestinationPair DirPair = new DirectorySourceDestinationPair(new DirectoryInfo(RoboSharp.Tests.Test_Setup.Source_Standard), new DirectoryInfo(RoboSharp.Tests.Test_Setup.TestDestination));
        public static FileSourceDestinationPair FilePair = new FileSourceDestinationPair(new FileInfo(RoboSharp.Tests.Test_Setup.Source_Standard + @"\1024_Bytes.txt"), new FileInfo(RoboSharp.Tests.Test_Setup.TestDestination + @"\1024_Bytes.txt"));

        public static DirectorySourceDestinationPair DiffDriveDirPair = new DirectorySourceDestinationPair(new DirectoryInfo(RoboSharp.Tests.Test_Setup.Source_Standard), new DirectoryInfo(@"D:\"));
        public static FileSourceDestinationPair DiffDriveFilePair = new FileSourceDestinationPair(new FileInfo(RoboSharp.Tests.Test_Setup.Source_Standard + @"\1024_Bytes.txt"), new FileInfo(@"D:\1024_Bytes.txt"));

        public static FileSourceDestinationPair SameFile = new FileSourceDestinationPair(FilePair.Source, FilePair.Source);
        public static DirectorySourceDestinationPair SameDir = new DirectorySourceDestinationPair(DirPair.Source, DirPair.Source);

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
            Assert.IsTrue(new FileSourceDestinationPair(FilePair.Destination, FilePair.Source).IsDestinationNewer());
        }

        [TestMethod()]
        public void IsSameDateTest()
        {
            Assert.IsTrue(SameFile.IsSameDate());
        }

        [TestMethod()]
        public void CreateSourceChildTest()
        {
            Assert.IsNotNull(DirPair.CreateSourceChild(new DirectoryInfo(Path.Combine(DirPair.Source.FullName, "Test")), (O, E) => new DirectorySourceDestinationPair(O, E)));
        }

        [TestMethod()]
        public void CreateDestinationChildTest()
        {
            Assert.IsNotNull(DirPair.CreateDestinationChild(new DirectoryInfo(Path.Combine(DirPair.Destination.FullName, "Test")), (O, E) => new DirectorySourceDestinationPair(O, E)));
        }

        [TestMethod()]
        public void CreateSourceChildTest1()
        {
            Assert.IsNotNull(DirPair.CreateSourceChild(new FileInfo(Path.Combine(DirPair.Source.FullName, "Test.TXT")), (O, E) => new FileSourceDestinationPair(O, E)));
        }

        [TestMethod()]
        public void CreateDestinationChildTest1()
        {
            Assert.IsNotNull(DirPair.CreateDestinationChild(new FileInfo(Path.Combine(DirPair.Destination.FullName, "Test.TXT")), (O, E) => new FileSourceDestinationPair(O, E)));
        }

        [TestMethod()]
        public void GetFilePairsTest()
        {
            Assert.AreEqual(4, DirPair.GetFilePairs().Count());
        }

        [TestMethod()]
        public void GetFilePairsEnumerableTest()
        {
            Assert.AreEqual(4, DirPair.GetFilePairsEnumerable().Count());
        }

        [TestMethod()]
        public void GetDirectoryPairsTest()
        {
            Assert.AreEqual(2, DirPair.GetDirectoryPairs().Count());
        }

        [TestMethod()]
        public void GetDirectoryPairsEnumerableTest()
        {
            Assert.AreEqual(2, DirPair.GetDirectoryPairsEnumerable().Count());
        }

        [TestMethod()]
        public void CachedEnumerableDigTest()
        {
            int SubDirs = 0;
            Dig(DirPair).Wait();
            Assert.AreEqual(4, SubDirs);
            async Task Dig(DirectorySourceDestinationPair dir)
            {
                foreach (var d in dir.GetDirectoryPairsEnumerable())
                {
                    SubDirs++;
                    await Dig(d);
                }
            }
        }
    }
}