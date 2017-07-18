using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StorjDotNet
{
    [StructLayout(LayoutKind.Sequential)]
    public struct StorjEnvironment
    {
        public storj_bridge_options bridgeOptions;
        EncryptionOptions encryptionOptions;
        HttpOptions httpOptions;
        LogOptions logOptions;
        string tempPath;
        IntPtr uvLoop;
        storj_log_levels_t logLevels;
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
        byte encryption_ctr;
        byte encryption_key;
        Aes256Context aes256Context;
    }

    public struct Aes256Context
    {

    }

    public struct AesState
    {

    }

    public struct storj_bridge_options
    {
        public string proto;
        public string host;
        public int port;
        public string user;
        public string pass;
    }

    public struct EncryptionOptions
    {
        public string mnemonic;
    }

    public struct HttpOptions
    {
        public string userAgent;
        public string proxyUrl;
        public ulong lowSpeedLimit;
        public ulong lowSpeedTime;
        public ulong timeout;
    }

    public struct LogOptions
    {

    }

    public struct storj_log_levels_t
    {

    }
}
