﻿using System;
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
using StorjDotNet.Encryption;

namespace StorjDotNet
{
    public class Crypto
    {
        private const string BucketNameMagic = "398734aab3c4c30c9f22590e83a95f7e43556a45fc2b3060e0c39fde31f50272";
        private readonly byte[] _bucketMetaMagic = new byte[32] { 66, 150, 71, 16, 50, 114, 88, 160, 163, 35, 154, 65, 162, 213, 226, 215, 70, 138, 57, 61, 52, 19, 210, 170, 38, 164, 162, 200, 86, 201, 2, 81 };
        private const int Sha256DigestSize = 32;
        private const int Sha512DigestSize = 64;

        private const int DeterministicKeyLength = 64;

        private readonly EcKeyPair _keyPair;
        private readonly byte[] _seed;

        public Crypto(string mnemonic)
        {
            _seed = BIP39.GetSeedBytes(mnemonic);
            _keyPair = new EcKeyPair(_seed);
        }

        public Crypto(BIP39 bip39)
        {
            _seed = bip39.SeedBytes;
            _keyPair = new EcKeyPair(_seed);
        }

        public string Pubkey => _keyPair.PublicKey.ToHexString();
            
        public string SignMessage(string message)
        {
            return _keyPair.Sign(message.ToByteArray()).ToHexString();
        }

        public void EncryptBucketName(CreateBucketRequestModel bucket)
        {
            bucket.Name = EncryptMeta(bucket.Name, BucketNameMagic);
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
            string decryptedBucketName = DecryptMeta(bucket.Name, BucketNameMagic);
            bucket.Name = decryptedBucketName ?? bucket.Name;
            
        }

        internal void TryDecryptFileNames(IEnumerable<StorjFile> files)
        {
            foreach (var file in files)
            {
                TryDecryptFileName(file);
            }
        }

        public void TryDecryptFileName(StorjFile file)
        {
            if (string.IsNullOrEmpty(file?.Bucket))
            {
                throw new ArgumentException("File is invalid, must contain bucket reference", nameof(file));
            }
            string decryptedBucketName = DecryptMeta(file.FileName, file.Bucket);
            file.FileName = decryptedBucketName ?? file.FileName;
        }

        public string EncryptMeta(string meta, string bucketId)
        {
            string bucketKey = GenerateBucketKey(bucketId);
            byte[] key = GetHmacSha512Key(bucketKey.HexStringToBytes(), _bucketMetaMagic);
            byte[] iv = GetHmacSha512Key(bucketKey.HexStringToBytes(), meta.ToByteArray()).Take(Sha256DigestSize).ToArray();
            return AESGCM.SimpleEncrypt(meta, key, iv);
        }

        public string DecryptMeta(string encryptedMeta, string bucketId)
        {
            string bucketKey = GenerateBucketKey(bucketId);
            byte[] hmac = GetHmacSha512Key(bucketKey.HexStringToBytes(), _bucketMetaMagic);
            byte[] key = hmac.Take(Sha256DigestSize).ToArray();
            return AESGCM.SimpleDecrypt(encryptedMeta, key);
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
            return GetDeterministicKey(_seed, 128, bucketId);
        }

        public string GenerateFileKey(string bucketId, string index)
        {
            string bucketKey = GenerateBucketKey(bucketId);
            return GetDeterministicKey(bucketKey.HexStringToBytes(), 64, index);
        }

        private string GetDeterministicKey(byte[] seed, int keyLength, string id)
        {
            string sha512InputString = seed.ToHexString() + id;
            byte[] sha512Input = sha512InputString.HexStringToBytes();
            SHA512 sha = SHA512.Create();
            byte[] shaHash = sha.ComputeHash(sha512Input);
            return new string(shaHash.ToHexString().Take(64).ToArray());
        }

        private byte[] GetHmacSha512Key(byte[] key, byte[] update)
        {
            HMac hmac = new HMac(new Sha512Digest());
            hmac.Init(new KeyParameter(key));
            hmac.BlockUpdate(update, 0, update.Length);
            byte[] digest = new byte[hmac.GetMacSize()];
            hmac.DoFinal(digest, 0);
            return digest.Take(Sha256DigestSize).ToArray();
        }
    }
}
