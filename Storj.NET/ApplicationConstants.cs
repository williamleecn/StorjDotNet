using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorjDotNet
{
    public enum MnemonicLanguage
    {
        English,
        Japanese,
        Spanish,
        ChineseSimplified,
        ChineseTraditional,
        French,
        Unknown
    };

    public static class ApplicationConstants
    {
        // File transfer success
        public const int STORJ_TRANSFER_OK = 0;
        public const int STORJ_TRANSFER_CANCELED = 1;

        // Bridge related errors 1000 to 1999
        public const int STORJ_BRIDGE_REQUEST_ERROR = 1000;
        public const int STORJ_BRIDGE_AUTH_ERROR = 1001;
        public const int STORJ_BRIDGE_TOKEN_ERROR = 1002;
        public const int STORJ_BRIDGE_TIMEOUT_ERROR = 1003;
        public const int STORJ_BRIDGE_INTERNAL_ERROR = 1004;
        public const int STORJ_BRIDGE_RATE_ERROR = 1005;
        public const int STORJ_BRIDGE_BUCKET_NOTFOUND_ERROR = 1006;
        public const int STORJ_BRIDGE_FILE_NOTFOUND_ERROR = 1007;
        public const int STORJ_BRIDGE_JSON_ERROR = 1008;
        public const int STORJ_BRIDGE_FRAME_ERROR = 1009;
        public const int STORJ_BRIDGE_POINTER_ERROR = 1010;
        public const int STORJ_BRIDGE_REPOINTER_ERROR = 1011;
        public const int STORJ_BRIDGE_FILEINFO_ERROR = 1012;
        public const int STORJ_BRIDGE_BUCKET_FILE_EXISTS = 1013;
        public const int STORJ_BRIDGE_OFFER_ERROR = 1014;

        // Farmer related errors 2000 to 2999
        public const int STORJ_FARMER_REQUEST_ERROR = 2000;
        public const int STORJ_FARMER_TIMEOUT_ERROR = 2001;
        public const int STORJ_FARMER_AUTH_ERROR = 2002;
        public const int STORJ_FARMER_EXHAUSTED_ERROR = 2003;
        public const int STORJ_FARMER_INTEGRITY_ERROR = 2004;

        // File related errors 3000 to 3999
        public const int STORJ_FILE_INTEGRITY_ERROR = 3000;
        public const int STORJ_FILE_WRITE_ERROR = 3001;
        public const int STORJ_FILE_ENCRYPTION_ERROR = 3002;
        public const int STORJ_FILE_SIZE_ERROR = 3003;
        public const int STORJ_FILE_DECRYPTION_ERROR = 3004;
        public const int STORJ_FILE_GENERATE_HMAC_ERROR = 3005;
        public const int STORJ_FILE_READ_ERROR = 3006;
        public const int STORJ_FILE_SHARD_MISSING_ERROR = 3007;
        public const int STORJ_FILE_RECOVER_ERROR = 3008;
        public const int STORJ_FILE_RESIZE_ERROR = 3009;
        public const int STORJ_FILE_UNSUPPORTED_ERASURE = 3010;
        public const int STORJ_FILE_PARITY_ERROR = 3011;

        // Memory related errors
        public const int STORJ_MEMORY_ERROR = 4000;
        public const int STORJ_MAPPING_ERROR = 4001;
        public const int STORJ_UNMAPPING_ERROR = 4002;

        // Queue related errors
        public const int STORJ_QUEUE_ERROR = 5000;

        // Meta related errors 6000 to 6999
        public const int STORJ_META_ENCRYPTION_ERROR = 6000;
        public const int STORJ_META_DECRYPTION_ERROR = 6001;

        // Miscellaneous errors
        public const int STORJ_HEX_DECODE_ERROR = 7000;

        // Exchange report codes
        public const int STORJ_REPORT_SUCCESS = 1000;
        public const int STORJ_REPORT_FAILURE = 1100;

        // Exchange report messages
        public const string STORJ_REPORT_FAILED_INTEGRITY = "FAILED_INTEGRITY";
        public const string STORJ_REPORT_SHARD_DOWNLOADED = "SHARD_DOWNLOADED";
        public const string STORJ_REPORT_SHARD_UPLOADED = "SHARD_UPLOADED";
        public const string STORJ_REPORT_DOWNLOAD_ERROR = "DOWNLOAD_ERROR";
        public const string STORJ_REPORT_UPLOAD_ERROR = "TRANSFER_FAILED";

        public const int STORJ_SHARD_CHALLENGES = 4;
        public const long STORJ_LOW_SPEED_LIMIT = 30720L;
        public const long STORJ_LOW_SPEED_TIME = 20L;
        public const long STORJ_HTTP_TIMEOUT = 60L;
    }
}
