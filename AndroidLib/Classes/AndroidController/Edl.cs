
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Mrivai.Pelitabangsa
{
    /// <summary>
    /// Holds formatted commands to execute through <see cref="EdlCommand"/>
    /// </summary>
    /// <remarks><para>Can only be created with <c>Edl.EdlCommand()</c></para>
    /// <para>Can only be executed with <c>Edl.ExecuteEdlCommand()</c> or <c>Edl.ExecuteEdlCommandNoReturn()</c></para></remarks>
    public class EdlCommand
    {
        internal string Command { get; }
        internal int Timeout { get; private set; }
        internal EdlCommand(string command) { Command = command; Timeout = Mrivai.Command.DEFAULT_TIMEOUT; }

        /// <summary>
        /// Sets the timeout for the EdlCommand
        /// </summary>
        /// <param name="timeout">The timeout for the command in milliseconds</param>
        public EdlCommand WithTimeout(int timeout)
        {
            Timeout = timeout;
            return this;
        }
    }

    /// <summary>
    /// class to controll device from edl mode
    /// </summary>
    public static class Edl
    {
        private static object _lock = new object();
        private const string EDL_EXE = "emmcdl.exe";
        private const string COM_EXE = "lsusb.exe";
        /// <summary>
        /// contains list devices partition
        /// </summary>
        public static List<Gpt> partition = new List<Gpt>();
        private static string[] gptraw;
        private static Loaders Loader = new Loaders();
        /// <summary>
        /// contain raw program of partition
        /// </summary>
        public static string rawProgram;

        public static string Size_LBA { get; private set; }
        public static string Start_LBA { get; private set; }


        /// <summary>
        /// command lsusb.exe
        /// </summary>
        public static EdlCommand FormLsCommand(string command, params string[] args)
        {
            string fbCmd = (args.Length > 0) ? command + " " : command;

            for (int i = 0; i < args.Length; i++)
                fbCmd += args[i] + " ";
            return new EdlCommand(fbCmd);
        }
        /// <summary>
        /// command emmcdl.exe
        /// </summary>
        public static EdlCommand EdlFlashCommand(string command, params string[] args)
        {
            var comand = "-p " + Port() + " -f " + @AndroidController.Instance.ResourceDirectory + "prog_emmc_firehose_8976_ddr.mbn ";
            string fbCmd = (args.Length > 0) ? command + " " : command;
            for (int i = 0; i < args.Length; i++)
                fbCmd += args[i] + " ";
            string cmd = comand + fbCmd;
            Logger.w(cmd, "miudl command", null);
            return new EdlCommand(cmd);
        }

        /// <summary>
        /// execute lsusb.exe command
        /// </summary>
        public static string ExecuteCOMCommand(EdlCommand command)
        {
            return Command.RunProcessReturnOutput(@AndroidController.Instance.ResourceDirectory + COM_EXE, command.Command, command.Timeout);
        }

        /// <summary>
        /// execute emmcdl.exe command return output
        /// </summary>
        public static string ExecuteEdlCommand(EdlCommand command)
        {
            string result = "";
            lock (_lock)
            {
                result = Command.RunProcessReturnOutput(@AndroidController.Instance.ResourceDirectory + EDL_EXE, command.Command, command.Timeout);
            }
            return result;
        }
        /// <summary>
        /// execute emmcdl.exe command no return
        /// </summary>
        public static void ExecuteEdlCommandNoReturn(EdlCommand command)
        {
            lock (_lock)
            {
                Command.RunProcessNoReturn(@AndroidController.Instance.ResourceDirectory + EDL_EXE, command.Command, command.Timeout);
            }
        }


        internal static string Devices()
        {
            return ExecuteCOMCommand(FormLsCommand("qcLsUsb"));
        }

        internal static string Port()
        {
            string port = "";
            //string[] strArray = Regex.Split(new Cmd("").Execute(null, qcLsUsb), "\r\n");
            string[] strArray = Regex.Split(Devices(), "\r\n");
            for (int index = 0; index < strArray.Length; ++index)
            {
                if (!string.IsNullOrEmpty(strArray[index]) && strArray[index].IndexOf("900") > 0)
                {
                    string str = strArray[index].Split('(')[1].Replace(')', ' ');
                    port += str.Trim();
                }
            }
            return port;
        }

        internal static string MsmHwId()
        {
            var info = ExecuteEdlCommand(EdlFlashCommand("-info > "));
            StringReader stringReader = new StringReader(info);
            while (stringReader.Peek() != -1)
            {
                info = stringReader.ReadLine();
                if (info.Contains("MSM_HW_ID:"))
                {
                    string str3 = info.Replace("MSM_HW_ID:", "").Replace(" ", "").Replace("(", "").Replace(")", "");
                    info = str3;
                }
            }
            return info;
        }

        internal static string OemPkHash()
        {
            var info = ExecuteEdlCommand(EdlFlashCommand("-info > "));
            StringReader stringReader = new StringReader(info);
            while (stringReader.Peek() != -1)
            {
                info = stringReader.ReadLine();
                if (info.Contains("OEM_PK_HASH:"))
                {
                    string str3 = info.Replace("OEM_PK_HASH:", "").Replace(" ", "").Replace("(", "").Replace(")", "");
                    info = str3;
                }
            }
            return info;
        }

        internal static string OemBrand()
        {
            string Result = "Undefined";
            string OemHashs = OemPkHash();
            foreach (MerekHP M in Loader.ListLoader)
            {
                foreach (OemHash H in M.Hashs)
                {
                    if(H.Hash == OemHashs)
                    {
                        Result = M.Name;
                    }
                }
            }
            return Result;
        }

        internal static string SerialNumberEdl()
        {
            var info = ExecuteEdlCommand(EdlFlashCommand("-info > "));
            StringReader stringReader = new StringReader(info);
            while (stringReader.Peek() != -1)
            {
                info = stringReader.ReadLine();
                if (info.Contains("SerialNumber:"))
                {
                    string str3 = info.Replace("SerialNumber:", "").Replace(" ", "").Replace("(", "").Replace(")", "");
                    info = str3;
                }
            }
            return info;
        }

        internal static string Programmer()
        {
            var Proggramer = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"File\Programmer\" + MsmHwId() + ".mbn";
            return Proggramer;
        }

        internal static bool IsPartitionExist(string partnamme)
        {
            bool flag = false;
            string str = ExecuteEdlCommand(EdlFlashCommand("-f  -gpt"));
            using (StringReader stringReader = new StringReader(str))
            {
                while (stringReader.Peek() != -1)
                {
                    string str1 = stringReader.ReadLine();
                    if (!str1.Contains("Partition Name:"))
                    {
                        continue;
                    }
                    using (StringReader stringReader1 = new StringReader(str1))
                    {
                        while (stringReader1.Peek() != -1)
                        {
                            str1 = stringReader1.ReadLine();
                            foreach (string str2 in PartitionCommand(str1.Replace("\r", "")))
                            {
                                if (!str1.Contains("Partition Name:") || !str1.Contains(partnamme))
                                {
                                    continue;
                                }
                                Rich(str1);
                                flag = true;
                            }
                        }
                    }
                }
            }
            return flag;
        }

        private static void Rich(string line)
        {
            string str = line.Substring(line.IndexOf("Size in LBA:"));
            Size_LBA = str.Substring(str.IndexOf(":") + 2);
            string str1 = line.Substring(line.IndexOf("Start LBA:"));
            str1 = str1.Substring(0, str1.IndexOf("Size in LBA:"));
            Start_LBA = str1.Substring(str1.IndexOf(":") + 2);
        }

        private static IEnumerable<string> PartitionCommand(string v)
        {
            List<string> strs = new List<string>();
            using (StringReader stringReader = new StringReader(v ?? string.Empty))
            {
                while (stringReader.Peek() != -1)
                {
                    string str = stringReader.ReadLine();
                    if (str == "")
                    {
                        continue;
                    }
                    strs.Add(str);
                }
            }
            return strs;
        }

        internal static void GetGPT()
        {
            if (partition.Count <= 0)
            {
                var contents = ExecuteEdlCommand(EdlFlashCommand("-gpt > "));
                //var contents = File.ReadAllText(raw);
                int lst = contents.IndexOf("1. Partition Name:");
                contents = contents.Remove(0, lst);
                contents = contents.Replace(" Partition Name: ", "name");
                contents = contents.Replace("Start LBA: ", "cok");
                contents = contents.Replace("Size in LBA: ", "end");
                contents = contents.Remove(contents.IndexOf("Status"));
                string[] m = contents.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                gptraw = m;
                var name = "";
                var start = "";
                var end = "";
                foreach (string z in m)
                {
                    int f4 = z.IndexOf("name");
                    int f5 = z.IndexOf("cok");
                    int f6 = z.IndexOf("end");
                    end = z.Substring(f6 + 3);
                    start = z.Substring(f5 + 3);
                    start = start.Substring(0, 7);
                    name = z.Substring(f4 + 4, f5 - 7).Replace(" ", "");
                    partition.Add(new Gpt(name, start, end));
                }
            }
        }

        internal static string MakeRawProgram()
        {
            rawProgram = "<?xml version=\"1.0\" ?>\n";
            rawProgram += "<data>\n";
            foreach (Gpt part in partition)
            {
                int kb = Convert.ToInt32(part.end) / 2;
                var dec = Convert.ToInt32(part.start) * 2;
                var hex = string.Format("{0:x}", dec);
                rawProgram += string.Format("  <program SECTOR_SIZE_IN_BYTES=\"512\" file_sector_offset=\"0\" filename=\"{0}.img\" label=\"{0}\" num_partition_sectors=\"{2}\" physical_partition_number=\"0\" size_in_KB=\"{3}.0\" sparse=\"false\" start_byte_hex=\"0x{4}00L\" start_sector=\"{1}\"/>\n", part.name, part.start, part.end, kb, hex);
            }
            rawProgram += "</data>";
            return rawProgram;
        }
    }
    /// <summary>
    /// class GPT
    /// </summary>
    public class Gpt
    {
        /// <summary>
        /// Name of partition
        /// </summary>
        public string name;
        /// <summary>
        /// Start address of partition
        /// </summary>
        public string start;
        /// <summary>
        /// end address of partition
        /// </summary>
        public string end;

        internal Gpt(string nm, string st, string en)
        {
            name = nm;
            start = st;
            end = en;
        }
    }
}
