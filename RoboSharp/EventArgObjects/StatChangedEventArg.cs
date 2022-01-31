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
    /// Interface helper for dealing with Statistic Event Args
    /// </summary>
    public interface IStatisticPropertyChangedEventArg
    {
        /// <inheritdoc cref="Statistic.Type"/>
        Statistic.StatType StatType { get; }
        
        /// <summary>TRUE if of type <see cref="StatChangedEventArg"/>. Otherwise false.</summary>
        bool Is_StatChangedEventArg { get; }

        /// <summary>TRUE if of type <see cref="StatisticPropertyChangedEventArgs"/>. Otherwise false.</summary>
        bool Is_StatisticPropertyChangedEventArgs { get; }
        
        /// <inheritdoc cref="PropertyChangedEventArgs.PropertyName"/>
        string PropertyName { get; }
        
    }
    
    /// <summary>
    /// EventArgs provided by <see cref="Statistic.StatChangedHandler"/> when any individual property gets modified.
    /// </summary>
    /// <remarks>
    /// Under most circumstances, the 'PropertyName' property will detail which parameter has been updated. <br/>
    /// When the Statistic object has multiple values change via a method call ( Reset / Add / Subtract methods ), then PropertyName will be String.Empty, indicating multiple values have changed. <br/>
    /// If this is the case, then the <see cref="StatChangedEventArg.NewValue"/>, <see cref="StatChangedEventArg.OldValue"/>, and <see cref="StatChangedEventArg.Difference"/> will report the value from the sender's <see cref="Statistic.Total"/> property.
    /// </remarks>
    public class StatChangedEventArg : PropertyChangedEventArgs, IStatisticPropertyChangedEventArg
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

        bool IStatisticPropertyChangedEventArg.Is_StatChangedEventArg => true;

        bool IStatisticPropertyChangedEventArg.Is_StatisticPropertyChangedEventArgs => false;
    }

    /// <summary>
    /// EventArgs provided by <see cref="Statistic.PropertyChanged"/>
    /// </summary>
    /// <remarks>
    /// Under most circumstances, the 'PropertyName' property will detail which parameter has been updated. <br/>
    /// When the Statistic object has multiple values change via a method call ( Reset / Add / Subtract methods ), then PropertyName will be String.Empty, indicating multiple values have changed. <br/>
    /// If this is the case, then the <see cref="StatChangedEventArg.NewValue"/>, <see cref="StatChangedEventArg.OldValue"/>, and <see cref="StatChangedEventArg.Difference"/> will report the value from the sender's <see cref="Statistic.Total"/> property.
    /// </remarks>
    public class StatisticPropertyChangedEventArgs : PropertyChangedEventArgs, IStatisticPropertyChangedEventArg
    {
        private StatisticPropertyChangedEventArgs() : base("") { }
        internal StatisticPropertyChangedEventArgs(Statistic stat, Statistic oldValue, string PropertyName) : base(PropertyName)
        {
            //Sender = stat;
            StatType = stat.Type;
            OldValue = oldValue;
            NewValue = stat.Clone();
            Lazydifference = new Lazy<Statistic>(() => Statistic.Subtract(NewValue, OldValue));
        }

        /// <inheritdoc cref="Statistic.Type"/>
        public Statistic.StatType StatType { get; }

        /// <summary> Old Value of the object </summary>
        public IStatistic OldValue { get; }

        /// <summary> Current Value of the object </summary>
        public IStatistic NewValue { get; }

        /// <summary>
        /// Result of NewValue - OldValue
        /// </summary>
        public IStatistic Difference => Lazydifference.Value;
        private Lazy<Statistic> Lazydifference;

        bool IStatisticPropertyChangedEventArg.Is_StatChangedEventArg => false;

        bool IStatisticPropertyChangedEventArg.Is_StatisticPropertyChangedEventArgs => true;
    }
}
