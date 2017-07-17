using System;
using StorjDotNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StorjTests
{
    [TestClass]
    public class StorjTests
    {
        [TestMethod]
        public void ShouldReturnMnemonic_128()
        {
            string mnemonic = Storj.GenerateMnemonic(128);

            Assert.IsFalse(string.IsNullOrEmpty(mnemonic), "Mnemonic should not be null or empty");
            Assert.IsTrue(mnemonic.Split(' ').Length == 12, "Mnemonic should be 12 words");
        }

        [TestMethod]
        public void ShouldReturnMnemonic_256()
        {
            string mnemonic = Storj.GenerateMnemonic(256);

            Assert.IsFalse(string.IsNullOrEmpty(mnemonic), "Mnemonic should not be null or empty");
            Assert.IsTrue(mnemonic.Split(' ').Length == 24, "Mnemonic should be 24 words");
        }

        [TestMethod]
        public void MnemonicIsValid_128()
        {
            string mnemonic = "prevent stadium inmate diary south notice wreck shuffle chaos trend fish library";

            Assert.IsTrue(Storj.CheckMnemonic(mnemonic));
        }

        [TestMethod]
        public void MnemonicIsValid_256()
        {
            string mnemonic = "render exile spot lamp boat magic valley pact sea unfair fix glove hood tragic country husband climb frown narrow axis ability pencil space shiver";

            Assert.IsTrue(Storj.CheckMnemonic(mnemonic));
        }

        [TestMethod]
        public void ShouldReturnTimestamp()
        {
            long timestamp = Storj.GetTimestamp();
            Assert.AreNotEqual(timestamp, 0);
        }

        [TestMethod]
        public void TimeStampIsCurrentTime()
        {
            long timestamp = Storj.GetTimestamp();
            DateTime storjTime = Helpers.DateTimeFromUnixTime(timestamp);
            DateTime currentTime = DateTime.UtcNow;

            TimeSpan timeDifference = storjTime - currentTime;

            Assert.IsTrue(timeDifference.TotalSeconds < 1);
        }
    }
}
