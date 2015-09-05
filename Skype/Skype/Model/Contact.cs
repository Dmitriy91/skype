using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace Skype.Model
{
    public class Contact : INotifyPropertyChanged, IEquatable<Contact>
    {
        #region Fields
        private string _login;
        private string _email;
        private byte[] _avatarBytes;
        private byte[] _statusLogoBytes;
        #endregion

        #region Properties
        public int Id { get; set; }
        public string Login
        {
            get
            {
                return _login;
            }
            set
            {
                if (value != _login)
                {
                    _login = value;
                    NotifyPropertyChanged("Login");
                }
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
                if (value != _email)
                {
                    _email = value;
                    NotifyPropertyChanged("Email");
                }
            }
        }
        public byte[] AvatarBytes
        {
            get
            {
                return _avatarBytes;
            }
            set
            {
                if (value != _avatarBytes)
                {
                    _avatarBytes = value;
                    NotifyPropertyChanged("AvatarBytes");
                }
            }
        }
        public byte[] StatusLogoBytes
        {
            get
            {
                return _statusLogoBytes;
            }
            set
            {
                if (value != _statusLogoBytes)
                {
                    _statusLogoBytes = value;
                    NotifyPropertyChanged("StatusLogoBytes");
                }
            }
        }
        #endregion

        #region Methods
        public bool Equals(Contact other)
        {
            if (other == null)
                return false;

            return other.Id == this.Id;
        }
        public override int GetHashCode()
        {
            return Id;
        }
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
