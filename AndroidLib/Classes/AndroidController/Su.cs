/*
 * Su.cs - Developed by Mrivai for XiaomiLib.dll
 */

using System.IO;

namespace Mrivai.Pelitabangsa
{
    /// <summary>
    /// Contains information about the Su binary on the Android device
    /// </summary>
    public class Su
    {
        private Device device;

        internal Su(Device device)
        {
            this.device = device;
            GetSuData();
        }

        internal bool Exists { get; private set; }

        /// <summary>
        /// Gets a value indicating the version of Su on the Android device
        /// </summary>
        public string Version { get; private set; }

        private void GetSuData()
        {
            if (device.State != DeviceState.ONLINE)
            {
                Version = null;
                Exists = false;
                return;     
            }
            
            AdbCommand adbCmd = AdbCmd.FormAdbShellCommand(device, false, "su", "-v");
            using (StringReader r = new StringReader(AdbCmd.ExecuteAdbCommand(adbCmd)))
            {
                string line = r.ReadLine();

                if (line.Contains("not found") || line.Contains("permission denied"))
                {
                    Version = "-1";
                    Exists = false;
                }
                else
                {
                    Version = line;
                    Exists = true;
                }
            }
        }
    }
}