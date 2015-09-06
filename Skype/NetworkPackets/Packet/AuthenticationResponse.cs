using NetworkPackets.Enum;
using NetworkPackets.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace NetworkPackets.Packet
{
    [Serializable]
    public class AuthenticationResponse : Packet
    {
        public User Profile { get; set; }
        public List<User> ContactList { get; set; }
        public List<NetworkPackets.Packet.Message> UnreceivedMessages { get; set; }
        public bool HasError{get ; set;}
        public string LoginError { get; set; }
        public string PasswordError { get; set; }

        public byte[] CreateTransferablePacket()
        {
            return CreateTransferablePacket(PacketType.AuthenticationResponse);
        }

    }
}
