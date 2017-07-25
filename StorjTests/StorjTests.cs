using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StorjDotNet;
using StorjDotNet.Models;
using System.Net.Http;

namespace StorjTests
{
    [TestClass]
    public class StorjTests
    {
        private const string mnemonic24 = "amount culture oblige crystal elephant leisure run library host hurdle taxi cool odor sword parade picnic fence pass remove sudden cloud concert recipe weather";
        private const string mnemonic12 = "abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon about";
        private const string defaultPass = "f7a7cf08e76001545c9767265503802dd972a7dd3bfb430b54b664dda5ba6529";
        private const string defaultUser = "ssa3512+StorjDotNetCI@gmail.com";
        private const string defaultBridgeUrl = "api.storj.io";
        private static string m_EcdsaKey;
        private static IStorj m_LibStorjBasicAuth;
        private static IStorj m_LibStorjEcdsaAuth;
        private static string m_bridgeUser;

        [ClassInitialize]
        public static void TestClassinitialize(TestContext context)
        {
            m_bridgeUser = context.Properties.Contains("bridgeUser") ?
                context.Properties["bridgeUser"].ToString() : defaultUser;
            string bridgePassword = context.Properties.Contains("bridgePass") ?
                context.Properties["bridgePass"].ToString() : defaultPass;
            string bridgeHost = context.Properties.Contains("bridgeUrl") ?
                context.Properties["bridgeUrl"].ToString() : defaultBridgeUrl;
            string mnemonic = context.Properties.Contains("testMnemonic12") ?
                context.Properties["testMnemonic12"].ToString() : mnemonic12;

            var basicAuthBridgeOptions = new BridgeOptions()
            {
                BridgeUrl = bridgeHost,
                Protocol = BridgeProtocol.HTTPS,
                Username = m_bridgeUser,
                DefaultAuthenticationMethod = AuthenticationMethod.Basic
            };
            basicAuthBridgeOptions.SetPasswordHash(bridgePassword);

            var ecdsaAuthBridgeOptions = new BridgeOptions()
            {
                BridgeUrl = bridgeHost,
                Protocol = BridgeProtocol.HTTPS,
                Username = m_bridgeUser,
                DefaultAuthenticationMethod = AuthenticationMethod.ECDSA
            };

            var keyPair = new Bitcoin.BitcoinUtilities.EcKeyPair(Bitcoin.BIP39.BIP39.GetSeedBytes(mnemonic));
            m_EcdsaKey = keyPair.PublicKey.ToHexString();

            var encryptionOptions = new EncryptionOptions(mnemonic);
            
            m_LibStorjBasicAuth = new Storj(basicAuthBridgeOptions, encryptionOptions);
            m_LibStorjEcdsaAuth = new Storj(ecdsaAuthBridgeOptions, encryptionOptions);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldRequireBridgeOptions()
        {
            new Storj(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldRequireEncryptionOptions()
        {
            BridgeOptions bridgeOptions = new BridgeOptions()
            {
                DefaultAuthenticationMethod = AuthenticationMethod.ECDSA
            };
            new Storj(bridgeOptions, null);
        }

        [TestMethod]
        public void ShouldGenerateMnemonic12Words()
        {
            string mnemonic = m_LibStorjBasicAuth.GenerateMnemonic(128).Result;
            Assert.IsFalse(string.IsNullOrEmpty(mnemonic), "Mnemonic should not be null or empty");
            Assert.IsTrue(mnemonic.Split(' ').Length == 12, "Mnemonic should be 24 words");
            Console.WriteLine("Mnemonic is \"{0}\"", mnemonic);
        }

        [TestMethod]
        public void ShouldGenerateMnemonic24Words()
        {
            string mnemonic = m_LibStorjBasicAuth.GenerateMnemonic(256).Result;
            Assert.IsFalse(string.IsNullOrEmpty(mnemonic), "Mnemonic should not be null or empty");
            Assert.IsTrue(mnemonic.Split(' ').Length == 24, "Mnemonic should be 24 words");
            Console.WriteLine("Mnemonic is \"{0}\"", mnemonic);
        }

        [TestMethod]
        public async Task ShouldGetBridge()
        {
            Bridge bridge = await m_LibStorjBasicAuth.GetBridge();
            Assert.IsNotNull(bridge);
            Assert.IsNotNull(bridge.Info);
            Assert.AreEqual(bridge.Info.Title, "Storj Bridge");
        }

        [TestMethod]
        public async Task ShouldGetContacts()
        {
            IEnumerable<Contact> contacts = await m_LibStorjBasicAuth.GetContacts();
            Assert.IsNotNull(contacts);
            Assert.IsTrue(contacts.Any(), "Contact count should be greater than zero.");
            Assert.IsTrue(contacts.All(c => !string.IsNullOrEmpty(c.NodeId)), "All contacts should have a node ID");
        }

        [TestMethod]
        public async Task ShouldGetContactsFiltered()
        {
            string expectedAddress = "storj3.ddns.net";
            ContactListRequestModel model = new ContactListRequestModel()
            {
                Address = expectedAddress 
            };
            IEnumerable<Contact> contacts = await m_LibStorjBasicAuth.GetContacts(model);
            Assert.IsNotNull(contacts);
            Assert.IsTrue(contacts.Any());
            Assert.IsTrue(contacts.All(c => c.Address == expectedAddress), "All contacts retrieved should have specified address");
        }

        [TestMethod]
        public async Task ShouldGetContact()
        {
            var contactModel = new ContactRequestModel()
            {
                NodeId = "b7d0cdcca39ea63c1d226d164ee8086ecc99c2db"
            };
            
            Contact contact = await m_LibStorjBasicAuth.GetContact(contactModel);
            Assert.IsNotNull(contact);
            Assert.AreEqual("b7d0cdcca39ea63c1d226d164ee8086ecc99c2db", contact.NodeId);
            Assert.IsTrue(contact.LastSeenDateTime > new DateTime(2017, 7, 21), "Contact should be seen after July 21, 2017");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException), "Email is already registered")]
        public async Task ShouldGetUserExistsError()
        {
            try
            {
                BridgeUser user = await m_LibStorjBasicAuth.BridgeRegister(m_bridgeUser, "password");
                Assert.Fail("Register should error");
            }
            catch(HttpRequestException ex)
            {
                Assert.AreEqual(ex.Message, "Email is already registered");
                throw;
            }
            
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException), "Public key is already registered")]
        public async Task ShouldGetKeyAlreadyRegisteredError()
        {
            AuthKeyRequestModel model = new AuthKeyRequestModel()
            {
                Key = m_EcdsaKey
            };
            try
            {
                AuthKeyModel authKeyResponse = await m_LibStorjBasicAuth.RegisterAuthKey(model);
                Assert.Fail("Register should error");
            }
            catch(HttpRequestException ex)
            {
                Assert.AreEqual(ex.Message, "Public key is already registered");
                throw;
            }
        }

        [TestMethod]
        public async Task ShouldGetBucketsBasicAuth()
        {
            var buckets = await m_LibStorjBasicAuth.GetBuckets();
            Assert.IsNotNull(buckets);
            Assert.IsTrue(buckets.Any());
        }

        [TestMethod]
        public async Task ShouldCreateBucket()
        {
            Bucket createdBucket = await m_LibStorjBasicAuth.CreateBucket(null);
            Assert.IsNotNull(createdBucket);
            Console.WriteLine("Created bucket {0}", createdBucket.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException), "Name already used by another bucket")]
        public async Task ShouldFailBucketExists()
        {
            CreateBucketRequestModel request = new CreateBucketRequestModel()
            {
                Name = "BucketExists"
            };
            try
            {
                Bucket createdBucket = await m_LibStorjBasicAuth.CreateBucket(request);
                Assert.Fail("CreateBucket should error");
            }
            catch(HttpRequestException ex)
            {
                Assert.AreEqual(ex.Message, "Name already used by another bucket");
                throw;
            }
            
            
        }
    }
}
