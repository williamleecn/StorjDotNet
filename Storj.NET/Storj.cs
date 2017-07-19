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
        private readonly Crypto crypto;

        public Storj(BridgeOptions bridgeOptions, EncryptionOptions encryptionOptions)
        {
            if (bridgeOptions == null)
            {
                throw new ArgumentNullException(nameof(bridgeOptions), "Bridge options can not be null");
            }
            if (bridgeOptions.DefaultAuthenticationMethod == AuthenticationMethod.ECDSA && encryptionOptions == null)
            {
                throw new ArgumentNullException(nameof(encryptionOptions), "Encryption options can not be null when using ECDSA authentication");
            }
            m_BridgeOptions = bridgeOptions;
            crypto = encryptionOptions?.GetCrypto();
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

            return await JsonRequest<BridgeUser>("users", HttpMethod.Post, model, AuthenticationMethod.None);

        }

        public async Task<IEnumerable<Bucket>> GetBuckets()
        {
            return await JsonRequest<IEnumerable<Bucket>>("buckets", HttpMethod.Get, null, m_BridgeOptions.DefaultAuthenticationMethod);
        }

        public async Task<Bucket> CreateBucket(CreateBucketRequestModel model)
        {
            return await JsonPost<Bucket>("buckets", model);
        }

        public async Task<AuthKeyModel> RegisterAuthKey(AuthKeyRequestModel model)
        {
            return await JsonPost<AuthKeyModel>("keys", model);
        }

        #region [ Private Helpers ]

        private async Task<TResult> JsonPost<TResult>(string path, RequestModel model)
        {
            return await JsonRequest<TResult>(path, HttpMethod.Post, model, m_BridgeOptions.DefaultAuthenticationMethod);
        }

        private async Task<T> JsonRequest<T>(string path, HttpMethod method, RequestModel data, AuthenticationMethod authenticationMethod)
        {
            Uri requestUri = new Uri(m_BridgeOptions.BridgeUri, path);
            HttpRequestMessage request = new HttpRequestMessage(method, requestUri);
            if (authenticationMethod == AuthenticationMethod.Basic)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", m_BridgeOptions.AuthorizationHeader);
            }
            else if (authenticationMethod == AuthenticationMethod.ECDSA)
            {
                if (method == HttpMethod.Get)
                {
                    // Todo: Add nonce to query string
                }
                else
                {
                    if(data == null)
                    {
                        data = new RequestModel();
                    }
                    data.SetNonce();
                }
            }

            if (method == HttpMethod.Post || method == HttpMethod.Put || method.Method == "PATCH")
            {
                string jsonObject = JsonConvert.SerializeObject(data);
                request.Content = new StringContent(jsonObject, Encoding.ASCII, "application/json");
                // sign content
                if (authenticationMethod == AuthenticationMethod.ECDSA)
                {
                    request.Headers.Add("x-signature", crypto.SignMessage(jsonObject));
                    request.Headers.Add("x-pubkey", crypto.Pubkey);
                }
            }
            else if (method == HttpMethod.Get || method == HttpMethod.Delete || method == HttpMethod.Options)
            {
                //TODO - query strings from data?
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

        #endregion
    }
}
