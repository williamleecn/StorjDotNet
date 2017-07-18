using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StorjDotNet;

namespace StorjTests
{
    [TestClass]
    public class StorjTests
    {
        private static IStorj m_libStorj;

        [ClassInitialize]
        public static void TestClassinitialize(TestContext context)
        {
            m_libStorj = new Storj();
        }

        [TestMethod]
        public void ShouldGenerateMnemonic12Words()
        {
            string mnemonic = m_libStorj.GenerateMnemonic(128).Result;
            Assert.IsFalse(string.IsNullOrEmpty(mnemonic), "Mnemonic should not be null or empty");
            Assert.IsTrue(mnemonic.Split(' ').Length == 12, "Mnemonic should be 24 words");
            Console.WriteLine("Mnemonic is \"{0}\"", mnemonic);
        }

        [TestMethod]
        public void ShouldGenerateMnemonic24Words()
        {
            string mnemonic = m_libStorj.GenerateMnemonic(256).Result;
            Assert.IsFalse(string.IsNullOrEmpty(mnemonic), "Mnemonic should not be null or empty");
            Assert.IsTrue(mnemonic.Split(' ').Length == 24, "Mnemonic should be 24 words");
            Console.WriteLine("Mnemonic is \"{0}\"", mnemonic);
        }
    }
}
