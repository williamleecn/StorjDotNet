using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorjDotNet
{
    public static class Helpers
    {
        public static DateTime DateTimeFromUnixTime(long timestamp)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return epoch.AddMilliseconds(timestamp);
        }
    }
}
