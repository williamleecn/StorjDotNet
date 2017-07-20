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
    public class ExtensionTests
    {
        [TestMethod]
        public void Base16Encodes()
        {
            byte[] bytes = "abcd12".HexStringToBytes();
            byte[] expectedBytes = new byte[] { 0xAB, 0xCD, 0x12 };
            CollectionAssert.AreEqual(expectedBytes, bytes);
        }

        [TestMethod]
        public void Base16Decodes()
        {
            string hexString= new byte[] { 0xAB, 0xCD, 0x12 }.ToHexString();
            string expectedHexString = "abcd12";
            Assert.AreEqual(expectedHexString, hexString);
        }

        [TestMethod]
        public void Base16EncodeDecode()
        {
            string str1 = "abcdef";
            byte[] bytes = str1.HexStringToBytes();
            byte[] bytes2 = str1.ToByteArray();
            Assert.AreEqual(3, bytes.Length);
            Assert.AreEqual(6, bytes2.Length);
            Assert.AreEqual(bytes2.Length, bytes.Length * 2);
        }
    }
}
