using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skype.Model;
using System.ComponentModel;
using System.Windows.Input;
using MicroMvvm;
using System.Windows;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Skype.View;
using SkypeNetLogic.Package;
using Client;
using SkypeNetLogic.Enum;
using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;

namespace Skype.ViewModel
{
    public enum RegistrationDialogResult { ShowAuthenticationWin, Registered }

    public class RegistrationWinViewModel : BaseViewModel
    {
        public static RegistrationWinViewModel CurrentViewModel { get; private set; }

        #region Fields
        private Action<RegistrationDialogResult> _closeWindow;
        private bool _defaultAvatar = true;
        private byte[] _avatarBytes = App.GetDefaultAvatar();
        private string _login;
        private string _loginErrorMessage;
        private string _email;
        private string _emailErrorMessage;
        private string _password;
        private string _passwordErrorMessage;
        private string _confirmedPassword;
        private string _confirmedPasswordErrorMessage;
        #endregion

        #region Properties
        public byte[] AvatarBytes
        {
            get
            {
                return _avatarBytes;
            }
            set
            {
                _avatarBytes = value;
                NotifyPropertyChanged("AvatarBytes");
            }
        }
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
        public string Email
        {
            get 
            {
                return _email;
            }
            set 
            {
                _email = value;
                NotifyPropertyChanged("Email");
            }
        }
        public string EmailErrorMessage
        {
            get
            {
                return _emailErrorMessage;
            }
            set
            {
                _emailErrorMessage = value;
                NotifyPropertyChanged("EmailErrorMessage");
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
        public string ConfirmedPassword
        { 
            get
            {
                return _confirmedPassword;
            }
            set
            {
                _confirmedPassword = value;
                NotifyPropertyChanged("ConfirmedPassword");
            }
        }
        public string ConfirmedPasswordErrorMessage
        {
            get
            {
                return _confirmedPasswordErrorMessage;
            }
            set
            {
                _confirmedPasswordErrorMessage = value;
                NotifyPropertyChanged("ConfirmedPasswordErrorMessage");
            }
        }
        #endregion

        #region Helper methods
        private bool CheckLogin()
        {
            if (string.IsNullOrEmpty(Login))
            {
                LoginErrorMessage = string.Empty;
                return false;
            }

            if (Regex.IsMatch(Login, @"^[A-Za-z0-9'.]{1,40}$"))
            {
                LoginErrorMessage = string.Empty;
                return true;
            }

            LoginErrorMessage = "Characters and digits only!";
            return false;
        }
        private bool CheckEmail()
        {
            if (string.IsNullOrEmpty(Email))
            {
                EmailErrorMessage = string.Empty;
                return false;
            }

            if (Regex.IsMatch(Email, @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" + @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$"))
            {
                EmailErrorMessage = string.Empty;
                return true;
            }
            
            EmailErrorMessage = "Invalid email address!";
            return false;
        }
        private bool CheckPassword()
        {
            if (string.IsNullOrEmpty(Password))
            {
                PasswordErrorMessage = string.Empty;
                ConfirmedPasswordErrorMessage = string.Empty;
                return false;
            }

            if (Regex.IsMatch(Password, @"(?!^[0-9]*$)(?!^[a-zA-Z]*$)^([a-zA-Z0-9]{8,32})$"))
            {
                PasswordErrorMessage = string.Empty;
                if (CheckConfimedPassword())
                    return true;
                
                return false;
            }

            PasswordErrorMessage = "It must be at least 8 characters long, contain at least one digit \nand one alphabetic character, and must not contain special characters.";
            return false;
        }
        private bool CheckConfimedPassword()
        {
            if (Password != ConfirmedPassword)
            {
                ConfirmedPasswordErrorMessage = "Doesn't match the password!";
                return false;
            }

            ConfirmedPasswordErrorMessage = string.Empty;
            return true;
        }
        #endregion

        #region Constractor

        public RegistrationWinViewModel(Action<RegistrationDialogResult> closeWindow)
        {
            CurrentViewModel = this;
            _closeWindow = closeWindow;
        }
        #endregion

        #region Commands
        private RelayCommand _changeAvatarCommand;
        private RelayCommand _registerCommand;
        private RelayCommand _resetCommand;
        private RelayCommand _cancelCommand;
        private RelayCommand _showAuthenticationWinCommand;

        private void ChangeAvatar()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = @"Image files (*.png;*.jpeg)|*.png;*.jpg;*.jpeg";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    AvatarBytes = File.ReadAllBytes(openFileDialog.FileName);
                    _defaultAvatar = false;
                }
                catch (Exception)
                {
                    //an error message goes here)
                }
            }
        }
        public ICommand ChangeAvatarCommand
        {
            get
            {
                if (_changeAvatarCommand == null)
                    _changeAvatarCommand = new RelayCommand((param) => { ChangeAvatar(); });

                return _changeAvatarCommand;
            }
        }

        private void ShowAuthenticationWin(Object param)
        {
            _closeWindow(RegistrationDialogResult.ShowAuthenticationWin);
        }
        public ICommand ShowAuthenticationWinCommand
        {
            get
            {
                if (_showAuthenticationWinCommand == null)
                    _showAuthenticationWinCommand = new RelayCommand(ShowAuthenticationWin);

                return _showAuthenticationWinCommand;
            }
        }

        private void Submit()
        {
            byte[] hash = null;
            using (SHA512 shaM = new SHA512Managed())
            {
                hash = shaM.ComputeHash(Encoding.UTF8.GetBytes(Password));
            }

            RegistrationRequest registrationRequest = new RegistrationRequest();
            registrationRequest.Login = Login;
            registrationRequest.Password = System.Text.Encoding.UTF8.GetString(hash);
            registrationRequest.Email = Email;
            if (_defaultAvatar == false)
                registrationRequest.ImageBytes = AvatarBytes;

            AsynchronousClientSocket.Send(registrationRequest.CreateTransferablePackage());
        }
        private bool CanExecuteSubmitCommand()
        {
            return !(CheckLogin() == false | CheckEmail() == false | CheckPassword() == false);
        }
        public ICommand SubmitCommand
        {
            get
            {
                if (_registerCommand == null)
                    _registerCommand = new RelayCommand((param) => { Submit(); }, (param) => { return CanExecuteSubmitCommand(); });

                return _registerCommand;
            }
        }

        private void Reset()
        {
            Login = Email = Password = ConfirmedPassword = string.Empty;
        }
        private bool CanExecuteResetCommand()
        {
            if (!string.IsNullOrWhiteSpace(Login) || !string.IsNullOrWhiteSpace(Email) || !string.IsNullOrWhiteSpace(Password) || !string.IsNullOrWhiteSpace(ConfirmedPassword))
                return true;

            return false;
        }
        public ICommand ResetCommand
        {
            get
            {
                if (_resetCommand == null)
                    _resetCommand = new RelayCommand((param)=>{Reset();}, (param) => { return CanExecuteResetCommand(); });

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
        #endregion

        #region Methods
        public void ProcessRegistrationResponse(RegistrationResponse registrationResponse)
        {
            if (registrationResponse.HasError == true)
            {
                LoginErrorMessage = registrationResponse.LoginError;
                MessageBox.Show("Error");
                MessageBox.Show(registrationResponse.LoginError);
            }
            else
            {
                MessageBox.Show("Done!");
                _closeWindow(RegistrationDialogResult.Registered);
            }
        }
        #endregion
    }
}
