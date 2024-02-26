using RoboSharp.Extensions.CopyFileEx;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// Class that provides extensions for FileInfo objects for Copying/Moving files asynchronously
    /// </summary>
    public static class FileInfoExtensions
    {
        private const string EmptyDestinationErr = "Destination Path can not be empty";

        /// <inheritdoc cref="FileFunctions.CopyFileProgressAsync"/>
        public static void CopyTo(this FileInfo source, string destination, CopyProgressCallback callback, CopyFileOptions options = CopyFileOptions.FAIL_IF_EXISTS)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(destination)) throw new ArgumentException(EmptyDestinationErr, nameof(source));
            _ = FileFunctions.CopyFile(source.FullName, destination, options, callback);
        }

        /// <inheritdoc cref="CopyToAsync(FileInfo, string, bool, CancellationToken)"/>
        public static Task CopyToAsync(this FileInfo source, string destination, CancellationToken token = default)
            => CopyToAsync(source, destination, false, token);

        /// <inheritdoc cref="FileFunctions.CopyFileAsync(string, string, bool, System.Threading.CancellationToken)"/>
        public static async Task CopyToAsync(this FileInfo source, string destination, bool overwrite, CancellationToken token = default)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(destination)) throw new ArgumentException(EmptyDestinationErr, nameof(source));
            await FileFunctions.CopyFileAsync(source.FullName, destination, overwrite, token).ConfigureAwait(false);
        }

        /// <inheritdoc cref="FileFunctions.CopyFileAsync(string, string, IProgress{double}, int, bool, CancellationToken)"/>
        public static async Task CopyToAsync(this FileInfo source, string destination, IProgress<double> progress, bool overwrite = false, int updateInterval = 100,  CancellationToken token = default)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(destination)) throw new ArgumentException(EmptyDestinationErr, nameof(source));
            await FileFunctions.CopyFileAsync(source.FullName, destination, progress, updateInterval, overwrite, token).ConfigureAwait(false);
        }

        /// <inheritdoc cref="FileFunctions.CopyFileAsync(string, string, IProgress{ProgressUpdate}, int, bool, CancellationToken)"/>
        public static async Task CopyToAsync(this FileInfo source, string destination, IProgress<ProgressUpdate> progress, bool overwrite = false, int updateInterval = 100, CancellationToken token = default)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(destination)) throw new ArgumentException(EmptyDestinationErr, nameof(source));
            await FileFunctions.CopyFileAsync(source.FullName, destination, progress, updateInterval, overwrite, token).ConfigureAwait(false);
        }

        /// <inheritdoc cref="FileFunctions.CopyFileProgressAsync"/>
        public static Task CopyToAsync(this FileInfo source, string destination, CopyProgressCallback callback, CopyFileOptions options, CancellationToken token = default)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(destination)) throw new ArgumentException(EmptyDestinationErr, nameof(source));
            return FileFunctions.CopyFileAsync(source.FullName, destination, options, callback, token: token);
        }

        /// <inheritdoc cref="FileFunctions.CopyFileProgressAsync"/>
        public static Task CopyToAsync(this FileInfo source, string destination, IProgress<double> progress, CopyFileOptions options, CancellationToken token = default)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(destination)) throw new ArgumentException(EmptyDestinationErr, nameof(source));
            return FileFunctions.CopyFileAsync(source.FullName, destination, progress, options, 100, token);
        }

        /// <inheritdoc cref="FileFunctions.CopyFileProgressAsync"/>
        public static Task CopyToAsync(this FileInfo source, string destination, IProgress<long> progress, CopyFileOptions options, CancellationToken token = default)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(destination)) throw new ArgumentException(EmptyDestinationErr, nameof(source));
            return FileFunctions.CopyFileAsync(source.FullName, destination, progress, options, 100, token);
        }

        /// <inheritdoc cref="MoveToAsync(FileInfo, string, bool, CancellationToken)"/>
        public static Task MoveToAsync(this FileInfo source, string destination, CancellationToken token = default)
            => MoveToAsync(source, destination, false, token);


        /// <inheritdoc cref="FileFunctions.MoveFileAsync(string, string, bool, CancellationToken)"/>
        public static async Task MoveToAsync(this FileInfo source, string destination, bool overwrite, CancellationToken token = default)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(destination)) throw new ArgumentException(EmptyDestinationErr, nameof(source));
            await FileFunctions.MoveFileAsync(source.FullName, destination, overwrite, token).ConfigureAwait(false);
        }

        /// <inheritdoc cref="FileFunctions.MoveFileAsync(string, string, IProgress{double}, int, bool, CancellationToken)"/>
        public static async Task MoveToAsync(this FileInfo source, string destination, IProgress<double> progress, bool overwrite = false, int updateInterval = 100, CancellationToken token = default)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(destination)) throw new ArgumentException(EmptyDestinationErr, nameof(source));
            await FileFunctions.MoveFileAsync(source.FullName, destination, progress, updateInterval, overwrite, token).ConfigureAwait(false);
        }

        /// <inheritdoc cref="FileFunctions.MoveFileAsync(string, string, IProgress{ProgressUpdate}, int, bool, CancellationToken)"/>
        public static async Task MoveToAsync(this FileInfo source, string destination, IProgress<ProgressUpdate> progress, bool overwrite = false, int updateInterval = 100, CancellationToken token = default)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(destination)) throw new ArgumentException(EmptyDestinationErr, nameof(source));
            await FileFunctions.MoveFileAsync(source.FullName, destination, progress, updateInterval, overwrite, token).ConfigureAwait(false);
        }

    }
}
