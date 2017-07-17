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
        public static extern int storj_mnemonic_generate(int strength, ref IntPtr buffer);

        public Storj()
        {
        }

        public string generateMnemonic(int strength)
        {
            IntPtr ptr = new IntPtr(Int64.MaxValue);
            int success = storj_mnemonic_generate(strength, ref ptr);

            if (success == 1)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
            return string.Empty;            
        }
    }
}
