using Skype.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Skype.View
{
    /// <summary>
    /// Interaction logic for ServerWindow.xaml
    /// </summary>
    public partial class ConnectionWindow : Window
    {
        private readonly ConnectionWinViewModel _connectionWinViewModel;

        public ConnectionWindow()
        {
            InitializeComponent();
            _connectionWinViewModel = new ConnectionWinViewModel((ClosingState closingState) =>
            {
                if (this.Dispatcher.CheckAccess())
                {
                    this.Tag = closingState;
                    this.Close();
                }
                else
                {
                    this.Tag = closingState;
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(this.Close));
                }
            });

            this.Closed += (sender, e) =>
            {
                if (this.Tag == null)
                {
                    Dispatcher.BeginInvoke(new Action(()=>
                    {
                        Process.GetCurrentProcess().CloseMainWindow();
                    }));
                }
                else if ((ClosingState)Tag == ClosingState.InitiatingConnection)
                {
                    App._startClient.Set();
                }
                this.Dispatcher.InvokeShutdown();
            };
            DataContext = _connectionWinViewModel;
        }
    }
}
