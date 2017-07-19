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

namespace StorjDotNet
{
    public class Crypto
    {
        private const string BUCKET_NAME_MAGIC = "398734aab3c4c30c9f22590e83a95f7e43556a45fc2b3060e0c39fde31f50272";
        private readonly byte[] BUCKET_META_MAGIC = new byte[32] { 66, 150, 71, 16, 50, 114, 88, 160, 163, 35, 154, 65, 162, 213, 226, 215, 70, 138, 57, 61, 52, 19, 210, 170, 38, 164, 162, 200, 86, 201, 2, 81 };
        private readonly int SHA256_DIGEST_SIZE = new Sha256Digest().GetDigestSize();
        private readonly int SHA512_DIGEST_SIZE = new Sha512Digest().GetDigestSize();
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
            byte[] bucketKey = GenerateBucketKey(BUCKET_NAME_MAGIC);
            
            byte[] key = GetHMAC_SHA512(bucketKey.Take(SHA256_DIGEST_SIZE).ToArray(),
                BUCKET_META_MAGIC.Take(SHA256_DIGEST_SIZE).ToArray());

            byte[] bucketNameIv = GetHMAC_SHA512(bucketKey.Take(SHA256_DIGEST_SIZE).ToArray(),
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
            byte[] bucketKey = GenerateBucketKey(BUCKET_NAME_MAGIC);

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

        private string EncryptMeta(string meta, byte[] encryptKey, byte[] encryptIv)
        {
            
            byte[] cipherText;
            
            using (var aesGcm = SymmetricAlgorithm.Create("Aes"))
            {
                using (var encryptor = aesGcm.CreateEncryptor(encryptKey, encryptIv))
                {
                    using (var memStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write))
                        {
                            using (var streamWriter = new StreamWriter(cryptoStream))
                            {
                                streamWriter.Write(meta);
                            }

                            cipherText = memStream.ToArray();
                        }
                    }
                }
            }

            // TODO: GCM digest + IV + encrypted name
            return Convert.ToBase64String(cipherText);

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

        public byte[] GenerateBucketKey(string bucketId)
        {
            return GetDeterministicKey(m_Seed, 128, bucketId.ToByteArray());
        }

        private byte[] GetDeterministicKey(byte[] seed, int keyLength, byte[] id)
        {
            byte[] sha512Input = new byte[keyLength + id.Length];
            seed.CopyTo(sha512Input, 0);
            id.CopyTo(sha512Input, keyLength);

            SHA512 sha = SHA512.Create();
            byte[] shaHash = sha.ComputeHash(sha512Input);
            return shaHash.Take(DETERMINISTIC_KEY_LENGTH).ToArray();
        }
    }
}
