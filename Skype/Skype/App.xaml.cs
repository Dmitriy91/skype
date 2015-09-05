using Client;
using Skype.View;
using Skype.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Skype
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Fields
        private static byte[] _defaultAvatarBytes;
        private static byte[] _onlineLogo;
        private static byte[] _offlineLogo;
        public static readonly ManualResetEvent _startClient = new ManualResetEvent(false);
        public static readonly ManualResetEvent _connected = new ManualResetEvent(false);
        #endregion

        #region Constructor
        static App()
        {
            string imagePath = null;

            try
            {
                imagePath = ConfigurationManager.AppSettings.Get("images");
            }
            catch(Exception)
            {
                //an error message goes here)
            }

            try
            {
                _defaultAvatarBytes = File.ReadAllBytes(imagePath + "default-avatar.png");
            }
            catch (Exception)
            {
                //an error message goes here)
            }

            try
            {
                _onlineLogo = File.ReadAllBytes(imagePath + "online-status-logo.png");
            }
            catch (Exception)
            {
                //an error message goes here)
            }

            try
            {
                _offlineLogo = File.ReadAllBytes(imagePath + "offline-status-logo.png");
            }
            catch (Exception)
            {
                //an error message goes here)
            }
        }
        #endregion

        #region Methods
        public static byte[] GetDefaultAvatar()
        {
            return _defaultAvatarBytes;
        }
        public static byte[] GetStatusLogo(bool isOnline)
        {
            if (isOnline)
                return _onlineLogo;

            return _offlineLogo;
        }
        public static void Authenticate()
        {
            while (true)
            {
                AuthenticationWindow authenticationWindow = new AuthenticationWindow();
                authenticationWindow.ShowDialog();

                if (authenticationWindow.Tag == null)
                {
                    AsynchronousClientSocket.Shutdown();
                    Process.GetCurrentProcess().CloseMainWindow();
                }
                else if ((AuthenticationDialogResult)authenticationWindow.Tag == AuthenticationDialogResult.Authenticated)
                {
                    break;
                }
                else if ((AuthenticationDialogResult)authenticationWindow.Tag == AuthenticationDialogResult.ShowRegWin)
                {
                    RegistrationWindow registrationWindow = new RegistrationWindow();
                    registrationWindow.ShowDialog();
                    
                    if (registrationWindow.Tag == null)
                    {
                        AsynchronousClientSocket.Shutdown();
                        Process.GetCurrentProcess().CloseMainWindow();
                    }
                }
            }
        }
        #endregion

        #region Event handlers
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
            MainWindow mainWindow = new MainWindow();
            MainWindow = mainWindow;

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    _startClient.Reset();

                    Thread newWindowThread = new Thread(() =>
                    {
                        ConnectionWindow connectionWindow = new ConnectionWindow();
                        connectionWindow.Show();
                        System.Windows.Threading.Dispatcher.Run();
                    });

                    newWindowThread.SetApartmentState(ApartmentState.STA);
                    newWindowThread.IsBackground = true;
                    newWindowThread.Start();

                    try
                    {
                        _startClient.WaitOne();
                        AsynchronousClientSocket.StartClient();
                        //the function waites until a user inputs an ip address using the window created above
                        //if a connection fails, a new ip window pops up in a dffrent thread
                    }
                    catch (Exception _e)
                    {
                        MessageBox.Show(_e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            });

            _connected.WaitOne();//the signal is received from AsynchronousClientSocket.StartClient() when it gets connected

            mainWindow.Show();
            mainWindow.Activate();
        }
        #endregion
    }
}
