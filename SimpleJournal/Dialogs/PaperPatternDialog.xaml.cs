using Helper;
using MahApps.Metro.Controls;
using SimpleJournal.Common;
using SimpleJournal.Modules;
using System;
using System.Windows;
using System.Windows.Interop;

namespace Dialogs
{
    /// <summary>
    /// Interaction logic for PaperPatternDialog.xaml
    /// </summary>
    public partial class PaperPatternDialog : MetroWindow
    {
        public PaperPatternDialog(PaperType paperType)
        {
            InitializeComponent();
            PaperPattern.SelectPaperType(paperType);

            // Assign module events
            PaperPattern.ApplyTabbedModuleToWindow(this);
        }
    }
}