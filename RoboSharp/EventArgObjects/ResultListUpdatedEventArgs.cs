using System;
using RoboSharp.Results;
using RoboSharp.Interfaces;

namespace RoboSharp.EventArgObjects
{
    /// <summary> EventArgs for the <see cref="RoboCopyResultsList.ResultsListUpdated"/> delegate </summary>
    public class ResultListUpdatedEventArgs : EventArgs
    {
        private ResultListUpdatedEventArgs() { }

        /// <summary> Create the EventArgs for the <see cref="RoboCopyResultsList.ResultsListUpdated"/> delegate </summary>
        /// <param name="list">Results list to present as an interface</param>
        public ResultListUpdatedEventArgs(IRoboCopyResultsList list)
        {
            ResultsList = list;
        }

        /// <summary>
        /// Read-Only interface to the List that has been updated.
        /// </summary>
        public IRoboCopyResultsList ResultsList { get; }
    }
}
