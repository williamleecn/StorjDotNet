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
        private const string defaultMnemonic24 = "amount culture oblige crystal elephant leisure run library host hurdle taxi cool odor sword parade picnic fence pass remove sudden cloud concert recipe weather";
        private const string defaultMnemonic12 = "zoo cake isolate rapid stereo change finish length second camp spoil endless";
        private static BIP39 m_Bip39_24Words;
        private static BIP39 m_Bip39_12Words;

        [ClassInitialize]
        public static void TestClassinitialize(TestContext context)
        {
            string mnemonic24 = context.Properties.Contains("testMnemonic24") ?
                context.Properties["testMnemonic24"].ToString() : defaultMnemonic24;
            string mnemonic12 = context.Properties.Contains("testMnemonic12") ?
                context.Properties["testMnemonic12"].ToString() : defaultMnemonic12;

            m_Bip39_24Words = new BIP39(mnemonic24);
            m_Bip39_12Words = new BIP39(mnemonic12);
        }

        [TestMethod]
        public void GeneratesSeed()
        {
            string mnemonic = "abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon about";
            string expectedSeed = "5eb00bbddcf069084889a8ab9155568165f5c453ccb85e70811aaed6f6da5fc19a5ac40b389cd370d086206dec8aa6c43daea6690f20ad3d8d48b2d2ce9e38e4";
            Assert.AreEqual(expectedSeed, BIP39.GetSeedBytesHexString(mnemonic));
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
            Assert.AreEqual("47048f994c9c53833fb19b2a332070074601aab69b35b9d5ceb2a2fa4a13f4e2", keyPair.PrivateKey.ToHexString());
            Assert.AreEqual("03b3fe5883a563a6aa8fdd867608d14a27ad2ab197aa181c12c2b8e060cadb78fd", keyPair.PublicKey.ToHexString());
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
            string bucket_id = "0123456789ab0123456789ab";
            string expected_bucket_key = "b2464469e364834ad21e24c64f637c39083af5067693605c84e259447644f6f6";

            char[] expectedBucketKeyArray = expected_bucket_key.ToCharArray();

            Console.WriteLine("Expected bucket key length is {0}", expected_bucket_key.Length);
            string bucket_key = new Crypto(mnemonic).GenerateBucketKey(bucket_id);
            Console.WriteLine("Actual bucket key length is {0}", bucket_key.Length);
            char[] bucketKeyChars = new char[bucket_key.Length];
            //bucket_key.CopyTo(bucketKeyChars, 0);
            //string chars = new string(Encoding.UTF8.GetChars(bucket_key));
            Assert.AreEqual(expected_bucket_key, bucket_key);
            
        }

        [TestMethod]
        public void DecryptsBucketName()
        {
            Bucket bucket = new Bucket()
            {
                User = "ssa3512+StorjDotNetCI@gmail.com",
                EncryptionKey = string.Empty,
                PublicPermissions = new string[0],
                Created = "2017-07-19T14:39:27.116Z",
                Name = "VlEEPnbgeOshaNr2/9XDA52nrBe4DDiDKfAFbBqxMMPF1QgwTxyBrjZp6VWplK14TttWafLwXw8HEs869FqY",
                Pubkeys = new string[0],
                Status = "Active",
                Transfer = 0,
                Storage = 0,
                Id = "a97a342dcb351184303a26bb"
            };

            new Crypto(m_Bip39_24Words).TryDecryptBucket(bucket);
            Assert.AreEqual("EncryptedBucket", bucket.Name);
        }

        [TestMethod]
        public void EncryptsBucketName()
        {
            Bucket bucket = new Bucket()
            {
                Name = "EncryptedBucket"
            };
            new Crypto(m_Bip39_24Words).EncryptBucketName(bucket);
            Assert.AreEqual("VlEEPnbgeOshaNr2/9XDA52nrBe4DDiDKfAFbBqxMMPF1QgwTxyBrjZp6VWplK14TttWafLwXw8HEs869FqY", bucket.Name);
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
