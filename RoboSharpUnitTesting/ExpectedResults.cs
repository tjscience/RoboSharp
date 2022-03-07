using RoboSharp.Interfaces;
using RoboSharp.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharpUnitTesting
{
    internal class ExpectedResults : IResults
    {
        public Statistic ByteStatistics { get; } = new Statistic(type: Statistic.StatType.Bytes);
        public Statistic DirectoriesStatistics { get; } = new Statistic(type: Statistic.StatType.Directories);
        public Statistic FileStatistics { get; } = new Statistic(type: Statistic.StatType.Files);

        IStatistic IResults.DirectoriesStatistic => DirectoriesStatistics;

        IStatistic IResults.FilesStatistic => FileStatistics;

        IStatistic IResults.BytesStatistic => ByteStatistics;

        RoboCopyExitStatus IResults.Status => throw new NotImplementedException();
    }
}
