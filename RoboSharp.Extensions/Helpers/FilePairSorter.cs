using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions.Helpers
{
    /// <summary>
    /// Sort FilePairs by their <see cref="ProcessedFileInfo.FileClass"/>
    /// </summary>
    public sealed class FilePairSorter<T> : IComparer<T> where T: IFilePair
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public FilePairSorter(RoboSharpConfiguration config)
        {
            Config = config;
        }

        private RoboSharpConfiguration Config { get; }

        /// <summary>
        /// Evaluate the <see cref="IFilePair.ProcessResult"/> and sort accordingly
        /// </summary>
        /// <inheritdoc/>
        public int Compare(T x, T y)
        {
            return Compare(x?.ProcessResult, y?.ProcessResult);
        }

        /// <inheritdoc cref="Compare(T, T)"/>
        public int Compare<T1>(T1 x, T1 y) where T1 : IFilePair
        {
            return Compare(x?.ProcessResult, y?.ProcessResult);
        }

        /// <inheritdoc cref="IComparer{T}.Compare(T, T)"/>
        public int Compare(ProcessedFileInfo x, ProcessedFileInfo y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            _ = x.TryGetFileClass(Config, out var XF);
            _ = y.TryGetFileClass(Config, out var YF);
            int result = XF.CompareTo(YF);
            return result;
        }
    }
}
