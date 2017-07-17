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
            Storj storj = new Storj();
            string mnemonic = storj.generateMnemonic(128);

            Assert.IsFalse(string.IsNullOrEmpty(mnemonic), "Mnemonic should not be null or empty");
            Assert.IsTrue(mnemonic.Split(' ').Length == 12, "Mnemonic should be 12 words");
        }

        [TestMethod]
        public void ShouldReturnMnemonic_256()
        {
            Storj storj = new Storj();
            string mnemonic = storj.generateMnemonic(256);

            Assert.IsFalse(string.IsNullOrEmpty(mnemonic), "Mnemonic should not be null or empty");
            Assert.IsTrue(mnemonic.Split(' ').Length == 24, "Mnemonic should be 24 words");
        }
    }
}
