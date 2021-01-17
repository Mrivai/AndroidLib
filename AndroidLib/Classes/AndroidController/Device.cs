/*
 * Device.cs - Developed by Mrivai for XiaomiLib.dll
 */

using Mrivai.Pelitabangsa.Modul;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Media.Imaging;

namespace Mrivai.Pelitabangsa
{
    /// <summary>
    /// Manages connected Android device's info and commands
    /// </summary>
    public partial class Device
    {
        private int keys;
        private string recovery;
        private string rawprogram;
        private bool edl = false;
        private bool unlocked = false;
        private bool Arb = false;
        private DeviceState state;
        //adb backup
        private string adbcommand;
        private string status = null;
        private string swipecommand;
        private string tapcommand;
        private string inputcommand;
        private string FlashImgCmd;
        private string imgedl;
        private string EdlBackupCommand;
        private string EdlErasecommand;
        private FileStream fs;
        private StreamWriter sw;
        private string adbsideloadcmd;
        private string abootFile;
        private List<RawDrive> rawdrives = new List<RawDrive>();
        private RawDrive SelectedDrive = new RawDrive();

        /// <summary>
        /// Initializes a new instance of the Device class
        /// </summary>
        /// <param name="deviceSerial">Serial number of Android device</param>
        internal Device(string deviceSerial)
        {
            SerialNumber = deviceSerial;
            state = SetState();
            Update();
        }

        private DeviceState SetState()
        {
            getstate(AdbCmd.Devices());
            getstate(Fastboot.Devices());
            IsEdl();
            isISp();
            switch (status)
            {
                case "device":
                    return DeviceState.ADB;
                case "recovery":
                    return DeviceState.RECOVERY;
                case "fastboot":
                    OemInfo();
                    return DeviceState.FASTBOOT;
                case "sideload":
                    return DeviceState.SIDELOAD;
                case "unauthorized":
                    return DeviceState.UNAUTHORIZED;
                case "9008":
                    return DeviceState.EDL;
                case "900E":
                    return DeviceState.QDL;
                case "DISK":
                    return DeviceState.ISP;
                default:
                    return DeviceState.UNKNOWN;
            }
        }

        private void getstate(string source)
        {
            var line = "";
            using (StringReader r = new StringReader(source))
            {
                while (r.Peek() != -1)
                {
                    line = r.ReadLine();
                    if (line.Contains(SerialNumber))
                    {
                        status = line.Substring(line.IndexOf('\t') + 1);
                    }
                }
            }
        }

        private void isISp()
        {
            rawdrives.Clear();
            rawdrives = Emmc.Devices();
            foreach(RawDrive drive in rawdrives)
            {
                if (drive.ID.ToString().Contains(SerialNumber))
                {
                    SelectedDrive = drive;
                    status = "DISK";
                }
            }
        }

        private void IsEdl()
        {
            var res = "";
            var line = "";
            edl = false;
            var x = Edl.Devices();
            if (x.Length >= 0)
            {
                using (StringReader s = new StringReader(x))
                {
                    while (s.Peek() != -1)
                    {
                        line = s.ReadLine();
                        if (line.IndexOf("9008") != -1)
                        {
                            res = line.Substring(0, line.Length - 8);
                            status = res.Substring(line.IndexOf("9008"));
                            edl = true;
                            Edl.GetGPT();
                        }
                        else if (line.IndexOf("900E") != -1)
                        {
                            res = line.Substring(0, line.Length - 8);
                            status = res.Substring(line.IndexOf("900E"));
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Gets the device's <see cref="BatteryInfo"/> instance
        /// </summary>
        /// <remarks>See <see cref="BatteryInfo"/> for more details</remarks>
        public BatteryInfo Battery { get; private set; }

        /// <summary>
        /// path of programer file
        /// </summary>
        public string Programer { get { return AndroidController.Instance.ResourceDirectory + "prog_emmc_firehose_8976_ddr.mbn"; } }

        /// <summary>
        /// Gets the device's <see cref="BatteryInfo"/> instance
        /// </summary>
        /// <remarks>See <see cref="BatteryInfo"/> for more details</remarks>
        public Info DeviceInfo { get; private set; }
        /// <summary>
        /// Gets the device's <see cref="BuildProp"/> instance
        /// </summary>
        /// <remarks>See <see cref="BuildProp"/> for more details</remarks>
        public BuildProp BuildProp { get; private set; }

        /// <summary>
        /// Gets the device's <see cref="BusyBox"/> instance
        /// </summary>
        /// <remarks>See <see cref="BusyBox"/> for more details</remarks>
        public BusyBox BusyBox { get; private set; }

        /// <summary>
        /// Gets the device's <see cref="FileSystem"/> instance
        /// </summary>
        /// <remarks>See <see cref="FileSystem"/> for more details</remarks>
        public FileSystem FileSystem { get; private set; }

        ///// <summary>
        ///// Gets the device's <see cref="PackageManager"/> instance
        ///// </summary>
        ///// <remarks>See <see cref="PackageManager"/> for more details</remarks>
        //public PackageManager PackageManager { get { return packageManager; } }

        /// <summary>
        /// Gets the device's <see cref="Phone"/> instance
        /// </summary>
        /// <remarks>See <see cref="Phone"/> for more details</remarks>
        public Phones Phone { get; private set; }

        ///// <summary>
        ///// Gets the device's <see cref="Processes"/> instance
        ///// </summary>
        ///// <remarks>See <see cref="Processes"/> for more details</remarks>
        //public Processes Processes { get { return processes; } }

        /// <summary>
        /// Gets the device's <see cref="Su"/> instance
        /// </summary>
        /// <remarks>See <see cref="Su"/> for more details</remarks>
        public Su Su { get; private set; }

        /// <summary>
        /// Gets the device's serial number or physical drive index on isp mode
        /// </summary>
        public string SerialNumber { get; }

        /// <summary>
        /// Gets the device's Diag State
        /// </summary>
        public bool IsDiag { get { return checkDiagMode(); } }

        /// <summary>
        /// Gets the devices unlock state
        /// </summary>
        public bool IsUnlocked { get { OemInfo(); return unlocked; } }

        /// <summary>
        /// Gets the devices Arb Status
        /// </summary>
        public bool IsArbActive { get { ARBInfo(); return Arb; } }

        /// <summary>
        /// Gets a value indicating the device's current state
        /// </summary>
        /// <remarks>See <see cref="DeviceState"/> for more details</remarks>
        /// public DeviceState State { get { return state = SetState(); } internal set { state = value; } }
        public DeviceState State { get { return state= SetState(); } internal set{ state = value;} }

        /// <summary>
        /// Gets a value indicating if the device has root
        /// </summary>
        public bool HasRoot { get { return Su.Exists; } }
        /// <summary>
        /// chek if device's now in edl mode
        /// </summary>
        public bool isEdl { get { IsEdl(); return edl; } }
        
        
        /// <summary>
        /// Reboots the device regularly auto detec via adb or fastboot
        /// </summary>
        public void Reboot()
        {
            if (State == DeviceState.ADB || State == DeviceState.RECOVERY)
                new Thread(new ThreadStart(RebootThread)).Start();
            else if (State == DeviceState.FASTBOOT)
                new Thread(new ThreadStart(FastbootRebootThread)).Start();
            else if (State == DeviceState.EDL)
                new Thread(new ThreadStart(EdlRebootThread)).Start();
            else return;
        }

        private void RebootThread()
        {
            AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "reboot"));
        }


        private void FastbootRebootThread()
        {
            Fastboot.ExecuteFastbootCommandNoReturn(Fastboot.FormFastbootCommand(this, "reboot"));
        }

        private void EdlRebootThread()
        {
            Edl.ExecuteEdlCommand(Edl.EdlFlashCommand("-r"));
        }

        /// <summary>
        /// Remove Screen lock or Screen guard 
        /// root required
        /// </summary>
        public void RemoveScreenLock()
        {
            if (State == DeviceState.ADB)
                new Thread(new ThreadStart(RemoveScreenLockThread)).Start();
        }

        private void RemoveScreenLockThread()
        {
            if(state == DeviceState.ADB)
            {
                if (HasRoot)
                {
                    AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "su", "- c rm /data/system/*.key"));
                    AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "su", "- c rm /data/system/*.db"));
                    AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "su", "- c rm /data/system/*.db-shm"));
                    AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "su", "- c rm /data/system/*.db-wal"));
                }
            }
            else if(state == DeviceState.SIDELOAD)
            {
                var unlock = "sideload " + AndroidController.Instance.ResourceDirectory + "Key.zip";
                AdbCmd.ExecuteAdbCommand(AdbCmd.FormAdbCommand(unlock));
            }
        }

        /// <summary>
        /// Read Screen lock Gesture Key
        /// root required
        /// </summary>\
        public string Gesture_Pattern()
        {
            string gesture = "";
            if (state == DeviceState.ADB)
            {
                if (HasRoot)
                {
                    AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "su", "- c dd if=/data/system/gesture.key of=/sdcard/kv.key"));
                    AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "su", "- c pull /sdcard/kv.key"));
                    BinaryReader b = new BinaryReader(new FileStream(@"kv.key", FileMode.Open, FileAccess.Read, FileShare.None));
                    b.BaseStream.Position = 0x0;
                    byte[] data = b.ReadBytes(0x30);
                    b.Close();
                    string key = BitConverter.ToString(data).Replace("-", "").Replace(" ", "").Replace("-", "");

                    foreach (string item in File.ReadLines("Res\\Data.ini"))
                    {
                        if (item.Contains(key))
                        {
                            item.Replace(key, "");
                        }
                    }
                    gesture = key;
                }
            }
            return gesture;
        }


        /// <summary>
        /// RemoveFrp
        /// </summary>
        public void RemoveFrp()
        {
            if (State == DeviceState.ADB)
                new Thread(new ThreadStart(RemoveFrpThread)).Start();
        }

        private void RemoveFrpThread()
        {
            if (state == DeviceState.ADB)
            {
                AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, false, "", "content insert --uri content://settings/secure --bind name:s:user_setup_complete --bind value:s:1"));
            }
        }

        /// <summary>
        /// Reboots the device into EDL
        /// </summary>
        public void RebootEdl()
        {
            if (State == DeviceState.ADB || State == DeviceState.RECOVERY)
            {
                SetUbl();
                new Thread(new ThreadStart(AdbRebootEdlThread)).Start();
            }
            else if (State == DeviceState.FASTBOOT)
            {
                new Thread(new ThreadStart(FastbootRebootEdlThread)).Start();
            }
            else if (State == DeviceState.UNKNOWN)
            {
                return;
            }
        }

        private void AdbRebootEdlThread()
        {
            AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "reboot", "edl"));
        }

        private void FastbootRebootEdlThread()
        {
            Fastboot.EDL();
        }
        /// <summary>
        /// Reboots the device into recovery sideload
        /// </summary>
        public void RebootSideload()
        {
            new Thread(new ThreadStart(RebootSideloadThread)).Start();
        }

        private void RebootSideloadThread()
        {
            AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "reboot", "sideload"));
        }
        /// <summary>
        /// Reboots the device into recovery
        /// </summary>
        public void RebootRecovery()
        {
            new Thread(new ThreadStart(RebootRecoveryThread)).Start();
        }

        private void RebootRecoveryThread()
        {
            if (State == DeviceState.ADB)
                AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "reboot", "recovery"));
            else if (State == DeviceState.FASTBOOT)
                Fastboot.ExecuteFastbootCommandNoReturn(Fastboot.FormFastbootCommand(this, "reboot recovery"));

        }

        /// <summary>
        /// Reboots the device into the bootloader
        /// </summary>
        public void RebootBootloader()
        {
            new Thread(new ThreadStart(RebootBootloaderThread)).Start();
        }

        private void RebootBootloaderThread()
        {
            AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "reboot", "bootloader"));
        }

        /// <summary>
        /// Reboots the device into download mode
        /// </summary>
        public void RebootDownload()
        {
            new Thread(new ThreadStart(RebootDownloadThread)).Start();
        }

        private void RebootDownloadThread()
        {
            AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "reboot", "download"));
        }

        /// <summary>
        /// Pulls a file from the device
        /// </summary>
        /// <param name="fileOnDevice">Path to file to pull from device</param>
        /// <param name="destinationDirectory">Directory on local computer to pull file to</param>
        /// /// <param name="timeout">The timeout for this operation in milliseconds (Default = -1)</param>
        /// <returns>True if file is pulled, false if pull failed</returns>
        public bool PullFile(string fileOnDevice, string destinationDirectory, int timeout = Command.DEFAULT_TIMEOUT)
        {
            AdbCommand adbCmd = AdbCmd.FormAdbShellCommand(this, "pull", "\"" + fileOnDevice + "\"", "\"" + destinationDirectory + "\"");
            return (AdbCmd.ExecuteAdbCommandReturnExitCode(adbCmd.WithTimeout(timeout)) == 0);
        }

        /// <summary>
        /// Pushes a file to the device
        /// </summary>
        /// <param name="filePath">The path to the file on the computer you want to push</param>
        /// <param name="destinationFilePath">The desired full path of the file after pushing to the device (including file name and extension)</param>
        /// <param name="timeout">The timeout for this operation in milliseconds (Default = -1)</param>
        /// <returns>If the push was successful</returns>
        public bool PushFile(string filePath, string destinationFilePath, int timeout = Command.DEFAULT_TIMEOUT)
        {
            AdbCommand adbCmd = AdbCmd.FormAdbShellCommand(this, "push", "\"" + filePath + "\"", "\"" + destinationFilePath + "\"");
            return (AdbCmd.ExecuteAdbCommandReturnExitCode(adbCmd.WithTimeout(timeout)) == 0);
        }

        /// <summary>
        /// Pulls a full directory recursively from the device
        /// </summary>
        /// <param name="location">Path to folder to pull from device</param>
        /// <param name="destination">Directory on local computer to pull file to</param>
        /// <param name="timeout">The timeout for this operation in milliseconds (Default = -1)</param>
        /// <returns>True if directory is pulled, false if pull failed</returns>
        public bool PullDirectory(string location, string destination, int timeout = Command.DEFAULT_TIMEOUT)
        {
            AdbCommand adbCmd = AdbCmd.FormAdbShellCommand(this, "pull", "\"" + (location.EndsWith("/") ? location : location + "/") + "\"",  destination);
            var result = (AdbCmd.ExecuteAdbCommandReturnExitCode(adbCmd.WithTimeout(timeout)) == 0);
            return result;
        }

        /// <summary>
        /// Installs an application from the local computer to the Android device
        /// </summary>
        /// <param name="location">Full path of apk on computer</param>
        /// <param name="timeout">The timeout for this operation in milliseconds (Default = -1)</param>
        /// <returns>True if install is successful, False if install fails for any reason</returns>
        public bool InstallApk(string location, int timeout = Command.DEFAULT_TIMEOUT)
        {
            return !AdbCmd.ExecuteAdbCommand(AdbCmd.FormAdbShellCommand(this, "install", "\"" + location + "\"").WithTimeout(timeout), true).Contains("Failure");
        }

        /// <summary>
        /// Uninstalls an application from the local computer to the Android device
        /// </summary>
        /// <param name="PackageName">Full path of apk on computer</param>
        /// <param name="timeout">The timeout for this operation in milliseconds (Default = -1)</param>
        /// <returns>True if install is successful, False if install fails for any reason</returns>
        public bool UninstallApk(string PackageName, int timeout = Command.DEFAULT_TIMEOUT)
        {
            return !AdbCmd.ExecuteAdbCommand(AdbCmd.FormAdbShellCommand(this, "uninstall", "\"" + PackageName + "\"").WithTimeout(timeout), true).Contains("Failure");
        }

        /// <summary>
        /// Updates all values in current instance of <see cref="Device"/>
        /// </summary>
        public void Update()
        {
            if (state != DeviceState.EDL || state != DeviceState.FASTBOOT)
            {
                Su = new Su(this);
                Battery = new BatteryInfo(this);
                BuildProp = new BuildProp(this);
                BusyBox = new BusyBox(this);
                Phone = new Phones(this);
                FileSystem = new FileSystem(this);
                DeviceInfo = new Info(this);
            }
        }

        private void SetUbl()
        {
            if (state == DeviceState.ADB)
            {
                abootFile = string.Empty;
                if (abootFile == null)
                {
                    var v = DeviceInfo.MIuiVersion;
                    var a = DeviceInfo.AndroidVersion.Split('.');
                    var emmc = @"File\ubl\aboot_" + v + "_" + a[0] + ".mbn";
                    abootFile = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + emmc;
                }
            }
        }
        /// <summary>
        /// Unofficial Ubl
        /// </summary>
        public void UblUnofficial()
        {
            EdlFlasher(abootFile);
        }
        /// <summary>
        /// enable diagnostic mode
        /// </summary>
        public void EnableDiagMode()
        {
            AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, true, "setprop sys.usb.config diag,adb"));
        }
        /// <summary>
        /// disable diagnostic mode
        /// </summary>
        public void DisableDiagMode()
        {
            AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, true, "setprop sys.usb.config adb,mtp"));
        }
        /// <summary>
        /// check diagnostic mode
        /// </summary>
        private bool checkDiagMode()
        {
            bool diag;
            string sts = AdbCmd.ExecuteAdbCommand(AdbCmd.FormAdbCommand("shell getprop sys.usb.config"));
            string param = "diag,adb";
            if (Regex.Match(param, sts, RegexOptions.IgnoreCase | RegexOptions.Singleline).Success)
            {
                diag = true;
            }
            else
            {
                diag = false;
            }
            return diag;
        }


        /// <summary>
        /// oem unlock go command
        /// </summary>
        public void OemUnlock()
        {
            if (State == DeviceState.FASTBOOT)
                new Thread(new ThreadStart(OemUnlockThread)).Start();
            OemInfo();
        }
        private void OemUnlockThread()
        {
            Fastboot.ExecuteFastbootCommandNoReturn(Fastboot.FormFastbootCommand(this, "oem unlock-go"));
        }
        /// <summary>
        /// oem lock command
        /// </summary>
        public void Oemlock()
        {
            if (State == DeviceState.FASTBOOT)
                new Thread(new ThreadStart(OemlockThread)).Start();
        }
        private void OemlockThread()
        {
            Fastboot.ExecuteFastbootCommandNoReturn(Fastboot.FormFastbootCommand(this, "oem lock"));
        }
        /// <summary>
        /// oem device-info  to check devices is unlocked or not
        /// </summary>
        private void OemInfo()
        {
            string sts = Fastboot.info();
            string param = ".*?";
            param += "Device unlocked: true";
            Regex r = new Regex(param, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = r.Match(sts);
            if (m.Success)
            {
                unlocked = true;
            }
        }

        /// <summary>
        ///  ARB info to check devices Anti Roll Back is Active or Not
        /// </summary>
        private void ARBInfo()
        {
            string sts = Fastboot.ARB();
            if (sts.Contains("anti:"))
            {
                if (sts.Contains("4"))
                {
                    Arb = true;
                }
                else
                {
                    Arb = false;
                }
            }
        }

        /// <summary>
        /// boot device into temporary twrp
        /// </summary>
        /// <param name="img">Full path of recovery.img on computer</param>
        public void RebootTempRecovery(string img)
        {
            recovery = img;
            new Thread(new ThreadStart(RebootTempRecoveryThread)).Start();
        }

        private void RebootTempRecoveryThread()
        {
            Fastboot.ExecuteFastbootCommandNoReturn(Fastboot.FormFastbootCommand(this, "boot", recovery));
        }

        /// <summary>
        /// Reboots the device into recovery
        /// </summary>
        public void PowerOff()
        {
            new Thread(new ThreadStart(PowerOffThread)).Start();
        }

        private void PowerOffThread()
        {
            if (Su.Exists)
            {
                AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "su", "- c poweroff"));
            }
            else
            {
                AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "shell", "reboot -p"));
            }
            
        }

        /// <summary>
        /// enable data or gprs
        /// </summary>
        public void DataEnable()
        {
            new Thread(new ThreadStart(DataEnableThread)).Start();
        }

        private void DataEnableThread()
        {
            AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "svc", "data enable"));
        }
        /// <summary>
        /// disable data or gprs
        /// </summary>
        public void DataDisable()
        {
            new Thread(new ThreadStart(DataDisableThread)).Start();
        }

        private void DataDisableThread()
        {
            AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "svc", "data disable"));
        }
        /// <summary>
        /// Kill and Star Adb Server
        /// </summary>
        public void AdbRestart()
        {
            AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "kill-server"));
            AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "start-server"));
        }

        /// <summary>
        /// backup device via adb
        /// </summary>
        public void AdbBackup(bool Apk, bool Shared, bool Sistem, string Path)
        {
            var fld = " -f \"" + Path + "\"";
            var apk = " -noapk";
            var shared = " -noshared";
            var all = " -all";
            var system = " -nosystem";
            if (Apk == true) { apk = " -apk"; }
            if (Shared == true) { shared = " -shared"; }
            if (Sistem == true) { system = " -system"; }
            adbcommand = "backup" + all + shared + apk  + system + fld;
            Logger.w(adbcommand, "AdbBackup devices.cs", "");
            new Thread(new ThreadStart(AdbBackupThread)).Start();
        }

        private void AdbBackupThread()
        {
            AdbCmd.ExecuteAdbCommandExitWait(AdbCmd.FormAdbCommand(adbcommand));
        }
        /// <summary>
        /// restore device via adb
        /// </summary>
        public void AdbRestore(string Path)
        {
            adbcommand = "restore " + Path;
             new Thread(new ThreadStart(AdbRestoreThread)).Start();
        }

        private void AdbRestoreThread()
        {
            AdbCmd.ExecuteAdbCommand(AdbCmd.FormAdbCommand(adbcommand));
        }

        /// <summary>
        /// sideload device via adb
        /// </summary>
        public void Sideload(string file)
        {
            if (State == DeviceState.SIDELOAD)
            {
                adbsideloadcmd = "sideload " + @file;
            }
            else if (State == DeviceState.ADB)
            {
                adbsideloadcmd = "reboot sideload " + @file;
            }
            
            new Thread(new ThreadStart(SideloadThread)).Start();
        }
        /// <summary>
        /// sideload device via adb
        /// </summary>
        
        private void SideloadThread()
        {
            AdbCmd.ExecuteAdbCommand(AdbCmd.FormAdbCommand(adbsideloadcmd));
        }

        /// <summary>
        /// insert or write text into devices
        /// </summary>
        public void InputText(string text)
        {
            inputcommand = text;
            new Thread(new ThreadStart(InputTextThread)).Start();
        }

        private void InputTextThread()
        {
            AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, false, "input", "text", inputcommand));
        }

        /// <summary>
        /// Press button devices
        /// </summary>
        public void PressButton(KeyEventCode key)
        {
            keys = (int)key;
            new Thread(new ThreadStart(PressButtonThread)).Start();
        }

        private void PressButtonThread()
        {
            var res = AdbCmd.ExecuteAdbCommand(AdbCmd.FormAdbShellCommand(this, false, "input", "keyevent", keys));
        }

        /// <summary>
        /// tap devices screen
        /// </summary>
        public void Tap(string x, string y)
        {
            tapcommand = x + " " + y;
            new Thread(new ThreadStart(TapThread)).Start();
        }

        private void TapThread()
        {
            AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, false, "input", "tap", tapcommand));
        }

        /// <summary>
        /// swipe devices screen
        /// </summary>
        public void Swipe(string p, string q, string r, string s)
        {
            swipecommand = p + " " + q + " " + r + " " + s;
            new Thread(new ThreadStart(SwipeThread)).Start();
        }

        private void SwipeThread()
        {
            AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, false, "input", "swipe", swipecommand));
        }
        /// <summary>
        /// swipe devices screen
        /// </summary>
        public Bitmap GetScreenAsync()
        {
            bool ss;
            var file = @AndroidController.Instance.ResourceDirectory + Guid.NewGuid() + ".raw";
            BitmapImage bitmap = new BitmapImage();
            ss = AdbCmd.GetSS(file);
            if (ss)
            {
                BinaryReader binReader = new BinaryReader(File.Open(file, FileMode.Open));
                FileInfo fileInfo = new FileInfo(file);
                byte[] bytes = binReader.ReadBytes((int)fileInfo.Length);
                binReader.Close();

                // Init bitmap
                
                bitmap.BeginInit();
                bitmap.StreamSource = new MemoryStream(bytes);
                bitmap.EndInit();
                if (File.Exists(file)) { File.Delete(file); }
            }
            return bitob(bitmap);
            //return AdbCmd.GetSS();
            //return AndroidController.Instance.ResourceDirectory + "SS.png";
        }
        
        /// <summary>
        /// star minicap server
        /// </summary>
        public void StartMinicapServer()
        {
            new Thread(new ThreadStart(StartMinicapServerThread)).Start();
        }
        /// <summary>
        /// check if minicap server is started or not
        /// </summary>
        public bool IsMiniCapStart()
        {
            var x = AdbCmd.ExecuteAdbCommand(AdbCmd.FormAdbCommand("adb forward --list"));
            if (x.Length> 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void StartMinicapServerThread()
        {
            AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbCommand("forward--remove -all"));
            AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbCommand("forward --no-rebind tcp:1313 localabstract:minicap"));
            AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbCommand("shell LD_LIBRARY_PATH=/data/local/tmp /data/local/tmp/minicap -P 1080x1920@1080x1920/0"));
            //AdbCmd.ExecuteAdbCommandNoReturn(AdbCmd.FormAdbShellCommand(this, "LD_LIBRARY_PATH=/data/local/tmp /data/local/tmp/minicap -P 1080x1920@1080x1920/0"));
        }
        /// <summary>
        /// star minicap server
        /// </summary>
        private void installMinicap()
        {
            var path = "/data/local/tmp";
            var abi = AdbCmd.ExecuteAdbCommand(AdbCmd.FormAdbCommand("shell getprop ro.product.cpu.abi"));
            var sdk = AdbCmd.ExecuteAdbCommand(AdbCmd.FormAdbCommand("shell getprop ro.build.version.sdk"));
            var minicapfile = AppDomain.CurrentDomain.SetupInformation.ApplicationBase  + string.Format("Lib/minicap/bin/{0}/minicap", abi);
            var minicaplib = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + string.Format("Lib/minicap/shared/android-{0}/{1}/minicap.so", sdk, abi);
            var minitouch = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + string.Format("Lib/minitouch/{0}/minitouch", abi);
            PushFile(minicapfile, path);
            PushFile(minicaplib, path);
            AdbCmd.ExecuteAdbCommand(AdbCmd.FormAdbCommand("shell chmod 777 "+path+"/minicap"));
        }

        /// <summary>
        /// flashing Android device in edl mode using rawprogram0.xml
        /// </summary>
        /// <remarks><paramref name="XmlFile"/> should be a raw path of a swFirmware (C:\xiaomi\rawprogram0.xml)</remarks>
        /// <param name="XmlFile">path of rawprogram0.xml</param>

        public void EdlRawFlasher(string XmlFile)
        {
            string rawxml = "-x ";
            if (State == DeviceState.EDL)
            {
                rawxml += XmlFile;
                rawprogram = rawxml;
                new Thread(new ThreadStart(EdlRawFlasherThread)).Start();
            }
        }

        private void EdlRawFlasherThread()
        {
            Edl.ExecuteEdlCommand(Edl.EdlFlashCommand(rawprogram));
        }
        /// <summary>
        /// flashing Android img, mbn, bin in edl mode
        /// </summary>
        ///<remarks><paramref name="img"/> should be a raw path of file(img,mbn,bin) example (C:\xiaomi\boot.img)</remarks>
        /// <param name="img">path of file</param>
        public void EdlFlasher(string img)
        {
            var Parts = new Dictionary<string, string[]>();
            var flist = new List<string>();
            Parts.Add("boot.img", new string[] { "boot" });
            Parts.Add("cache.*|cache", new string[] { "cache" });
            Parts.Add("cust.*|cust", new string[] { "cust" });
            Parts.Add("emmc_appsboot.mbn|aboot|abootbak", new string[] { "aboot", "abootbak" });
            Parts.Add("NON-HLOS.bin|modem.img", new string[] { "modem" });
            Parts.Add("modemst1.*|modemst1", new string[] { "modemst1" });
            Parts.Add("modemst2.*|modemst2", new string[] { "modemst2" });
            Parts.Add("persist.*|persist", new string[] { "persist" });
            Parts.Add("recovery.*|twrp|recovery", new string[] { "recovery" });
            Parts.Add("splash.*|splash", new string[] { "splash" });
            Parts.Add("system.*|system", new string[] { "system" });
            Parts.Add("userdata.*|userdata", new string[] { "userdata" });

            foreach (string regex in Parts.Keys)
            {
                if (Regex.Match(img, "^" + regex).Success)
                {
                    foreach (string partisi in Parts[regex])
                    {
                        var x = "-b ";
                        x += partisi + " " + img;
                        flist.Add(x);
                        x = null;
                    }
                }
            }
            using (List<string>.Enumerator enumerator = flist.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    imgedl = enumerator.Current;
                    new Thread(new ThreadStart(EdlFlasherThread)).Start();
                }
            }
        }

        private void EdlFlasherThread()
        {
            var res = Edl.ExecuteEdlCommand(Edl.EdlFlashCommand(imgedl));
            Logger.w(res, "EDL IMG FLASHER Thread", null);
        }
        
        /// <summary>
        /// Backup partition via edl
        /// </summary>
        /// <remarks><paramref name="partname"/> should be a device partition name</remarks>
        /// <remarks><paramref name="folder"/> should be a path of partition file</remarks>
        /// <param name="partname">bootimg</param>
        /// <param name="folder">C:\xiaomi\</param>
        public void EdlBackup(string partname, string folder)
        {
            EdlBackupCommand = "-d " + partname + " -o " +"\""+folder +"\\"+ partname+ ".img\"";
            new Thread(new ThreadStart(EdlBackupThread)).Start();
        }
        
        private void EdlBackupThread()
        {
            var res = Edl.ExecuteEdlCommand(Edl.EdlFlashCommand(EdlBackupCommand));
            Logger.w(res, "EDL IMG Backup Thread", null);
        }
        /// <summary>
        /// Backup selected list partition 
        /// </summary>
        /// <remarks><paramref name="Backuplist"/>list of partition</remarks>
        /// <remarks><paramref name="folder"/> should be a path of partition file</remarks>
        /// <param name="Backuplist">list of partition</param>
        /// <param name="folder">C:\xiaomi\</param>
        public void EdlBackupList(List<string> Backuplist, string folder)
        {
            var path = @folder + "\\images\\";
            var rawfile = @path + "\\rawprogram0.xml";
            if (!(new DirectoryInfo(path)).Exists)
            {
                Directory.CreateDirectory(path);
                if (File.Exists(rawfile)) { File.Delete(rawfile); }
                File.Copy(Programer, path + "prog_emmc_firehose_8976_ddr.mbn");
                CreateRawBackup(Backuplist, rawfile);
            }
            
            foreach (string partisi in Backuplist)
            {
                //EdlBackupCommand = "-d " + partisi + " -o " + @path + partisi + ".img";
                //Logger.w(EdlBackupCommand, "partisi edlbackup list", null);
                Edl.ExecuteEdlCommand(Edl.EdlFlashCommand("-d " + partisi + " -o " + @path + partisi + ".img"));
                //new Thread(new ThreadStart(EdlBackupThread)).Start();
            }
        }

        private void CreateRawBackup(List<string> Backuplist, string rawfile)
        {
            var rawProgram = "<?xml version=\"1.0\" ?>\n";
            rawProgram += "<data>\n";
            List<Gpt> parts = new List<Gpt>();
            parts = Edl.partition;
            
            using (List<string>.Enumerator enumerator = Backuplist.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    foreach (Gpt part in parts)
                    {
                        if (enumerator.Current == part.name)
                        {
                            int kb = Convert.ToInt32(part.end) / 2;
                            var dec = Convert.ToInt32(part.start) * 2;
                            var hex = string.Format("{0:x}", dec);
                            rawProgram += string.Format("  <program SECTOR_SIZE_IN_BYTES=\"512\" file_sector_offset=\"0\" filename=\"{0}.img\" label=\"{0}\" num_partition_sectors=\"{2}\" physical_partition_number=\"0\" size_in_KB=\"{3}.0\" sparse=\"false\" start_byte_hex=\"0x{4}00L\" start_sector=\"{1}\"/>\n", part.name, part.start, part.end, kb, hex);
                        }
                    }
                }
            }
            rawProgram += "</data>";
            if (!File.Exists(rawfile))
            {
                File.Create(rawfile).Close();
            }
            fs = new FileStream(rawfile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            sw = new StreamWriter(fs);
            sw.WriteLine(rawProgram);
            sw.Close();
            fs.Close();
        }

        /// <summary>
        /// format Android partision in edl mode
        /// </summary>
        /// <remarks><paramref name="partname"/> should be a device partition name</remarks>
        /// <param name="partname">persist</param>
        public void EdlErase(string partname)
        {
            EdlErasecommand = "-e " + partname;
            new Thread(new ThreadStart(EdlEraseThread)).Start();
        }

        private void EdlEraseThread()
        {
            Edl.ExecuteEdlCommandNoReturn(Edl.EdlFlashCommand(EdlErasecommand));
        }
        /// <summary>
        /// Make RawProgram0.xml
        /// </summary>
        /// <remarks><paramref name="path"/> should be a device partition name</remarks>
        /// <param name="path">persist</param>
        public void MakeRaw(string path)
        {
            var fullpath = path + "rawprogram0.xml";
            if (!File.Exists(fullpath))
            {
                File.Create(fullpath).Close();
            }
            fs = new FileStream(fullpath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            sw = new StreamWriter(fs);
            sw.WriteLine(Edl.MakeRawProgram());
            sw.Close();
            fs.Close();
        }
        /// <summary>
        /// flashing img file via fastboot
        /// </summary>
        /// <remarks><paramref name="file"/> should be a full path img file name</remarks>
        ///<param name="file">C:/recovery.img</param>
        public void FastbootFlashImg(string file)
        {
            var Parts = new Dictionary<string, string[]>();
            Parts.Add("boot.*|boot", new string[] { "boot" });
            Parts.Add("cache.*|cache", new string[] { "cache" });
            Parts.Add("cust.*|cust", new string[] { "cust" });
            Parts.Add("emmc_appsboot.mbn|aboot.*|aboot|aboot*", new string[] { "aboot, abootbak" });
            Parts.Add("NON-HLOS.*|modem.*|modem", new string[] { "modem" });
            Parts.Add("modemst1.*|modemst1", new string[] { "modemst1" });
            Parts.Add("modemst2.*|modemst2", new string[] { "modemst2" });
            Parts.Add("persist.*|persist", new string[] { "persist" });
            Parts.Add("recovery.*|twrp|recovery", new string[] { "recovery" });
            Parts.Add("splash.*|splash", new string[] { "splash" });
            Parts.Add("system.*|system", new string[] { "system" });
            Parts.Add("userdata.*|userdata", new string[] { "userdata" });

            foreach (string regex in Parts.Keys)
            {
                if (Regex.Match(file, regex, RegexOptions.IgnoreCase| RegexOptions.Singleline).Success)
                {
                    //Parts[keys] = "kernel", "boot" 
                    foreach (string partisi in Parts[regex])
                    {
                        //flash method disini 
                        FlashImgCmd = partisi + " " + file;
                        new Thread(new ThreadStart(FastbootFlashImgThread)).Start();

                    }
                }
            }
        }

        private void FastbootFlashImgThread()
        {
            Fastboot.ExecuteFastbootCommandNoReturn(Fastboot.FormFastbootCommand(this, "flash", FlashImgCmd));
        }
        /// <summary>
        /// erase specifik partition over fastboot mode
        /// </summary>
        public void FastbootErase(string file)
        {
            Fastboot.ExecuteFastbootCommandNoReturn(Fastboot.FormFastbootCommand(this, "erase", file));
        }
        /// <summary>
        /// send command based current device state
        /// </summary>
        public string Commander(string command)
        {
            var res = "";
            if (State == DeviceState.ADB || State == DeviceState.RECOVERY)
            {
                if(command == null) { command = "adb"; }
                res = AdbCmd.ExecuteAdbCommand(AdbCmd.FormAdbCommand(command));
            }
            else if (State == DeviceState.FASTBOOT)
            {
                if (command == null) { command = "fastboot"; }
                res = Fastboot.ExecuteFastbootCommand(Fastboot.FormFastbootCommand(command));
            }
            else if (State == DeviceState.EDL)
            {
                if (command == null) { command = "emmcdl"; }
                res = Edl.ExecuteEdlCommand(Edl.EdlFlashCommand(command));
            }
            return res;
        }

        /// <summary>
        /// bypass frp helper
        /// </summary>
        public void BypassFrpHelper(string vid, string pid, string link)
        {
            AdbCmd.ExecuteLinuxAdkCommand(AdbCmd.FormAdbShellCommand(this, "-d", string.Concat(new string[] { vid, ":", pid, " -D GST_BYPASS_FRP_HELPER -m ANDROID -M OS -u https://www.youtube.com/account_privacy" })));
        }


        private Bitmap bitob(BitmapImage bi)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BmpBitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bi, null, null, null));
                enc.Save(ms);
                Bitmap b = new Bitmap(ms);
                return new Bitmap(b);
            }
        }

        private Image UpDisp(string p)
        {
            Bitmap bm;
            Image imgs;

            using (imgs = Image.FromFile(p))
            {
                bm = new Bitmap(imgs);
            }
            return bm;
        }
    }
}