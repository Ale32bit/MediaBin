using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace MediaBin.Models
{
    public class File
    {
        public string FileName { get; set; }
        public string Name { get => Path.GetFileNameWithoutExtension(FileName); }
        public string Extension { get => Path.GetExtension(FileName); }
        public DateTime CreationDate { get; set; }
        public byte[] Content { get; set; }
        public string Type
        {
            get => MimeTypes.GetMimeType(Extension);
        }
    }
}
