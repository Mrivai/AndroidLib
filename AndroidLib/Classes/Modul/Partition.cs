using System;

namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// Manage Partition
    /// </summary>
    public class Partition
    {
        /// <summary>
        /// Selected Partition
        /// </summary>
        public bool Selected { get; set; }
        /// <summary>
        /// Partition ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Partition Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Partition start address
        /// </summary>
        public long StartAddress { get; set; }
        /// <summary>
        /// Partition end address
        /// </summary>
        public long EndAddress { get; set; }
        /// <summary>
        /// Partition size
        /// </summary>
        public long Size { get; set; }
        /// <summary>
        /// Partition type
        /// </summary>
        public Guid partitionType;

        /// <summary>
        /// Partition
        /// </summary>
        public Partition()
        {
        }
    }
}
