using Device;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Touch_Device_Manager
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<DeviceWMI> devices = null;

        public MainWindow()
        {
            InitializeComponent();
            RefreshList();
        }

        public void RefreshList()
        {
            try
            {
                devices = DeviceWMI.GetPNPDevicesWithNames(new string[] { "touch screen", "touchscreen" });
                ListDevices.ItemsSource = devices;
                Title = string.Format(Properties.Resources.strTitle, devices.Count);

                if (devices.Count > 0)
                    ListDevices.SelectedIndex = devices.Count - 1;
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format(Properties.Resources.strErrorOccuredWhileQueryingWMIData, e.Message), SimpleJournal.SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonDisable_Click(object sender, RoutedEventArgs e)
        {
            SetStateForSelectedIndex(ListDevices.SelectedIndex, false);
        }

        private void ButtonEnable_Click(object sender, RoutedEventArgs e)
        {
            SetStateForSelectedIndex(ListDevices.SelectedIndex, true);
        }

        private void SetStateForSelectedIndex(int index, bool state)
        {
            if (index < 0 || index > devices.Count - 1)
            {
                MessageBox.Show(Properties.Resources.strNoDeviceSelected_Message, Properties.Resources.strNoDeviceSelected_Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                devices[index].SetDeviceEnabled(state);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.strFailedToSetDeviceState, state, devices[index], ex.Message), SimpleJournal.SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            RefreshList();
        }
    }
}