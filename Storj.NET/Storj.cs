using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bitcoin.BIP39;

namespace StorjDotNet
{
    public class Storj : IStorj
    {
        public async Task<string> GenerateMnemonic(int strength)
        {
            return await GenerateMnemonic(strength, string.Empty, MnemonicLanguage.English);
        }

        public async Task<string> GenerateMnemonic(int strength, string passphrase)
        {
            return await GenerateMnemonic(strength, passphrase, MnemonicLanguage.English);
        }

        public async Task<string> GenerateMnemonic(int strength, string passphrase, MnemonicLanguage language)
        {
            Task<BIP39> mnemonicGenerator = BIP39.GetBIP39Async(strength, passphrase, Helpers.StorjToBIP39Language(language));
            return (await mnemonicGenerator).MnemonicSentence;
        }
    }
}
