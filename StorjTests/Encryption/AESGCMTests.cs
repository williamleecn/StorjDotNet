using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StorjDotNet;
using StorjDotNet.Encryption;

namespace StorjTests.Encryption
{
    [TestClass]
    public class AESGCMTests
    {
        private readonly byte[] _key = new byte[32] {215,99,0,133,172,219,64,35,54,53,171,23,146,160,
            81,126,137,21,253,171,48,217,184,188,8,137,3,4,83,50,30,251};
        private readonly byte[] _iv = new byte[32] {70,219,247,135,162,7,93,193,44,123,188,234,203,115,129,
            82,70,219,247,135,162,7,93,193,44,123,188,234,203,115,129,82};
        private readonly string _secretMessage = "Secret";

        private readonly string _encryptedMessage =
            "mjWjwoR2YRcSeHf+OqV/uUbb94eiB13BLHu86stzgVJG2/eHogddwSx7vOrLc4FSjQTgqkta";

        [TestMethod]
        public void GeneratesKey()
        {
            var key = AESGCM.NewKey();
            Assert.IsNotNull(key);
            Assert.IsInstanceOfType(key, typeof(byte[]));
            Assert.AreEqual(AESGCM.KeyBitSize / 8, key.Length);
            Assert.IsFalse(key.All(b => b == 0));
        }

        [TestMethod]
        public void EncryptMessage()
        {
            string encrypted = AESGCM.SimpleEncrypt(_secretMessage, _key, _iv); 
            Console.WriteLine("Encrypted message is {0}", encrypted);
            Assert.IsFalse(string.IsNullOrEmpty(encrypted), "Encrypted message should not be null or empty.");
            Assert.AreNotEqual(_secretMessage, encrypted, "Encrypted message should not equal cleartext secret");
        }

        [TestMethod]
        public void DecryptsMessage()
        {
            string clearText = AESGCM.SimpleDecrypt(_encryptedMessage, _key);
            Console.WriteLine("Decrypted message is {0}", clearText);
            Assert.AreEqual(_secretMessage, clearText);
        }

        #region [ Error Checking ]

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EncryptErrorsWhenKeyIsWrongLength()
        {
            AESGCM.SimpleEncrypt("hello", _key.Take(5).ToArray(), _iv);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EncryptErrorsWhenMessageIsNull()
        {
            AESGCM.SimpleEncrypt("", _key, _iv);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EncryptBytesErrorsWhenMessageIsNull()
        {
            AESGCM.SimpleEncrypt(new byte[0], _key, _iv);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DecryptErrorsWhenMessageIsNull()
        {
            AESGCM.SimpleDecrypt("", _key);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DecryptErrorsWhenKeyIsNull()
        {
            AESGCM.SimpleDecrypt(_encryptedMessage, _key.Take(5).ToArray());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DecryptErrorsWhenKeyWrongLength()
        {
            AESGCM.SimpleDecrypt("", _key.Take(5).ToArray());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DecryptBytesErrorsWhenMessageIsNull()
        {
            AESGCM.SimpleDecrypt(new byte[0], _key);
        }

        [TestMethod]
        public void DecryptReturnsNullWhenBadKey()
        {
            string clearText = AESGCM.SimpleDecrypt(_encryptedMessage, AESGCM.NewKey());
            Assert.IsNull(clearText);
        }

        #endregion
    }
}
