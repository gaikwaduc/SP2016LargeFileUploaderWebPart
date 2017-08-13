using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SPLargeFileUploader
{
    public class TokenConstants
    {
        public static CookieContainer fedAuth = new CookieContainer();

        public static string fedAuthRequestCtx = string.Empty;

        public static string adfsTrustEndpoint = "usernamemixed"; // OOTB ADFS, do not change
        public static string adfsTrustPath = "adfs/services/trust/13"; // OOTB ADFS, do not change

        public static string domain = "xxxxxxx"; // domain of AD account; should be in config file or propmted from the command line
        public static string userName = "xxxxxxxxx@xxx.com"; // UPN of domain account in AD
        public static string pwd = "xxxxxxxxxx"; // password of domain account; should be a secure string from config file, but you get the idea
        public static string adfsBaseUri = "https://adfsdev2016.xxxxxxx.com"; // your ADFS farm URL
        public static string adfsEndpoint = "https://dev2016.xxxxxxx.com/_trust/"; // Endpoint for site from where we get the token 
        public static string adfsRealm = "urn:sharepoint:dev2016"; // ADFS realm that contains the endpoint above

    }
}
