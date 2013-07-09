﻿using System;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace CIXNotifier
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow
    {
        public AboutWindow()
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Version ver = Assembly.GetEntryAssembly().GetName().Version;
            string versionString = string.Format("{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);

            Run titleRun = new Run
                {
                    FontWeight = FontWeights.Bold,
                    Text = string.Format("{0} {1}", Properties.Resources.CIXNotifier, versionString),
                };
            aboutTextBlock.Inlines.Add(titleRun);
            aboutTextBlock.Inlines.Add(new LineBreak());

            Run textRun = new Run
                {
                    Text = AssemblyCopyright()
                };
            aboutTextBlock.Inlines.Add(textRun);
            aboutTextBlock.Inlines.Add(new LineBreak());

            textRun = new Run
                {
                    Text = Properties.Resources.AllRightsReserved
                };
            aboutTextBlock.Inlines.Add(textRun);
        }

        // Return the assembly copyright from the assemblyinfo module
        string AssemblyCopyright()
        {
            var attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
        }
    }
}
