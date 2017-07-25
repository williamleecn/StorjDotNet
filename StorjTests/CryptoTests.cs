using Bitcoin.BIP39;
using Bitcoin.BitcoinUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StorjDotNet;
using StorjDotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;

namespace StorjTests
{
    [TestClass]
    public class CryptoTests
    {
        private const string mnemonic24 = "amount culture oblige crystal elephant leisure run library host hurdle taxi cool odor sword parade picnic fence pass remove sudden cloud concert recipe weather";
        private const string mnemonic12 = "abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon about";
        private static BIP39 m_Bip39_24Words;
        private static BIP39 m_Bip39_12Words;

        [ClassInitialize]
        public static void TestClassinitialize(TestContext context)
        {
            m_Bip39_24Words = new BIP39(mnemonic24);
            m_Bip39_12Words = new BIP39(mnemonic12);
        }

        [TestMethod]
        public void GeneratesSeed()
        {
            string expectedSeed = "5eb00bbddcf069084889a8ab9155568165f5c453ccb85e70811aaed6f6da5fc19a5ac40b389cd370d086206dec8aa6c43daea6690f20ad3d8d48b2d2ce9e38e4";
            Assert.AreEqual(expectedSeed, BIP39.GetSeedBytesHexString(mnemonic12));
        }

        [TestMethod]
        public void GeneratesKeyPair24()
        {
            EcKeyPair keyPair = new EcKeyPair(m_Bip39_24Words.SeedBytes);
            Assert.AreEqual("00f8e2ca0ecb3c0f45feb0d819c5dd4f7b44ba4f42af15ad6d4acc98372309fc", keyPair.PrivateKey.ToHexString());
            Assert.AreEqual("02f83622b9e020a5d986f4800ae580cf60df14772e29356b93a901e20febf547ee", keyPair.PublicKey.ToHexString());
        }

        [TestMethod]
        public void GeneratesKeyPair12()
        {
            EcKeyPair keyPair = new EcKeyPair(m_Bip39_12Words.SeedBytes);
            Assert.AreEqual("5eb00bbddcf069084889a8ab9155568165f5c453ccb85e70811aaed6f6da5fc1", keyPair.PrivateKey.ToHexString());
            Assert.AreEqual("03889008040060652abb1e3a7900669ed8669f9b88d8110d8fa2935d19e55f541f", keyPair.PublicKey.ToHexString());
        }

        [TestMethod]
        public void SignsMessage()
        {
            EcKeyPair keyPair = new EcKeyPair(m_Bip39_24Words.SeedBytes);
            Crypto crypto = new Crypto(m_Bip39_24Words);
            string signedMessage = crypto.SignMessage("test message");
            Assert.IsTrue(keyPair.Verify("test message".ToByteArray(), signedMessage.HexStringToBytes()));
        }

        [TestMethod]
        public void VerifiesSignature()
        {
            EcKeyPair keyPair = new EcKeyPair(m_Bip39_24Words.SeedBytes);
            string message = "test message";
            string signature = "304402204023e48cb38bc3d12cada5376b65b8b450233ac6735ad37fa8945d3b2794169502204925749bdb9759267531696ba1285968b851093aa047c3ce5d4d42a32cd1a0a1";
            Assert.IsTrue(keyPair.Verify(message.ToByteArray(), signature.HexStringToBytes()));
        }

        [TestMethod]
        public void GeneratesBucketKey()
        {
            string mnemonic = "abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon about";
            string bucketId = "0123456789ab0123456789ab";
            string expectedBucketKey = "b2464469e364834ad21e24c64f637c39083af5067693605c84e259447644f6f6";

            string bucketKey = new Crypto(mnemonic).GenerateBucketKey(bucketId);

            Assert.AreEqual(expectedBucketKey, bucketKey);     
        }

        [TestMethod]
        public void GeneratesFileKey()
        {
            string mnemonic = "abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon about";
            string bucketId = "0123456789ab0123456789ab";
            string filename = "samplefile.txt";
            string index = "150589c9593bbebc0e795d8c4fa97304b42c110d9f0095abfac644763beca66e";
            string expectedFileKey = "bb3552fc2e16d24a147af4b2d163e3164e6dbd04bbc45fc1c3eab69f384337e9";

            string actualFileKey = new Crypto(mnemonic).GenerateFileKey(bucketId, index);
            Assert.AreEqual(expectedFileKey, expectedFileKey);
        }

        [TestMethod]
        public void EncryptsDecryptsMeta()
        {
            byte[] key = new byte[32] {215,99,0,133,172,219,64,35,54,53,171,23,146,160,
                81,126,137,21,253,171,48,217,184,188,8,137,3,4,83,50,30,251};
            byte[] iv = new byte[32] {70,219,247,135,162,7,93,193,44,123,188,234,203,115,129,
                      82,70,219,247,135,162,7,93,193,44,123,188,234,203,115,129,82};

            string meta = "encrypt this text";
            Crypto crypto = new Crypto(mnemonic12);
            string encrypted = crypto.EncryptMeta(meta, key, iv);
            Assert.IsFalse(string.IsNullOrEmpty(encrypted));
            string decrypted = crypto.DecryptMeta(encrypted, key);
            Assert.AreEqual(meta, decrypted);
        }
        
        [TestMethod]
        public void DecryptsBucketLongerName()
        {
            Bucket bucket = new Bucket()
            {
                Name = "KC4zcZRJtwrMyCZ2d2+tLQI7VScKPDCRI6yiGnp30pM3Uhz3GPdHwxOLvCvXGONbeOBmF+wL3EI7V4SD4ec3dQ0or9PuZlghlQ=="
            };

            new Crypto(m_Bip39_12Words).TryDecryptBucket(bucket);
            Assert.AreEqual("EncryptedBucketLongerName", bucket.Name);
        }

        [TestMethod]
        public void DecryptsBucketName()
        {
            Bucket bucket = new Bucket()
            {
                Name = "MBfnRAFCQFDwfitJixvOViAYCfg3ZsByiqvymUwOTykwdMH8r3zAjgdk84UEcSx+X+oQAE4F4CYGGu4skGan"
            };

            new Crypto(m_Bip39_12Words).TryDecryptBucket(bucket);
            Assert.AreEqual("EncryptedBucket", bucket.Name);
        }

        [TestMethod]
        public void EncryptsBucketName()
        {
            Bucket bucket = new Bucket()
            {
                Name = "EncryptedBucket"
            };
            new Crypto(m_Bip39_12Words).EncryptBucketName(bucket);

            Assert.IsFalse(string.IsNullOrEmpty(bucket.Name));
            Assert.AreNotEqual("EncryptedBucket", bucket.Name);
            Console.WriteLine("Bucket name is {0}",bucket.Name);
        }

        [TestMethod]
        public void HMACTest()
        {
            // parameters from libnettle hmac tests
            byte[] key = "Jefe".ToByteArray();
            byte[] update = "what do ya want for nothing?".ToByteArray();
            byte[] shouldEqual = "164b7a7bfcf819e2e395fbe73b56e0a387bd64222e831fd610270cd7ea2505549758bf75c05a994a6d034f65f8f0e6fdcaeab1a34d4a6b4b636e070a38bce737".HexStringToBytes();

            byte[] digest = new Crypto(m_Bip39_24Words).GetHMAC_SHA512(key, update);

            CollectionAssert.AreEqual(shouldEqual, digest);
        }
    }
}
