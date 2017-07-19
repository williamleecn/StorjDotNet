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
    }
}
