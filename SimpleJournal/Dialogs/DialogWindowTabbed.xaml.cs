using Helper;
using MahApps.Metro.Controls;
using SimpleJournal.Modules;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Dialogs
{
    /// <summary>
    /// Interaction logic for DialogWindowTabbed.xaml
    /// </summary>
    public partial class DialogWindowTabbed : MetroWindow
    {
        private readonly ITabbedModule module;

        public DialogWindowTabbed(ITabbedModule module)
        {
            this.module = module;
            if (module.WindowSize != null)
            {
                Width = module.WindowSize.Width;
                Height = module.WindowSize.Height;
            }
            InitializeComponent();

            Title = module.Title;

            if (module.CanToggleWindowState)
            {
                ResizeMode = ResizeMode.CanResize;
                ShowMaxRestoreButton = true;
            }
            else
            {
                ResizeMode = ResizeMode.NoResize;
                ShowMaxRestoreButton = false;
            }

            module.TitleChanged += DialogWindow_TitleChanged;
            module.ModuleClosed += DialogWindow_ModuleClosed;
            module.ApplyTabbedModuleToWindow(this);

            Content = module as UserControl;
        }

        private void DialogWindow_TitleChanged(object? sender, string title)
        {
            Title = title;
        }

        private void DialogWindow_ModuleClosed(object? sender, bool result)
        {
            DialogResult = result;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            module.OnClosing();
        }
    }
}