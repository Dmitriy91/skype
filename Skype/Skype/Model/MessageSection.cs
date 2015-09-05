using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skype.Model
{
    public class MessageSection
    {
        public string Sender { get; set; }
        public string Message { get; set; }
        public string SendingDateTimeStr { get; set; }
        public byte[] AvatarBytes { get; set; }
        public bool IsOutgoing { get; set; }
    }
}
