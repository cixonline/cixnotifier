using System;
using System.Web;

namespace CIXNotifier
{
    public static class CIXOAuth
    {
        /// <summary>
        /// Construct a URL for a given API call using the tokens stored in the settings.
        /// </summary>
        /// <param name="baseUrl">The fully qualified base URL</param>
        /// <returns>A URL string with the requested parameters</returns>
        static public string Uri(string baseUrl)
        {
            string tokenString = Properties.Settings.Default.oauthToken;
            string tokenStringSecret = Properties.Settings.Default.oauthTokenSecret;
            return Uri(baseUrl, tokenString, tokenStringSecret);
        }

        /// <summary>
        /// Construct a URL for a given API call with the tokens returned from the last
        /// authorization or authentication call. Passing through the tokens is required in
        /// order to generate a signature that takes their values into account.
        /// </summary>
        /// <param name="baseUrl">The fully qualified base URL</param>
        /// <param name="tokenString">OAuth token string</param>
        /// <param name="tokenStringSecret">OAuth token secret string</param>
        /// <returns>A URL string with the requested parameters</returns>
        static public string Uri(string baseUrl, string tokenString, string tokenStringSecret)
        {
            string webUrl;
            string requestParam;

            OAuthBase oAuth = new OAuthBase();
            string nonce = oAuth.GenerateNonce();
            string timeStamp = oAuth.GenerateTimeStamp();
            string sig = oAuth.GenerateSignature(new Uri(baseUrl),
                CIXOAuthKeys.ConsumerKey, CIXOAuthKeys.ConsumerSecret,
                tokenString, tokenStringSecret,
                "GET", timeStamp, nonce,
                out webUrl,
                out requestParam);
            sig = HttpUtility.UrlEncode(sig);
            return webUrl + "?" + requestParam + "&oauth_signature=" + sig;
        }
    }
}
