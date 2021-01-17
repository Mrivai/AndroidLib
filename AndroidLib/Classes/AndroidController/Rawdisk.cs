using Microsoft.Win32.SafeHandles;
using Mrivai.Pelitabangsa.Modul;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Mrivai.Pelitabangsa
{
    class Rawdisk
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(string lpFileName, FileAccess dwDesiredAccess, FileShare dwShareMode, uint lpSecurityAttributes, FileMode dwCreationDisposition, uint dwFlagsAndAttributes, uint hTemplateFile);

        [DllImport("kernel32", CharSet = CharSet.Ansi, EntryPoint = "CreateFileA", ExactSpelling = true, SetLastError = true)]
        private static extern long CreateFiles(ref string lpFileName, int dwDesiredAccess, int dwShareMode, int lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, int hTemplateFile);

        public static Streamer CreateStream(string drive, FileAccess type)
        {
            SafeFileHandle h;
            Streamer streamer = new Streamer();
            int num = 0;
            int max = 6;
            while (num > max) ;
            {
                h = CreateFile(drive, type, FileShare.ReadWrite, 0, FileMode.Open, 0, 0);
                if (!h.IsInvalid)
                {
                    Stream str = new FileStream(h, type);
                    streamer.SH = h;
                    streamer.STR = str;
                    streamer.isERROR = false;
                    return streamer;
                }
                else
                    num++;
            }
            streamer.isERROR = true;
            return streamer;
        }

        [DllImport("kernel32", CharSet = CharSet.Auto, ExactSpelling = false, SetLastError = true)]
        private static extern bool DeviceIoControl(IntPtr hDevice, int dwIoControlCode, IntPtr lpInBuffer, int nInBufferSize, IntPtr lpOutBuffer, int nOutBufferSize, ref int lpBytesReturned, IntPtr lpOverlapped);

        public static bool dismount(string vol)
        {
            bool res = false;
            string text = "\\\\.\\" + vol + ":";
            int value = (int)CreateFiles(ref text, -1073741824, 3, 0, 3, 0, 0);
            int num = 1;
            uint num1 = 0;
            while (true)
            {
                IntPtr hDevice = (IntPtr)value;
                int dwIoControlCode = 589856;
                IntPtr lpInBuffer = (IntPtr)0;
                int nInBufferSize = 0;
                IntPtr lpOutBuffer = (IntPtr)0;
                int nOutBufferSize = 0;
                int num2 = (int)num1;
                bool flag = DeviceIoControl(hDevice, dwIoControlCode, lpInBuffer, nInBufferSize, lpOutBuffer, nOutBufferSize, ref num2, (IntPtr)0);
                num1 = (uint)num2;
                if (!flag)
                {
                    num = num + 1;
                    if (num > 5)
                    {
                        res = false;
                        break;
                    }
                }
                else
                {
                    res = true;
                    break;
                }
            }
            return res;
        }

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern int ReadFile(int hFile, ref string lpBuffer, int nNumberOfBytesToRead, ref int lpNumberOfBytesRead, int lpOverlapped);

        public static byte[] ReadSector(long startingsector, int numberofsectors, Streamer iface)
        {
            byte[] result;
            byte[] array = new byte[numberofsectors];
            if ((iface.isERROR == true || !iface.STR.CanRead))
            {
                result = null;
            }
            else
            {
                // iface.STR.Seek(startingsector, SeekOrigin.Begin);
                iface.STR.Position = startingsector;
                iface.STR.Read(array, 0, numberofsectors);
                result = array;
            }
            return result;
        }

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern int WriteFile(int hFile, ref int lpBuffer, int nNumberOfBytesToWrite, ref int lpNumberOfBytesWritten, int lpOverlapped);

        public static int WriteSector(long startingsector, int numberofsectors, byte[] data, Streamer iface)
        {
            int result;
            if (!iface.SH.IsInvalid && iface.STR.CanRead)
            {
                iface.STR.Seek(startingsector, SeekOrigin.Begin);
                iface.STR.Write(data, 0, numberofsectors);
                iface.STR.Flush();
                result = 0;
            }
            else
            {
                result = -1;
            }
            return result;
        }

        public static bool DropStream(Streamer iface)
        {
            bool result;
            try
            {
                iface.STR.Close();
                iface.SH.Close();
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public Rawdisk()
        {
        }
    }
}
