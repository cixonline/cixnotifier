using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace CIXNotifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : IDisposable
    {
        readonly InboxMessages _messages = new InboxMessages();
        readonly NotifyIcon _notifyIcon = new NotifyIcon();
        private NotificationWindow _notify;
        private readonly Timer _notifyTimer;
        int _lastCount = -1;
        private bool _isDisposed;

        // The number of seconds we check for unread messages. Don't
        // make this too small.
        private const int tickCount = 60*1000;

        public MainWindow()
        {
            InitializeComponent();

            ContextMenu notifyMenu = new ContextMenu();

            MenuItem viewMessagesMenu = new MenuItem(Properties.Resources.ViewMessages, OnViewMessagesMenu)
                {
                    DefaultItem = true
                };

            notifyMenu.MenuItems.Add(0, viewMessagesMenu);
            notifyMenu.MenuItems.Add(1, new MenuItem(Properties.Resources.CheckNow, OnCheckNowMenu));
            notifyMenu.MenuItems.Add(2, new MenuItem(Properties.Resources.TellMeAgain, OnTellMeAgain));
            notifyMenu.MenuItems.Add(3, new MenuItem(Properties.Resources.About, OnAboutMenu));
            notifyMenu.MenuItems.Add(4, new MenuItem("-"));
            notifyMenu.MenuItems.Add(5, new MenuItem(Properties.Resources.Exit, OnExitMenu));

            _notifyIcon.Visible = true;
            _notifyIcon.Icon = new Icon(GetType(), "Notify_NoUnread.ico");
            _notifyIcon.Text = Properties.Resources.CIXNotifier;
            _notifyIcon.ContextMenu = notifyMenu;

            _notifyTimer = new Timer { Interval = tickCount };
            _notifyTimer.Tick += NotifyTimer_Tick;
            _notifyTimer.Start();

            CheckAndUpdate(true);
        }

        /// <summary>
        /// Has this app been authenticated? Do we have non-empty cached tokens.
        /// </summary>
        /// <returns>True if we're authenticated, false otherwise</returns>
        private static bool IsAuthenticated()
        {
            string oauthToken = Properties.Settings.Default.oauthToken;
            string oauthTokenSecret = Properties.Settings.Default.oauthTokenSecret;
            return !string.IsNullOrWhiteSpace(oauthToken) && !string.IsNullOrWhiteSpace(oauthTokenSecret);
        }

        /// <summary>
        /// Invoke the CIX Inbox page on the web.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void OnViewMessagesMenu(Object sender, EventArgs e)
        {
            Process.Start("http://forums.cixonline.com/secure/inbox.aspx?pm=inbox");
        }

        /// <summary>
        /// Display the last notification again.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void OnTellMeAgain(Object sender, EventArgs e)
        {
            if (_messages != null && _messages.Count > 0)
            {
                _notify = new NotificationWindow();
                _notify.Update(_messages);
                _notify.Show();
            }
        }

        /// <summary>
        /// Does an immediate check for unread messages. To avoid a race condition,
        /// we also reset the timer here.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void OnCheckNowMenu(Object sender, EventArgs e)
        {
            CheckAndUpdate(true);
        }

        /// <summary>
        /// Displays the About window.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void OnAboutMenu(Object sender, EventArgs e)
        {
            if (_notify != null)
            {
                _notify.Clear();
            }
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }

        /// <summary>
        /// Shuts down the notification application.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void OnExitMenu(Object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// Timer tick handler. This is the bit that gets called on schedule to
        /// poll and check for any change in unread messages.
        /// </summary>
        /// <param name="sender">Timer object</param>
        /// <param name="e">Timer event arguments</param>
        private void NotifyTimer_Tick(Object sender, EventArgs e)
        {
            CheckAndUpdate(false);
        }

        /// <summary>
        /// Do a check for unread messages and then update the notification icon
        /// and flag a notification alert if anything has changed.
        /// </summary>
        /// <param name="alwaysShow">If true, we always change state even if the
        /// count of unread messages hasn't changed</param>
        private void CheckAndUpdate(bool alwaysShow)
        {
            _notifyTimer.Stop();

            if (!IsAuthenticated())
            {
                CixLogin loginWindow = new CixLogin();
                loginWindow.ShowDialog();
            }

            // Only bother progressing if we got authenticated. If not,
            // kill the timer too so we don't get all these popups!
            if (IsAuthenticated())
            {
                int newCount = CheckMessages();
                if (newCount != _lastCount || alwaysShow)
                {
                    if (_messages.Count == 0)
                    {
                        _notifyIcon.Icon = new Icon(GetType(), "Notify_NoUnread.ico");
                    }
                    else
                    {
                        _notifyIcon.Icon = new Icon(GetType(), "Notify.ico");
                        _notify = new NotificationWindow();
                        _notify.Update(_messages);
                        _notify.Show();
                    }
                    _lastCount = newCount;
                }
                _notifyTimer.Start();
            }
            else
            {
                _notifyIcon.Icon = new Icon(GetType(), "Notify_NoAuth.ico");
                _lastCount = -1;
            }
        }

        /// <summary>
        /// This is the guts of the message check. We call the inbox API to get a list of all messages
        /// and iterate through them to create our internal list of unread messages which are a little
        /// easier to manage than the serialised XML objects.
        /// </summary>
        /// <returns>The count of unread messages in the inbox</returns>
        private int CheckMessages()
        {
            WebRequest wrGeturl = WebRequest.Create(CIXOAuth.Uri("http://api.cixonline.com/v1.0/cix.svc/personalmessage/inbox.xml"));
            wrGeturl.Method = "GET";

            try
            {
                Stream objStream = wrGeturl.GetResponse().GetResponseStream();
                if (objStream != null)
                {
                    using (XmlReader reader = XmlReader.Create(objStream))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof (ConversationInboxSet));
                        ConversationInboxSet inboxSet = (ConversationInboxSet) serializer.Deserialize(reader);

                        _messages.Clear();

                        foreach (CIXInboxItem conv in inboxSet.Conversations)
                        {
                            bool isUnread;
                            if (Boolean.TryParse(conv.Unread, out isUnread) && isUnread)
                            {
                                InboxMessage newMessage = new InboxMessage
                                    {
                                        Body = conv.Body,
                                        Date = DateTime.Parse(conv.Date),
                                        Sender = conv.Sender,
                                        Subject = conv.Subject
                                    };
                                _messages.Add(newMessage);
                            }
                        }
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Message.Contains("401"))
                {
                    // Authentication failed. Likely the app has been revoked. Clear
                    // the tokens and force re-authentication.
                    Properties.Settings.Default.oauthToken = "";
                    Properties.Settings.Default.oauthTokenSecret = "";
                }
            }
            return _messages.Count;
        }

        /// <summary>
        /// Internal method that controls disposing the notifyicon object
        /// when the main class is disposed.
        /// </summary>
        /// <param name="disposing">Are we disposing or not?</param>
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _notifyIcon.Dispose();
                }
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Calls Dispose to remove any lingering resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
