using NetworkPackets.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace NetworkPackets.Packet
{
    [Serializable]
    public class Message : Packet
    {
        public int SenderID { get; set; }
        public int RecipientID { get; set; }
        public string MessageBody { get; set; }
        public DateTime SendingDateTime { get; set;}

        public byte[] CreateTransferablePacket()
        {
            return CreateTransferablePacket(PacketType.Message);
        }
    }
}
