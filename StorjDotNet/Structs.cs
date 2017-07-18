using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StorjDotNet
{
    [StructLayout(LayoutKind.Sequential)]
    public struct storj_env
    {
        public storj_bridge_options bridge_options;
        public storj_encrypt_options encrypt_options;
        public storj_http_options http_options;
        public storj_log_options log_options;
        public IntPtr tmp_path;
        public IntPtr loop;
        public storj_log_levels log;
    }

    public enum ExchangeReportStatus
    {
        NotPrepared = 0,
        AwaitingSend = 1,
        ReportSending = 2,
        ReportSent = 3
    }

    public struct EncryptionContext
    {
        public byte encryption_ctr;
        public byte encryption_key;
        public Aes256Context aes256Context;
    }

    public struct Aes256Context
    {

    }

    public struct AesState
    {

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct storj_bridge_options
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string proto;
        [MarshalAs(UnmanagedType.LPStr)]
        public string host;
        public int port;
        [MarshalAs(UnmanagedType.LPStr)]
        public string user;
        [MarshalAs(UnmanagedType.LPStr)]
        public string pass;
    }

    public struct storj_encrypt_options
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string mnemonic;
    }

    public struct storj_http_options
    {
        public char[] user_agent;
        public char[] proxy_url;
        public ulong low_speed_limit;
        public ulong low_speed_time;
        public ulong timeout;
    }

    public delegate void storj_logger_fn(IntPtr message, int level, IntPtr handle);
    public delegate void storj_logger_format_fn(storj_log_options options, IntPtr handle, IntPtr format);

    public struct storj_log_options
    {
        public storj_logger_fn logger;
        public int level;
    }

    public struct storj_log_levels
    {
        public storj_logger_format_fn debug;
        public storj_logger_format_fn info;
        public storj_logger_format_fn warn;
        public storj_logger_format_fn error;
    }
}
