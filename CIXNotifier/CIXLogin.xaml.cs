using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace CIXNotifier
{
    /// <summary>
    /// Interaction logic for CIXLogin.xaml
    /// </summary>
    public partial class CixLogin
    {
        string _oauthToken = "";
        string _oauthTokenSecret = "";

        public CixLogin()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            WebRequest wrGeturl = WebRequest.Create(CIXOAuth.Uri("http://api.cixonline.com/v1.0/cix.svc/getrequesttoken", string.Empty, string.Empty));

            using (Stream objStream = wrGeturl.GetResponse().GetResponseStream())
            {
                if (objStream != null)
                {
                    StreamReader objReader = new StreamReader(objStream);
                    string sLine = objReader.ReadLine();

                    if (sLine != null)
                    {
                        string[] items = sLine.Split(new[] { '&' });
                        foreach (string t in items)
                        {
                            if (t.StartsWith("oauth_token="))
                            {
                                _oauthToken = t.Substring(12);
                            }
                            if (t.StartsWith("oauth_token_secret="))
                            {
                                _oauthTokenSecret = t.Substring(19);
                            }
                        }
                    }
                }
            }

            string webUrl = "http://forums.cixonline.com/secure/authapp.aspx?oauth_token=" + _oauthToken + "&oauth_callback=notifyapp://home";
            webForm.Source = new Uri(webUrl);
        }

        private void OnNavigate(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.Uri.AbsoluteUri == "notifyapp://home/")
            {
                WebRequest wrGeturl = WebRequest.Create(CIXOAuth.Uri("http://api.cixonline.com/v1.0/cix.svc/getaccesstoken", _oauthToken, _oauthTokenSecret));
                wrGeturl.Method = "GET";

                Stream objStream = wrGeturl.GetResponse().GetResponseStream();
                if (objStream != null)
                {
                    StreamReader objReader = new StreamReader(objStream);
                    string sLine = objReader.ReadLine();

                    if (sLine != null)
                    {
                        string[] items = sLine.Split(new[] {'&'});
                        foreach (string t in items)
                        {
                            if (t.StartsWith("oauth_token="))
                            {
                                Properties.Settings.Default.oauthToken = t.Substring(12);
                            }
                            if (t.StartsWith("oauth_token_secret="))
                            {
                                Properties.Settings.Default.oauthTokenSecret = t.Substring(19);
                            }
                        }
                    }
                }

                Properties.Settings.Default.Save();
                Close();
            }
        }

        private void OnNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            HideScriptErrors(webForm, true);
        }

        private void HideScriptErrors(WebBrowser wb, bool hide)
        {
            var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser != null)
            {
                var objComWebBrowser = fiComWebBrowser.GetValue(wb);
                if (objComWebBrowser == null)
                {
                    wb.Loaded += (o, s) => HideScriptErrors(wb, hide); // In case we are too early
                    return;
                }
                objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
            }
        }
    }
}
