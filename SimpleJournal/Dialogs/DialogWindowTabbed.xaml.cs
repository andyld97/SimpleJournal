using Fluent;
using Helper;
using MahApps.Metro.Controls;
using SimpleJournal.Modules;
using System;
using System.ComponentModel;
using System.Security.Cryptography.Pkcs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dialogs
{
    /// <summary>
    /// Interaction logic for DialogWindowTabbed.xaml
    /// </summary>
    public partial class DialogWindowTabbed : RibbonWindow
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
            module.PassOwner(this);
            module.ApplyTabbedModuleToWindow(this);

            MainBorder.Child = module as UserControl;
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

        #region Closing Button (Workaround for MahApps.Metro Theming)

        private void Grid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            GridClosingbutton.Background = FindResource("Fluent.Ribbon.Brushes.Accent80") as SolidColorBrush; //  new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A92831"));
        }

        private void Grid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            GridClosingbutton.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void GridClosingbutton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                Close();
        }
        #endregion
    }
}