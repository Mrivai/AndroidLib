/*
 * Sfk.cs - Developed by Mrivai for XiaomiLib.dll
 */


namespace Mrivai.Pelitabangsa
{
    public class SfkCommand
    {
        private string command;
        private int timeout;
        internal string Command { get { return command; } }
        internal int Timeout { get { return timeout; } }
        internal SfkCommand(string command) { this.command = command; timeout = Mrivai.Command.DEFAULT_TIMEOUT; }
        
        /// <summary>
        /// Sets the timeout for the SfkCommand
        /// </summary>
        /// <param name="timeout">The timeout for the command in milliseconds</param>
        public SfkCommand WithTimeout(int timeout) { this.timeout = timeout; return this; }
    }

    /// <summary>
    /// Controls all commands sent to Sfk
    /// </summary>
    public static class Sfk
    {
        private const string Sfk_EXE = "sfk.exe";

        /// <summary>
        /// Forms a <see cref="SfkCommand"/> that is passed to <c>Sfk.ExecuteSfkCommand()</c>
        /// </summary>
        /// <param name="command">The Sfk command to run</param>
        /// <param name="args">Any arguments that need to be sent to <paramref name="command"/></param>
        /// <returns><see cref="SfkCommand"/> that contains formatted command information</returns>
        
        public static SfkCommand FormSfkCommand(string command, params string[] args)
        {
            string fhCmd = (args.Length > 0) ? command + " " : command;
            for (int i = 0; i < args.Length; i++)
                fhCmd += args[i] + " ";
            return new SfkCommand(fhCmd);
        }
        
        /// <summary>
        /// Executes a <see cref="SfkCommand"/>
        /// </summary>
        /// <param name="command">Instance of <see cref="SfkCommand"/></param>
        /// <returns>Output of <paramref name="command"/> run in Sfk</returns>
        public static string ExecuteSfkCommand(SfkCommand command)
        {
            return Command.RunProcessReturnOutput(AndroidController.Instance.ResourceDirectory + Sfk_EXE, command.Command, command.Timeout);
        }

        /// <summary>
        /// Executes a <see cref="SfkCommand"/>
        /// </summary>
        /// <remarks>Should be used if you do not want the output of the command; good for quick Sfk commands</remarks>
        /// <param name="command">Instance of <see cref="SfkCommand"/></param>
        public static void ExecuteSfkCommandNoReturn(SfkCommand command)
        {
            Command.RunProcessNoReturn(AndroidController.Instance.ResourceDirectory + Sfk_EXE, command.Command, command.Timeout);
        }
        
    }
}
