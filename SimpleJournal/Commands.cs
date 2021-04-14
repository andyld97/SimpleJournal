using System.Windows.Input;

namespace SimpleJournal
{
    public class Commands
    {
        public static readonly RoutedUICommand InsertFromClipboard = new RoutedUICommand("Insert From Clipboard", "Insert From Clipboard", typeof(Window2));
    }
}