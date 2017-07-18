using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bitcoin.BIP39;
using Newtonsoft.Json;
using System.Net;
using StorjDotNet.Models;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace StorjDotNet
{
    public class Storj : IStorj
    {
        private BridgeOptions m_BridgeOptions;
        private static readonly HttpClient client = new HttpClient();

        public Storj(BridgeOptions bridgeOptions)
        {
            m_BridgeOptions = bridgeOptions;
        }

        public BridgeOptions BridgeOptions
        {
            set
            {
                m_BridgeOptions = value;
            }
        }
        
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

        public async Task<Bridge> GetBridge()
        {
            Bridge bridge = null;
            try
            {
                HttpResponseMessage response = await client.GetAsync(m_BridgeOptions.BridgeUri);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                bridge = JsonConvert.DeserializeObject<Bridge>(content);
                return bridge;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<BridgeUser> BridgeRegister(string email, string password)
        {
            string hashedPassword = password.GetSha256Hash();
            BridgeRegisterModel model = new BridgeRegisterModel()
            {
                Email = email,
                Password = hashedPassword
            };

            return await JsonRequest<BridgeUser>("users", HttpMethod.Post, model, false);

        }

        public async Task<IEnumerable<Bucket>> GetBuckets()
        {
            return await JsonRequest<IEnumerable<Bucket>>("buckets", HttpMethod.Get, null, true);
        }

        public async Task<Bucket> CreateBucket(Bucket bucket)
        {
            var model = bucket != null ? new
            {
                name = bucket.Name,
                pubkeys = bucket.Pubkeys ?? new string[0]  
            } : null;

            return await JsonRequest<Bucket>("buckets", HttpMethod.Post, model, true);
        }

        public async Task<T> JsonRequest<T>(string path, HttpMethod method, object data, bool authentication)
        {
            Uri requestUri = new Uri(m_BridgeOptions.BridgeUri, path);
            HttpRequestMessage request = new HttpRequestMessage(method, requestUri);
            if (authentication)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", m_BridgeOptions.AuthorizationHeader);
            }

            if (data != null)
            {
                request.Content = Helpers.CreateHttpContentRequest(data);
            }

            string responseContent = null;
            try
            {
                HttpResponseMessage response = await client.SendAsync(request);
                responseContent = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                T responseObject = JsonConvert.DeserializeObject<T>(responseContent);
                return responseObject;
            }
            catch (HttpRequestException ex)
            {
                ResponseError error = JsonConvert.DeserializeObject<ResponseError>(responseContent);
                throw new HttpRequestException(error.Error, ex);
            }
        }
    }
}
