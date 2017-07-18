using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StorjDotNet.Models
{
    public class Bridge
    {
        public string Swagger { get; set; }
        public BridgeInfo Info { get; set; }
        public string Host { get; set; }
        public string BasePath { get; set; }
        public IEnumerable<string> Schemes { get; set; }
        public IEnumerable<string> Consumes { get; set; }
        public IEnumerable<string> Produces { get; set; }
        public BridgeSecurityDefinitionList SecurityDefinitions { get; set; }
        public object Paths { get; set; }
    }

    public class BridgeInfo
    {
        public string Title { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        [JsonProperty(PropertyName="x-protocol-version")]
        public string XProtocolVersion { get; set; }
        [JsonProperty(PropertyName = "x-core-version")]
        public string XCoreVersion { get; set; }
    }

    public class BridgeSecurityDefinitionList
    {
        public BridgeSecurityDefinition Basic { get; set; }
        [JsonProperty(PropertyName = "ecdsa public key")]
        public BridgeSecurityDefinition EcdsaPublickey { get; set; }
        [JsonProperty(PropertyName = "ecdsa signature")]
        public BridgeSecurityDefinition EcdsaSignature { get; set; }

    }

    public class BridgeSecurityDefinition
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string In { get; set; }
    }

    public class BridgeTag
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class BridgeUser
    {
        public string Email { get; set; }
        public string Created { get; set; }
        public bool Activated { get; set; }
        public DateTime? CreatedDate
        {
            get
            {
                DateTime date;
                if(DateTime.TryParse(Created, out date))
                {
                    return date;
                }
                return null;
            }
        }
    }

    public class BridgeRegisterModel
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

    public class ResponseError
    {
        public string Error { get; set; }
    }
}
