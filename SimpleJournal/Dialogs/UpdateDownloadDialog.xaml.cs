using SimpleJournal.Common.Helper;
using System;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Threading;

namespace SimpleJournal.Dialogs
{
    /// <summary>
    /// Interaction logic for UpdateDownloadDialog.xaml
    /// </summary>
    public partial class UpdateDownloadDialog : Window
	{
		private int lastSecond;
		private long bytesInLastSecond = 0;	

		public string LocalFilePath { get; set; }

		private string Url { get; set; }

		public UpdateDownloadDialog(string message, string url)
		{
            InitializeComponent();

            TextDescription.Text = message;
			Url = url;
			Loaded += UpdateDownloadDialog_Loaded;
		}

		private async void UpdateDownloadDialog_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				using (HttpClient client = new HttpClient())
				{
					var progress = new DownloadProgress();
					progress.OnProgressChanged += Progress_OnProgressChanged;
					using (System.IO.FileStream fs = new FileStream(LocalFilePath, FileMode.OpenOrCreate))
					{
						lastSecond = DateTime.Now.Second;
						await client.DownloadDataAsync(Url, fs, progress);
						DialogResult = true;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"{Properties.Resources.strUpdateDownloadDialog_FailedToDownloadUpdate}{Environment.NewLine}{Environment.NewLine}{ex.Message}", SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
				DialogResult = false;
			}
		}

		private void Progress_OnProgressChanged(long bytesRead, long totalBytesRead, long length)
		{
			if (length == 0)
				return;

			Dispatcher.Invoke(() =>
			{
				var now = DateTime.Now;

				if (lastSecond != now.Second)
				{
					// x MB of 5,5 GB (11,3 MB/s)
					RunSpeed.Text = string.Format(Properties.Resources.strUpdateDownloadSpeedMessage, ByteUnit.FindUnit(totalBytesRead), ByteUnit.FindUnit(length), ByteUnit.FindUnit(bytesInLastSecond).ToString(true));
					bytesInLastSecond = 0;
					lastSecond = now.Second;
				}
				else
					bytesInLastSecond += bytesRead;

				ProgressDownload.Value = Convert.ToInt32((totalBytesRead / (double)length) * 100.0);
			});
		}
	}
}