using System;

namespace SimpleJournal.Modules
{
    public interface IModule
    {
        EventHandler<bool> ModuleClosed { get; set; }

        EventHandler<string> TitleChanged { get; set; }
    }

    public interface ITabbedModule : IModule
    {
        EventHandler ToggleMinimizeMaximize { get; set; }

        EventHandler Move { get; set; }
    }
}