using NetworkPackets.Enum;
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
    public class RegistrationResponse : Packet
    {
        public bool HasError { get; set; }
        public string LoginError { get; set; }

        public byte[] CreateTransferablePacket()
        {
            return CreateTransferablePacket(PacketType.RegistrationResponse);
        }
    }
}
