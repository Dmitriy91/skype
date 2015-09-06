using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using Skype.View;
using Skype.ViewModel;
using Skype.Model;
using System.Windows;
using Skype;
using NetworkPackets.Enum;
using NetworkPackets.Packet;

namespace Client
{
    public class AsynchronousClientSocket
    {
        private static readonly int _port = int.Parse(ConfigurationManager.AppSettings.Get("Port"));
        private static Socket _clientSocket;
        public static string IpAddressStr { get; set; }

        public static void Shutdown()
        {
            // Release the socket.
            _clientSocket.Shutdown(SocketShutdown.Both);
            _clientSocket.Close();
        }
        public static void StartClient()
        {
            // Connect to a remote device.
            try
            {
               // IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                IPAddress ipAddress = IPAddress.Parse(IpAddressStr); //ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, _port);

                // Create a TCP/IP socket.
                _clientSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                _clientSocket.Connect(remoteEP);
                App._connected.Set();//the signal to the main window to be shown
                Task.Factory.StartNew(StartReceiving).Wait();
            } 
            catch (Exception)
            {
                //Logging an error if needed and rethrowing
                throw;
            }
        }
        public static void Send(byte[] data)
        {
            int byteNumber = data.Length;
            int sentByteNumber = 0;

            try
            {
                lock (_clientSocket)
                {
                    while (sentByteNumber < byteNumber)
                    {
                        Console.WriteLine("Sending message..");
                        sentByteNumber += _clientSocket.Send(data, sentByteNumber, byteNumber - sentByteNumber, SocketFlags.None);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void StartReceiving()
        {
            MemoryStream stream = new MemoryStream();
            const int BufferSize = 8192;
            byte[] buffer = new byte[BufferSize];
            bool newPacket = true;
            int receivedBytes = 0;
            int unreceivedBytes = 0;
            PacketType PacketType = PacketType.None;

            try
            {
                while (true)
                {
                    if (newPacket)
                    {
                        receivedBytes = _clientSocket.Receive(buffer, BufferSize, SocketFlags.None);
                        PacketType = (PacketType)BitConverter.ToInt32(buffer, 0);
                        unreceivedBytes = BitConverter.ToInt32(buffer, 4);
                        receivedBytes -= 8;
                        unreceivedBytes -= receivedBytes;
                        stream.Position = 0;
                        stream.Write(buffer, 8, receivedBytes);
                        newPacket = false;
                    }
                    else
                    {
                        receivedBytes = _clientSocket.Receive(buffer, BufferSize, SocketFlags.None);
                        unreceivedBytes -= receivedBytes;
                        stream.Write(buffer, 0, receivedBytes);
                    }

                    if (unreceivedBytes == 0) // all fragments received
                    {
                        Task.Factory.StartNew(() =>
                        {
                            ProcessPacket(PacketType, new MemoryStream(stream.ToArray()));
                        });

                        newPacket = true;
                    }
                }
            }
            catch (Exception e)
            {
                stream.Close();
            }
        }
        private static void ProcessPacket(PacketType PacketType, Stream stream)
        {
            switch (PacketType)
            {
                case PacketType.Message:
                    {
                        Message msg = Packet.Deserialize<Message>(stream, true);

                        if (msg != null)
                        {
                            if (MainWinViewModel.CurrentViewModel != null)
                            {
                                MainWinViewModel.CurrentViewModel.ProcessIncomingMessage(msg);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Registration response deserialization failure!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
                case PacketType.AuthenticationResponse:
                    {
                        AuthenticationResponse ar = Packet.Deserialize<AuthenticationResponse>(stream, true);

                        if (ar != null)
                        {
                            if (AuthenticationWinViewModel.CurrentViewModel != null)
                            {
                                AuthenticationWinViewModel.CurrentViewModel.ProcessAuthenticationResponse(ar);
                            }
                        }
                        else 
                        {
                            MessageBox.Show("Authentication response deserialization failure!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
                case PacketType.RegistrationResponse:
                    {
                        RegistrationResponse registrationResponse = Packet.Deserialize<RegistrationResponse>(stream, true);

                        if (registrationResponse != null)
                        {
                            if (RegistrationWinViewModel.CurrentViewModel != null)
                            {
                                RegistrationWinViewModel.CurrentViewModel.ProcessRegistrationResponse(registrationResponse);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Registration response deserialization failure!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
                case PacketType.SearchResponse:
                    {
                        SearchResponse searchResponse = Packet.Deserialize<SearchResponse>(stream, true);
                        if (searchResponse != null)
                        {
                            if (MainWinViewModel.CurrentViewModel != null)
                            {
                                MainWinViewModel.CurrentViewModel.ProcessSearchResponse(searchResponse);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Search response deserialization failure!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
                case PacketType.ContactResponse:
                    {
                        ContactResponse contactResponse = Packet.Deserialize<ContactResponse>(stream, true);

                        if (contactResponse != null)
                        {
                            MainWinViewModel.CurrentViewModel.ProcessContactResponse(contactResponse);
                        }
                        else
                        {
                            MessageBox.Show("Contact response deserialization failure!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
                case PacketType.NewContact:
                    {
                        NewContact newContact = Packet.Deserialize<NewContact>(stream, true);

                        if (newContact != null)
                        {
                            MainWinViewModel.CurrentViewModel.ProcessNewContact(newContact);
                        }
                        else
                        {
                            MessageBox.Show("New contact deserialization failure!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
                case PacketType.ContactIsOffline:
                    {
                        ContactIsOffline contactIsOffline = Packet.Deserialize<ContactIsOffline>(stream, true);

                        if (contactIsOffline != null)
                        {
                            MainWinViewModel.CurrentViewModel.ProcessOfflineContact(contactIsOffline);
                        }
                        else
                        {
                            MessageBox.Show("Offline contact deserialization failure!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
                case PacketType.ContactIsOnline:
                    {
                        ContactIsOnline contactIsOnline = Packet.Deserialize<ContactIsOnline>(stream, true);

                        if (contactIsOnline != null)
                        {
                            MainWinViewModel.CurrentViewModel.ProcessOnlineContact(contactIsOnline);
                        }
                        else
                        {
                            MessageBox.Show("Online contact deserialization failure!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
                case PacketType.RemovingContactResponse:
                    {
                        RemovingContactResponse removingContactResponse = Packet.Deserialize<RemovingContactResponse>(stream, true);

                        if (removingContactResponse != null)
                        {
                            MainWinViewModel.CurrentViewModel.ProcessRemovingContactResponse(removingContactResponse);
                        }
                        else
                        {
                            MessageBox.Show("Removing contact response deserialization failure!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;
            }
        }
    }
}
