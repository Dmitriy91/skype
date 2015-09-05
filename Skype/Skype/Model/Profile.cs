using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Skype.Model
{
    public class Profile
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public byte[] AvatarBytes { get; set; }
        
    }
}
