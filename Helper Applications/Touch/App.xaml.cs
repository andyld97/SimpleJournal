using Device;
using System;
using System.Linq;
using System.Windows;

namespace Touch
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length >= 2)
            {
                if (args[1] == "/off")
                    TouchHelper.SetTouchState(false);
                else if (args[1] == "/on")
                    TouchHelper.SetTouchState(true);  
            }

            Environment.Exit(0);
        }
    }

    public static class TouchHelper
    {
        private static readonly string[] TOUCH_SCREEN_NAMES = new string[] { "touchscreen", "touch screen" };

        public static void SetTouchState(bool state)
        {
            try
            {
                // var test = DeviceWMI.GetPNPDevicesWithNames(TOUCH_SCREEN_NAMES);
                // MessageBox.Show(test.Count.ToString() + propertyName );
                var device = DeviceWMI.GetPNPDevicesWithNames(TOUCH_SCREEN_NAMES).FirstOrDefault();
                device?.SetDeviceEnabled(state);
            }
            catch (Exception)
            {
               
            }
        }
    }
}