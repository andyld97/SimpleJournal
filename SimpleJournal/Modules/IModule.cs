using System;
using System.Windows.Controls;

namespace SimpleJournal.Modules
{
    public interface IModule
    {
        EventHandler<bool> ModuleClosed { get; set; }

        /// <summary>
        /// Called when the parent window gets closed
        /// </summary>
        void OnClosing() { }

        EventHandler<string> TitleChanged { get; set; }
    }

    public interface ITabbedModule : IModule
    {
        EventHandler ToggleMinimizeMaximize { get; set; }

        EventHandler Move { get; set; }

        TabControl TabControl { get; }

        Grid MainGrid { get; }
    }
}