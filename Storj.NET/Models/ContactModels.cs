using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorjDotNet.Models
{
    public class Contact
    {
        public string LastSeen { get; set; }
        public int Port { get; set; }
        public string Address { get; set; }
        public string UserAgent { get; set; }
        public string Protocol { get; set; }
        public decimal ResponseTime { get; set; }
        public string LastTimeout { get; set; }
        public decimal TimeoutRate { get; set; }
        public string NodeId { get; set; }

        public DateTime? LastSeenDateTime
        {
            get
            {
                DateTime lastSeen;
                if (DateTime.TryParse(LastSeen, out lastSeen))
                {
                    return lastSeen;
                }
                return null;
            }
        }

        public DateTime? LastTimeoutDateTime
        {
            get
            {
                DateTime lastTimeout;
                if (DateTime.TryParse(LastTimeout, out lastTimeout))
                {
                    return lastTimeout;
                }
                return null;
            }
        }
    }
}
