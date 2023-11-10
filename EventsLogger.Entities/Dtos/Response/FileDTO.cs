using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventsLogger.Entities.Dtos.Response
{
    public class FileDTO
    {
        public string? FileName { get; set; }
        public string? fileContainer { get; set; }
        public string? Url { get; set; }



    }
}