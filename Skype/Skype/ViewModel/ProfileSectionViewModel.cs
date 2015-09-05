using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Skype.Model;
using System.Windows.Input;
using MicroMvvm;
using System.Windows.Media.Imaging;
using System.IO;
using System.Configuration;

namespace Skype.ViewModel
{
    public class ProfileSectionViewModel : BaseViewModel
    {
        private Profile _profile = new Profile();

        #region Properties
        public string Login
        {
            get
            {
                return _profile.Login;
            }
            set 
            {
                _profile.Login = value;
                NotifyPropertyChanged("Login");
            }
        }
        public string Email
        {
            get
            {
                return _profile.Email;
            }
            set
            {
                _profile.Email = value;
                NotifyPropertyChanged("Email");
            }
        }
        public byte[] AvatarBytes 
        {
            get
            {
                return _profile.AvatarBytes;
            }
            set
            {
                _profile.AvatarBytes = value;
                NotifyPropertyChanged("AvatarBytes");
            }
        }
        #endregion

        #region Methods
        public void LoadProfile(SkypeNetLogic.Model.User user)
        {
            Login = user.Login;
            Email = user.Email;
            AvatarBytes = (user.ImageBytes == null ? App.GetDefaultAvatar() : user.ImageBytes);
        }
        #endregion
    }
}
