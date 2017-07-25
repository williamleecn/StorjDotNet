﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorjDotNet.Models
{
    public class StorjFile
    {
        public string Bucket { get; set; }
        public string MimeType { get; set; }
        public string FileName { get; set; }
        public string Frame { get; set; }
        public long Size { get; set; }
        public string Id { get; set; }
        public string Created { get; set; }
        public FileHmac Hmac { get; set; }

        public DateTime? CreatedDateTime
        {
            get
            {
                DateTime createdDateTime;
                if (DateTime.TryParse(Created, out createdDateTime))
                {
                    return createdDateTime;
                }
                return null;
            }
        }
    }

    public class FileHmac
    {
        public string Value { get; set; }
        public string Type { get; set; }
    }
}
