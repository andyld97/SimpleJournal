using System.Windows.Controls;

namespace SimpleJournal.Controls
{
    public class ListBoxScroll : ListBox
    {
        public ListBoxScroll() : base()
        {
            SelectionChanged += ListBoxScroll_SelectionChanged;
        }

        private void ListBoxScroll_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScrollIntoView(SelectedItem);
        }
    }
}