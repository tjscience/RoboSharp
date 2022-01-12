﻿using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RoboSharp.EventArgObjects;

namespace RoboSharp.Results
{
    /// <summary>
    /// Provide Read-Only access to a <see cref="Statistic"/> object
    /// </summary>
    public interface IStatistic : INotifyPropertyChanged
    {

        #region < Properties >

        /// <summary>
        /// Name of the Statistics Object
        /// </summary>
        string Name { get; }

        /// <summary>
        /// <inheritdoc cref="Statistic.StatType"/>
        /// </summary>
        Statistic.StatType Type { get; }

        /// <summary> Total Scanned during the run</summary>
        long Total { get; }

        /// <summary> Total Copied </summary>
        long Copied { get; }

        /// <summary> Total Skipped </summary>
        long Skipped { get; }

        /// <summary>  </summary>
        long Mismatch { get; }

        /// <summary> Total that failed to copy or move </summary>
        long Failed { get; }

        /// <summary> Total Extra that exist in the Destination (but are missing from the Source)</summary>
        long Extras { get; }

        #endregion

        #region < Events >

        /// <inheritdoc cref="Statistic.PropertyChanged"/>
        new event PropertyChangedEventHandler PropertyChanged;

        /// <summary> Occurs when the <see cref="Total"/> Property is updated. </summary>
        event Statistic.StatChangedHandler OnTotalChanged;

        /// <summary> Occurs when the <see cref="Copied"/> Property is updated. </summary>
        event Statistic.StatChangedHandler OnCopiedChanged;

        /// <summary> Occurs when the <see cref="Skipped"/> Property is updated. </summary>
        event Statistic.StatChangedHandler OnSkippedChanged;

        /// <summary> Occurs when the <see cref="Mismatch"/> Property is updated. </summary>
        event Statistic.StatChangedHandler OnMisMatchChanged;

        /// <summary> Occurs when the <see cref="Failed"/> Property is updated. </summary>
        event Statistic.StatChangedHandler OnFailedChanged;

        /// <summary> Occurs when the <see cref="Extras"/> Property is updated. </summary>
        event Statistic.StatChangedHandler OnExtrasChanged;

        #endregion

        #region < ToString Methods >

        /// <inheritdoc cref="SpeedStatistic.ToString"/>
        string ToString();

        /// <inheritdoc cref="Statistic.ToString(bool, bool, string, bool)"/>
        string ToString(bool IncludeType, bool IncludePrefix, string Delimiter, bool DelimiterAfterType=false);

        /// <inheritdoc cref="Statistic.ToString_Type()"/>
        string ToString_Type();

        /// <inheritdoc cref="Statistic.ToString_Total(bool, bool)"/>
        string ToString_Total(bool IncludeType = false, bool IncludePrefix = true);

        /// <inheritdoc cref="Statistic.ToString_Copied"/>
        string ToString_Copied(bool IncludeType = false, bool IncludePrefix = true);

        /// <inheritdoc cref="Statistic.ToString_Extras"/>
        string ToString_Extras(bool IncludeType = false, bool IncludePrefix = true);

        /// <inheritdoc cref="Statistic.ToString_Failed"/>
        string ToString_Failed(bool IncludeType = false, bool IncludePrefix = true);

        /// <inheritdoc cref="Statistic.ToString_Mismatch"/>
        string ToString_Mismatch(bool IncludeType = false, bool IncludePrefix = true);

        /// <inheritdoc cref="Statistic.ToString_Skipped"/>
        string ToString_Skipped(bool IncludeType = false, bool IncludePrefix = true);
        
        #endregion
    }

    /// <summary>
    /// Information about number of items Copied, Skipped, Failed, etc.
    /// </summary>
    /// <remarks>
    /// <see cref="RoboCopyResults"/> will not typically raise any events, but this object is used for other items, such as <see cref="ProgressEstimator"/> and <see cref="RoboCopyResultsList"/> to present results whose values may update periodically.
    /// </remarks>
    public class Statistic : IStatistic
    {
        #region < Constructors >

        /// <summary> Create a new Statistic object of <see cref="StatType"/> </summary>
        [Obsolete("Statistic Types require Initialization with a StatType")]
        private Statistic() { }

        /// <summary> Create a new Statistic object </summary>
        public Statistic(StatType type) { Type = type; }

        /// <summary> Create a new Statistic object </summary>
        public Statistic(StatType type, string name) { Type = type; Name = name; }

        /// <summary> Describe the Type of Statistics Object </summary>
        public enum StatType
        {
            /// <summary> Statistics object represents count of Directories </summary>
            Directories,
            /// <summary> Statistics object represents count of Files </summary>
            Files,
            /// <summary> Statistics object represents a Size ( number of bytes )</summary>
            Bytes
        }

        #endregion 

        #region < Fields >

        private string NameField;
        private long TotalField;
        private long CopiedField;
        private long SkippedField;
        private long MismatchField;
        private long FailedField;
        private long ExtrasField;

        #endregion

        #region < Events >

        /// <summary> This toggle Enables/Disables firing the <see cref="PropertyChanged"/> Event to avoid firing it when doing multiple consecutive changes to the values </summary>
        private bool EnablePropertyChangeEvent = true;

        /// <summary>
        /// This event will fire when the value of the statistic is updated via Adding / Subtracting methods. <br/>
        /// Provides <see cref="StatChangedEventArg"/> object.
        /// </summary>
        /// <remarks>
        /// Allows use with both binding to controls and <see cref="ProgressEstimator"/> binding. <br/>
        /// EventArgs can be passed into <see cref="AddStatistic(PropertyChangedEventArgs)"/> after casting.
        /// </remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Handles any value changes </summary>
        public delegate void StatChangedHandler(Statistic sender, StatChangedEventArg e);

        /// <summary> Occurs when the <see cref="Total"/> Property is updated. </summary>
        public event StatChangedHandler OnTotalChanged;

        /// <summary> Occurs when the <see cref="Copied"/> Property is updated. </summary>
        public event StatChangedHandler OnCopiedChanged;

        /// <summary> Occurs when the <see cref="Skipped"/> Property is updated. </summary>
        public event StatChangedHandler OnSkippedChanged;

        /// <summary> Occurs when the <see cref="Mismatch"/> Property is updated. </summary>
        public event StatChangedHandler OnMisMatchChanged;

        /// <summary> Occurs when the <see cref="Failed"/> Property is updated. </summary>
        public event StatChangedHandler OnFailedChanged;

        /// <summary> Occurs when the <see cref="Extras"/> Property is updated. </summary>
        public event StatChangedHandler OnExtrasChanged;

        private Lazy<StatChangedEventArg> PrepEventArgs(long OldValue, long NewValue, string PropertyName) => new Lazy<StatChangedEventArg>(() => new StatChangedEventArg(this, OldValue, NewValue, PropertyName));

        #endregion

        #region < Properties >

        /// <summary>
        /// Name of the Statistics Object
        /// </summary>
        public string Name
        {
            get => NameField;
            set
            {
                if (value != NameField)
                {
                    NameField = value ?? "";
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
                }
            }
        }
        

        /// <summary>
        /// <inheritdoc cref="StatType"/>
        /// </summary>
        public StatType Type { get; }

        /// <inheritdoc cref="IStatistic.Total"/>
        public long Total { 
            get => TotalField;
            internal set
            {
                if (TotalField != value)
                {
                    var e = PrepEventArgs(TotalField, value, "Total");
                    TotalField = value;
                    if (EnablePropertyChangeEvent)
                    {
                        PropertyChanged?.Invoke(this, e.Value);
                        OnTotalChanged?.Invoke(this, e.Value);
                    }
                }
            }
        }

        /// <inheritdoc cref="IStatistic.Copied"/>
        public long Copied { 
            get => CopiedField;
            internal set
            {
                if (CopiedField != value)
                {
                    var e = PrepEventArgs(CopiedField, value, "Copied");
                    CopiedField = value;
                    if (EnablePropertyChangeEvent)
                    {
                        PropertyChanged?.Invoke(this, e.Value);
                        OnCopiedChanged?.Invoke(this, e.Value);
                    }
                }
            }
        }

        /// <inheritdoc cref="IStatistic.Skipped"/>
        public long Skipped { 
            get => SkippedField;
            internal set
            {
                if (SkippedField != value)
                {
                    var e = PrepEventArgs(SkippedField, value, "Skipped");
                    SkippedField = value;
                    if (EnablePropertyChangeEvent)
                    {
                        PropertyChanged?.Invoke(this, e.Value);
                        OnSkippedChanged?.Invoke(this, e.Value);
                    }
                }
            }
        }

        /// <inheritdoc cref="IStatistic.Mismatch"/>
        public long Mismatch { 
            get => MismatchField;
            internal set
            {
                if (MismatchField != value)
                {
                    var e = PrepEventArgs(MismatchField, value, "Mismatch");
                    MismatchField = value;
                    if (EnablePropertyChangeEvent)
                    {
                        PropertyChanged?.Invoke(this, e.Value);
                        OnMisMatchChanged?.Invoke(this, e.Value);
                    }
                }
            }
        }

        /// <inheritdoc cref="IStatistic.Failed"/>
        public long Failed { 
            get => FailedField;
            internal set
            {
                if (FailedField != value)
                {
                    var e = PrepEventArgs(FailedField, value, "Failed");
                    FailedField = value;
                    if (EnablePropertyChangeEvent)
                    {
                        PropertyChanged?.Invoke(this, e.Value);
                        OnFailedChanged?.Invoke(this, e.Value);
                    }
                }
            }
        }

        /// <inheritdoc cref="IStatistic.Extras"/>
        public long Extras { 
            get => ExtrasField;
            internal set
            {
                if (ExtrasField != value)
                {
                    var e = PrepEventArgs(ExtrasField, value, "Extras");
                    ExtrasField = value;
                    if (EnablePropertyChangeEvent)
                    {
                        PropertyChanged?.Invoke(this, e.Value);
                        OnExtrasChanged?.Invoke(this, e.Value);
                    }
                }
            }
        }

        #endregion

        #region < ToString Methods >

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString() => ToString(false, true, ", ");

        /// <summary>
        /// Customize the returned string
        /// </summary>
        /// <param name="IncludeType">Include string representation of <see cref="Type"/></param>
        /// <param name="IncludePrefix">Include "Total:" / "Copied:" / etc in the string to identify the values</param>
        /// <param name="Delimiter">Value Delimieter</param>
        /// <param name="DelimiterAfterType">
        /// Include the delimiter after the 'Type' - Only used if <paramref name="IncludeType"/> us also true. <br/>
        /// When <paramref name="IncludeType"/> is true, a space always exist after the type string. This would add delimiter instead of the space. 
        /// </param>
        /// <returns>
        /// TRUE, TRUE, "," --> $"{Type} Total: {Total}, Copied: {Copied}, Skipped: {Skipped}, Mismatch: {Mismatch}, Failed: {Failed}, Extras: {Extras}" <para/>
        /// FALSE, TRUE, "," --> $"Total: {Total}, Copied: {Copied}, Skipped: {Skipped}, Mismatch: {Mismatch}, Failed: {Failed}, Extras: {Extras}" <para/>
        /// FALSE, FALSE, "," --> $"{Total}, {Copied}, {Skipped}, {Mismatch}, {Failed}, {Extras}"
        /// </returns>
        public string ToString(bool IncludeType, bool IncludePrefix, string Delimiter, bool DelimiterAfterType = false)
        {
            return $"{ToString_Type(IncludeType, DelimiterAfterType)}" + $"{(IncludeType && DelimiterAfterType ? Delimiter : "")}" +
                $"{ToString_Total(false, IncludePrefix)}{Delimiter}" +
                $"{ToString_Copied(false, IncludePrefix)}{Delimiter}" +
                $"{ToString_Skipped(false, IncludePrefix)}{Delimiter}" +
                $"{ToString_Mismatch(false, IncludePrefix)}{Delimiter}" +
                $"{ToString_Failed(false, IncludePrefix)}{Delimiter}" +
                $"{ToString_Extras(false, IncludePrefix)}";
        }

        /// <summary> Get the <see cref="Type"/> as a string </summary>
        public string ToString_Type() => ToString_Type(true).Trim();
        private string ToString_Type(bool IncludeType, bool Trim = false) => IncludeType ? $"{Type}{(Trim? "" : " ")}" : "";

        /// <summary>Get the string describing the <see cref="Total"/></summary>
        /// <returns></returns>
        /// <inheritdoc cref="ToString(bool, bool, string, bool)"/>
        public string ToString_Total(bool IncludeType = false, bool IncludePrefix = true) => $"{ToString_Type(IncludeType)}{(IncludePrefix? "Total: " : "")}{Total}";

        /// <summary>Get the string describing the <see cref="Copied"/></summary>
        /// <inheritdoc cref="ToString_Total"/>
        public string ToString_Copied(bool IncludeType = false, bool IncludePrefix = true) => $"{ToString_Type(IncludeType)}{(IncludePrefix ? "Copied: " : "")}{Copied}";

        /// <summary>Get the string describing the <see cref="Extras"/></summary>
        /// <inheritdoc cref="ToString_Total"/>
        public string ToString_Extras(bool IncludeType = false, bool IncludePrefix = true) => $"{ToString_Type(IncludeType)}{(IncludePrefix ? "Extras: " : "")}{Extras}";

        /// <summary>Get the string describing the <see cref="Failed"/></summary>
        /// <inheritdoc cref="ToString_Total"/>
        public string ToString_Failed(bool IncludeType = false, bool IncludePrefix = true) => $"{ToString_Type(IncludeType)}{(IncludePrefix ? "Failed: " : "")}{Failed}";

        /// <summary>Get the string describing the <see cref="Mismatch"/></summary>
        /// <inheritdoc cref="ToString_Total"/>
        public string ToString_Mismatch(bool IncludeType = false, bool IncludePrefix = true) => $"{ToString_Type(IncludeType)}{(IncludePrefix ? "Mismatch: " : "")}{Mismatch}";

        /// <summary>Get the string describing the <see cref="Skipped"/></summary>
        /// <inheritdoc cref="ToString_Total"/>
        public string ToString_Skipped(bool IncludeType = false, bool IncludePrefix = true) => $"{ToString_Type(IncludeType)}{(IncludePrefix ? "Skipped: " : "")}{Skipped}";

        #endregion

        #region < Parsing Methods >

        /// <summary>
        /// Parse a string and for the tokens reported by RoboCopy
        /// </summary>
        /// <param name="type">Statistic Type to produce</param>
        /// <param name="line">LogLine produced by RoboCopy in Summary Section</param>
        /// <returns>New Statistic Object</returns>
        public static Statistic Parse(StatType type, string line)
        {
            var res = new Statistic(type);

            var tokenNames = new[] { nameof(Total), nameof(Copied), nameof(Skipped), nameof(Mismatch), nameof(Failed), nameof(Extras) };
            var patternBuilder = new StringBuilder(@"^.*:");

            foreach (var tokenName in tokenNames)
            {
                var tokenPattern = GetTokenPattern(tokenName);
                patternBuilder.Append(@"\s+").Append(tokenPattern);
            }

            var pattern = patternBuilder.ToString();
            var match = Regex.Match(line, pattern);
            if (!match.Success)
                return res;

            var props = res.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var tokenName in tokenNames)
            {
                var prop = props.FirstOrDefault(x => x.Name == tokenName);
                if (prop == null)
                    continue;

                var tokenString = match.Groups[tokenName].Value;
                var tokenValue = ParseTokenString(tokenString);
                prop.SetValue(res, tokenValue, null);
            }

            return res;
        }

        private static string GetTokenPattern(string tokenName)
        {
            return $@"(?<{tokenName}>[\d\.]+(\s\w)?)";
        }

        private static long ParseTokenString(string tokenString)
        {
            if (string.IsNullOrWhiteSpace(tokenString))
                return 0;

            tokenString = tokenString.Trim();
            if (Regex.IsMatch(tokenString, @"^\d+$", RegexOptions.Compiled))
                return long.Parse(tokenString);

            var match = Regex.Match(tokenString, @"(?<Mains>[\d\.,]+)(\.(?<Fraction>\d+))\s(?<Unit>\w)", RegexOptions.Compiled);
            if (match.Success)
            {
                var mains = match.Groups["Mains"].Value.Replace(".", "").Replace(",", "");
                var fraction = match.Groups["Fraction"].Value;
                var unit = match.Groups["Unit"].Value.ToLower();

                var number = double.Parse($"{mains}.{fraction}", NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                switch (unit)
                {
                    case "k":
                        // Byte = kBytes * 1024
                        number *= Math.Pow(1024, 1);
                        break;
                    case "m":
                        // Byte = MBytes * 1024 * 1024
                        number *= Math.Pow(1024, 2);
                        break;
                    case "g":
                        // Byte = GBytes * 1024 * 1024 * 1024
                        number *= Math.Pow(1024, 3);
                        break;
                    case "t":
                        // Byte = TBytes * 1024 * 1024 * 1024 * 1024
                        number *= Math.Pow(1024, 4);
                        break;
                }

                return Convert.ToInt64(number);
            }

            return 0;
        }

        #endregion

        #region < Reset Method >

        /// <summary>
        /// Reset all values to Zero ( 0 ) -- Used by <see cref="RoboCopyResultsList"/> for the properties
        /// </summary>
#if !NET40
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
#endif
        public void Reset()
        {
            Copied = 0;
            Extras = 0;
            Failed = 0;
            Mismatch = 0;
            Skipped = 0;
            Total = 0;
        }

        /// <summary>
        /// Set the values for this object to 0
        /// </summary>
        public void Reset(bool enablePropertyChangeEvent)
        {
            EnablePropertyChangeEvent = enablePropertyChangeEvent;
            Reset();
            EnablePropertyChangeEvent = true;
        }

        #endregion

        #region < ADD Methods >

        /// <summary>
        /// Add the results of the supplied Statistics object to this Statistics object.
        /// </summary>
        /// <param name="stat">Statistics Item to add</param>
#if !NET40
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
#endif
        public void AddStatistic(IStatistic stat)
        {
            if (stat.Type != this.Type) return;
            Total += stat?.Total ?? 0;
            Copied += stat?.Copied ?? 0;
            Extras += stat?.Extras ?? 0;
            Failed += stat?.Failed ?? 0;
            Mismatch += stat?.Mismatch ?? 0;
            Skipped += stat?.Skipped ?? 0;
        }

        
        #pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
        /// <param name="enablePropertyChangedEvent"><inheritdoc cref="EnablePropertyChangeEvent" path="*"/></param>
        /// <inheritdoc cref="AddStatistic(IStatistic)"/>        
        internal void AddStatistic(IStatistic stats, bool enablePropertyChangedEvent)
        {
            EnablePropertyChangeEvent = enablePropertyChangedEvent;
            AddStatistic(stats);
            EnablePropertyChangeEvent = true;

        }
        #pragma warning restore CS1573


        /// <summary>
        /// Add the results of the supplied Statistics objects to this Statistics object.
        /// </summary>
        /// <param name="stats">Statistics Item to add</param>
        public void AddStatistic(IEnumerable<IStatistic> stats)
        {
            foreach (Statistic stat in stats)
            {
                EnablePropertyChangeEvent = stat == stats.Last();
                AddStatistic(stat);
            }
        }

        /// <summary>
        /// Adds <see cref="StatChangedEventArg.Difference"/> to the appropriate property based on the 'PropertyChanged' value. <br/>
        /// Will only add the value if the <see cref="StatChangedEventArg.StatType"/> == <see cref="Type"/>.
        /// </summary>
        /// <param name="eventArgs">Arg provided by either <see cref="PropertyChanged"/> or a Statistic's object's On*Changed events</param>
        public void AddStatistic(PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.GetType() != typeof(StatChangedEventArg)) return;
            var e = (StatChangedEventArg)eventArgs;
            if (e.StatType == this.Type)
            {
                switch (e.PropertyName)
                {
                    case "Copied":
                        this.Copied += e.Difference;
                        break;
                    case "Extras":
                        this.Extras += e.Difference;
                        break;
                    case "Failed":
                        this.Failed += e.Difference;
                        break;
                    case "Mismatch":
                        this.Mismatch += e.Difference;
                        break;
                    case "Skipped":
                        this.Skipped += e.Difference;
                        break;
                    case "Total":
                        this.Total += e.Difference;
                        break;
                }
            }
        }

        /// <summary>
        /// Combine the results of the supplied statistics objects of the specified type.
        /// </summary>
        /// <param name="stats">Collection of <see cref="Statistic"/> objects</param>
        /// <param name="statType">Create a new Statistic object of this type.</param>
        /// <returns>New Statistics Object</returns>
        public static Statistic AddStatistics(IEnumerable<IStatistic> stats, StatType statType)
        {
            Statistic ret = new Statistic(statType);
            ret.AddStatistic(stats.Where(s => s.Type == statType) );
            return ret;
        }


        #endregion ADD

        #region < AVERAGE Methods >

        /// <summary>
        /// Combine the supplied <see cref="Statistic"/> objects, then get the average.
        /// </summary>
        /// <param name="stats">Array of Stats objects</param>
        public void AverageStatistic(IEnumerable<IStatistic> stats)
        {
            this.AddStatistic(stats);
            int cnt = stats.Count() + 1;
            Total /= cnt;
            Copied /= cnt;
            Extras /= cnt;
            Failed /= cnt;
            Mismatch /= cnt;
            Skipped /= cnt;

        }

        /// <returns>New Statistics Object</returns>
        /// <inheritdoc cref=" AverageStatistic(IEnumerable{IStatistic})"/>
        public static Statistic AverageStatistics(IEnumerable<IStatistic> stats, StatType statType)
        {
            Statistic stat = AddStatistics(stats, statType);
            int cnt = stats.Count(s => s.Type == statType);
            if (cnt > 1)
            {
                stat.Total /= cnt;
                stat.Copied /= cnt;
                stat.Extras /= cnt;
                stat.Failed /= cnt;
                stat.Mismatch /= cnt;
                stat.Skipped /= cnt;
            }
            return stat;
        }

        #endregion AVERAGE

        #region < Subtract Methods >

        /// <summary>
        /// Subtract Method used by <see cref="RoboCopyResultsList"/>
        /// </summary>
        /// <param name="stat">Statistics Item to subtract</param>
#if !NET40
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
#endif
        public void Subtract(IStatistic stat)
        {
            Copied -= stat?.Copied ?? 0;
            Extras -= stat?.Extras ?? 0;
            Failed -= stat?.Failed ?? 0;
            Mismatch -= stat?.Mismatch ?? 0;
            Skipped -= stat?.Skipped ?? 0;
            Total -= stat?.Total ?? 0;
        }

        #pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
        /// <param name="enablePropertyChangedEvent"><inheritdoc cref="EnablePropertyChangeEvent" path="*"/></param>
        /// <inheritdoc cref="Subtract(IStatistic)"/>        
        internal void Subtract(IStatistic stats, bool enablePropertyChangedEvent)
        {
            EnablePropertyChangeEvent = enablePropertyChangedEvent;
            Subtract(stats);
            EnablePropertyChangeEvent = true;
        }
        #pragma warning restore CS1573

        /// <summary>
        /// Add the results of the supplied Statistics objects to this Statistics object.
        /// </summary>
        /// <param name="stats">Statistics Item to add</param>
        public void Subtract(IEnumerable<IStatistic> stats)
        {
            foreach (Statistic stat in stats)
            {
                EnablePropertyChangeEvent = stat == stats.Last();
                Subtract(stat);
            }
        }

        #endregion Subtract




    }
}
