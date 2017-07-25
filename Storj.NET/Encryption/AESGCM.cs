using System;
using System.IO;
using System.Text;
using System.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace StorjDotNet.Encryption
{
    public static class AESGCM
    {
        private static readonly SecureRandom Random = new SecureRandom();

        public const int GcmDigestLength = 16;

        // TODO: enforce this?
        public const int IvLength = 32;

        //Preconfigured Encryption Parameters
        public const int NonceBitSize = 128;
        public const int MacBitSize = 128;
        public const int KeyBitSize = 256;

        /// <summary>
        /// Helper that generates a random new key on each call.
        /// </summary>
        /// <returns></returns>
        public static byte[] NewKey()
        {
            var key = new byte[KeyBitSize / 8];
            Random.NextBytes(key);
            return key;
        }

        /// <summary>
        /// Simple Encryption And Authentication (AES-GCM) of a UTF8 string.
        /// </summary>
        /// <param name="secretMessage">The secret message.</param>
        /// <param name="key">The key.</param>
        /// <param name="iv">The IV used to encrypt.</param>
        /// <returns>
        /// Encrypted Message
        /// </returns>
        /// <exception cref="System.ArgumentException">Secret Message Required!;secretMessage</exception>
        /// <remarks>
        /// Adds overhead of (Optional-Payload + BlockSize(16) + Message +  HMac-Tag(16)) * 1.33 Base64
        /// </remarks>
        public static string SimpleEncrypt(string secretMessage, byte[] key, byte[] iv)
        {
            if (string.IsNullOrEmpty(secretMessage))
                throw new ArgumentException("Secret Message Required!", nameof(secretMessage));

            var plainText = Encoding.UTF8.GetBytes(secretMessage);
            var cipherText = SimpleEncrypt(plainText, key, iv);
            return Convert.ToBase64String(cipherText);
        }


        /// <summary>
        /// Simple Decryption & Authentication (AES-GCM) of a UTF8 Message
        /// </summary>
        /// <param name="encryptedMessage">The encrypted message.</param>
        /// <param name="key">The key.</param>
        /// <returns>Decrypted Message</returns>
        public static string SimpleDecrypt(string encryptedMessage, byte[] key)
        {
            if (string.IsNullOrEmpty(encryptedMessage))
                throw new ArgumentException("Encrypted Message Required!", nameof(encryptedMessage));

            byte[] cipherText;
            try
            {
                cipherText = Convert.FromBase64String(encryptedMessage);
            }
            catch (FormatException)
            {
                return null;
            }
            var plaintext = SimpleDecrypt(cipherText, key);
            return plaintext == null ? null : Encoding.UTF8.GetString(plaintext).Replace("\0","");
        }

        public static byte[] SimpleEncrypt(byte[] secretMessage, byte[] key, byte[] iv)
        {
            //User Error Checks
            if (key == null || key.Length != KeyBitSize / 8)
                throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), nameof(key));

            if (secretMessage == null || secretMessage.Length == 0)
                throw new ArgumentException("Secret Message Required!", nameof(secretMessage));

            var cipher = new GcmBlockCipher(new AesFastEngine());
            var parameters = new ParametersWithIV(new KeyParameter(key), iv);
            cipher.Init(true, parameters);

            //Generate Cipher Text With Auth Tag
            var cipherText = new byte[cipher.GetOutputSize(secretMessage.Length)];
            var len = cipher.ProcessBytes(secretMessage, 0, secretMessage.Length, cipherText, 0);
            cipher.DoFinal(cipherText, len);

            //Assemble Message
            using (var combinedStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(combinedStream))
                {
                    //Prepend Authenticated Payload
                    binaryWriter.Write(cipher.GetMac());
                    //Prepend Nonce
                    binaryWriter.Write(iv);
                    //Write Cipher Text
                    binaryWriter.Write(cipherText, 0, secretMessage.Length);
                }
                return combinedStream.ToArray();
            }
        }

        public static byte[] SimpleDecrypt(byte[] encryptedMessage, byte[] key)
        {
            //User Error Checks
            if (key == null || key.Length != KeyBitSize / 8)
                throw new ArgumentException(string.Format("Key needs to be {0} bit!", KeyBitSize), nameof(key));

            if (encryptedMessage == null || encryptedMessage.Length == 0)
                throw new ArgumentException("Encrypted Message Required!", nameof(encryptedMessage));

            if (encryptedMessage.Length <= IvLength + GcmDigestLength)
            {
                // no encrypted message
                return null;
            }
            using (var cipherStream = new MemoryStream(encryptedMessage))
            using (var cipherReader = new BinaryReader(cipherStream))
            {
                //Grab MAC/digest
                var mac = cipherReader.ReadBytes(GcmDigestLength);

                //Grab IV
                var iv = cipherReader.ReadBytes(IvLength);

                var cipher = new GcmBlockCipher(new AesFastEngine());
                var parameters = new ParametersWithIV(new KeyParameter(key), iv);
                cipher.Init(false, parameters);

                //Decrypt Cipher Text
                var cipherText = cipherReader.ReadBytes(encryptedMessage.Length - GcmDigestLength - iv.Length);
                var cipherTextWithMac = new byte[cipherText.Length + mac.Length];
                cipherText.CopyTo(cipherTextWithMac, 0);
                mac.CopyTo(cipherTextWithMac, cipherText.Length);
                var plainText = new byte[cipher.GetOutputSize(cipherTextWithMac.Length)];

                try
                {
                    var len = cipher.ProcessBytes(cipherTextWithMac, 0, cipherTextWithMac.Length, plainText, 0);
                    cipher.DoFinal(plainText, len);

                }
                catch (InvalidCipherTextException ex)
                {
                    // TODO: remove this once working
                    Console.WriteLine(ex.Message);
                    //Return null if it doesn't authenticate
                    return null;
                }
                return plainText;
            }
        }
    }
}