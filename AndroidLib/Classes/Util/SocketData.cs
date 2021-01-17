using System;

namespace Mrivai
{
    /// <summary>
    /// Manages Remote Service Socket Command
    /// </summary>
    [Serializable]
    public class SocketData
    {
        /// <summary>
        /// get set socket name
        /// </summary>
        public string ShowName { get; set; }
        /// <summary>
        /// get set socket command
        /// </summary>
        public int Command { get; set; }
        /// <summary>
        /// get set socket data
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// set socket data
        /// </summary>
        /// <param name="name">name of socket</param>
        /// <param name="command">command number</param>
        /// <param name="data">object of socket data</param>
        public SocketData(string name, int command, object data)
        {
            ShowName = name;
            Command = command;
            Data = data;
        }
        /// <summary>
        /// init socket data
        /// </summary>
        public SocketData() { }
    }
    /// <summary>
    /// contains enum list of socket command type
    /// </summary>
    public enum SocketCommand
    {
        /// <summary>
        /// Connect Command
        /// </summary>
        CONNECT,
        /// <summary>
        /// Message Command
        /// </summary>
        MESSAGE,
        /// <summary>
        /// Screenshot Command
        /// </summary>
        SCREENSHOT,
        /// <summary>
        /// Message Command
        /// </summary>
        IMAGE,
        /// <summary>
        /// Sound Command
        /// </summary>
        SOUND,
        /// <summary>
        /// File Command
        /// </summary>
        FILE,
        /// <summary>
        /// Point Command
        /// </summary>
        POINT,
        /// <summary>
        /// Click Command
        /// </summary>
        CLICK,
        /// <summary>
        /// Double Command
        /// </summary>
        DOUBLE,
        /// <summary>
        /// Mouse left Down Command
        /// </summary>
        MOUSELEFTDOWN,
        /// <summary>
        /// Mouse Left Up Command
        /// </summary>
        MOUSELEFTUP,
        /// <summary>
        /// Mouse Rigth Down Command
        /// </summary>
        MOUSERIGTHDOWN,
        /// <summary>
        /// Mouse Rigth Up Command
        /// </summary>
        MOUSERIGTHUP,
        /// <summary>
        /// Mouse Middle Down Command
        /// </summary>
        MOUSEMIDDLEDOWN,
        /// <summary>
        /// Mouse Middle Up Command
        /// </summary>
        MOUSEMIDDLEUP,
        /// <summary>
        /// Key Up Command
        /// </summary>
        KEYUP,
        /// <summary>
        /// Key Down Command
        /// </summary>
        KEYDOWN,
        /// <summary>
        /// Resolution Command
        /// </summary>
        RESOLUTION
    }
}
