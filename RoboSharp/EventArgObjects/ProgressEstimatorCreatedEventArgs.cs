using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoboSharp.Interfaces;

// Do Not change NameSpace here! -> Must be RoboSharp due to prior releases
namespace RoboSharp.EventArgObjects
{
    /// <summary>
    /// Reports that a ProgressEstimator object is now available for binding
    /// </summary>
    public class ProgressEstimatorCreatedEventArgs : EventArgs
    {
        private ProgressEstimatorCreatedEventArgs() : base() { }

        internal ProgressEstimatorCreatedEventArgs(IProgressEstimator estimator) : base()
        {
            ResultsEstimate = estimator;
        }

        /// <summary>
        /// <inheritdoc cref="Results.ProgressEstimator"/>
        /// </summary>
        public IProgressEstimator ResultsEstimate { get; }
        
    }
}

