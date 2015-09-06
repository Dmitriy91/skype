using Client;
using MicroMvvm;
using Skype.Model;
using Skype.Resources.Model;
using NetworkPackets.Enum;
using NetworkPackets.Packet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Skype.ViewModel
{
    public class MainWinMenuViewModel : BaseViewModel
    {
        #region Commands
        private RelayCommand _exitCommand;
        private RelayCommand _signOutCommand;

        private void Exit()
        {
            AsynchronousClientSocket.Shutdown();
            Process.GetCurrentProcess().CloseMainWindow();
        }
        public ICommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                    _exitCommand = new RelayCommand((param) => { Exit(); });

                return _exitCommand;
            }
        }

        private void SignOut()
        {
            App.Authenticate();
        }
        public ICommand SignOutCommand
        {
            get
            {
                if (_signOutCommand == null)
                    _signOutCommand = new RelayCommand((param) => { SignOut(); });

                return _signOutCommand;
            }
        }
        #endregion
    }
}
