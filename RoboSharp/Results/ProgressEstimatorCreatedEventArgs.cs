using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboSharp.Results
{
    /// <summary>
    /// Reports that a ProgressEstimator object is now available for binding
    /// </summary>
    public class ProgressEstimatorCreatedEventArgs : EventArgs
    {
        private ProgressEstimatorCreatedEventArgs() : base() { }

        internal ProgressEstimatorCreatedEventArgs(ProgressEstimator estimator) : base()
        {
            ResultsEstimate = estimator;
        }

        /// <summary>
        /// <inheritdoc cref="ProgressEstimator"/>
        /// </summary>
        public ProgressEstimator ResultsEstimate { get; }
        
    }
}

