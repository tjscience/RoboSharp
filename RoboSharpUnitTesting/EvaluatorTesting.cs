using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoboSharp;
using RoboSharp.Extensions;
using RoboSharp.Results;

namespace RoboSharp.Tests
{
    public class CustomIRoboCommand : RoboSharp.RoboCommand
    {
        public CustomIRoboCommand()
        {
            base.CopyOptions.Source = RoboSharp.Tests.Test_Setup.Source_Standard;
            base.CopyOptions.Destination = RoboSharp.Tests.Test_Setup.TestDestination;
        }

        public RoboCopyResults Results => results;

        public override Task Start(string domain = "", string username = "", string password = "")
        {
            var evaluator = new RoboSharp.Extensions.SourceDestinationEvaluator(this);
            var estimator = new ProgressEstimator(this);
            DirectoryInfo source = new DirectoryInfo(base.CopyOptions.Source);
            DirectoryInfo dest = new DirectoryInfo(base.CopyOptions.Destination);
            DirectorySourceDestinationPair SourceDestPair = new DirectorySourceDestinationPair(source, dest);

            // Recursive routine that digs into the folder structure
            void Dig(DirectorySourceDestinationPair dirs)
            {
                //Get the files in each directory
                foreach (var pair in dirs.GetFilePairsEnumerable())
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
                foreach (var pair in dirs.GetDirectoryPairsEnumerable())
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
