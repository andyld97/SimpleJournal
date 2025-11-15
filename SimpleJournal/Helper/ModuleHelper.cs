using SimpleJournal.Helper;
using SimpleJournal.Modules;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;

namespace Helper
{
    public static class ModuleHelper
    {
        public static void ApplyTabbedFeatures(this ITabbedModule tabbedModule)
        {
            foreach (var tb in tabbedModule.TabControl.Items)
            {
                var tabItem = (tb as TabItem);
                tabItem.ApplyTemplate();

                var grid = tabItem.Template.FindName("PART_Header", tabItem) as Grid;
                grid.PreviewMouseDown += delegate (object sender, System.Windows.Input.MouseButtonEventArgs e)
                {
                    HandleMouseMove(tabbedModule, e, sender);
                };
            }

            tabbedModule.MainGrid.MouseDown += delegate (object sender, System.Windows.Input.MouseButtonEventArgs e)
            {
                if (e.OriginalSource is TabPanel)
                    HandleMouseMove(tabbedModule, e, sender);
            };
        }

        private static void HandleMouseMove(ITabbedModule module, MouseButtonEventArgs e, object sender)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (e.ClickCount == 2)
            {
                module.ToggleWindowState?.Invoke(sender, EventArgs.Empty);
                e.Handled = true;
                return;
            }

            module.Move?.Invoke(sender, EventArgs.Empty);
        }

        public static void ApplyTabbedModuleToWindow(this ITabbedModule module, Window moduleOwner)
        {
            module.ToggleWindowState += delegate (object sender, EventArgs e)
            {
                if (module.CanToggleWindowState)
                    ToggleNormalMax(moduleOwner);
            };

            module.Move += delegate (object sender, EventArgs e)
            {
                Move(moduleOwner);
            };
        }
 
        #region Move
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private static void Move(Window window)
        {
            NativeMethods.ReleaseCapture();
            IntPtr windowHandle = new WindowInteropHelper(window).Handle;
            _ = NativeMethods.SendMessage(windowHandle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private static void ToggleNormalMax(Window window)
        {
            if (window.WindowState == WindowState.Maximized)
               window. WindowState = WindowState.Normal;
            else if (window.WindowState == WindowState.Normal)
                window.WindowState = WindowState.Maximized;
        }
        #endregion
    }
}