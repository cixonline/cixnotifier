using System;
using System.Web;

namespace CIXNotifier
{
    public static class CIXOAuth
    {
        private const string ConsumerKey = "f63076cf4dde49d2a3ebe9e1a1cc3d";
        private const string ConsumerSecret = "387c2d6cf13a69b059d01d1b4ad878";

        static public string Uri(string baseUrl)
        {
            string tokenString = Properties.Settings.Default.oauthToken;
            string tokenStringSecret = Properties.Settings.Default.oauthTokenSecret;
            return Uri(baseUrl, tokenString, tokenStringSecret);
        }

        static public string Uri(string baseUrl, string tokenString, string tokenStringSecret)
        {
            string webUrl;
            string requestParam;

            OAuthBase oAuth = new OAuthBase();
            string nonce = oAuth.GenerateNonce();
            string timeStamp = oAuth.GenerateTimeStamp();
            string sig = oAuth.GenerateSignature(new Uri(baseUrl),
                ConsumerKey, ConsumerSecret,
                tokenString, tokenStringSecret,
                "GET", timeStamp, nonce,
                out webUrl,
                out requestParam);
            sig = HttpUtility.UrlEncode(sig);
            return webUrl + "?" + requestParam + "&oauth_signature=" + sig;
        }
    }
}
