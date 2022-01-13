using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using RoboSharp.Results;
using RoboSharp.Interfaces;

namespace RoboSharp.EventArgObjects
{
    /// <summary>
    /// EventArgs provided by <see cref="Statistic.PropertyChanged"/> and other Events generated from a <see cref="Statistic"/> object.
    /// </summary>
    /// <remarks>
    /// Under most circumstances, the 'PropertyName' property will detail which parameter has been updated. <br/>
    /// When the Statistic object has multiple values change via a method call ( Reset / Add / Subtract methods ), then PropertyName will be String.Empty, indicating multiple values have changed. <br/>
    /// If this is the case, then the <see cref="StatChangedEventArg.NewValue"/>, <see cref="StatChangedEventArg.OldValue"/>, and <see cref="StatChangedEventArg.Difference"/> will report the value from the sender's <see cref="Statistic.Total"/> property.
    /// </remarks>
    public class StatChangedEventArg : PropertyChangedEventArgs
    {
        private StatChangedEventArg() : base("") { }
        internal StatChangedEventArg(Statistic stat, long oldValue, long newValue, string PropertyName) : base(PropertyName)
        {
            Sender = stat;
            StatType = stat.Type;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary> This is a reference to the Statistic that generated the EventArg object </summary>
        public IStatistic Sender { get; }

        /// <inheritdoc cref="Statistic.Type"/>
        public Statistic.StatType StatType { get; }

        /// <summary> Old Value of the object </summary>
        public long OldValue { get; }

        /// <summary> Current Value of the object </summary>
        public long NewValue { get; }

        /// <summary>
        /// Result of NewValue - OldValue
        /// </summary>
        public long Difference => NewValue - OldValue;
    }
}
