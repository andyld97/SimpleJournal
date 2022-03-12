using SimpleJournal.Common.Data;
using SimpleJournal.Common.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
		private Version version;	

		public string LocalFilePath { get; set; }

		public UpdateDownloadDialog(Version version)
		{
			InitializeComponent();
			this.version = version;
			Loaded += UpdateDownloadDialog_Loaded;
			RunVersion.Text = version.ToString(4);
		}

		private async void UpdateDownloadDialog_Loaded(object sender, RoutedEventArgs e)
		{
			LocalFilePath = System.IO.Path.Combine(FileSystemHelper.GetDownloadsPath(), $"SimpleJournal-{version:4}.exe");

			try
			{
				using (HttpClient client = new HttpClient())
				{
					var progress = new DownloadProgress();
					progress.OnProgressChanged += Progress_OnProgressChanged;
					using (System.IO.FileStream fs = new FileStream(LocalFilePath, FileMode.OpenOrCreate))
					{
						lastSecond = DateTime.Now.Second;
						await client.DownloadDataAsync(Consts.DownloadUrl, fs, progress);
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
					RunSpeed.Text = string.Format(Properties.Resources.strUpdateDownloadSpeedMessage, ByteUnit.Calculate(totalBytesRead), ByteUnit.Calculate(length), ByteUnit.Calculate(bytesInLastSecond).ToString(true));
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