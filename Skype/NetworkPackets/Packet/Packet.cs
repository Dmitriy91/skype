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
    public class Packet
    {
        protected byte[] CreateTransferablePacket(PacketType packageType)
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, this);
            byte[] buffer = stream.ToArray();
            stream.Close();

            byte[] packageBody = new byte[8 + buffer.Length];

            BitConverter.GetBytes((int)packageType).CopyTo(packageBody, 0);
            BitConverter.GetBytes(buffer.Length).CopyTo(packageBody, 4);
            buffer.CopyTo(packageBody, 8);
            return packageBody;
        }

        public static TPackage Deserialize<TPackage>(Stream stream, bool closeStream)
            where TPackage: Packet
        {
            IFormatter formatter = new BinaryFormatter();
            Object package = formatter.Deserialize(stream);

            if (closeStream)
                stream.Close();

            if (package is TPackage)
                return (TPackage)package;

            return null;
        }
    }
}
