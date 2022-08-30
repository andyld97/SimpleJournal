using System;
using System.Windows.Controls;

namespace SimpleJournal.Modules
{
    public interface IModule
    {
        /// <summary>
        /// Equivalent to DialogResult, should be implemented like DialogResult
        /// </summary>
        EventHandler<bool> ModuleClosed { get; set; }

        /// <summary>
        /// Called when the parent window gets closed
        /// </summary>
        void OnClosing() { }

        /// <summary>
        /// Is called when the owner of this module shoud change the title
        /// </summary>
        EventHandler<string> TitleChanged { get; set; }
    }

    public interface ITabbedModule : IModule
    {
        /// <summary>
        /// Is called when the module wants to change the WindowState
        /// </summary>
        EventHandler ToggleWindowState { get; set; }

        /// <summary>
        /// Is called when the module gets moved via mouse
        /// </summary>
        EventHandler Move { get; set; }

        /// <summary>
        /// The main tabcontrol
        /// </summary>
        TabControl TabControl { get; }

        /// <summary>
        /// The main grid
        /// </summary>
        Grid MainGrid { get; }

        /// <summary>
        /// Determines if this module lets his owner change the WindowState
        /// </summary>
        bool CanToggleWindowState { get; }
    }
}