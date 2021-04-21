using System.Windows.Input;

namespace SimpleJournal
{
    public class Commands
    {
        public static readonly RoutedUICommand InsertFromClipboard = new RoutedUICommand("Insert From Clipboard", "Insert From Clipboard", typeof(Window2));

        public static readonly RoutedUICommand DisableTouchScreen = new RoutedUICommand("Disable Touch Screen", "Disable Touch Screen", typeof(Window2));

        public static readonly RoutedUICommand EnableTouchScreen = new RoutedUICommand("Disable Touch Screen", "Disable Touch Screen", typeof(Window2));
    }
}