using Client;
using MicroMvvm;
using Skype.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Skype.ViewModel
{
    public enum ClosingState { ShuttingDownApp, InitiatingConnection}
    public class ConnectionWinViewModel : BaseViewModel
    {
        #region Fields
        private string _ip = Dns.Resolve(Dns.GetHostName()).AddressList[0].ToString();
        //for testing reasons
        private string _ipErrorMessage;
        private Action<ClosingState> _closeWindow;
        #endregion

        #region Properties
        public string IP
        {
            get
            {
                return _ip;
            }
            set
            {
                _ip = value;
                NotifyPropertyChanged("IP");
            }
        }
        public string IPErrorMessage
        {
            get
            {
                return _ipErrorMessage;
            }
            set
            {
                _ipErrorMessage = value;
                NotifyPropertyChanged("IPErrorMessage");
            }
        }
        #endregion

        #region Constructor
        public ConnectionWinViewModel(Action<ClosingState> closeWindow)
        {
            _closeWindow = closeWindow;
        }
        #endregion

        #region Commands
        private RelayCommand _submitCommand;

        private void Submit()
        {
            AsynchronousClientSocket.IpAddressStr = IP;
            _closeWindow(ClosingState.InitiatingConnection);
        }
        private bool CanExecuteSubmitCommand()
        {
            if (string.IsNullOrWhiteSpace(IP))
            {
                IPErrorMessage = string.Empty;
                return false;
            }

            if (!Regex.IsMatch(IP.Trim(), @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$"))
            {
                IPErrorMessage = "Invalid IP format!";
                return false;
            }

            IPErrorMessage = string.Empty;
            return true;
        }
        public ICommand SubmitCommand
        {
            get
            {
                if (_submitCommand == null)
                    _submitCommand = new RelayCommand((param) => { Submit(); }, (param) => { return CanExecuteSubmitCommand(); });

                return _submitCommand;
            }
        }
        #endregion  
    }
}
