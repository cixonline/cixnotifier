using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace CIXNotifier
{
    /// <summary>
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow
    {
        public NotificationWindow()
        {
            InitializeComponent();

            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                var workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
                var presentationSource = PresentationSource.FromVisual(this);
                if (presentationSource == null) return;
                if (presentationSource.CompositionTarget == null) return;
                var transform = presentationSource.CompositionTarget.TransformFromDevice;
                var corner = transform.Transform(new Point(workingArea.Right, workingArea.Bottom));

                Left = corner.X - ActualWidth - 100;
                Top = corner.Y - ActualHeight;
            }));
        }

        public void Update(InboxMessages messages)
        {
            if (messages.Count == 0)
            {
                notifyText.Text = "Your inbox contains no unread conversations";
            }
            else if (messages.Count > 1)
            {
                Run titleRun = new Run
                    {
                        FontWeight = FontWeights.Bold,
                        Text = string.Format("Your inbox contains {0} unread conversations", messages.Count.ToString(CultureInfo.InvariantCulture))
                    };
                notifyText.Inlines.Add(titleRun);
            }
            else
            {
                DateTime messageDate = new DateTime(messages[0].Date.Year, messages[0].Date.Month, messages[0].Date.Day);
                string bodyText = messages[0].Body;

                Run dateRun = new Run
                    {
                        Text =
                            messageDate == DateTime.Today
                                ? messages[0].Date.ToShortTimeString()
                                : messages[0].Date.ToShortDateString()
                    };
                notifyText.Inlines.Add(dateRun);

                Run titleRun = new Run
                    {
                        FontWeight = FontWeights.Bold,
                        Text = " " + messages[0].Sender
                    };
                notifyText.Inlines.Add(titleRun);

                notifyText.Inlines.Add(new LineBreak());

                Run textRun = new Run
                    {
                        Text = messages[0].Subject
                    };
                notifyText.Inlines.Add(textRun);

                notifyText.Inlines.Add(new LineBreak());

                textRun = new Run
                    {
                        FontStyle = FontStyles.Italic,
                        Text = bodyText.Substring(0, Math.Min(bodyText.Length, 80))
                    };
                notifyText.Inlines.Add(textRun);
            }
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("http://forums.cixonline.com/secure/inbox.aspx?pm=inbox");
        }

        private void OnCompleted(object sender, EventArgs e)
        {
            Close();
        }
    }
}
