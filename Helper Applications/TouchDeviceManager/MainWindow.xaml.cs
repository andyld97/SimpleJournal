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
                Title = $"Touch Device Manager - {devices.Count} Devices found!";

                if (devices.Count > 0)
                    ListDevices.SelectedIndex = devices.Count - 1;
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred while querying for WMI data: " + e.Message);
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
                MessageBox.Show($"Please select a device first!", "No device selected!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                devices[index].SetDeviceEnabled(state);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set state {state} for device: {devices[index]}: {ex}", "Fail!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            RefreshList();
        }
    }
}