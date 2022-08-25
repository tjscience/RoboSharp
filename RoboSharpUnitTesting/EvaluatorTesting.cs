using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoboSharp;
using RoboSharp.ConsumerHelpers;
using RoboSharp.Results;

namespace RoboSharpUnitTesting
{
    public class CustomIRoboCommand : RoboSharp.RoboCommand
    {
        public CustomIRoboCommand()
        {
            base.CopyOptions.Source = RoboSharpUnitTesting.Test_Setup.Source_Standard;
            base.CopyOptions.Destination = RoboSharpUnitTesting.Test_Setup.TestDestination;
        }

        public RoboCopyResults Results => results;

        public override Task Start(string domain = "", string username = "", string password = "")
        {
            var evaluator = new RoboSharp.ConsumerHelpers.SourceDestinationEvaluator(this);
            var estimator = new ProgressEstimator(this);
            DirectoryInfo source = new DirectoryInfo(base.CopyOptions.Source);
            DirectoryInfo dest = new DirectoryInfo(base.CopyOptions.Destination);
            DirSourceDestinationPair SourceDestPair = new DirSourceDestinationPair(source, dest);

            FileSourceDestinationPair CreateFilePair(FileInfo sourcefile)
            {
                return new FileSourceDestinationPair(sourcefile, new FileInfo(source.FullName.Replace(source.FullName, dest.FullName)));
            }
            DirSourceDestinationPair CreateDirPair(DirectoryInfo dir)
            {
                return new DirSourceDestinationPair(dir, new DirectoryInfo(source.FullName.Replace(source.FullName, dest.FullName)));
            }

            // Recursive routine that digs into the folder structure
            void Dig(DirSourceDestinationPair dirs)
            {
                //Get the files in each directory
                List<FileSourceDestinationPair> pairs = new List<FileSourceDestinationPair>();
                foreach (FileInfo f in dirs.Source.GetFiles())
                    pairs.Add(CreateFilePair(f));
                if (dirs.Destination.Exists)
                    foreach (FileInfo f in dirs.Destination.GetFiles())
                        if (pairs.Any(d => d.Destination.FullName == f.FullName))
                        { }
                        else pairs.Add(CreateFilePair(f));

                foreach (var pair in pairs)
                {
                    bool shouldCopy = evaluator.ShouldCopyFile(pair, out ProcessedFileInfo info);
                    estimator.AddFile(info, true);
                    if (shouldCopy)
                    {
                        dirs.Destination.Create();
                        pair.Source.CopyTo(pair.Destination.FullName, true);
                        estimator.AddFileCopied(info);
                    }
                }

                //Get the directories in source and destination
                List<DirSourceDestinationPair> dirPairs = new List<DirSourceDestinationPair>();
                foreach (DirectoryInfo f in dirs.Source.GetDirectories())
                    dirPairs.Add(CreateDirPair(f));
                if (dirs.Destination.Exists)
                    foreach (DirectoryInfo f in dirs.Destination.GetDirectories())
                        if (dirPairs.Any(d => d.Destination.FullName == f.FullName))
                        { }
                        else dirPairs.Add(CreateDirPair(f));

                foreach (var pair in dirPairs)
                {
                    bool shouldDig = evaluator.ShouldCopyDir(pair, out var info);
                    estimator.AddDir(info);
                    if (shouldDig) Dig(pair);
                }

            }

            Dig(SourceDestPair);
            results = estimator.GetResults();
            return Task.CompletedTask;

        }
    }

    class EvaluatorTesting
    {
    }


}
