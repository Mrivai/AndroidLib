/*
 * FhLoader.cs - Developed by Mrivai for XiaomiLib.dll
 */

using System.IO;

namespace Mrivai.Pelitabangsa
{
    public class FhLoaderCommand
    {

        internal string Command { get; }
        internal int Timeout { get; private set; }
        internal FhLoaderCommand(string command) { this.Command = command; Timeout = Mrivai.Command.DEFAULT_TIMEOUT; }
        
        /// <summary>
        /// Sets the timeout for the FhLoaderCommand
        /// </summary>
        /// <param name="timeout">The timeout for the command in milliseconds</param>
        public FhLoaderCommand WithTimeout(int timeout) { this.Timeout = timeout; return this; }
    }

    /// <summary>
    /// Controls all commands sent to FhLoader
    /// </summary>
    public static class FhLoader
    {
        private const string FhLoader_EXE = "FhLoader.exe";

        /// <summary>
        /// Forms a <see cref="FhLoaderCommand"/> that is passed to <c>FhLoader.ExecuteFhLoaderCommand()</c>
        /// </summary>
        /// <param name="command">The FhLoader command to run</param>
        /// <param name="args">Any arguments that need to be sent to <paramref name="command"/></param>
        /// <returns><see cref="FhLoaderCommand"/> that contains formatted command information</returns>
        
        public static FhLoaderCommand FormFhLoaderCommand(string command, params string[] args)
        {
            string fhCmd = (args.Length > 0) ? command + " " : command;
            for (int i = 0; i < args.Length; i++)
                fhCmd += args[i] + " ";
            return new FhLoaderCommand(fhCmd);
        }
        
        /// <summary>
        /// Executes a <see cref="FhLoaderCommand"/>
        /// </summary>
        /// <param name="command">Instance of <see cref="FhLoaderCommand"/></param>
        /// <returns>Output of <paramref name="command"/> run in FhLoader</returns>
        public static string ExecuteFhLoaderCommand(FhLoaderCommand command)
        {
            return Command.RunProcessReturnOutput(AndroidController.Instance.ResourceDirectory + FhLoader_EXE, command.Command, command.Timeout);
        }

        /// <summary>
        /// Executes a <see cref="FhLoaderCommand"/>
        /// </summary>
        /// <remarks>Should be used if you do not want the output of the command; good for quick FhLoader commands</remarks>
        /// <param name="command">Instance of <see cref="FhLoaderCommand"/></param>
        public static void ExecuteFhLoaderCommandNoReturn(FhLoaderCommand command)
        {
            Command.RunProcessNoReturn(AndroidController.Instance.ResourceDirectory + FhLoader_EXE, command.Command, command.Timeout);
        }
        
        internal static void Reboot()
        {
            ExecuteFhLoaderCommandNoReturn(FormFhLoaderCommand("--port=" + Edl.Port() + " --noprompt --showpercentagecomplete --zlpawarehost=1 --memoryname=eMMC --reset"));
        }

        internal static void WritePartition(string filename, string start, string endd, string working, string Memory)
        {
            ExecuteFhLoaderCommandNoReturn(FormFhLoaderCommand(string.Concat(new string[] { "--port=\\\\.\\", Edl.Port(), " --sendimage=\"", filename, "\" --start_sector=", start, " --num_sectors=", endd, " --noprompt --loglevel=2 --showpercentagecomplete --zlpawarehost=1 --memoryname=", Memory, " --search_path=\"", working, "\"" })));
        }

        public static void Writexml(string filename, string working, string Memory)
        {
            ExecuteFhLoaderCommandNoReturn(FormFhLoaderCommand(string.Concat(new string[] { "--port=\\\\.\\", Edl.Port(), " --sendxml=", filename, " --noprompt --showpercentagecomplete --loglevel=0 --zlpawarehost=1 --memoryname=", Memory, " --search_path=\"", working, "\"" })));
        }
    }
}
