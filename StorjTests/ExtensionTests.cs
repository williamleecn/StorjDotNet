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
        public void Base16Encodes1()
        {
            byte[] bytes = "abcd12".HexStringToBytes();
            byte[] expectedBytes = new byte[] { 0xAB, 0xCD, 0x12 };
            CollectionAssert.AreEqual(expectedBytes, bytes);
        }

        [TestMethod]
        public void Base16Encodes2()
        {
            string data = "632442ba2e5f28a3a4e68dcb0b45d1d8f097d5b47479d74e2259055aa25a08aa";
            byte[] expectedBytes = new byte[32] { 99,36,66,186,46,95,40,163,164,230,141,203,11,69,
                209,216,240,151,213,180,116,121,215,78,34,89,5,90,162,90,8,170};

            byte[] actualBytes = data.HexStringToBytes();

            CollectionAssert.AreEqual(expectedBytes, actualBytes);
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
