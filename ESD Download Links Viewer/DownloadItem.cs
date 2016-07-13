using System;

namespace ESD_Download_Links_Viewer
{
    class DownloadItem
    {
        public string Name { get; private set; }
        public string Url { get; private set; }
        public ulong Size { get; private set; }
        public string FileType { get; private set; }
        public DateTime ModifiedDate { get; private set; }
        public string SHA1 { get; private set; }
        public string SHA256 { get; private set; }

        public DownloadItem(string name, string url, ulong size, string ext, DateTime date, string sha1, string sha256)
        {
            Name = name;
            Url = url;
            Size = size;
            FileType = ext;
            ModifiedDate = date;
            SHA1 = sha1;
            SHA256 = sha256;
        }
    }
}
