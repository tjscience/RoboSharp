using System;
using System.Text;

namespace RoboSharp
{
    /// <summary>
    /// RoboCopy switches for how to react if a copy/move operation errors
    /// </summary>
    /// <remarks>
    /// <see href="https://github.com/tjscience/RoboSharp/wiki/RetryOptions"/>
    /// </remarks>
    public class RetryOptions : ICloneable
    {
        #region Constructors 

        /// <summary>
        /// Create new RetryOptions with Default Settings
        /// </summary>
        public RetryOptions() { }

        /// <summary>
        /// Clone a RetryOptions Object
        /// </summary>
        /// <param name="options">RetryOptions object to clone</param>
        public RetryOptions(RetryOptions options)
        {
            WaitForSharenames = options.WaitForSharenames;
            SaveToRegistry = options.SaveToRegistry;
            RetryWaitTime = options.RetryWaitTime;
            RetryCount = options.RetryCount;
        }

        /// <inheritdoc cref="RetryOptions.RetryOptions(RetryOptions)"/>
        public RetryOptions Clone() => new RetryOptions(this);

        object ICloneable.Clone() => Clone();

        #endregion

        internal const string RETRY_COUNT = "/R:{0} ";
        internal const string RETRY_WAIT_TIME = "/W:{0} ";
        internal const string SAVE_TO_REGISTRY = "/REG ";
        internal const string WAIT_FOR_SHARENAMES = "/TBD ";

        private int retryCount = 0;
        private int retryWaitTime = 30;

        /// <summary>
        /// Specifies the number of retries N on failed copies (default is 0).
        /// [/R:N]
        /// </summary>
        public virtual int RetryCount
        {
            get { return retryCount; }
            set { retryCount = value; }
        }
        /// <summary>
        /// Specifies the wait time N in seconds between retries (default is 30).
        /// [/W:N]
        /// </summary>
        public virtual int RetryWaitTime
        {
            get { return retryWaitTime; }
            set { retryWaitTime = value; }
        }
        /// <summary>
        /// Saves RetryCount and RetryWaitTime in the Registry as default settings.
        /// [/REG]
        /// </summary>
        public virtual bool SaveToRegistry { get; set; }
        /// <summary>
        /// Wait for sharenames to be defined.
        /// [/TBD]
        /// </summary>
        public virtual bool WaitForSharenames { get; set; }

        internal string Parse()
        {
            var options = new StringBuilder();

            options.Append(string.Format(RETRY_COUNT, RetryCount));
            options.Append(string.Format(RETRY_WAIT_TIME, RetryWaitTime));

            if (SaveToRegistry)
                options.Append(SAVE_TO_REGISTRY);
            if (WaitForSharenames)
                options.Append(WAIT_FOR_SHARENAMES);

            return options.ToString();
        }

        /// <summary>
        /// Combine this object with another RetryOptions object. <br/>
        /// Any properties marked as true take priority. IEnumerable items are combined. <br/>
        /// String Values will only be replaced if the primary object has a null/empty value for that property.
        /// </summary>
        /// <param name="options"></param>
        public void Merge(RetryOptions options)
        {
            RetryCount = RetryCount.GetGreaterVal(options.RetryCount);
            RetryWaitTime = RetryWaitTime.GetGreaterVal(options.RetryWaitTime);
            WaitForSharenames |= options.WaitForSharenames;
            SaveToRegistry |= options.SaveToRegistry;
        }
    }
}
