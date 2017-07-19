using Bitcoin.BIP39;
using Bitcoin.BitcoinUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StorjDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public void GeneratesKeyPair24()
        {
            EcKeyPair keyPair = new EcKeyPair(m_Bip39_24Words.SeedBytes);
            Assert.AreEqual("00f8e2ca0ecb3c0f45feb0d819c5dd4f7b44ba4f42af15ad6d4acc98372309fc", keyPair.PrivateKey.ToAsciiString());
            Assert.AreEqual("02f83622b9e020a5d986f4800ae580cf60df14772e29356b93a901e20febf547ee", keyPair.PublicKey.ToAsciiString());
        }

        [TestMethod]
        public void GeneratesKeyPair12()
        {
            EcKeyPair keyPair = new EcKeyPair(m_Bip39_12Words.SeedBytes);
            Assert.AreEqual("47048f994c9c53833fb19b2a332070074601aab69b35b9d5ceb2a2fa4a13f4e2", keyPair.PrivateKey.ToAsciiString());
            Assert.AreEqual("03b3fe5883a563a6aa8fdd867608d14a27ad2ab197aa181c12c2b8e060cadb78fd", keyPair.PublicKey.ToAsciiString());
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
    }
}
