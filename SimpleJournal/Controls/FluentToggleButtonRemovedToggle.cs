using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SimpleJournal.Controls
{
    public class FluentToggleButtonRemovedToggle : Fluent.ToggleButton 
    {
        public FluentToggleButtonRemovedToggle()
        {
            this.Checked += FluentToggleButtonRemovedToggle_Checked;     
        }

        private void FluentToggleButtonRemovedToggle_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.IsChecked = false;
        }
    }
}