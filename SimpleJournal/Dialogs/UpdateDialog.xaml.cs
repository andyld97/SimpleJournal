﻿using Helper;
using Microsoft.Web.WebView2.Core;
using SimpleJournal.Common.Helper;
using SimpleJournal.Documents.UI;
using System;
using System.Windows;

namespace SimpleJournal.Dialogs
{
    /// <summary>
    /// Interaktionslogik für UpdateDialog.xaml
    /// </summary>
    public partial class UpdateDialog : Window
    {
        private readonly Version version;
        private readonly string hash;

        public UpdateDialog(Version v, string hash)
        {
            InitializeComponent();
            this.version = v;
            this.hash = hash;

            try
            {
                txtVersion.Text = string.Format(Properties.Resources.strUpdateDialogVersionText, v.ToString(4));
            }
            catch
            {
                txtVersion.Text = string.Format(Properties.Resources.strUpdateDialogVersionText, v.ToString(3));
            }

            Loaded += UpdateDialog_Loaded;
        }

        private async void UpdateDialog_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load Changelog
                var webView2Environment = await CoreWebView2Environment.CreateAsync(null, Consts.WebView2CachePath);
                await webChangelog.EnsureCoreWebView2Async(webView2Environment);
                webChangelog.Source = new Uri(string.Format(Consts.ChangelogUrl, Properties.Resources.strLang, Settings.Instance.UseDarkMode ? 1 : 0));
            }
            catch (Exception)
            { 
                // ToDo: *** Show a message that WebView2 must be installed instead of the control (maybe auto install?)
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            string message = string.Format(Properties.Resources.strUpdateDownloadDialog_DownloadText, version.ToString(4));
            string localFilePath = System.IO.Path.Combine(FileSystemHelper.GetDownloadsPath(), $"SimpleJournal-{version:4}.exe");

            if (System.IO.File.Exists(localFilePath))
            {
                try
                {
                    // Delete file (if it already exists, it will be not overwritten correctly (hash doesn't seem to match then)
                    System.IO.File.Delete(localFilePath);
                }
                catch
                {
                    // ignore
                }
            }

            UpdateDownloadDialog updateDownloadDialog = new UpdateDownloadDialog(message, Consts.DownloadUrl, hash) { LocalFilePath = localFilePath };

            var result = updateDownloadDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                DialogResult = true;

                try
                {
                    System.Diagnostics.Process.Start(updateDownloadDialog.LocalFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{Properties.Resources.strUpdateDownloadDialog_FailedToOpenSetup}{Environment.NewLine}{Environment.NewLine}{ex.Message}", SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                    DialogResult = false;
                    return;
                }

                // Clear update cache file to ensure no update notifications are displayed!
                UpdateHelper.ResetCache();

                // Exit to make sure user can easily update without problems
                GeneralHelper.Shutdown();
            }
            else
            {
                DialogResult = false;
                return;
            }
        }
    }
}
