﻿using System.Windows;

namespace SimpleJournal.Dialogs
{
    /// <summary>
    /// Interaktionslogik für WaitingDialog.xaml
    /// </summary>
    public partial class WaitingDialog : Window
    {
        private readonly string text = Properties.Resources.strWatingText;
        private readonly string documentName = string.Empty;

        public WaitingDialog(string documentName, int pages)
        {
            InitializeComponent();
            this.documentName = documentName;
            txtInfo.Text = text.Replace("{0}", documentName).Replace("{1}", "1").Replace("{2}", pages.ToString());
        }

        public void SetProgress(double d, int pageFrom, int pageTo)
        {
            Dispatcher.Invoke(() => {

                prgProgress.IsIndeterminate = false;
                prgProgress.Maximum = 1;
                prgProgress.Value = d;

                txtInfo.Text = text.Replace("{0}", documentName).Replace("{1}", pageFrom.ToString()).Replace("{2}", pageTo.ToString());
            });            
        }
    }
}
