using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorjDotNet
{
    public class EncryptionOptions
    {
        private string m_Mnemonic;

        public EncryptionOptions(string mnemonic)
        {
            m_Mnemonic = mnemonic;
        }

        public Crypto GetCrypto()
        {
            return new Crypto(m_Mnemonic);
        }
    }
}
