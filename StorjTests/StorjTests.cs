using System;
using StorjDotNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices;

namespace StorjTests
{
    [TestClass]
    public class StorjTests
    {
        private static LibStorjFunctions libstorj;
        private static storj_bridge_options bridgeOptions;
        private static storj_encrypt_options encryptOptions;
        private static storj_http_options httpOptions;
        private static storj_log_options logOptions;

        [ClassInitialize]
        public static void TestClassinitialize(TestContext context)
        {
            libstorj = new LibStorjFunctions();

            bridgeOptions = new storj_bridge_options();
            bridgeOptions.proto = context.Properties["bridgeProto"].ToString();
            bridgeOptions.port = Convert.ToInt32(context.Properties["bridgePort"]);
            bridgeOptions.host = context.Properties["bridgeHost"].ToString();
            bridgeOptions.user = context.Properties["bridgeUser"].ToString();
            bridgeOptions.pass = context.Properties["bridgePass"].ToString();

            encryptOptions = new storj_encrypt_options();
            encryptOptions.mnemonic = "prevent stadium inmate diary south notice wreck shuffle chaos trend fish library";

            httpOptions = new storj_http_options();
            httpOptions.user_agent = "Storj.NET test runner".ToCharArray();
            httpOptions.low_speed_limit = 30720L;
            httpOptions.low_speed_time = 20L;
            httpOptions.timeout = 60L;

            logOptions = new storj_log_options();
            logOptions.level = 4;
            logOptions.logger = consoleLogger;
        }

        public static readonly storj_logger_fn consoleLogger = logToConsole;
        public static void logToConsole(IntPtr message, int level, IntPtr handle)
        {
            string log = Marshal.PtrToStringAnsi(message);
            Console.WriteLine("\"message\": \"{0}\", \"level\": {1}, \"timestamp\": {2}\n",
                message, level, DateTime.Now.ToString());
        }

        #region [ storj_init_env tests ]

        [TestMethod]
        public void ReturnsStorjEnv()
        {
            storj_env? env = null;
            env = libstorj.storj_init_env(bridgeOptions, encryptOptions, httpOptions, logOptions);
            Assert.IsNotNull(env);
        }

        #endregion

        #region [ storj_mnemonic_generate tests ]

        [TestMethod]
        public void ShouldReturnMnemonic_128()
        {
            string mnemonic = libstorj.storj_mnemonic_generate(128);

            Assert.IsFalse(string.IsNullOrEmpty(mnemonic), "Mnemonic should not be null or empty");
            Assert.IsTrue(mnemonic.Split(' ').Length == 12, "Mnemonic should be 12 words");
        }

        [TestMethod]
        public void ShouldReturnMnemonic_256()
        {
            string mnemonic = libstorj.storj_mnemonic_generate(256);

            Assert.IsFalse(string.IsNullOrEmpty(mnemonic), "Mnemonic should not be null or empty");
            Assert.IsTrue(mnemonic.Split(' ').Length == 24, "Mnemonic should be 24 words");
        }

        #endregion

        #region [ storj_mnemonic_check tests ]

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BadStrengthMnemonicShouldThrowException()
        {
            libstorj.storj_mnemonic_generate(152);
        }

        [TestMethod]
        public void MnemonicIsValid_128()
        {
            string mnemonic = "prevent stadium inmate diary south notice wreck shuffle chaos trend fish library";
            Assert.IsTrue(libstorj.storj_mnemonic_check(mnemonic));
        }

        [TestMethod]
        public void MnemonicIsValid_256()
        {
            string mnemonic = "render exile spot lamp boat magic valley pact sea unfair fix glove hood tragic country husband climb frown narrow axis ability pencil space shiver";
            Assert.IsTrue(libstorj.storj_mnemonic_check(mnemonic));
        }

        #endregion

        #region [ storj_util_timestamp tests ]

        [TestMethod]
        public void ShouldReturnCurrentTimestamp()
        {
            long timestamp = libstorj.storj_util_timestamp();
            DateTime currentTime = DateTime.UtcNow;

            Assert.AreNotEqual(timestamp, 0);

            DateTime storjTime = Helpers.DateTimeFromUnixTime(timestamp);
            TimeSpan timeDifference = storjTime - currentTime;

            Assert.IsTrue(timeDifference.TotalSeconds < 1);
        }

        #endregion

        #region [ storj_strerror tests ]

        [TestMethod]
        public void ShouldReturnBridgeRequestErrorMessage()
        {
            string error = libstorj.storj_strerror(1000);
            Assert.AreEqual("Bridge request error", error);
        }

        [TestMethod]
        public void ShouldReturnBridgeRateErrorMessage()
        {
            LibStorjFunctions libstorj = new LibStorjFunctions();
            string error = libstorj.storj_strerror(1005);
            Assert.AreEqual("Bridge rate limit error", error);
        }

        [TestMethod]
        public void ShouldReturnFarmerExhaustedErrorMessage()
        {
            LibStorjFunctions libstorj = new LibStorjFunctions();
            string error = libstorj.storj_strerror(2003);
            Assert.AreEqual("Farmer exhausted error", error);
        }

        #endregion
    }
}
