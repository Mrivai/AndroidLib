using System;
using System.Collections.Generic;
using System.Threading;

namespace Mrivai.Pelitabangsa
{
    /// <summary>
    /// Manages connected Android device's installed App 
    /// </summary>
    public class Apps
    {
        /// <summary>
        /// Apps Name
        /// </summary>
        public string name;
        /// <summary>
        /// Package Name
        /// </summary>
        public string packagename;
        private static List<Apps> Applist;

        internal Apps(string nm, string pm)
        {
            name = nm;
            packagename = pm;
        }
        /// <summary>
        /// Get list Of installed App on connected device
        /// </summary>
        public static List<Apps> getlist()
        {
            Applist = new List<Apps>();
            Applist.Clear();
            new Thread(new ThreadStart(GetInstaledAppsThread)).Start();
            return Applist;
        }

        private static void GetInstaledAppsThread()
        {
            string sts = AdbCmd.ExecuteAdbCommand(AdbCmd.FormAdbCommand("shell pm list packages -3"));
            string[] m = sts.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string z in m)
            {
                var name = z.Replace("package:", "");
                var pm = name;
                name = name.Replace("com.", "").Replace(".", " ");
                Applist.Add(new Apps(name, pm));
            }
        }
    }
}
