using Device;
using System;
using System.Linq;
using System.Windows;

namespace SimpleJournal.Helper
{
    public static class TouchHelper
    {
        private static readonly string[] TOUCH_SCREEN_NAMES = new string[] { "touchscreen", "touch screen" };

        public static void SetTouchState(bool state)
        {
            try
            {
                var device = DeviceWMI.GetPNPDevicesWithNames(TOUCH_SCREEN_NAMES).FirstOrDefault();
                device?.SetDeviceEnabled(state);
            }
            catch
            {

            }
        }

        public static bool HasTouchscreen()
        {
            try
            {
                return DeviceWMI.GetPNPDevicesWithNames(TOUCH_SCREEN_NAMES).Any();
            }
            catch
            { }

            return false;
        }

        #region Powershell MECHANIC - OLD AND UNUSED

        public static void RunPowershellScript(bool state)
        {
            try
            {
                //System.Diagnostics.Process.Start("devcon.exe", @"/disable @HID\ELAN0732&COL01\4&2E44C06E&0&0000");
                string command = GeneratePowershellScriptForTouchscreen(Properties.Resources.strLang, state);
                System.Diagnostics.Process.Start("powershell.exe", command);
            }
            catch
            {

            }
        }

        public static void SetClipboardTextExecutionPolicy()
        {
            try
            {
                System.Windows.Forms.Clipboard.SetText("Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy Unrestricted");
            }
            catch { }
        }

        public static void GenerateBatchFiles()
        {
            using (System.Windows.Forms.FolderBrowserDialog flbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (flbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fileOn = System.IO.Path.Combine(flbd.SelectedPath, "Touch-On.bat");
                    string fileOff = System.IO.Path.Combine(flbd.SelectedPath, "Touch-Off.bat");

                    try
                    {
                        string batchCommand = "start powershell.exe \"{0}\"";

                        System.IO.File.WriteAllText(fileOn, string.Format(batchCommand, TouchHelper.GeneratePowershellScriptForTouchscreen(Properties.Resources.strLang, true)));
                        System.IO.File.WriteAllText(fileOff, string.Format(batchCommand, TouchHelper.GeneratePowershellScriptForTouchscreen(Properties.Resources.strLang, false)));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format(Properties.Resources.strMessageFailedToExportPsScripts, ex.Message), Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        public static string GeneratePowershellScriptForTouchscreen(string lang, bool on)
        {
            // German Version
            // Get-PnpDevice | Where-Object {$_.FriendlyName -like '*Touchscreen*'} | Disable-PnpDevice -Confirm:$false
            // Get-PnpDevice | Where-Object {$_.FriendlyName -like '*Touchscreen*'} | Enable-PnpDevice -Confirm:$false

            // English Version
            // Get-PnpDevice | Where-Object {$_.FriendlyName -like '*touch screen*'} | Disable-PnpDevice -Confirm:$false
            // Get-PnpDevice | Where-Object {$_.FriendlyName -like '*touch screen*'} | Enable-PnpDevice -Confirm:$false

            // Command needed to execute this in powershell (admin):
            // Set -ExecutionPolicy -Scope CurrentUser -ExecutionPolicy Unrestricted
            string friendlyName = (lang == "de" ? "Touchscreen" : "touch screen");
            string state = (on ? "Enable" : "Disable");
            return string.Format("Get-PnpDevice | Where-Object [$_.FriendlyName -like '*{0}*'] | {1}-PnpDevice -Confirm:$false", friendlyName, state).Replace("[", "{").Replace("]", "}");
        }
        #endregion
    }
}
