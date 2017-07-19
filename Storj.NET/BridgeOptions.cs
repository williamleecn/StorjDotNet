using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorjDotNet
{
    public class BridgeOptions
    {
        public string BridgeUrl { get; set; }
        public BridgeProtocol Protocol { get; set; }
        public AuthenticationMethod DefaultAuthenticationMethod { get; set; }
        public string Username { get; set; }
        private string m_Password;
        public string Password
        {
            get { return m_Password; }
            set { m_Password = value.GetSha256Hash(); }
        }
        public Uri BridgeUri
        {
            get
            {
                string protocol = Protocol == BridgeProtocol.HTTPS ? "https://" : "http://";
                return new Uri(protocol + BridgeUrl + "/");
            }
        }

        public string AuthorizationHeader
        {
            get
            {
                string combined = string.Format("{0}:{1}", Username, m_Password);
                return Convert.ToBase64String(combined.ToByteArray());
            }
        }

        public void SetPasswordHash(string value)
        {
            m_Password = value;
        }
    }
}
