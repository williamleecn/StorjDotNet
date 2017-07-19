using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bitcoin.BIP39;
using System.Net.Http;
using Newtonsoft.Json;
using StorjDotNet.Models;

namespace StorjDotNet
{
    public class Helpers
    {
        public static DateTime DateTimeFromUnixTime(long timestamp)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return epoch.AddMilliseconds(timestamp);
        }

        public static BIP39.Language StorjToBIP39Language(MnemonicLanguage storjLanguage)
        {
            return (BIP39.Language)Enum.Parse(typeof(BIP39.Language), Enum.GetName(typeof(MnemonicLanguage), storjLanguage));
        }
    }
}
