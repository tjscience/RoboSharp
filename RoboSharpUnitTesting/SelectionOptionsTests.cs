using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp;
using RoboSharp.Results;
using System;
using System.Collections.Generic;
using System.IO;

namespace RoboSharpUnitTesting
{
    [TestClass]
    public class SelectionOptionsTest
    {
        const FileAttributes R = FileAttributes.ReadOnly;
        const FileAttributes RA = R | FileAttributes.Archive;
        const FileAttributes RAS = RA | FileAttributes.System;
        const FileAttributes RASH = RAS | FileAttributes.Hidden;
        const FileAttributes RASHN = RASH | FileAttributes.NotContentIndexed;
        const FileAttributes RASHNE = RASHN | FileAttributes.Encrypted;
        const FileAttributes RASHNET = RASHNE | FileAttributes.Temporary;
        const FileAttributes RASHNETO = RASHNET | FileAttributes.Offline;
        const FileAttributes UNUSED_VALUES = ~RASHNETO;

        [DataRow(R, FileAttributes.ReadOnly)]
        [DataRow(RA, FileAttributes.ReadOnly | FileAttributes.Archive)]
        [DataRow(RAS, FileAttributes.ReadOnly | FileAttributes.Archive | FileAttributes.System)]
        [DataRow(RASH, FileAttributes.ReadOnly | FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden)]
        [DataRow(RASHN, FileAttributes.ReadOnly | FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden | FileAttributes.NotContentIndexed)]
        [DataRow(RASHNE, FileAttributes.ReadOnly | FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden | FileAttributes.NotContentIndexed | FileAttributes.Encrypted)]
        [DataRow(RASHNET, FileAttributes.ReadOnly | FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden | FileAttributes.NotContentIndexed | FileAttributes.Encrypted | FileAttributes.Temporary)]
        [DataRow(RASHNETO, FileAttributes.ReadOnly | FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden | FileAttributes.NotContentIndexed | FileAttributes.Encrypted | FileAttributes.Temporary | FileAttributes.Offline)]
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
        [DataRow("RASHNETO", RASHNETO)]
        [TestMethod]
        public void Test_ConvertFileAttrStringToEnum(string input, FileAttributes? value)
        {
            Assert.AreEqual(value, SelectionOptions.ConvertFileAttrStringToEnum(input));
        }

        [DataRow("", null)]
        [DataRow("R", R)]
        [DataRow("RA", RA)]
        [DataRow("RAS", RAS)]
        [DataRow("RASH", RASH)]
        [DataRow("RASHN", RASHN)]
        [DataRow("RASHNE", RASHNE)]
        [DataRow("RASHNET", RASHNET)]
        [DataRow("RASHNETO", RASHNETO)]
        [TestMethod]
        public void Test_ConvertFileAttrToString(string output, FileAttributes? input)
        {
            Assert.AreEqual(output, SelectionOptions.ConvertFileAttrToString(input));
        }

        [TestMethod]
        public void Test_ConvertFileAttrToString_InvalidString()
        {
            Assert.ThrowsException<ArgumentException>(() => SelectionOptions.ConvertFileAttrStringToEnum("Q"));
        }

        [DataRow("", null)]
        [DataRow("R", R)]
        [DataRow("RA", RA)]
        [DataRow("RAS", RAS)]
        [DataRow("RASH", RASH)]
        [DataRow("RASHN", RASHN)]
        [DataRow("RASHNE", RASHNE)]
        [DataRow("RASHNET", RASHNET)]
        [DataRow("RASHNETO", RASHNETO)]
        [TestMethod]
        public void Test_IncludedAttributes(string value, FileAttributes? expected)
        {
            var options = new SelectionOptions();
            options.IncludeAttributes = value;
            Assert.AreEqual(expected, options.GetIncludedAttributes());
            Assert.AreEqual(options.IncludeAttributes, value);
        }

        [DataRow("", null)]
        [DataRow("R", R)]
        [DataRow("RA", RA)]
        [DataRow("RAS", RAS)]
        [DataRow("RASH", RASH)]
        [DataRow("RASHN", RASHN)]
        [DataRow("RASHNE", RASHNE)]
        [DataRow("RASHNET", RASHNET)]
        [DataRow("RASHNETO", RASHNETO)]
        [TestMethod]
        public void Test_ExcludedAttributes(string value, FileAttributes? expected)
        {
            var options = new SelectionOptions();
            options.ExcludeAttributes = value;
            Assert.AreEqual(expected, options.GetExcludedAttributes());
            Assert.AreEqual(value, options.ExcludeAttributes);
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
        [DataRow("RASHNETO", RASHNETO)]
        [TestMethod]
        public void Test_SetExcludedAttributes(string value, FileAttributes? input)
        {
            var options = new SelectionOptions();
            options.SetExcludedAttributes(input);
            Assert.AreEqual(value, options.ExcludeAttributes);
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
        [DataRow("RASHNETO", RASHNETO)]
        [DataRow("RASHNETO", RASHNETO)]
        [TestMethod]
        public void Test_SetIncludedAttributes(string value, FileAttributes? input)
        {
            var options = new SelectionOptions();
            options.SetIncludedAttributes(input);
            Assert.AreEqual(value, options.IncludeAttributes);
        }


        //[DataRow(SelectionFlags.Default)]
        //[DataRow(SelectionFlags.ExcludeChanged)]
        //[DataRow(SelectionFlags.ExcludeExtra)]
        //[DataRow(SelectionFlags.ExcludeJunctionPoints)]
        //[DataRow(SelectionFlags.ExcludeJunctionPointsForDirectories)]
        //[DataRow(SelectionFlags.ExcludeJunctionPointsForFiles)]
        //[DataRow(SelectionFlags.ExcludeLonely)]
        //[DataRow(SelectionFlags.ExcludeNewer)]
        //[DataRow(SelectionFlags.ExcludeOlder)]
        //[DataRow(SelectionFlags.IncludeSame)]
        //[DataRow(SelectionFlags.IncludeTweaked)]
        //[DataRow(SelectionFlags.OnlyCopyArchiveFiles)]
        //[DataRow(SelectionFlags.OnlyCopyArchiveFilesAndResetArchiveFlag)]
        [TestMethod]
        public void Test_ApplySelectionFlags()
        {
            foreach(SelectionFlags flag in typeof(SelectionFlags).GetEnumValues())
            {
                var options = new SelectionOptions();
                try
                {
                    options.ApplySelectionFlags(flag);
                    Assert.AreEqual(flag, options.GetSelectionFlags());
                    Assert.AreEqual(flag.HasFlag(SelectionFlags.ExcludeChanged), options.ExcludeChanged);
                    Assert.AreEqual(flag.HasFlag(SelectionFlags.ExcludeExtra), options.ExcludeExtra);
                    Assert.AreEqual(flag.HasFlag(SelectionFlags.ExcludeJunctionPoints), options.ExcludeJunctionPoints);
                    Assert.AreEqual(flag.HasFlag(SelectionFlags.ExcludeJunctionPointsForDirectories), options.ExcludeJunctionPointsForDirectories);
                    Assert.AreEqual(flag.HasFlag(SelectionFlags.ExcludeJunctionPointsForFiles), options.ExcludeJunctionPointsForFiles);
                    Assert.AreEqual(flag.HasFlag(SelectionFlags.ExcludeLonely), options.ExcludeLonely);
                    Assert.AreEqual(flag.HasFlag(SelectionFlags.ExcludeNewer), options.ExcludeNewer);
                    Assert.AreEqual(flag.HasFlag(SelectionFlags.ExcludeOlder), options.ExcludeOlder);
                    Assert.AreEqual(flag.HasFlag(SelectionFlags.IncludeSame), options.IncludeSame);
                    Assert.AreEqual(flag.HasFlag(SelectionFlags.IncludeTweaked), options.IncludeTweaked);
                    Assert.AreEqual(flag.HasFlag(SelectionFlags.OnlyCopyArchiveFiles), options.OnlyCopyArchiveFiles);
                    Assert.AreEqual(flag.HasFlag(SelectionFlags.OnlyCopyArchiveFilesAndResetArchiveFlag), options.OnlyCopyArchiveFilesAndResetArchiveFlag);
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
