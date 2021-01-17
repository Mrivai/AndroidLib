using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Mrivai.Pelitabangsa.Modul;

namespace Mrivai.Pelitabangsa
{
    // <summary>
    /// Holds formatted commands to execute through <see cref="Emmc"/>
    /// </summary>
    /// <remarks><para>Can only be created with <c>Emmc.FormEmmcCommand()</c></para>
    /// <para>Can only be executed with <c>Emmc.ExecuteEmmcCommand()</c> or <c>Emmc.ExecuteEmmcCommandNoReturn()</c></para></remarks>
    public class EmmcCommand
    {
        internal string Command { get; }
        internal int Timeout { get; private set; }
        internal EmmcCommand(string command) { Command = command; Timeout = Mrivai.Command.DEFAULT_TIMEOUT; }

        /// <summary>
        /// Sets the timeout for the EmmcCommand
        /// </summary>
        /// <param name="timeout">The timeout for the command in milliseconds</param>
        public EmmcCommand WithTimeout(int timeout) { Timeout = timeout; return this; }
    }

    /// <summary>
    /// Controls all commands sent to Emmc
    /// </summary>
    public static class Emmc
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(string lpFileName, FileAccess dwDesiredAccess, FileShare dwShareMode, uint lpSecurityAttributes, FileMode dwCreationDisposition, uint dwFlagsAndAttributes, uint hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, ref long lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);


        /// <summary>
        /// Executes a Emmc device Command, for checking connected android device
        /// </summary>
        internal static List<RawDrive> Devices()
        {
            List<RawDrive> rawdrives = new List<RawDrive>();
            WqlObjectQuery query = new WqlObjectQuery("SELECT * FROM Win32_DiskDrive");
            ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(query);
            int num = 0;
            foreach (ManagementBaseObject managementBaseObject in managementObjectSearcher.Get())
            {
                ManagementObject managementObject = (ManagementObject)managementBaseObject;
                if (managementObject["MediaType"] != null )
                {
                    if(managementObject["MediaType"].ToString().Contains("Removable") || managementObject["Model"].ToString().Contains("Qualcomm MMC Storage USB Device"))
                    {
                        RawDrive rawdrive = new RawDrive();
                        rawdrive.index = num;
                        rawdrive.ID = managementObject["DeviceID"].ToString().Replace("\\\\.\\", string.Empty);
                        rawdrive.Size = EmmcSize(managementObject["DeviceID"].ToString());
                        rawdrive.Description = managementObject["Description"].ToString();
                        rawdrive.Manufacturer = managementObject["Manufacturer"].ToString();
                        rawdrive.MediaType = managementObject["MediaType"].ToString();
                        rawdrive.Model = managementObject["Model"].ToString();
                        rawdrives.Add(rawdrive);
                        num++;
                    }
                }
            }
            return rawdrives;
        }

        public static long EmmcSize(string drive)
        {
            SafeFileHandle h = CreateFile(drive, FileAccess.ReadWrite, FileShare.None, 0U, FileMode.Open, 0U, 0U);
            long diskSize = GetDiskSize(h);
            h.Close();
            return diskSize;
        }

        public static long GetDiskSize(SafeFileHandle Handle)
        {
            long num = 0L;
            uint num2;
            long result;
            if (DeviceIoControl(Handle.DangerousGetHandle(), 475228U, IntPtr.Zero, 0U, ref num, 8U, out num2, IntPtr.Zero))
            {
                result = num;
            }
            else
            {
                result = 0L;
            }
            return result;
        }

        /// <summary>
        /// Forms a <see cref="EmmcCommand"/> that is passed to <c>Emmc.ExecuteEmmcCommand()</c>
        /// </summary>
        /// <param name="command">The Emmc command to run</param>
        /// <param name="args">Any arguments that need to be sent to <paramref name="command"/></param>
        /// <returns><see cref="EmmcCommand"/> that contains formatted command information</returns>
        /// <remarks>Should be used only for non device-specific Emmc commands such as <c>Emmc devices</c> or <c>Emmc version</c></remarks>
        /// <example>This example demonstrates how to create a non device-specific <see cref="EmmcCommand"/>
        /// <code>//This example shows how to create a non device-specific EmmcCommand
        /// //This demonstarates the Emmc command "Emmc devices"
        /// //Notice how you do not include the "Emmc" executable in the method, as the method takes care of it internally
        /// 
        /// EmmcCommand fbCmd = Emmc.FormEmmcCommand("devices");
        /// 
        /// </code>
        /// </example>
        public static EmmcCommand FormEmmcCommand(string command, params string[] args)
        {
            string fbCmd = (args.Length > 0) ? command + " " : command;

            for (int i = 0; i < args.Length; i++)
                fbCmd += args[i] + " ";
            return new EmmcCommand(fbCmd);
        }

        /// <summary>
        /// Forms a <see cref="EmmcCommand"/> that is passed to <c>Emmc.ExecuteEmmcCommand()</c>
        /// </summary>
        /// <remarks>Should be used only for device-specific Emmc commands such as <c>Emmc reboot</c> or <c>Emmc getvar all</c></remarks>
        /// <param name="device">Specific <see cref="Device"/> to run the comand on</param>
        /// <param name="command">The command to run on Emmc</param>
        /// <param name="args">Any arguments that need to be sent to <paramref name="command"/></param>
        /// <returns><see cref="EmmcCommand"/> that contains formatted command information</returns>
        /// <example>This example demonstrates how to create a non device-specific <see cref="EmmcCommand"/>
        /// <code>//This example shows how to create a device-specific EmmcCommand
        /// //This demonstarates the Emmc command "Emmc flash zip C:\rom.zip"
        /// //Notice how you do not include the "Emmc" executable in the method, as the method takes care of it internally
        /// //This example also assumes there is an instance of Device named device
        /// 
        /// EmmcComand fbCmd = Emmc.FormEmmcCommand(device, "flash", @"zip C:\rom.zip");
        /// 
        /// </code>
        /// </example>
        public static EmmcCommand FormEmmcCommand(Device device, string command, params string[] args)
        {
            string fbCmd = "-s " + device.SerialNumber + " ";

            fbCmd += (args.Length > 0) ? command + " " : command;

            for (int i = 0; i < args.Length; i++)
                fbCmd += args[i] + " ";

            return new EmmcCommand(fbCmd);
        }

        /*

        /// <summary>
        /// Executes a <see cref="EmmcCommand"/>
        /// </summary>
        /// <param name="command">Instance of <see cref="EmmcCommand"/></param>
        /// <returns>Output of <paramref name="command"/> run in Emmc</returns>
        public static string ExecuteEmmcCommand(EmmcCommand command)
        {
            return Command.RunProcessReturnOutput(XiaomiController.Instance.ResourceDirectory + Emmc_EXE, command.Command, command.Timeout);
        }

        /// <summary>
        /// Executes a <see cref="EmmcCommand"/>
        /// </summary>
        /// <remarks>Should be used if you do not want the output of the command; good for quick Emmc commands</remarks>
        /// <param name="command">Instance of <see cref="EmmcCommand"/></param>
        public static void ExecuteEmmcCommandNoReturn(EmmcCommand command)
        {
            Command.RunProcessNoReturn(XiaomiController.Instance.ResourceDirectory + Emmc_EXE, command.Command, command.Timeout);
        }
        */

    }
}
