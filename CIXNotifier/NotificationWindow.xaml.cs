using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace CIXNotifier
{
    /// <summary>
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow
    {
        private InboxMessages _messages;
        private int _index;

        public NotificationWindow()
        {
            InitializeComponent();

            // This code basically shoves the window into the task bar area.
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

        /// <summary>
        /// Remove any notification window immediately. Don't wait for the transition
        /// to complete.
        /// </summary>
        public void Clear()
        {
            Close();
        }

        /// <summary>
        /// Update the notification window with the specified set of messages.
        /// </summary>
        /// <param name="messages"></param>
        public void Update(InboxMessages messages)
        {
            if (messages.Count == 0)
            {
                notifyText1.Text = Properties.Resources.NoUnread;
            }
            else
            {
                _messages = messages;
                _index = 0;
                ShowNextMessage();
            }
        }

        /// <summary>
        /// Show the next message in the array of messages passed to Update, wrapping round
        /// if we went past the end.
        /// </summary>
        private void ShowNextMessage()
        {
            DateTime messageDate = new DateTime(_messages[_index].Date.Year, _messages[_index].Date.Month, _messages[_index].Date.Day);
            string bodyText = _messages[_index].Body.Trim();

            notifyText1.Inlines.Clear();
            Run chevronRun = new Run
                {
                    Text = "»",
                    Foreground = new SolidColorBrush(Colors.Red)
                };
            notifyText1.Inlines.Add(chevronRun);

            Run countRun = new Run
                {
                    Text = string.Format("{0} of {1} - ", _index + 1, _messages.Count)
                };
            notifyText1.Inlines.Add(countRun);

            Run dateRun = new Run
                {
                    Text =
                        messageDate == DateTime.Today
                            ? _messages[_index].Date.ToShortTimeString()
                            : _messages[_index].Date.ToShortDateString()
                };
            notifyText1.Inlines.Add(dateRun);

            Run titleRun = new Run
                {
                    FontWeight = FontWeights.Bold,
                    Text = " " + _messages[_index].Sender
                };
            notifyText1.Inlines.Add(titleRun);

            notifyText2.Inlines.Clear();
            Run textRun = new Run
                {
                    FontWeight = FontWeights.Bold,
                    Text = _messages[_index].Subject
                };
            notifyText2.Inlines.Add(textRun);

            notifyText3.Inlines.Clear();
            textRun = new Run
                {
                    FontStyle = FontStyles.Italic,
                    Text = bodyText.Substring(0, Math.Min(bodyText.Length, 80))
                };
            notifyText3.Inlines.Add(textRun);

            if (++_index == _messages.Count)
            {
                _index = 0;
            }
        }

        /// <summary>
        /// Trap the mouse down event on the notification to show the next unread message
        /// in the sequence.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Mouse event arguments</param>
        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowNextMessage();
        }

        /// <summary>
        /// Called when the storyboard completes. We close the notification window.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void OnCompleted(object sender, EventArgs e)
        {
            Close();
        }
    }
}
