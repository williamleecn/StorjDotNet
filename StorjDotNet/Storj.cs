using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StorjDotNet
{
    public class Storj
    {
        [DllImport("libstorj-0.dll")]
        private static extern bool storj_mnemonic_check(string mnemonic);
        [DllImport("libstorj-0.dll")]
        private static extern int storj_mnemonic_generate(int strength, ref IntPtr buffer);
        [DllImport("libstorj-0.dll")]
        private static extern long storj_util_timestamp();

        public Storj()
        {
        }

        /// <summary>
        /// Generates a 128 or 256 bit mnemonic phrase to use as an encryption key
        /// </summary>
        /// <param name="strength">The strength of key to generate (128 or 256 bits)</param>
        /// <returns>Array of mnemonic words</returns>
        public static string GenerateMnemonic(int strength)
        {
            IntPtr ptr = new IntPtr(Int64.MaxValue);
            int success = storj_mnemonic_generate(strength, ref ptr);

            if (success == 1)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
            return string.Empty;            
        }

        public static bool CheckMnemonic(string mnemonic)
        {
            return storj_mnemonic_check(mnemonic);
        }

        /// <summary>
        /// Get the current storj Unix style timestamp in UTC
        /// </summary>
        /// <returns>UTC timestamp</returns>
        public static long GetTimestamp()
        {
            return storj_util_timestamp();
        }
    }
}
