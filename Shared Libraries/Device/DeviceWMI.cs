using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace Device
{
    public class DeviceWMI
    {
        public string Name { get; set; }

        public string ClassGUID { get; set; }

        public List<string> HardwareID { get; set; }

        public string DeviceID { get; set; }

        public string PnpDeviceID { get; set; }

        public DeviceWMI()
        { }

        public DeviceWMI(string name, string classGuid, List<string> hardwareID, string deviceID, string pnpDeviceID)
        {
            Name = name;
            ClassGUID = classGuid;
            HardwareID = hardwareID;
            DeviceID = deviceID;
            PnpDeviceID = pnpDeviceID;
        }

        public override string ToString()
        {
            return $"Name: {Name}{Environment.NewLine}GUID: {ClassGUID}{Environment.NewLine}Device ID: {DeviceID}";
        }

        public static List<DeviceWMI> GetPNPDevicesWithNames(string[] names)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity");
            List<DeviceWMI> devices = new List<DeviceWMI>();

            foreach (ManagementObject queryObj in searcher.Get())
            {
                string name = queryObj["Name"]?.ToString();

                if (!string.IsNullOrEmpty(name) && names.Any(p => name.ToLower().Contains(p.ToLower())))
                {
                    string guid = queryObj["ClassGuid"].ToString();
                    var hardwareID = (string[])queryObj["HardwareID"];
                    string deviceID = queryObj["DeviceID"].ToString();
                    string pnpDeviceID = queryObj["PNPDeviceID"].ToString();

                    devices.Add(new DeviceWMI(name, guid, hardwareID.ToList(), deviceID, pnpDeviceID));
                }
            }

            return devices;
        }
    }
}