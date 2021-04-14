using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleJournal.Controls
{
    /// <summary>
    /// Interaktionslogik für NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        private bool handleTextChanged = true;
        private int value = 0;

        public delegate void onChanged(int oldValue, int newValue);

        /// <summary>
        /// Will be fired if user changed the number
        /// </summary>
        public event onChanged OnChanged;

        /// <summary>
        /// The lowest possible number
        /// </summary>
        public int Minimum { get; set; } = 0;

        /// <summary>
        /// The highest possible number
        /// </summary>
        public int Maximum { get; set; } = 100;

        /// <summary>
        /// The value how much the actual value will be incremented/decremented
        /// </summary>
        public int Step { get; set; } = 1;

        /// <summary>
        /// The actual/current value 
        /// </summary>
        public int Value
        {
            get => value; set
            {
                handleTextChanged = false;
                int oldValue = this.value;
                this.value = value;
                this.NUDTextBox.Text = value.ToString();
                NUDTextBox.SelectionStart = NUDTextBox.Text.Length;

                this.OnChanged?.Invoke(oldValue, value);

                handleTextChanged = true;
            }
        }

        public NumericUpDown()
        {
            InitializeComponent();
            NUDTextBox.Text = Value.ToString();
        }

        private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (Value + Step > Maximum)
                Value = Minimum;
            else
                Value += Step;
        }

        private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (Value - Step < Minimum)
                Value = Maximum;
            else
                Value -= Step;
        }

        private void NUDTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Add)
            {
                NUDButtonUP.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonUP, new object[] { true });
            }

            if (e.Key == Key.Down || e.Key == Key.Subtract)
            {
                NUDButtonDown.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonDown, new object[] { true });
            }
        }

        private void NUDTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Add)
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonUP, new object[] { false });

            if (e.Key == Key.Down || e.Key == Key.Subtract)
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonDown, new object[] { false });
        }

        private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!handleTextChanged)
                return;

            if (!string.IsNullOrEmpty(NUDTextBox.Text))
            {
                if (int.TryParse(NUDTextBox.Text, out  int value))
                {
                    if (value > Maximum)
                        Value = Maximum;
                    else if (value < Minimum)
                        Value = Minimum;
                    else
                        Value = value;
                }
                else
                    Value = Value;
            }
        }
    }
}
