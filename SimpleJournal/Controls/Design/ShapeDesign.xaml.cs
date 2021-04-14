﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SimpleJournal.Controls
{
    /// <summary>
    /// Interaktionslogik für ShapeDesign.xaml
    /// </summary>
    public partial class ShapeDesign : UserControl
    {
        public delegate void onChanged(ShapeInfo info);
        public event onChanged OnChanged;

        private bool isInitalized = false;

        public ShapeInfo Info { get; private set; } = new ShapeInfo();

        public ShapeDesign()
        {
            InitializeComponent();

            borderColorPicker.SetBorderToRectangle();
            backgroundColorPicker.SetBorderToRectangle();
        }

        public void Load(ShapeInfo info)
        {
            // Load values from info
            isInitalized = false;
            this.Info = info;

            numAngle.Value = info.Angle;
            numBorderWidth.Value = info.BorderWidth;
            borderColorPicker.SelectedColor = info.BorderColor;
            backgroundColorPicker.SelectedColor = (info.BackgroundColor == Colors.Transparent ? Colors.Black : info.BackgroundColor);
            chkTransparent.IsChecked = (info.BackgroundColor == Colors.Transparent);

            isInitalized = true;
        }

        private void NumAngle_OnChanged(int oldValue, int newValue)
        {
            if (!isInitalized)
                return;

            Info.Angle = newValue;
            OnChanged?.Invoke(Info);
        }

        private void NumBorderWidth_OnChanged(int oldValue, int newValue)
        {
            if (!isInitalized)
                return;

            Info.BorderWidth = newValue;
            OnChanged?.Invoke(Info);
        }

        private void BorderColorPicker_ColorChanged(Color c)
        {
            if (!isInitalized)
                return;

            Info.BorderColor = c;
            OnChanged?.Invoke(Info);
        }

        private void BackgroundColorPicker_ColorChanged(Color c)
        {
            if (!isInitalized)
                return;

            Info.BackgroundColor = c;
            OnChanged?.Invoke(Info);
        }

        private void ChkTransparent_Checked(object sender, RoutedEventArgs e)
        {
            checkedChangedChkTransparent();
        }

        private void ChkTransparent_Unchecked(object sender, RoutedEventArgs e)
        {
            checkedChangedChkTransparent();
        }

        private void checkedChangedChkTransparent()
        {
            if (!isInitalized)
                return;

            Info.BackgroundColor = (chkTransparent.IsChecked.Value ? Colors.Transparent : backgroundColorPicker.SelectedColor);
            OnChanged?.Invoke(Info);
        }
    }

    public class ShapeInfo
    {
        public Color BackgroundColor;

        public Color BorderColor;

        public int BorderWidth;

        public int Angle;

        public ShapeInfo()
        {

        }

        public ShapeInfo(Color background, Color borderColor, int borderWidth, int angle)
        {
            this.BackgroundColor = background;
            this.BorderColor = borderColor;
            this.BorderWidth = borderWidth;
            this.Angle = angle;
        }
    }

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool v)
                return !v;            

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
