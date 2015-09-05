using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using SkypeNetLogic;
using SkypeNetLogic.Enum;
using SkypeNetLogic.Package;
using System.Configuration;
using Skype.View;
using Skype.ViewModel;
using Skype.Model;
using System.Windows;
using Skype;

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
            bool newPackage = true;
            int receivedBytes = 0;
            int unreceivedBytes = 0;
            PackageType packageType = PackageType.None;

            try
            {
                while (true)
                {
                    if (newPackage)
                    {
                        receivedBytes = _clientSocket.Receive(buffer, BufferSize, SocketFlags.None);
                        packageType = (PackageType)BitConverter.ToInt32(buffer, 0);
                        unreceivedBytes = BitConverter.ToInt32(buffer, 4);
                        receivedBytes -= 8;
                        unreceivedBytes -= receivedBytes;
                        stream.Position = 0;
                        stream.Write(buffer, 8, receivedBytes);
                        newPackage = false;
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
                            ProcessPacket(packageType, new MemoryStream(stream.ToArray()));
                        });

                        newPackage = true;
                    }
                }
            }
            catch (Exception e)
            {
                stream.Close();
                //MessageBox.Show(e.ToString(), "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private static void ProcessPacket(PackageType packageType, Stream stream)
        {
            switch (packageType)
            {
                case PackageType.Message:
                    {
                        Message msg = Package.Deserialize<Message>(stream, true);

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
                case PackageType.AuthenticationResponse:
                    {
                        AuthenticationResponse ar = Package.Deserialize<AuthenticationResponse>(stream, true);

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
                case PackageType.RegistrationResponse:
                    {
                        RegistrationResponse registrationResponse = Package.Deserialize<RegistrationResponse>(stream, true);

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
                case PackageType.SearchResponse:
                    {
                        SearchResponse searchResponse = Package.Deserialize<SearchResponse>(stream, true);
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
                case PackageType.ContactResponse:
                    {
                        ContactResponse contactResponse = Package.Deserialize<ContactResponse>(stream, true);

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
                case PackageType.NewContact:
                    {
                        NewContact newContact = Package.Deserialize<NewContact>(stream, true);

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
                case PackageType.ContactIsOffline:
                    {
                        ContactIsOffline contactIsOffline = Package.Deserialize<ContactIsOffline>(stream, true);

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
                case PackageType.ContactIsOnline:
                    {
                        ContactIsOnline contactIsOnline = Package.Deserialize<ContactIsOnline>(stream, true);

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
                case PackageType.RemovingContactResponse:
                    {
                        RemovingContactResponse removingContactResponse = Package.Deserialize<RemovingContactResponse>(stream, true);

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
