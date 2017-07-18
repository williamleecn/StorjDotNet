using Bitcoin.BIP39;
using StorjDotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorjDotNet
{
    public interface IStorj
    {
        Task<string> GenerateMnemonic(int strength);
        Task<string> GenerateMnemonic(int strength, string passphrase);
        Task<string> GenerateMnemonic(int strength, string passphrase, MnemonicLanguage language);
        Task<Bridge> GetBridge();
        Task<BridgeUser> BridgeRegister(string email, string password);
        Task<IEnumerable<Bucket>> GetBuckets();
        Task<Bucket> CreateBucket(Bucket bucket);
    }
}
