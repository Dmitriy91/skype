using SkypeServer.BO;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SkypeNetLogic;
using System.IO;

namespace SkypeServer
{
    public class SkypeServer
    {
        public static void Main(String[] args)
        {
            AsynchronousServerSocket.StartListening();
        }
    }
}
