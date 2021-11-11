using System.Text;

namespace RoboSharp
{
    /// <summary>
    /// RoboCopy switches for how to react if a copy/move operation errors
    /// </summary>
    public class RetryOptions
    {
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
        public int RetryCount
        {
            get { return retryCount; }
            set { retryCount = value; }
        }
        /// <summary>
        /// Specifies the wait time N in seconds between retries (default is 30).
        /// [/W:N]
        /// </summary>
        public int RetryWaitTime
        {
            get { return retryWaitTime; }
            set { retryWaitTime = value; }
        }
        /// <summary>
        /// Saves RetryCount and RetryWaitTime in the Registry as default settings.
        /// [/REG]
        /// </summary>
        public bool SaveToRegistry { get; set; }
        /// <summary>
        /// Wait for sharenames to be defined.
        /// [/TBD]
        /// </summary>
        public bool WaitForSharenames { get; set; }

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
    }
}
