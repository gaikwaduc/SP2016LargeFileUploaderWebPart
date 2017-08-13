using Microsoft.SharePoint.Client;
using System;
using System.Net;
using System.Web;
using System.Text;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Xml;
using SP = Microsoft.SharePoint.Client;

namespace SPLargeFileUploader
{
    public static class ClientContextProvider
    {
        public static ClientContext GetSPContext(string webUrl)
        {
            TokenConstants.fedAuthRequestCtx = HttpUtility.UrlEncode(string.Format("{0}/_layouts/15/Authenticate.aspx?Source=%2f", webUrl));
            EndpointAddress ep = new EndpointAddress(TokenConstants.adfsBaseUri + "/" + TokenConstants.adfsTrustPath + "/" + TokenConstants.adfsTrustEndpoint);
            WS2007HttpBinding binding = new WS2007HttpBinding(SecurityMode.TransportWithMessageCredential);
            binding.Security.Message.EstablishSecurityContext = false;
            binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;

            WSTrustChannelFactory wstcf = new WSTrustChannelFactory(binding, ep);
            wstcf.TrustVersion = TrustVersion.WSTrust13;
            NetworkCredential cred = new NetworkCredential(TokenConstants.userName, TokenConstants.pwd, TokenConstants.domain);
            wstcf.Credentials.Windows.ClientCredential = cred;
            wstcf.Credentials.UserName.UserName = cred.UserName;
            wstcf.Credentials.UserName.Password = cred.Password;
            var channel = wstcf.CreateChannel();

            string[] tokenType = { "urn:oasis:names:tc:SAML:1.0:assertion", "urn:oasis:names:tc:SAML:2.0:assertion" };
            RequestSecurityToken rst = new RequestSecurityToken();
            rst.RequestType = RequestTypes.Issue;
            rst.AppliesTo = new EndpointReference(TokenConstants.adfsRealm);
            rst.KeyType = KeyTypes.Bearer;
            rst.TokenType = tokenType[0]; // Use the first one because that is what SharePoint itself uses (as observed in Fiddler).

            RequestSecurityTokenResponse rstr = new RequestSecurityTokenResponse();
            var cbk = new System.Net.Security.RemoteCertificateValidationCallback(ValidateRemoteCertificate);
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; };
            SecurityToken token = null;
            try
            {
                token = channel.Issue(rst, out rstr);
            }
            finally
            {
                ServicePointManager.ServerCertificateValidationCallback = cbk;
            }

            Cookie fedAuthCookie = TransformTokenToFedAuth(((GenericXmlSecurityToken)token).TokenXml.OuterXml);
            TokenConstants.fedAuth.Add(fedAuthCookie);
            SP.ClientContext ctx = new SP.ClientContext(webUrl);
            ctx.ExecutingWebRequest += ctx_ExecutingWebRequest;
            return ctx;
        }

        static void ctx_ExecutingWebRequest(object sender, SP.WebRequestEventArgs e)
        {
            e.WebRequestExecutor.WebRequest.CookieContainer = TokenConstants.fedAuth;
        }

        private static Cookie TransformTokenToFedAuth(string samlToken)
        {
            samlToken = WrapInSoapMessage(samlToken, TokenConstants.adfsRealm);
            string stringData = String.Format("wa=wsignin1.0&wctx={0}&wresult={1}", TokenConstants.fedAuthRequestCtx, HttpUtility.UrlEncode(samlToken));
            HttpWebRequest req = HttpWebRequest.Create(TokenConstants.adfsEndpoint) as HttpWebRequest;
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.CookieContainer = new CookieContainer();
            req.AllowAutoRedirect = false;
            Stream newStream = req.GetRequestStream();

            byte[] data = Encoding.UTF8.GetBytes(stringData);
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
            var encoding = ASCIIEncoding.ASCII;
            string responseText = "";
            using (var reader = new System.IO.StreamReader(resp.GetResponseStream(), encoding))
            {
                responseText = reader.ReadToEnd();
            }
            return resp.Cookies["FedAuth"];
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Xml.XmlDocument.CreateTextNode(System.String)")]
        private static string WrapInSoapMessage(string stsResponse, string relyingPartyIdentifier)
        {
            XmlDocument samlAssertion = new XmlDocument();
            samlAssertion.PreserveWhitespace = true;
            samlAssertion.LoadXml(stsResponse);

            //Select the book node with the matching attribute value.
            String notBefore = /*samlAssertion.DocumentElement["Conditions"]*/samlAssertion.DocumentElement.FirstChild.Attributes["NotBefore"].Value;
            String notOnOrAfter = /*samlAssertion.DocumentElement["Conditions"]*/samlAssertion.DocumentElement.FirstChild.Attributes["NotOnOrAfter"].Value;

            XmlDocument soapMessage = new XmlDocument();
            XmlElement soapEnvelope = soapMessage.CreateElement("t", "RequestSecurityTokenResponse", "http://schemas.xmlsoap.org/ws/2005/02/trust");
            soapMessage.AppendChild(soapEnvelope);
            XmlElement lifeTime = soapMessage.CreateElement("t", "Lifetime", soapMessage.DocumentElement.NamespaceURI);
            soapEnvelope.AppendChild(lifeTime);
            XmlElement created = soapMessage.CreateElement("wsu", "Created", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            XmlText createdValue = soapMessage.CreateTextNode(notBefore);
            created.AppendChild(createdValue);
            lifeTime.AppendChild(created);
            XmlElement expires = soapMessage.CreateElement("wsu", "Expires", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            XmlText expiresValue = soapMessage.CreateTextNode(notOnOrAfter);
            expires.AppendChild(expiresValue);
            lifeTime.AppendChild(expires);
            XmlElement appliesTo = soapMessage.CreateElement("wsp", "AppliesTo", "http://schemas.xmlsoap.org/ws/2004/09/policy");
            soapEnvelope.AppendChild(appliesTo);
            XmlElement endPointReference = soapMessage.CreateElement("wsa", "EndpointReference", "http://www.w3.org/2005/08/addressing");
            appliesTo.AppendChild(endPointReference);
            XmlElement address = soapMessage.CreateElement("wsa", "Address", endPointReference.NamespaceURI);
            XmlText addressValue = soapMessage.CreateTextNode(relyingPartyIdentifier);
            address.AppendChild(addressValue);
            endPointReference.AppendChild(address);
            XmlElement requestedSecurityToken = soapMessage.CreateElement("t", "RequestedSecurityToken", soapMessage.DocumentElement.NamespaceURI);
            XmlNode samlToken = soapMessage.ImportNode(samlAssertion.DocumentElement, true);
            requestedSecurityToken.AppendChild(samlToken);
            soapEnvelope.AppendChild(requestedSecurityToken);
            XmlElement tokenType = soapMessage.CreateElement("t", "TokenType", soapMessage.DocumentElement.NamespaceURI);
            XmlText tokenTypeValue = soapMessage.CreateTextNode("urn:oasis:names:tc:SAML:1.0:assertion");
            tokenType.AppendChild(tokenTypeValue);
            soapEnvelope.AppendChild(tokenType);
            XmlElement requestType = soapMessage.CreateElement("t", "RequestType", soapMessage.DocumentElement.NamespaceURI);
            XmlText requestTypeValue = soapMessage.CreateTextNode("http://schemas.xmlsoap.org/ws/2005/02/trust/Issue");
            requestType.AppendChild(requestTypeValue);
            soapEnvelope.AppendChild(requestType);
            XmlElement keyType = soapMessage.CreateElement("t", "KeyType", soapMessage.DocumentElement.NamespaceURI);
            XmlText keyTypeValue = soapMessage.CreateTextNode("http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey");
            keyType.AppendChild(keyTypeValue);
            soapEnvelope.AppendChild(keyType);

            return soapMessage.OuterXml;
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            // It may be insightful to examine the parameters here, while debugging, although the function only returns a value of true regardless of those parameters.
            return true;
        }
    }
}