using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Collections.Generic;

namespace RoboSharp
{
    internal static class ExtensionMethods
    {
        /// <summary> Encase the LogPath in quotes if needed </summary>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        internal static string WrapPath(this string logPath) => (!logPath.StartsWith("\"") && logPath.Contains(" ")) ? $"\"{logPath}\"" : logPath;

        /// <remarks> Extension method provided by RoboSharp package </remarks>
        /// <inheritdoc cref="System.String.IsNullOrWhiteSpace(string)"/>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        internal static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        internal static long TryConvertLong(this string val)
        {
            try
            {
                return Convert.ToInt64(val);
            }
            catch
            {
                return 0;
            }
        }

        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        internal static int TryConvertInt(this string val)
        {
            try
            {
                return Convert.ToInt32(val);
            }
            catch
            {
                return 0;
            }

        }
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

    }
}

namespace System.Threading

{
    /// <summary>
    /// Contains methods for CancelleableSleep and WaitUntil
    /// </summary>
    internal static class ThreadEx
    {

        /// <summary>
        /// Wait synchronously until this task has reached the specified <see cref="TaskStatus"/>
        /// </summary>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        public static void WaitUntil(this Task t, TaskStatus status)
        {
            while (t.Status < status)
                Thread.Sleep(100);
        }

        /// <summary>
        /// Wait asynchronously until this task has reached the specified <see cref="TaskStatus"/> <br/>
        /// Checks every 100ms
        /// </summary>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        public static async Task WaitUntilAsync(this Task t, TaskStatus status)
        {
            while (t.Status < status)
                await Task.Delay(100);
        }

        /// <summary>
        /// Wait synchronously until this task has reached the specified <see cref="TaskStatus"/><br/>
        /// Checks every <paramref name="interval"/> milliseconds
        /// </summary>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        public static async Task WaitUntilAsync(this Task t, TaskStatus status, int interval)
        {
            while (t.Status < status)
                await Task.Delay(interval);
        }

        /// <param name="timeSpan">TimeSpan to sleep the thread</param>
        /// <param name="token"><inheritdoc cref="CancellationToken"/></param>
        /// <inheritdoc cref="CancellableSleep(int, CancellationToken)"/>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        internal static Task<bool> CancellableSleep(TimeSpan timeSpan, CancellationToken token)
        {
            return CancellableSleep((int)timeSpan.TotalMilliseconds, token);
        }

        /// <inheritdoc cref="CancellableSleep(int, CancellationToken[])"/>
        /// <inheritdoc cref="CancellableSleep(int, CancellationToken)"/>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        internal static Task<bool> CancellableSleep(TimeSpan timeSpan, CancellationToken[] tokens)
        {
            return CancellableSleep((int)timeSpan.TotalMilliseconds, tokens);
        }

        /// <summary>
        /// Use await Task.Delay to sleep the thread. <br/>
        /// Supplied token is used to create a LinkedToken that can cancel the sleep at any point.
        /// </summary>
        /// <returns>True if timer has expired (full duration slep), otherwise false.</returns>
        /// <param name="millisecondsTimeout">Number of milliseconds to wait"/></param>
        /// <param name="token"><inheritdoc cref="CancellationToken"/></param>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        internal static async Task<bool> CancellableSleep(int millisecondsTimeout, CancellationToken token)
        {
            try 
            {
                await Task.Delay(millisecondsTimeout, token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Use await Task.Delay to sleep the thread. <br/>
        /// Supplied tokens are used to create a LinkedToken that can cancel the sleep at any point.
        /// </summary>
        /// <returns>True if slept full duration, otherwise false.</returns>
        /// <param name="millisecondsTimeout">Number of milliseconds to wait"/></param>
        /// <param name="tokens">Use <see cref="CancellationTokenSource.CreateLinkedTokenSource(CancellationToken[])"/> to create the token used to cancel the delay</param>
        /// <inheritdoc cref="CancellableSleep(int, CancellationToken)"/>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        internal static async Task<bool> CancellableSleep(int millisecondsTimeout, CancellationToken[] tokens)
        {
            try
            {
                var token = CancellationTokenSource.CreateLinkedTokenSource(tokens).Token;
                await Task.Delay(millisecondsTimeout, token);
                return true;
            }
            catch
            {
                return false;
            }
        }


    }
}

namespace System.Diagnostics
{
    /// <summary>
    /// Contains methods for Process.WaitForExitAsync
    /// </summary>
    internal static class ProcessExtensions
    {
        /// <summary>
        /// Create a task that asynchronously waits for a process to exit. <see cref="Process.HasExited"/> will be evaluated every 100ms.
        /// </summary>
        /// <inheritdoc cref="WaitForExitAsync(Process, int)"/>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        internal static async Task WaitForExitAsync(this Process process)
        {
            while (!process.HasExited)
                await Task.Delay(100);
        }

        /// <summary>
        /// Create a task that asynchronously waits for a process to exit.
        /// </summary>
        /// <param name="process">Process to wait for</param>
        /// <param name="interval">interval (Milliseconds) to evaluate <see cref="Process.HasExited"/></param>
        /// <returns></returns>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        internal static async Task WaitForExitAsync(this Process process, int interval)
        {
            while (!process.HasExited)
                await Task.Delay(interval);

        }
    }
}
