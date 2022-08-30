using System;

namespace SimpleJournal.Modules
{
    public interface IModule
    {
        EventHandler<bool> ModuleClosed { get; set; }

        EventHandler<string> TitleChanged { get; set; }
    }
}