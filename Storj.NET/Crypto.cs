using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bitcoin.BIP39;
using Bitcoin.BitcoinUtilities;

namespace StorjDotNet
{
    public class Crypto
    {
        private readonly EcKeyPair m_KeyPair;

        public Crypto(string mnemonic)
        {
            m_KeyPair = new EcKeyPair(BIP39.GetSeedBytes(mnemonic));
        }

        public Crypto(BIP39 bip39)
        {
            m_KeyPair = new EcKeyPair(bip39.SeedBytes);
        }

        public string Pubkey
        {
            get
            {
                return m_KeyPair.PublicKey.ToAsciiString();
            }
        }
        public string SignMessage(string message)
        {
            return m_KeyPair.Sign(message.ToByteArray()).ToAsciiString();
        }
    }
}
