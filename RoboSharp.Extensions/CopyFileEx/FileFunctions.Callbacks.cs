using RoboSharp.Extensions.CopyFileEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoboSharp.Extensions.CopyFileEx
{
    public static partial class FileFunctions
    {
        /// <summary>
        /// Create a callback new callback from a function that takes the max file size and number of bytes copied, then returns the required result.
        /// </summary>
        /// <param name="callback">The callback to wrap</param>
        /// <returns>A new <see cref="CopyProgressCallback"/></returns>
        public static CopyProgressCallback CreateCallback(BytesTransferredCallback callback)
        {
            return new CopyProgressCallback((tfs, bc, _1, _2, _3, reason, _4, _5, _6) => callback(tfs, bc));
        }

        /// <summary>
        /// Create a callback new callback that wraps the <paramref name="token"/> in order to detected a cancellation request and pass it back to CopyFileEx
        /// </summary>
        /// <param name="token">The token to wrap</param>
        /// <returns>A new <see cref="CopyProgressCallback"/></returns>
        public static CopyProgressCallback CreateCallback(CancellationToken token)
        {
            return GetResult;
            CopyProgressCallbackResult GetResult(long tfs, long bc, long _1, long _2, uint _3, CopyProgressCallbackReason reason, IntPtr _4, IntPtr _5, IntPtr _6)
            {
                return token.IsCancellationRequested ? CopyProgressCallbackResult.CANCEL : CopyProgressCallbackResult.CONTINUE;
            }
        }

        /// <summary>
        /// Wrap an existing callback method, combinining with the the token to detect if the copy operation should be cancelled.
        /// </summary>
        /// <returns>A new <see cref="CopyProgressCallback"/></returns>
        public static CopyProgressCallback CreateCallback(CopyProgressCallback callback, CancellationToken token)
        {
            return GetResult;
            CopyProgressCallbackResult GetResult(long a, long b, long c, long d, uint e, CopyProgressCallbackReason f, IntPtr g, IntPtr h, IntPtr i)
            {
                var result = callback?.Invoke(a, b, c, d, e, f, g, h, i);
                if (token.IsCancellationRequested)
                    result = CopyProgressCallbackResult.CANCEL;
                return result ?? CopyProgressCallbackResult.CONTINUE;
            }
        }

        /// <summary>
        /// Wrap a callback with the provided <paramref name="token"/> to determine if cancellation is required
        /// </summary>
        /// <param name="callback">The callback</param>
        /// <param name="token">Token used to determine if the copy operation should be cancelled.</param>
        /// <returns>A new <see cref="CopyProgressCallback"/></returns>
        public static CopyProgressCallback CreateCallback(BytesTransferredCallback callback, CancellationToken token)
        {
            return GetResult;
            CopyProgressCallbackResult GetResult(long tfs, long bc, long _1, long _2, uint _3, CopyProgressCallbackReason reason, IntPtr _4, IntPtr _5, IntPtr _6)
            {
                var result = callback?.Invoke(tfs, bc);
                if (token.IsCancellationRequested)
                    result = CopyProgressCallbackResult.CANCEL;
                return result ?? CopyProgressCallbackResult.CONTINUE;
            }
        }

        /// <summary>
        /// Create a new CopyProgressCallback
        /// </summary>
        /// <param name="action">The action to perform. First parameter is total file size, second parameter is number of bytes copied.</param>
        /// <param name="token">Token used to determine if the copy operation should be cancelled.</param>
        /// <returns>A new <see cref="CopyProgressCallback"/></returns>
        public static CopyProgressCallback CreateCallback(Action<long, long> action, CancellationToken token)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            return GetResult;
            CopyProgressCallbackResult GetResult(long tfs, long bc, long _1, long _2, uint _3, CopyProgressCallbackReason reason, IntPtr _4, IntPtr _5, IntPtr _6)
            {
                action(tfs, bc);
                return token.IsCancellationRequested ? CopyProgressCallbackResult.CANCEL : CopyProgressCallbackResult.CONTINUE;
            }
        }

        /// <summary>
        /// Create a new callback that will calculate the current progress and report it to the <paramref name="progress"/> object
        /// </summary>
        /// <param name="progress">The object to report progress to</param>
        /// <param name="token">Token used to determine if the copy operation should be cancelled.</param>
        /// <returns>A new <see cref="CopyProgressCallback"/></returns>
        public static CopyProgressCallback CreateCallback(IProgress<double> progress, CancellationToken token = default)
        {
            if (progress is null && !token.CanBeCanceled) return null;
            if (progress is null) return token.CanBeCanceled ? CreateCallback(token) : null;
            return token.CanBeCanceled ? CreateCallback(report, token) : CreateCallback(report);

            CopyProgressCallbackResult report(long total, long processed)
            {
                progress.Report((double)100 * processed / total);
                if (progress is ProgressReporter pg)
                    return pg.Result;
                else 
                    return CopyProgressCallbackResult.CONTINUE;
            }
        }
    }
}
