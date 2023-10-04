using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp;
using RoboSharp.Results;
using System;
using System.Collections.Generic;
using System.IO;

namespace RoboSharp.UnitTests
{
    [TestClass]
    public class CopyOptionsTest
    {
        const FileAttributes R = FileAttributes.ReadOnly;
        const FileAttributes RA = R | FileAttributes.Archive;
        const FileAttributes RAS = RA | FileAttributes.System;
        const FileAttributes RASH = RAS | FileAttributes.Hidden;
        const FileAttributes RASHC = RASH | FileAttributes.Compressed;
        const FileAttributes RASHCN = RASHC | FileAttributes.NotContentIndexed;
        const FileAttributes RASHCNE = RASHCN | FileAttributes.Encrypted;
        const FileAttributes RASHCNET = RASHCNE | FileAttributes.Temporary;
        const FileAttributes UNUSED_VALUES = ~RASHCNET;

        [DataRow(R, FileAttributes.ReadOnly)]
        [DataRow(RA, FileAttributes.ReadOnly | FileAttributes.Archive)]
        [DataRow(RAS, FileAttributes.ReadOnly | FileAttributes.Archive | FileAttributes.System)]
        [DataRow(RASH, FileAttributes.ReadOnly | FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden)]
        [DataRow(RASHC, FileAttributes.ReadOnly | FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden | FileAttributes.Compressed)]
        [DataRow(RASHCN, FileAttributes.ReadOnly | FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden | FileAttributes.Compressed | FileAttributes.NotContentIndexed)]
        [DataRow(RASHCNE, FileAttributes.ReadOnly | FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden | FileAttributes.Compressed | FileAttributes.NotContentIndexed | FileAttributes.Encrypted)]
        [DataRow(RASHCNET, FileAttributes.ReadOnly | FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden | FileAttributes.Compressed | FileAttributes.NotContentIndexed | FileAttributes.Encrypted | FileAttributes.Temporary)]
        [TestMethod] // Verify the constants supplied to the other tests are value
        public void Test_Constants(FileAttributes value, FileAttributes expected)
        {
            Assert.AreEqual(expected, value);
        }

        [DataRow("", null)]
        [DataRow("R", R)]
        [DataRow("RA", RA)]
        [DataRow("RAS", RAS)]
        [DataRow("RASH", RASH)]
        [DataRow("RASHC", RASHC)]
        [DataRow("RASHCN", RASHCN)]
        [DataRow("RASHCNE", RASHCNE)]
        [DataRow("RASHCNET", RASHCNET)]
        [DataRow("RASHCNETO", RASHCNET, "RASHCNET")]
        [TestMethod]
        public void Test_AddAttributes(string input, FileAttributes? expected, string expectedstring = null)
        {
            var options = new CopyOptions
            {
                AddAttributes = input
            };
            Assert.AreEqual(expected, options.GetAddAttributes());
            Assert.AreEqual(expectedstring ?? input,options.AddAttributes);
        }

        [DataRow("", null)]
        [DataRow("R", R)]
        [DataRow("RA", RA)]
        [DataRow("RAS", RAS)]
        [DataRow("RASH", RASH)]
        [DataRow("RASHC", RASHC)]
        [DataRow("RASHCN", RASHCN)]
        [DataRow("RASHCNE", RASHCNE)]
        [DataRow("RASHCNET", RASHCNET)]
        [DataRow("RASHCNETO", RASHCNET, "RASHCNET")]
        [TestMethod]
        public void Test_RemoveAttributes(string input, FileAttributes? expected, string expectedstring = null)
        {
            var options = new CopyOptions
            {
                RemoveAttributes = input
            };
            Assert.AreEqual(expected, options.GetRemoveAttributes());
            Assert.AreEqual(expectedstring ?? input, options.RemoveAttributes);
        }

        [DataRow("", null)]
        [DataRow("", UNUSED_VALUES)]
        [DataRow("R", R)]
        [DataRow("RA", RA)]
        [DataRow("RAS", RAS)]
        [DataRow("RASH", RASH)]
        [DataRow("RASHC", RASHC)]
        [DataRow("RASHCN", RASHCN)]
        [DataRow("RASHCNE", RASHCNE)]
        [DataRow("RASHCNET", RASHCNET)]
        [DataRow("RASHCNET", RASHCNET | FileAttributes.Offline)]
        [TestMethod]
        public void Test_SetAddAttributes(string expected, FileAttributes? input)
        {
            var options = new CopyOptions();
            options.SetAddAttributes(input);
            Assert.AreEqual(expected, options.AddAttributes);
        }

        [DataRow("", null)]
        [DataRow("", UNUSED_VALUES)]
        [DataRow("R", R)]
        [DataRow("RA", RA)]
        [DataRow("RAS", RAS)]
        [DataRow("RASH", RASH)]
        [DataRow("RASHC", RASHC)]
        [DataRow("RASHCN", RASHCN)]
        [DataRow("RASHCNE", RASHCNE)]
        [DataRow("RASHCNET", RASHCNET)]
        [DataRow("RASHCNET", RASHCNET | FileAttributes.Offline)]
        [TestMethod]
        public void Test_SetRemoveAttributes(string expected, FileAttributes? input)
        {
            var options = new CopyOptions();
            options.SetRemoveAttributes(input);
            Assert.AreEqual(expected, options.RemoveAttributes);
        }

        [DataRow("0010", "1310")]
        [TestMethod]
        public void Test_RunHours(string startTime, string endTime)
        {
            var options = new CopyOptions
            {
                RunHours = $"{startTime}-{endTime}"
            };
            Assert.AreEqual(startTime, options.GetRunHours_StartTime());
            Assert.AreEqual(endTime, options.GetRunHours_EndTime());
        }

        [DataRow("0010-1310", true)]
        [DataRow("0010-10", false)]
        [DataRow("10-1010", false)]
        [DataRow("q000-1010", false)]
        [DataRow("1010-1l10", false)]
        [DataRow("1010", false)]
        [TestMethod]
        public void Test_IsRunHoursStringValid(string input, bool expected)
        {
            Assert.AreEqual(expected, CopyOptions.IsRunHoursStringValid(input));
            Assert.AreEqual(expected, new CopyOptions().CheckRunHoursString(input));
        }

        [TestMethod]
        public void Test_ApplyCopyFlags()
        {
            CopyOptions.SetCanEnableCompression(true);
            foreach (CopyActionFlags flag in typeof(CopyActionFlags).GetEnumValues())
            {
                var options = new CopyOptions();
                try
                {
                    options.ApplyActionFlags(flag);
                    Assert.AreEqual(flag, options.GetCopyActionFlags());
                    Assert.AreEqual(flag.HasFlag(CopyActionFlags.Compress), options.Compress);
                    Assert.AreEqual(flag.HasFlag(CopyActionFlags.CopySubdirectories), options.CopySubdirectories);
                    Assert.AreEqual(flag.HasFlag(CopyActionFlags.CopySubdirectoriesIncludingEmpty), options.CopySubdirectoriesIncludingEmpty);
                    Assert.AreEqual(flag.HasFlag(CopyActionFlags.CreateDirectoryAndFileTree), options.CreateDirectoryAndFileTree);
                    Assert.AreEqual(flag.HasFlag(CopyActionFlags.Mirror), options.Mirror);
                    Assert.AreEqual(flag.HasFlag(CopyActionFlags.MoveFiles), options.MoveFiles);
                    Assert.AreEqual(flag.HasFlag(CopyActionFlags.MoveFilesAndDirectories), options.MoveFilesAndDirectories);
                    Assert.AreEqual(flag.HasFlag(CopyActionFlags.Purge), options.Purge);
                }
                catch
                {
                    Console.WriteLine($"Error occured on flag: {flag}");
                    throw;
                }
            }
            CopyOptions.SetCanEnableCompression(false);
        }

        [TestMethod]
        public void Test_CanEnableCompression()
        {
            try
            {
                Assert.IsFalse(CopyOptions.CanEnableCompression);
                CopyOptions opt = new CopyOptions();
                Assert.IsFalse(opt.Compress);
                opt.Compress = true;
                Assert.IsFalse(opt.Compress);
                CopyOptions.SetCanEnableCompression(true);
                Assert.IsTrue(opt.Compress);
                Console.WriteLine("Current OS Version: " + Environment.OSVersion.VersionString);

                if (Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 19045)
                {
                    // Dev PC where its /compress is known to be allowed
                    CopyOptions.SetCanEnableCompression(false);
                    Assert.IsTrue(CopyOptions.TestCompressionFlag().Result);
                    Assert.IsTrue(CopyOptions.CanEnableCompression);
                }
            }
            finally
            {
                CopyOptions.SetCanEnableCompression(false);
            }
        }
    }
}
