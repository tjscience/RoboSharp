using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoboSharp.Interfaces;
using RoboSharp.Results;

// Do Not change NameSpace here! -> Must be RoboSharp due to prior releases
namespace RoboSharp.EventArgObjects
{
    /// <summary>
    /// Event Args provided by IProgressEstimator objects to notify the UI it should refresh the stat values
    /// </summary>
    public class IProgressEstimatorUpdateEventArgs : EventArgs
    {
        /// <summary> Dummy Args with Values of 0 to perform final updates through ProgressEstimator without creating new args every time</summary>
        internal static IProgressEstimatorUpdateEventArgs DummyArgs { get; } = new IProgressEstimatorUpdateEventArgs(null, null, null, null);
        
        private IProgressEstimatorUpdateEventArgs() : base() { }

        internal IProgressEstimatorUpdateEventArgs(IProgressEstimator estimator, IStatistic ByteChange, IStatistic FileChange, IStatistic DirChange) : base()
        {
            Estimator = estimator;
            ValueChange_Bytes = ByteChange ?? Statistic.Default_Bytes;
            ValueChange_Files = FileChange ?? Statistic.Default_Files;
            ValueChange_Directories = DirChange ?? Statistic.Default_Dirs;
        }

        /// <summary>
        /// <inheritdoc cref="Results.ProgressEstimator"/>
        /// </summary>
        private IProgressEstimator Estimator { get; }

        /// <inheritdoc cref="IProgressEstimator.BytesStatistic"/>
        public IStatistic BytesStatistic => Estimator?.BytesStatistic;

        /// <inheritdoc cref="IProgressEstimator.FilesStatistic"/>
        public IStatistic FilesStatistic => Estimator?.FilesStatistic;

        /// <inheritdoc cref="IProgressEstimator.DirectoriesStatistic"/>
        public IStatistic DirectoriesStatistic => Estimator?.DirectoriesStatistic;

        /// <summary>IStatistic Object that shows how much was added to the { <see cref="BytesStatistic"/> } object during this UI Update</summary>
        public IStatistic ValueChange_Bytes { get; }

        /// <summary>IStatistic Object that shows how much was added to the { <see cref="FilesStatistic"/> } object during this UI Update</summary>
        public IStatistic ValueChange_Files { get; }

        /// <summary>IStatistic Object that shows how much was added to the { <see cref="DirectoriesStatistic"/> } object during this UI Update</summary>
        public IStatistic ValueChange_Directories { get; }

    }
}

