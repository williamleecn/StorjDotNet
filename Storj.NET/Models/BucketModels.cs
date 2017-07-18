using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorjDotNet.Models
{
    public class Bucket
    {
        public string User { get; set; }
        public string EncryptionKey { get; set; }
        public IEnumerable<string> PublicPermissions { get; set; }
        public string Created { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> Pubkeys { get; set; }
        public string Status { get; set; }
        public int Transfer { get; set; }
        public int Storage { get; set; }
        public string Id { get; set; }
    }
}
