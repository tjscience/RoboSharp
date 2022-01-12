using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace RoboSharp
{
    internal static class ExtensionMethods
    {
        public static string CleanOptionInput(this string option)
        {
            // Get rid of forward slashes
            option = option.Replace("/", "");
            // Get rid of padding
            option = option.Trim();

            return option;
        }

        public static string CleanDirectoryPath(this string path)
        {
            // Get rid of single and double quotes
            path = path?.Replace("\"", "");
            path = path?.Replace("\'", "");

            //Validate against null / empty strings. 
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;

            // Get rid of padding
            path = path.Trim();

            // Get rid of trailing Directory Seperator Chars
            // Length greater than 3 because E:\ is the shortest valid path
            while (path.Length > 3 && path.EndsWithDirectorySeperator())
            {
                path = path.Substring(0, path.Length - 1);
            }

            //Sanitize invalid paths -- Convert E: to E:\
            if (path.Length <= 2)
            {
                if (DriveRootRegex.IsMatch(path))
                    return path.ToUpper() + '\\';
                else
                    return path;
            }

            // Fix UNC paths that are the root directory of a UNC drive
            if (Uri.TryCreate(path, UriKind.Absolute, out Uri URI) && URI.IsUnc)
            {
                if (path.EndsWith("$"))
                {
                    path += '\\';
                }
            }

            return path;
        }

        private static readonly Regex DriveRootRegex = new Regex("[A-Za-z]:", RegexOptions.Compiled);

        /// <summary>
        /// Check if the string ends with a directory seperator character
        /// </summary>
        public static bool EndsWithDirectorySeperator(this string path) => path.EndsWith(Path.DirectorySeparatorChar.ToString()) || path.EndsWith(Path.AltDirectorySeparatorChar.ToString());

        /// <summary>
        /// Wait synchronously until this task has reached the specified <see cref="TaskStatus"/>
        /// </summary>
        public static void WaitUntil(this Task t, TaskStatus status )
        {
            while (t.Status < status)
                System.Threading.Thread.Sleep(150);
        }

    }
}


namespace System.Threading

{
    internal static class ThreadEx
    {

        /// <param name="timeSpan"></param>
        /// <param name="token"><inheritdoc cref="CancellationToken"/></param>
        /// <inheritdoc cref="CancellableSleep(int, CancellationToken)"/>
#if !NET40
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
#endif
        internal static bool CancellableSleep(TimeSpan timeSpan, CancellationToken token)
        {
            return CancellableSleep((int)timeSpan.TotalMilliseconds, token);
        }

        /// <summary>
        /// Method that represents Task.Delay for all supported targets
        /// </summary>
        /// <remarks>
        /// If Net40 uses Creates a timer and registers against the supplied token, then returns the underlying task from a <see cref="TaskCompletionSource{TResult}"/>.<br/>
        /// Otherwise uses Task.Delay(int,CancellationToken)
        /// </remarks>
        /// <returns>True if timer has expired (full duration slep), otherwise false.</returns>
        /// <param name="millisecondsTimeout">Number of milliseconds to wait"/></param>
        /// <param name="token"><inheritdoc cref="CancellationToken"/></param>
#if !NET40
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
#endif
        internal static bool CancellableSleep(int millisecondsTimeout, CancellationToken token)
        {
#if NET40
            //https://stackoverflow.com/questions/15341962/how-to-put-a-task-to-sleep-or-delay-in-c-sharp-4-0
            var tcs = new TaskCompletionSource<bool>();
            bool ValidToken = token != null;

            if (ValidToken && token.IsCancellationRequested)
            {
                tcs.TrySetCanceled();
                return false;
            }

            Timer timer = null;
            timer = new Timer(
                (cb) =>
                {
                    timer.Dispose();        //stop the timer
                    tcs.TrySetResult(true); //Set Completed
                }
            , null, millisecondsTimeout, Timeout.Infinite); //Run X ms starting now, only run once.

            //Setup the Cancellation Token
            if (ValidToken)
                token.Register(() =>
                {
                    timer.Dispose();            //stop the timer
                    tcs.TrySetResult(false);    //Set Cancelled
                });

            try
            {
                tcs.Task.Wait();   
            }
            catch
            {
                return false;
            }
            return tcs.Task.Result;
#else
            try 
            {
                Task.Delay(millisecondsTimeout, token).Wait();
                return true;
            }
            catch
            {
                return false;
            }
#endif
        }




    }

}