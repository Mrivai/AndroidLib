using Microsoft.Win32.SafeHandles;
using System.IO;

namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// Manage Streamer
    /// </summary>
    public struct Streamer
    {
        /// <summary>
        /// Streamer Stream
        /// </summary>
        public Stream STR;
        /// <summary>
        /// Streamer SafeFileHandle
        /// </summary>
        public SafeFileHandle SH;
        /// <summary>
        /// Streamer Error status, true or false
        /// </summary>
        public bool isERROR;
    }
}
