using System;
using System.Windows.Controls;

namespace SimpleJournal.Controls.Templates
{
    public abstract class DropDownTemplate : UserControl 
    {
        protected DropDownToggleButton owner = default;

        public void SetOwner(DropDownToggleButton owner)
        {
            this.owner = owner;
            owner.DropDownOpened += Owner_DropDownOpened;
        }

        private void Owner_DropDownOpened(object sender, EventArgs e)
        {
            OnDropDownOpened();
        }

        public virtual void OnDropDownOpened()
        {
               
        }

        public void CloseDropDown()
        {
            if (owner == null)
                return;

            owner.IsDropDownOpen = false;
        }
    }
}
