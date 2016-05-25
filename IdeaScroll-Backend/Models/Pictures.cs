using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace IdeaScroll_Backend.Models
{
    public class Pictures
    {

        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public long FileSizeInBytes { get; set; }
        public long FileSizeInKb { get { return (long)Math.Ceiling((double)FileSizeInBytes / 1024); } }

    }
    public class PicDownloadModel
    {
        public MemoryStream BlobStream { get; set; }
        public string BlobFileName { get; set; }
        public string BlobContentType { get; set; }
        public long BlobLength { get; set; }
    }

}