using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoboSharp.Results;

namespace RoboSharp.Interfaces
{
    /// <summary>
    /// Interface to provide Read-Only access to a <see cref="RoboCopyResultsList"/>
    /// </summary>
    public interface IRoboCopyResultsList : IEnumerable<RoboCopyResults>, ICloneable
    {
        #region < Properties >

        /// <summary> Sum of all DirectoryStatistics objects </summary>
        IStatistic DirectoriesStatistic { get; }

        /// <summary> Sum of all ByteStatistics objects </summary>
        IStatistic BytesStatistic { get; }

        /// <summary> Sum of all FileStatistics objects </summary>
        IStatistic FilesStatistic { get; }

        /// <summary> Average of all SpeedStatistics objects </summary>
        ISpeedStatistic SpeedStatistic { get; }

        /// <summary> Sum of all RoboCopyExitStatus objects </summary>
        IRoboCopyCombinedExitStatus Status { get; }

        /// <inheritdoc cref="List{T}.Count"/>
        int Count { get; }

        #endregion

        #region < Methods >

        /// <summary>
        /// Get a snapshot of the ByteStatistics objects from this list.
        /// </summary>
        /// <returns>New array of the ByteStatistic objects</returns>
        IStatistic[] GetByteStatistics();

        /// <summary>
        /// Get a snapshot of the DirectoriesStatistic objects from this list.
        /// </summary>
        /// <returns>New array of the DirectoriesStatistic objects</returns>
        IStatistic[] GetDirectoriesStatistics();

        /// <summary>
        /// Get a snapshot of the FilesStatistic objects from this list.
        /// </summary>
        /// <returns>New array of the FilesStatistic objects</returns>
        IStatistic[] GetFilesStatistics();

        /// <summary>
        /// Get a snapshot of the FilesStatistic objects from this list.
        /// </summary>
        /// <returns>New array of the FilesStatistic objects</returns>
        RoboCopyExitStatus[] GetStatuses();

        /// <summary>
        /// Get a snapshot of the FilesStatistic objects from this list.
        /// </summary>
        /// <returns>New array of the FilesStatistic objects</returns>
        ISpeedStatistic[] GetSpeedStatistics();

        /// <summary>
        /// Copy the values within the list to a new object
        /// </summary>
        /// <returns>new <see cref="RoboCopyResultsList"/> object</returns>
        new RoboCopyResultsList Clone();

        #endregion
    }
}
