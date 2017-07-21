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

        private bool UseEncryption => crypto != null;

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

        public void SetBridgeOptions(BridgeOptions bridgeOptions)
        {
            m_BridgeOptions = bridgeOptions;
        }

        #region [ Mnemonic Functions ]

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

        #endregion

        #region [ Bridge Calls ]

        public async Task<Bridge> GetBridge()
        {
            return await JsonRequest<Bridge>(string.Empty, HttpMethod.Get, null, AuthenticationMethod.None);
        }

        public async Task<IEnumerable<Contact>> GetContacts()
        {
            return await GetContacts(null);
        }

        public async Task<IEnumerable<Contact>> GetContacts(ContactListRequestModel model)
        {
            return await JsonGet<IEnumerable<Contact>>("contacts", model);
        }

        public async Task<Contact> GetContact(ContactRequestModel model)
        {
            return await JsonGet<Contact>(string.Format("contacts/{0}", model.NodeId));
        }
        #endregion

        #region [ User APIs ]

        public async Task<BridgeUser> BridgeRegister(string email, string password)
        {
            string hashedPassword = password.GetSha256Hash();
            BridgeRegisterModel model = new BridgeRegisterModel()
            {
                Email = email,
                Password = hashedPassword
            };

            return await JsonPost<BridgeUser>("users", model);
        }

        public async Task<AuthKeyModel> RegisterAuthKey(AuthKeyRequestModel model)
        {
            return await JsonPost<AuthKeyModel>("keys", model);
        }

        #endregion

        #region [ Bucket Management ] 

        public async Task<IEnumerable<Bucket>> GetBuckets()
        {
            IEnumerable<Bucket> buckets = await JsonGet<IEnumerable<Bucket>>("buckets");
            if (UseEncryption)
            {
                crypto.TryDecryptBuckets(buckets);
            }
            return buckets;

        }

        public async Task<Bucket> CreateBucket(CreateBucketRequestModel model)
        {
            return await JsonPost<Bucket>("buckets", model);
        }

        #endregion
        
        #region [ Private Helpers ]

        private async Task<TResult> JsonGet<TResult>(string path, RequestModel model = null)
        {
            return await JsonRequest<TResult>(path, HttpMethod.Get, model, m_BridgeOptions.DefaultAuthenticationMethod);
        }

        private async Task<TResult> JsonPost<TResult>(string path, RequestModel model)
        {
            return await JsonRequest<TResult>(path, HttpMethod.Post, model, m_BridgeOptions.DefaultAuthenticationMethod);
        }

        private async Task<TResult> JsonRequest<TResult>(string path, HttpMethod method, RequestModel model, AuthenticationMethod authenticationMethod)
        {
            Uri requestUri = new Uri(m_BridgeOptions.BridgeUri, path);
            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = method;

            string jsonObject = null;
            if (method.IsDataRequest() && model != null)
            {
                if (authenticationMethod == AuthenticationMethod.ECDSA)
                {
                    model.SetNonce();
                }
                jsonObject = JsonConvert.SerializeObject(model);
                request.Content = new StringContent(jsonObject, Encoding.ASCII, "application/json");
            }
            
            // Authentication
            // Basic
            if (authenticationMethod == AuthenticationMethod.Basic)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", m_BridgeOptions.AuthorizationHeader);
            }
            // ECSDA signature
            else if (authenticationMethod == AuthenticationMethod.ECDSA)
            {
                if (model == null)
                {
                    model = new RequestModel();
                }
                model.SetNonce();

                // TODO: make this work
                string messageToSign;
                if (method.IsDataRequest())
                {
                    messageToSign = GetMessageToSign(path, method, jsonObject);
                }
                else
                {
                    string query = ConstructQueryString(model.GetQueryParams());
                    messageToSign = GetMessageToSign(path, method, query);
                }
                request.Headers.Add("x-signature", crypto.SignMessage(messageToSign));
                request.Headers.Add("x-pubkey", crypto.Pubkey);
            }

            if (!method.IsDataRequest())
            {
                string query = ConstructQueryString(model?.GetQueryParams());
                requestUri = new Uri(requestUri, query);
            }

            request.RequestUri = requestUri;
            string responseContent = null;
            try
            {
                HttpResponseMessage response = await client.SendAsync(request);
                responseContent = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                TResult responseObject = JsonConvert.DeserializeObject<TResult>(responseContent);
                return responseObject;
            }
            catch (HttpRequestException ex)
            {
                ResponseError error = JsonConvert.DeserializeObject<ResponseError>(responseContent);
                throw new HttpRequestException(error?.Error, ex);
            }
        }

        private static string ConstructQueryString(Dictionary<string, object> query)
        {
            if (query == null || !query.Any())
            {
                return string.Empty;
            }
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append("?");
            bool first = true;
            foreach (var keypair in query)
            {
                if (keypair.Value == null)
                {
                    continue;
                }
                if (!first)
                {
                    queryBuilder.Append("&");
                }
                queryBuilder.AppendFormat("{0}={1}", keypair.Key, keypair.Value);
                first = false;
            }
            return queryBuilder.ToString();
        }

        private string GetMessageToSign(string path, HttpMethod method, string data)
        {
            return string.Concat(method.Method, "\n/", path, "\n", data);
        }
        #endregion
    }
}
