using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace StorjDotNet.Models
{
    public class RequestModel
    {
        private long? m_Nonce;
        [JsonProperty(PropertyName = "__nonce", NullValueHandling = NullValueHandling.Ignore)]
        public long? Nonce { get { return m_Nonce; } }

        public void SetNonce()
        {
            m_Nonce = Helpers.GetNonce();
        }

        public virtual Dictionary<string, object> GetQueryParams()
        {
            return new Dictionary<string, object>()
            {
                { "__nonce", m_Nonce }
            };
        }
    }

    public class BridgeRegisterModel : RequestModel
    {
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("pubkey")]
        public string Pubkey { get; set; }
        [JsonProperty("referralPartner")]
        public string ReferralPartner { get; set; }
    }

    public class CreateBucketRequestModel : RequestModel
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "pubkeys")]
        public IEnumerable<string> Pubkeys { get; set; }
    }

    public class AuthKeyRequestModel : RequestModel
    {
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }
    }

    public class ContactListRequestModel : RequestModel
    {
        public int? PageNumber { get; set; }
        public string Address { get; set; }
        public string Protocol { get; set; }
        public string UserAgent { get; set; }
        public bool? Connected { get; set; }

        public override Dictionary<string, object> GetQueryParams()
        {
            return new Dictionary<string, object>()
            {
                { "page", PageNumber },
                { "address", Address },
                { "protocol", Protocol },
                { "userAgent", UserAgent },
                { "connected", Connected },
                { "__nonce", Nonce }
            };
        }
    }

    public class ContactRequestModel : RequestModel
    {
        public string NodeId { get; set; }

        public override Dictionary<string, object> GetQueryParams()
        {
            return new Dictionary<string, object>()
            {
                { "nodeID", NodeId }
            };
        }
    }

    public class FileRequestModel : RequestModel
    {
        public string BucketId { get; set; }
        public string FileId { get; set; }
        public int Skip { get; set; }
        public int Limit { get; set; }
        public IEnumerable<Contact> Exclude { get; set; }

        public override Dictionary<string, object> GetQueryParams()
        {
            var queryParams = base.GetQueryParams();
            queryParams.Add("skip", Skip);
            queryParams.Add("limit", Limit);
            queryParams.Add("exclude", GetFarmerCsv(Exclude));
            return queryParams;
        }

        private string GetFarmerCsv(IEnumerable<Contact> farmers)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var farmer in farmers)
            {
                if (!string.IsNullOrEmpty(farmer?.NodeId))
                {
                    if (!first)
                        sb.Append(",");
                    else
                        first = false;

                    sb.Append(farmer.NodeId);
                }
            }
            return sb.ToString();
        }
    }
}
