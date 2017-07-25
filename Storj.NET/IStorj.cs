using Bitcoin.BIP39;
using StorjDotNet.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
        Task<IEnumerable<Contact>> GetContacts();
        Task<IEnumerable<Contact>> GetContacts(ContactListRequestModel model);
        Task<Contact> GetContact(ContactRequestModel model);
        Task<BridgeUser> BridgeRegister(string email, string password);
        Task<Bucket> GetBucket(Bucket model);
        Task<IEnumerable<Bucket>> GetBuckets();
        Task<IEnumerable<StorjFile>> GetBucketContents(Bucket model);
        Task<StorjFile> GetFileInfo(StorjFile file);
        Task<Bucket> CreateBucket(CreateBucketRequestModel model);
        Task<Stream> DownloadFile(FileRequestModel model);
        Task<AuthKeyModel> RegisterAuthKey(AuthKeyRequestModel model);
    }
}
