using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp;
using RoboSharp.Results;
using System;
using System.Collections.Generic;
using System.IO;

namespace RoboSharpUnitTesting
{
    [TestClass]
    public class CopyOptionsTest
    {
        const FileAttributes R = FileAttributes.ReadOnly;
        const FileAttributes RA = R | FileAttributes.Archive;
        const FileAttributes RAS = RA | FileAttributes.System;
        const FileAttributes RASH = RAS | FileAttributes.Hidden;
        const FileAttributes RASHN = RASH | FileAttributes.NotContentIndexed;
        const FileAttributes RASHNE = RASHN | FileAttributes.Encrypted;
        const FileAttributes RASHNET = RASHNE | FileAttributes.Temporary;
        const FileAttributes UNUSED_VALUES = ~RASHNET;

        [DataRow(R, FileAttributes.ReadOnly)]
        [DataRow(RA, FileAttributes.ReadOnly | FileAttributes.Archive)]
        [DataRow(RAS, FileAttributes.ReadOnly | FileAttributes.Archive | FileAttributes.System)]
        [DataRow(RASH, FileAttributes.ReadOnly | FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden)]
        [DataRow(RASHN, FileAttributes.ReadOnly | FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden | FileAttributes.NotContentIndexed)]
        [DataRow(RASHNE, FileAttributes.ReadOnly | FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden | FileAttributes.NotContentIndexed | FileAttributes.Encrypted)]
        [DataRow(RASHNET, FileAttributes.ReadOnly | FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden | FileAttributes.NotContentIndexed | FileAttributes.Encrypted | FileAttributes.Temporary)]
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
        [DataRow("RASHN", RASHN)]
        [DataRow("RASHNE", RASHNE)]
        [DataRow("RASHNET", RASHNET)]
        [DataRow("RASHNETO", RASHNET)]
        [TestMethod]
        public void Test_AddAttributes(string input, FileAttributes? expected)
        {
            var options = new CopyOptions();
            options.AddAttributes = input;
            Assert.AreEqual(expected, options.GetAddAttributes());
            Assert.AreEqual(options.AddAttributes, input);
        }

        [DataRow("", null)]
        [DataRow("R", R)]
        [DataRow("RA", RA)]
        [DataRow("RAS", RAS)]
        [DataRow("RASH", RASH)]
        [DataRow("RASHN", RASHN)]
        [DataRow("RASHNE", RASHNE)]
        [DataRow("RASHNET", RASHNET)]
        [DataRow("RASHNETO", RASHNET)]
        [TestMethod]
        public void Test_RemoveAttributes(string input, FileAttributes? expected)
        {
            var options = new CopyOptions();
            options.RemoveAttributes = input;
            Assert.AreEqual(expected, options.GetRemoveAttributes());
            Assert.AreEqual(input, options.RemoveAttributes);
        }

        [DataRow("", null)]
        [DataRow("", UNUSED_VALUES)]
        [DataRow("R", R)]
        [DataRow("RA", RA)]
        [DataRow("RAS", RAS)]
        [DataRow("RASH", RASH)]
        [DataRow("RASHN", RASHN)]
        [DataRow("RASHNE", RASHNE)]
        [DataRow("RASHNET", RASHNET)]
        [DataRow("RASHNET", RASHNET | FileAttributes.Offline)]
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
        [DataRow("RASHN", RASHN)]
        [DataRow("RASHNE", RASHNE)]
        [DataRow("RASHNET", RASHNET)]
        [DataRow("RASHNET", RASHNET | FileAttributes.Offline)]
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
            var options = new CopyOptions();
            options.RunHours = $"{startTime}-{endTime}";
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
            foreach (CopyActionFlags flag in typeof(CopyActionFlags).GetEnumValues())
            {
                var options = new CopyOptions();
                try
                {
                    options.ApplyActionFlags(flag);
                    Assert.AreEqual(flag, options.GetCopyActionFlags());
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
        }
    }
}
