using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleJournal.Common.Helper
{
	/// <summary>
	/// https://gist.github.com/dalexsoto/9fd3c5bdbe9f61a717d47c5843384d11
	/// </summary>
	public static class HttpClientHelper
	{
		public static async Task DownloadDataAsync(this HttpClient client, string requestUrl, Stream destination, IDownloadProgress progress = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var response = await client.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead))
			{
				var contentLength = response.Content.Headers.ContentLength;
				using (var download = await response.Content.ReadAsStreamAsync())
				{
					// no progress... no contentLength... very sad
					if (progress is null || !contentLength.HasValue)
					{
						await download.CopyToAsync(destination);
						return;
					}

					progress.SetLength(contentLength.Value);

					// Such progress and contentLength much reporting Wow!
					await CopyToAsync(download, destination, 81920, progress, cancellationToken);
				}
			}
		}

		public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IDownloadProgress progress = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (bufferSize < 0)
				throw new ArgumentOutOfRangeException(nameof(bufferSize));
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			if (!source.CanRead)
				throw new InvalidOperationException($"'{nameof(source)}' is not readable.");
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));
			if (!destination.CanWrite)
				throw new InvalidOperationException($"'{nameof(destination)}' is not writable.");

			var buffer = new byte[bufferSize];
			long totalBytesRead = 0;
			int bytesRead;
			while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
			{
				await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
				totalBytesRead += bytesRead;
				progress?.Report(bytesRead, totalBytesRead);
			}
		}
	}

	public interface IDownloadProgress
	{
		void Report(long bytesRead, long totalBytesRead);

		void SetLength(long length);
	}

	public class DownloadProgress : IDownloadProgress
	{
		private long length;

		public delegate void onProgressChanged(long bytesRead, long totalBytesRead, long length);
		public event onProgressChanged OnProgressChanged;

		public void Report(long bytesRead, long totalBytesRead)
		{
			OnProgressChanged?.Invoke(bytesRead, totalBytesRead, this.length);
		}

		public void SetLength(long length)
		{
			this.length = length;
		}
	}
}