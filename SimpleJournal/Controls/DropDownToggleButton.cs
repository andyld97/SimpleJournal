using ControlzEx.Theming;
using SimpleJournal.Controls.Templates;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleJournal.Controls
{
    public class DropDownToggleButton : Fluent.DropDownButton
    {
        private bool isMouseOver = false;
        private bool isChecked = false;
        private bool isPressed = false;

        private Popup popup;
        private ContentPresenter presenter;
        private DropDownTemplate template;

        public delegate void click(object sender, EventArgs e);
        public event click Click;

        public DropDownTemplate DropDown
        {
            get => template;
            set
            {
                if (value != template)
                {
                    value.SetOwner(this);
                    template = value;
                }
            }
        }

        public bool IsChecked
        {
            get => isChecked;
            set
            {
                if (!IsToggleable)
                    return;

                if (isChecked != value)
                {
                    isChecked = value;

                    // Fire event
                    Click?.Invoke(this, EventArgs.Empty);
                    RefreshStyle();
                }
            }
        }

        /// <summary>
        /// Determine if this button can be toggled
        /// </summary>
        public bool IsToggleable { get; set; } = true;

        public DropDownToggleButton()
        {
            RefreshStyle();
            this.Style = Application.Current.Resources["stl"] as Style;
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            isMouseOver = true;
            RefreshStyle();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            isMouseOver = false;
            isPressed = false;
            RefreshStyle();
        }

        private void RefreshStyle()
        {
            var ac = ThemeManager.Current.GetTheme(GeneralHelper.GetCurrentTheme());
            Brush borderBrush;
            Brush backgroundBrush;

            if (isPressed && IsToggleable)
            {
                borderBrush = (Brush)ac.Resources["Fluent.Ribbon.Brushes.Button.Pressed.BorderBrush"];
                backgroundBrush = (Brush)ac.Resources["Fluent.Ribbon.Brushes.Button.Pressed.Background"];
            }
            else if (IsChecked && isMouseOver && !isPressed && IsToggleable)
            {
                borderBrush = (Brush)ac.Resources["Fluent.Ribbon.Brushes.ToggleButton.CheckedMouseOver.BorderBrush"];
                backgroundBrush = (Brush)ac.Resources["Fluent.Ribbon.Brushes.ToggleButton.CheckedMouseOver.Background"];
            }
            else if (IsChecked && IsToggleable)
            {
                borderBrush = (Brush)ac.Resources["Fluent.Ribbon.Brushes.ToggleButton.Checked.BorderBrush"];
                backgroundBrush = (Brush)ac.Resources["Fluent.Ribbon.Brushes.ToggleButton.Checked.Background"];
            }
            else if (isMouseOver)
            {
                borderBrush = (Brush)ac.Resources["Fluent.Ribbon.Brushes.Button.MouseOver.BorderBrush"];
                backgroundBrush = (Brush)ac.Resources["Fluent.Ribbon.Brushes.Button.MouseOver.Background"];
            }
            else
            {
                borderBrush = new SolidColorBrush(Colors.Transparent);
                backgroundBrush = new SolidColorBrush(Colors.Transparent);
            }

            this.BorderBrush = borderBrush;
            this.Background = backgroundBrush;
            this.BorderThickness = new Thickness(1);
        }


        #region Prevent DropDown While Clicking Presenter
       
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            try
            {
                var firstChild = this.GetVisualChild(0);
                if (firstChild is Grid grid)
                {
                    if (grid.Children[0] is Border border && border.Child is StackPanel panel && panel.Children[0] is ContentPresenter presenter && grid.Children[1] is Popup popup)
                    {
                        this.popup = popup;
                        this.presenter = presenter;

                        this.popup.Child = this.DropDown;
                    }
                }
            }
            catch (Exception)
            {
                // should never happen (silence is golden)
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            isPressed = true;
            RefreshStyle();

            var pos = e.GetPosition(this);

            if (presenter != null)
            {
                if (presenter.BoundsRelativeTo(this).Contains(pos))
                {
                    // Prevent drop down if user selects this pen
                    e.Handled = true;

                    if (!IsToggleable)
                    {
                        Click?.Invoke(this, EventArgs.Empty);
                        RefreshStyle();
                    }
                }                
            }
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);

            // Handle click even though user clicked on drop down
            IsChecked = true;
            isPressed = false;
            RefreshStyle();
        }
        #endregion
    }
}
