using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StorjDotNet
{
    public class LibStorjFunctions
    {
        public LibStorjFunctions()
        {
            _storj_init_env = NativeMethods.storj_init_env;
            _storj_mnemonic_generate = NativeMethods.storj_mnemonic_generate;
            _storj_mnemonic_check = NativeMethods.storj_mnemonic_check;
            _storj_strerror = NativeMethods.storj_strerror;
            _storj_util_timestamp = NativeMethods.storj_util_timestamp;
        }

        protected Func<storj_bridge_options, storj_encrypt_options,
            storj_http_options, storj_log_options, storj_env> _storj_init_env;
        public storj_env storj_init_env(storj_bridge_options options,
            storj_encrypt_options encrypt_options,
            storj_http_options http_options,
            storj_log_options log_options)
        {
            return _storj_init_env(options, encrypt_options, http_options, log_options);
        }

        protected Func<string, bool> _storj_mnemonic_check;
        public bool storj_mnemonic_check(string mnemonic)
        {
            return _storj_mnemonic_check(mnemonic);
        }

        protected delegate int storj_mnemonic_generate_func(int strength, ref IntPtr buffer);
        protected storj_mnemonic_generate_func _storj_mnemonic_generate;
        public string storj_mnemonic_generate(int strength)
        {
            IntPtr ptr = new IntPtr();
            int success = _storj_mnemonic_generate(strength, ref ptr);

            if (success == 1)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
            throw new ArgumentException("Error creating mnemonic. Mnemonic strength must be 128 or 256 bits");
        }

        protected Func<int, IntPtr> _storj_strerror;
        public string storj_strerror(int errorCode)
        {
            IntPtr ptr = _storj_strerror(errorCode);
            return ptr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr);
        }

        protected Func<long> _storj_util_timestamp;
        public long storj_util_timestamp()
        {
            return _storj_util_timestamp();
        }
        
        private static class NativeMethods
        {
            [DllImport("libstorj-0")]
            public static extern storj_env storj_init_env(storj_bridge_options options, storj_encrypt_options encrypt_options, storj_http_options http_options, storj_log_options log_options);
            [DllImport("libstorj-0")]
            public static extern bool storj_mnemonic_check(string mnemonic);
            [DllImport("libstorj-0")]
            public static extern int storj_mnemonic_generate(int strength, ref IntPtr buffer);
            [DllImport("libstorj-0")]
            public static extern IntPtr storj_strerror(int errorCode);
            [DllImport("libstorj-0")]
            public static extern long storj_util_timestamp();
        }
    }
}
