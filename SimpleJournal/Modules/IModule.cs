using Dialogs;
using SimpleJournal.Common.Data;
using System;
using System.Windows;
using System.Windows.Controls;
using Size = SimpleJournal.Common.Data.Size;

namespace SimpleJournal.Modules
{
    public interface IModule
    {
        /// <summary>
        /// Equivalent to DialogResult, should be implemented like DialogResult
        /// </summary>
        EventHandler<bool> ModuleClosed { get; set; }     

        /// <summary>
        /// Determines if this module lets his owner change the WindowState
        /// </summary>
        bool CanToggleWindowState => true;

        /// <summary>
        /// Title of this dialog/module (used as window title)
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Is called when the owner of this module shoud change the title
        /// </summary>
        EventHandler<string> TitleChanged { get; set; }

        /// <summary>
        /// The size of the parent window
        /// </summary>
        public Size WindowSize { get; }

        /// <summary>
        /// Called when the parent window gets closed
        /// </summary>
        void OnClosing() { }

        /// <summary>
        /// Shows this module as a dialog as a normal window or tabbed window
        /// </summary>
        /// <param name="isTabbedDialog"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        bool? ShowModuleWindow(bool isTabbedDialog, Window owner = null)
        {
            Window result;
            if (isTabbedDialog)
                result = new DialogWindowTabbed(this as ITabbedModule);
            else
                result = new DialogWindow(this);

            if (owner != null)
                result.Owner = owner;

            return result.ShowDialog();
        }
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
    }
}