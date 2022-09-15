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
        [DebuggerHidden()]
        internal static string WrapPath(this string logPath) => (!logPath.StartsWith("\"") && logPath.Contains(" ")) ? $"\"{logPath}\"" : logPath;

        /// <remarks> Extension method provided by RoboSharp package </remarks>
        /// <inheritdoc cref="System.String.IsNullOrWhiteSpace(string)"/>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden()]
        internal static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden()]
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
        [DebuggerHidden()]
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
        
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden()]
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
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden()]
        public static bool EndsWithDirectorySeperator(this string path) => path.EndsWith(Path.DirectorySeparatorChar.ToString()) || path.EndsWith(Path.AltDirectorySeparatorChar.ToString());

        /// <summary>
        /// Convert <paramref name="StrTwo"/> into a char[]. Perform a ForEach( Char in strTwo) loop, and append any characters in Str2 to the end of this string if they don't already exist within this string.
        /// </summary>
        /// <param name="StrOne"></param>
        /// <param name="StrTwo"></param>
        /// <returns></returns>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden()]
        internal static string CombineCharArr(this string StrOne, string StrTwo)
        {
            if (String.IsNullOrWhiteSpace(StrTwo)) return StrOne;
            if (String.IsNullOrWhiteSpace(StrOne)) return StrTwo ?? StrOne;
            string ret = StrOne;
            char[] S2 = StrTwo.ToArray();
            foreach (char c in S2)
            {
                if (!ret.Contains(c))
                    ret += c;
            }
            return ret;
        }

        /// <summary>
        /// Compare the current value to that of the supplied value, and take the greater of the two.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="i2"></param>
        /// <returns></returns>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden()]
        internal static int GetGreaterVal(this int i, int i2) => i >= i2 ? i : i2;

        /// <summary>
        /// Evaluate this string. If this string is null or empty, replace it with the supplied string.
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden()]
        internal static string ReplaceIfEmpty(this string str1, string str2) => String.IsNullOrWhiteSpace(str1) ? str2 ?? String.Empty : str1;
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
        /// </summary>
        /// <returns>True if timer has expired (full duration slep), otherwise false.</returns>
        /// <param name="millisecondsTimeout">Number of milliseconds to wait"/></param>
        /// <param name="token"><inheritdoc cref="CancellationToken"/></param>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        internal static Task<bool> CancellableSleep(int millisecondsTimeout, CancellationToken token)
        {
            return Task.Delay(millisecondsTimeout, token).ContinueWith(t => t.Exception == default);
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
        internal static Task<bool> CancellableSleep(int millisecondsTimeout, CancellationToken[] tokens)
        {
            var token = CancellationTokenSource.CreateLinkedTokenSource(tokens).Token;
            return Task.Delay(millisecondsTimeout, token).ContinueWith(t => t.Exception == default);
        }
    }
}
