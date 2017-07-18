using System;
using StorjDotNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StorjTests
{
    [TestClass]
    public class StorjTests
    {
        private static storj_bridge_options bridgeOptions;

        [ClassInitialize]
        public static void TestClassinitialize(TestContext context)
        {
            bridgeOptions = new storj_bridge_options();
            bridgeOptions.proto = context.Properties["bridgeProto"].ToString();
            bridgeOptions.port = Convert.ToInt32(context.Properties["bridgePort"]);
            bridgeOptions.host = context.Properties["bridgeHost"].ToString();
            bridgeOptions.user = context.Properties["bridgeUser"].ToString();
            bridgeOptions.pass = context.Properties["bridgePass"].ToString();
        }

        #region [ storj_mnemonic_generate tests ]

        [TestMethod]
        public void ShouldReturnMnemonic_128()
        {
            LibStorjFunctions libstorj = new LibStorjFunctions();
            string mnemonic = libstorj.storj_mnemonic_generate(128);

            Assert.IsFalse(string.IsNullOrEmpty(mnemonic), "Mnemonic should not be null or empty");
            Assert.IsTrue(mnemonic.Split(' ').Length == 12, "Mnemonic should be 12 words");
        }

        [TestMethod]
        public void ShouldReturnMnemonic_256()
        {
            LibStorjFunctions libstorj = new LibStorjFunctions();
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
            LibStorjFunctions libstorj = new LibStorjFunctions();
            libstorj.storj_mnemonic_generate(152);
        }

        [TestMethod]
        public void MnemonicIsValid_128()
        {
            string mnemonic = "prevent stadium inmate diary south notice wreck shuffle chaos trend fish library";
            LibStorjFunctions libstorj = new LibStorjFunctions();
            Assert.IsTrue(libstorj.storj_mnemonic_check(mnemonic));
        }

        [TestMethod]
        public void MnemonicIsValid_256()
        {
            string mnemonic = "render exile spot lamp boat magic valley pact sea unfair fix glove hood tragic country husband climb frown narrow axis ability pencil space shiver";
            LibStorjFunctions libstorj = new LibStorjFunctions();
            Assert.IsTrue(libstorj.storj_mnemonic_check(mnemonic));
        }

        #endregion

        #region [ storj_util_timestamp tests ]

        [TestMethod]
        public void ShouldReturnCurrentTimestamp()
        {
            LibStorjFunctions libstorj = new LibStorjFunctions();
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
            LibStorjFunctions libstorj = new LibStorjFunctions();
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
