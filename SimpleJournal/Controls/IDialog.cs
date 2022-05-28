using System;

namespace SimpleJournal.Controls
{
    public interface IDialog
    {
        EventHandler<bool> DialogClosed { get; set; }

        EventHandler<string> TitleChanged { get; set; }
    }
}