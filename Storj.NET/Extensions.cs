using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StorjDotNet
{
    public static class Extensions
    {
        public static byte[] ToByteArray(this string value)
        {
            return Encoding.ASCII.GetBytes(value);
        }

        public static byte[] HexStringToBytes(this string value)
        {
            return Enumerable.Range(0, value.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(value.Substring(x, 2), 16))
                     .ToArray();
        }

        public static string ToAsciiString(this byte[] value)
        {
            StringBuilder hex = new StringBuilder(value.Length * 2);
            foreach(byte b in value)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }

        public static string GetSha256Hash(this string valueToHash)
        {
            byte[] valueToHashBytes = valueToHash.ToByteArray();
            SHA256 sha = SHA256.Create();
            byte[] hashedValue = sha.ComputeHash(valueToHashBytes);
            return hashedValue.ToAsciiString();
        }
    }
}
