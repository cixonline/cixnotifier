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
        int _lastCount = -1;
        private bool _isDisposed;

        public MainWindow()
        {
            InitializeComponent();

            string oauthToken = Properties.Settings.Default.oauthToken;
            string oauthTokenSecret = Properties.Settings.Default.oauthTokenSecret;

            if (string.IsNullOrWhiteSpace(oauthToken) || string.IsNullOrWhiteSpace(oauthTokenSecret))
            {
                CixLogin loginWindow = new CixLogin();
                loginWindow.ShowDialog();
            }

            ContextMenu notifyMenu = new ContextMenu();

            notifyMenu.MenuItems.Add(0, new MenuItem(Properties.Resources.ViewMessages, OnViewMessagesMenu));
            notifyMenu.MenuItems.Add(1, new MenuItem(Properties.Resources.CheckNow, OnCheckNowMenu));
            notifyMenu.MenuItems.Add(2, new MenuItem(Properties.Resources.About, OnAboutMenu));
            notifyMenu.MenuItems.Add(3, new MenuItem("-"));
            notifyMenu.MenuItems.Add(4, new MenuItem(Properties.Resources.Exit, OnExitMenu));

            _notifyIcon.Visible = true;
            _notifyIcon.Icon = new Icon(GetType(), "Notify_NoUnread.ico");
            _notifyIcon.Text = Properties.Resources.CIXNotifier;
            _notifyIcon.ContextMenu = notifyMenu;

            Timer notifyTimer = new Timer {Interval = 60000};
            notifyTimer.Tick += NotifyTimer_Tick;
            notifyTimer.Start();

            CheckAndUpdate(true);
        }

        private void OnViewMessagesMenu(Object sender, EventArgs e)
        {
            Process.Start("http://forums.cixonline.com/secure/inbox.aspx?pm=inbox");
        }

        private void OnCheckNowMenu(Object sender, EventArgs e)
        {
            CheckAndUpdate(true);
        }

        private void OnAboutMenu(Object sender, EventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }

        private void OnExitMenu(Object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void NotifyTimer_Tick(Object sender, EventArgs e)
        {
            CheckAndUpdate(false);
        }

        private void CheckAndUpdate(bool alwaysShow)
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
                    NotificationWindow notify = new NotificationWindow();
                    notify.Update(_messages);
                    notify.Show();
                }
                _lastCount = newCount;
            }
        }

        private int CheckMessages()
        {
            WebRequest wrGeturl = WebRequest.Create(CIXOAuth.Uri("http://api.cixonline.com/v1.0/cix.svc/personalmessage/inbox.xml"));
            wrGeturl.Method = "GET";

            Stream objStream = wrGeturl.GetResponse().GetResponseStream();
            if (objStream != null)
            {
                using (XmlReader reader = XmlReader.Create(objStream))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof (ConversationInboxSet));
                    ConversationInboxSet inboxSet = (ConversationInboxSet)serializer.Deserialize(reader);

                    _messages.Clear();

                    foreach (CIXInboxItem conv in inboxSet.Conversations)
                    {
                        bool isUnread;
                        if (Boolean.TryParse(conv.Unread, out isUnread) && isUnread)
                        {
                            InboxMessage newMessage = new InboxMessage
                                {
                                    Id = conv.ID,
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
            return _messages.Count;
        }

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

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
