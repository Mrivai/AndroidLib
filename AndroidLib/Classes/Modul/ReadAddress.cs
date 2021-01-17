namespace Mrivai.Pelitabangsa.Modul
{
    /// <summary>
    /// Manage ReadAddress
    /// </summary>
    public class ReadAddress
    {
        /// <summary>
        /// ReadAddress Error
        /// </summary>
        public bool Error { get; set; }
        /// <summary>
        /// ReadAddress Status
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// ReadAddress Filename
        /// </summary>
        public string Filename { get; set; }
        /// <summary>
        /// ReadAddress FilePath
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// ReadAddress StartAddress
        /// </summary>
        public long StartAddress { get; set; }
        /// <summary>
        /// ReadAddress Length
        /// </summary>
        public long Length { get; set; }
        /// <summary>
        /// ReadAddress Partition
        /// </summary>
        public string Partition { get; set; }
        /// <summary>
        /// ReadAddress
        /// </summary>
        public ReadAddress()
        {
            Error = false;
            Status = string.Empty;
            Filename = string.Empty;
            StartAddress = 0L;
            Length = 0L;
            Partition = "-";
            FilePath = string.Empty;
        }
    }

    /// <summary>
    /// Manage ReadFUllImage
    /// </summary>
    public class ReadFUllImage
    {
        /// <summary>
        /// ReadFUllImage Error
        /// </summary>
        public bool Error { get; set; }
        /// <summary>
        /// ReadFUllImage Status
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// ReadFUllImage Filename
        /// </summary>
        public string Filename { get; set; }
        /// <summary>
        /// ReadFUllImage
        /// </summary>
        public ReadFUllImage()
        {
            Error = false;
            Status = string.Empty;
            Filename = string.Empty;
        }
    }

    /// <summary>
    /// Manage StatusError
    /// </summary>
    public class StatusError
    {
        /// <summary>
        /// StatusError Error
        /// </summary>
        public bool Error { get; set; }
        /// <summary>
        /// StatusError Status
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// StatusError
        /// </summary>
        public StatusError()
        {
            Error = false;
            Status = string.Empty;
        }
    }
}
