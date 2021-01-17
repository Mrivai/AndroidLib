namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// manage RawDrive
    /// </summary>
    public class RawDrive
    {
        /// <summary>
        /// RawDrive Index
        /// </summary>
        public int index { get; set; }
        /// <summary>
        /// RawDrive Id
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// RawDrive Size
        /// </summary>
        public long Size { get; set; }
        /// <summary>
        /// RawDrive Description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// RawDrive Manufacturer
        /// </summary>
        public string Manufacturer { get; set; }
        /// <summary>
        /// RawDrive MediaType
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// RawDrive Model
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// RawDrive
        /// </summary>
        public RawDrive()
        {
        }
    }
}
