using SimpleJournal.Modules;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Dialogs
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        private readonly IModule module;

        public DialogWindow(IModule module)
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
                ResizeMode = ResizeMode.CanResize;
            else
                ResizeMode = ResizeMode.NoResize;

            module.TitleChanged += DialogWindow_TitleChanged;
            module.ModuleClosed += DialogWindow_ModuleClosed;
            module.PassOwner(this);

            Content = module as UserControl;
        }

        private void DialogWindow_TitleChanged(object sender, string title)
        {
            Title = title;
        }

        private void DialogWindow_ModuleClosed(object sender, bool result)
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