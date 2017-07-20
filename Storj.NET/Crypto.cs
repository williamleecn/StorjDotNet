using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bitcoin.BIP39;
using Bitcoin.BitcoinUtilities;
using System.Security.Cryptography;
using StorjDotNet.Models;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using System.IO;
using Security.Cryptography;

namespace StorjDotNet
{
    public class Crypto
    {
        private const string BUCKET_NAME_MAGIC = "398734aab3c4c30c9f22590e83a95f7e43556a45fc2b3060e0c39fde31f50272";
        private readonly byte[] BUCKET_META_MAGIC = new byte[32] { 66, 150, 71, 16, 50, 114, 88, 160, 163, 35, 154, 65, 162, 213, 226, 215, 70, 138, 57, 61, 52, 19, 210, 170, 38, 164, 162, 200, 86, 201, 2, 81 };
        private const int SHA256_DIGEST_SIZE = 32;
        private const int SHA512_DIGEST_SIZE = 64;
        private const int GCM_DIGEST_SIZE = 16;
        private const int AES_BLOCK_SIZE = 16;

        private const int DETERMINISTIC_KEY_LENGTH = 64;

        private readonly EcKeyPair m_KeyPair;
        private readonly byte[] m_Seed;

        public Crypto(string mnemonic)
        {
            m_Seed = BIP39.GetSeedBytes(mnemonic);
            m_KeyPair = new EcKeyPair(m_Seed);
        }

        public Crypto(BIP39 bip39)
        {
            m_Seed = bip39.SeedBytes;
            m_KeyPair = new EcKeyPair(m_Seed);
        }

        public string Pubkey
        {
            get
            {
                return m_KeyPair.PublicKey.ToHexString();
            }
        }
        public string SignMessage(string message)
        {
            return m_KeyPair.Sign(message.ToByteArray()).ToHexString();
        }

        public void EncryptBucketName(Bucket bucket)
        {
            string bucketKey = GenerateBucketKey(BUCKET_NAME_MAGIC);
            
            byte[] key = GetHMAC_SHA512(bucketKey.ToByteArray().Take(SHA256_DIGEST_SIZE).ToArray(),
                BUCKET_META_MAGIC.Take(SHA256_DIGEST_SIZE).ToArray());

            byte[] bucketNameIv = GetHMAC_SHA512(bucketKey.HexStringToBytes().Take(SHA256_DIGEST_SIZE).ToArray(),
                bucket.Name.ToByteArray());

            bucket.Name = EncryptMeta(bucket.Name, key, bucketNameIv);
            

        }

        public void TryDecryptBuckets(IEnumerable<Bucket> buckets)
        {
            foreach(Bucket bucket in buckets)
            {
                TryDecryptBucket(bucket);
            }
        }

        public void TryDecryptBucket(Bucket bucket)
        {
            string bucketKey = GenerateBucketKey(BUCKET_NAME_MAGIC);
            byte[] hmac = GetHMAC_SHA512(bucketKey.ToByteArray(), BUCKET_META_MAGIC);
            byte[] key = hmac.Take(SHA256_DIGEST_SIZE).ToArray();
            string decryptedBucketName = DecryptMeta(bucket.Name, key);
            bucket.Name = decryptedBucketName;
            
        }

        public byte[] GetHMAC_SHA512(byte[] key, byte[] update)
        {
            HMac hmac = new HMac(new Sha512Digest());
            hmac.Init(new KeyParameter(key));
            foreach (byte b in update)
            {
                hmac.Update(b);
            }
            byte[] digest = new byte[SHA512_DIGEST_SIZE];
            hmac.DoFinal(digest, 0);
            return digest;
        }

        public string EncryptMeta(string meta, byte[] key, byte[] iv)
        {
            using (var aes = new AuthenticatedAesCng())
            {
                aes.CngMode = CngChainingMode.Gcm;
                aes.Key = key;
                aes.IV = iv;
                aes.Padding = PaddingMode.None;
                
                byte[] ciphertext;
                byte[] digest;
                using (MemoryStream ms = new MemoryStream())
                using (IAuthenticatedCryptoTransform encryptor = aes.CreateAuthenticatedEncryptor())
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    // Encrypt the secret message
                    byte[] plaintext = meta.ToByteArray();
                    cs.Write(plaintext, 0, plaintext.Length);

                    // Finish the encryption and get the output authentication tag and ciphertext
                    cs.FlushFinalBlock();
                    digest = encryptor.GetTag();
                    ciphertext = ms.ToArray();
                }

                // TODO: GCM digest + IV + encrypted name
                return Convert.ToBase64String(ciphertext);
            }
        }

        public string DecryptMeta(string encryptedMeta, byte[] key)
        {
            byte[] encryptedBucketName = Convert.FromBase64String(encryptedMeta);
            byte[] digest = encryptedBucketName.Take(GCM_DIGEST_SIZE).ToArray();
            byte[] iv = encryptedBucketName.Skip(GCM_DIGEST_SIZE).Take(SHA256_DIGEST_SIZE).ToArray().ToHexString().HexStringToBytes();
            byte[] cipherText = encryptedBucketName.Skip(GCM_DIGEST_SIZE + SHA256_DIGEST_SIZE).ToArray();
            byte[] clearText;

            var aes = new AesManaged();
            var blocksize = aes.BlockSize;
            var blocksizes = aes.LegalBlockSizes;
            using (var decryptor = aes.CreateDecryptor(key, iv))
            {
                using (var memStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Write))
                    {
                        using (var streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(cipherText);
                        }
                        cryptoStream.FlushFinalBlock();
                    }
                    clearText = memStream.ToArray();
                }
            }

            return new string(Encoding.UTF8.GetChars(clearText));
        }

        public static void EncryptFile(string sourceFilename, string destinationFilename, string password)
        {
            byte[] seasalt = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            int iterations = 1042;
            AesManaged aes = new AesManaged();
            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            // NB: Rfc2898DeriveBytes initialization and subsequent calls to   GetBytes   must be eactly the same, including order, on both the encryption and decryption sides.
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, seasalt, iterations);
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);
            aes.Mode = CipherMode.CBC;
            ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);

            using (FileStream destination = new FileStream(destinationFilename, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using (CryptoStream cryptoStream = new CryptoStream(destination, transform, CryptoStreamMode.Write))
                {
                    using (FileStream source = new FileStream(sourceFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        source.CopyTo(cryptoStream);
                        cryptoStream.FlushFinalBlock();
                    }
                }
            }

        }

        public void DecryptFile(string sourceFilename, string destinationFilename, string password)
        {
            byte[] seasalt = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            int iterations = 1042;

            AesManaged aes = new AesManaged();
            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            // NB: Rfc2898DeriveBytes initialization and subsequent calls to   GetBytes   must be eactly the same, including order, on both the encryption and decryption sides.
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, seasalt, iterations);
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);
            aes.Mode = CipherMode.CBC;
            ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);

            using (FileStream destination = new FileStream(destinationFilename, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using (CryptoStream cryptoStream = new CryptoStream(destination, transform, CryptoStreamMode.Write))
                {
                    try
                    {
                        using (FileStream source = new FileStream(sourceFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            source.CopyTo(cryptoStream);
                            //ErrorLog(0, "Shard Length After Decrypt: " + source.Length.ToString(), "StorjBase");
                            source.Flush();
                            //ErrorLog(0, "Shard Length After Decrypt Flush: " + source.Length.ToString(), "StorjBase");
                        }
                    }
                    catch (Exception ex)
                    {
                        //ErrorLog(0, "ERROR #01102017209: " + ex.Message, "StorjBase");
                    }
                }
            }
            using (FileStream source = new FileStream(destinationFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                //ErrorLog(0, "Shard Length After Decrypt: " + source.Length.ToString(), "StorjBase");
            }

        }

        public string GenerateBucketKey(string bucketId)
        {
            return GetDeterministicKey(m_Seed, 128, bucketId);
        }

        private string GetDeterministicKey(byte[] seed, int keyLength, string id)
        {
            string sha512InputString = seed.ToHexString() + id;
            byte[] sha512Input = sha512InputString.HexStringToBytes();
            SHA512 sha = SHA512.Create();
            byte[] shaHash = sha.ComputeHash(sha512Input);
            return new string(shaHash.ToHexString().Take(64).ToArray());
        }
    }
}
