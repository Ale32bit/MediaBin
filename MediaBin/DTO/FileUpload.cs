using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaBin.DTO
{
    public class FileUpload
    {
#nullable enable
        public string? FileName { get; set; }
#nullable restore
        public IFormFile File { get; set; }
    }
}
