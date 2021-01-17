namespace Mrivai.Pelitabangsa
{
    public class MediatekCommand
    {
        internal string Command { get; }
        internal int Timeout { get; private set; }
        internal MediatekCommand(string command) { Command = command; Timeout = Mrivai.Command.DEFAULT_TIMEOUT; }

        /// <summary>
        /// Sets the timeout for the  MediatekCommand
        /// </summary>
        /// <param name="timeout">The timeout for the command in milliseconds</param>
        public MediatekCommand WithTimeout(int timeout) { Timeout = timeout; return this; }
    }

    public class Mediatek
    {
        private const string  Mediatek_EXE = "download.exe";
        

        /// <summary>
        /// Forms a <see cref=" MediatekCommand"/> that is passed to <c> Mediatek.Execute MediatekCommand()</c>
        /// </summary>
        /// <param name="command">The  Mediatek command to run</param>
        /// <param name="args">Any arguments that need to be sent to <paramref name="command"/></param>
        /// <returns><see cref=" MediatekCommand"/> that contains formatted command information</returns>
        public static  MediatekCommand FormMediatekCommand(string command, params string[] args)
        {
            string fbCmd = (args.Length > 0) ? command + " " : command;

            for (int i = 0; i < args.Length; i++)
                fbCmd += args[i] + " ";
            return new  MediatekCommand(fbCmd);
        }

        /// <summary>
        /// Forms a <see cref=" MediatekCommand"/> that is passed to <c> Mediatek.Execute MediatekCommand()</c>
        /// </summary>
        /// <remarks>Should be used only for device-specific  Mediatek commands such as <c> Mediatek reboot</c> or <c> Mediatek getvar all</c></remarks>
        /// <param name="device">Specific <see cref="Device"/> to run the comand on</param>
        /// <param name="command">The command to run on  Mediatek</param>
        /// <param name="args">Any arguments that need to be sent to <paramref name="command"/></param>
        public static  MediatekCommand FormMediatekCommand(Device device, string command, params string[] args)
        {
            string fbCmd = "-s " + device.SerialNumber + " ";

            fbCmd += (args.Length > 0) ? command + " " : command;

            for (int i = 0; i < args.Length; i++)
                fbCmd += args[i] + " ";

            return new  MediatekCommand(fbCmd);
        }

        /// <summary>
        /// Executes a <see cref=" MediatekCommand"/>
        /// </summary>
        /// <param name="command">Instance of <see cref=" MediatekCommand"/></param>
        /// <returns>Output of <paramref name="command"/> run in  Mediatek</returns>
        public static string ExecuteMediatekCommand( MediatekCommand command)
        {
            return Command.RunProcessReturnOutput(AndroidController.Instance.ResourceDirectory +  Mediatek_EXE, command.Command, command.Timeout);
        }

        /// <summary>
        /// Executes a <see cref=" MediatekCommand"/>
        /// </summary>
        /// <remarks>Should be used if you do not want the output of the command; good for quick  Mediatek commands</remarks>
        /// <param name="command">Instance of <see cref=" MediatekCommand"/></param>
        public static void ExecuteMediatekCommandNoReturn( MediatekCommand command)
        {
            Command.RunProcessNoReturn(AndroidController.Instance.ResourceDirectory +  Mediatek_EXE, command.Command, command.Timeout);
        }
    }
}
