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
    public class NewContact : Packet
    {
        public User Contact { get; set; }

        public byte[] CreateTransferablePacket()
        {
            return CreateTransferablePacket(PacketType.NewContact);
        }
    }
}
