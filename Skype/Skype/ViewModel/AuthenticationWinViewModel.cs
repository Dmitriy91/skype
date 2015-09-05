using Client;
using MicroMvvm;
using Skype.View;
using SkypeNetLogic.Enum;
using SkypeNetLogic.Package;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Skype.ViewModel
{
    public enum AuthenticationDialogResult { ShowRegWin, Authenticated }

    public class AuthenticationWinViewModel : BaseViewModel
    {
        public static AuthenticationWinViewModel CurrentViewModel { get; private set; }

        #region Fields
        private string _login;
        private string _loginErrorMessage;
        private string _password;
        private string _passwordErrorMessage;
        private Action<AuthenticationDialogResult> _closeWindow;
        #endregion

        #region Properties
        public string Login
        {
            get
            {
                return _login;
            }
            set
            {
                _login = value;
                NotifyPropertyChanged("Login");
            }
        }
        public string LoginErrorMessage
        {
            get
            {
                return _loginErrorMessage;
            }
            set
            {
                _loginErrorMessage = value;
                NotifyPropertyChanged("LoginErrorMessage");
            }
        }
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                NotifyPropertyChanged("Password");
            }
        }
        public string PasswordErrorMessage
        {
            get
            {
                return _passwordErrorMessage;
            }
            set
            {
                _passwordErrorMessage = value;
                NotifyPropertyChanged("PasswordErrorMessage");
            }
        }
        #endregion

        #region Constructor
        public AuthenticationWinViewModel(Action<AuthenticationDialogResult> closeWindow)
        {
            CurrentViewModel = this;
            _closeWindow = closeWindow;
        }
        #endregion

        #region Commands
        private RelayCommand _submitCommand;
        private RelayCommand _resetCommand;
        private RelayCommand _cancelCommand;
        private RelayCommand _showRegWinCommand;

        private void ShowRegWin()
        {
            _closeWindow(AuthenticationDialogResult.ShowRegWin);
        }
        public ICommand ShowRegWinCommand
        {   
            get
            {
                if (_showRegWinCommand == null)
                    _showRegWinCommand = new RelayCommand((param) => { ShowRegWin(); });

                return _showRegWinCommand;
            }
        }

        private void Reset()
        {
            Login = Password  = string.Empty;
        }
        private bool CanExecuteResetCommand()
        {
            if (!string.IsNullOrWhiteSpace(Login) || !string.IsNullOrWhiteSpace(Password))
                return true;

            return false;
        }
        public ICommand ResetCommand
        {
            get
            {
                if (_resetCommand == null)
                    _resetCommand = new RelayCommand((param) => { Reset(); }, (param) => { return CanExecuteResetCommand(); });

                return _resetCommand;
            }
        }

        private void Cancel()
        {
            AsynchronousClientSocket.Shutdown();
            Process.GetCurrentProcess().CloseMainWindow();
        }
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                    _cancelCommand = new RelayCommand((param) => { Cancel(); });

                return _cancelCommand;
            }
        }

        private void Submit()
        {
            AuthenticationRequest ar = new AuthenticationRequest();
            byte[] hash = null;

            using (SHA512 shaM = new SHA512Managed())
            {
                hash = shaM.ComputeHash(Encoding.UTF8.GetBytes(Password));
            }

            ar.Login = Login;
            ar.Password = System.Text.Encoding.UTF8.GetString(hash);
            AsynchronousClientSocket.Send(ar.CreateTransferablePackage());
        }
        private bool CanExecuteSubmitCommand()
        {
            if (!string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password))
                return true;

            return false;
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

        #region Methods
        public void ProcessAuthenticationResponse(AuthenticationResponse ar)
        {
            if (ar.HasError == true)
            {
                LoginErrorMessage = ar.LoginError;
                PasswordErrorMessage = ar.PasswordError;
            }
            else
            {
                MainWinViewModel.CurrentViewModel.LoadProfile(ar.Profile);
                MainWinViewModel.CurrentViewModel.LoadContacts(ar.ContactList);
                ar.UnreceivedMessages.ForEach((i) =>
                {
                    MainWinViewModel.CurrentViewModel.ProcessIncomingMessage(i);
                });
                _closeWindow(AuthenticationDialogResult.Authenticated);
            }
        }
        #endregion
    }
}
