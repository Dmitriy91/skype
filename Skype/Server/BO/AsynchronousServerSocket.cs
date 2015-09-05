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
using SkypeNetLogic;
using System.Windows.Forms;
using SkypeServer.DAO;
using SkypeNetLogic.Enum;
using SkypeNetLogic.Package;

namespace SkypeServer.BO
{
    class AsynchronousServerSocket
    {
        private static Dictionary<Socket, int> _clientSocketDictionary = new Dictionary<Socket, int>();
        private static Socket _serverSocket;
        private static AutoResetEvent _exit = new AutoResetEvent(false);
        private static List<SkypeNetLogic.Package.Message> _unsentMessages = new List<SkypeNetLogic.Package.Message>();

        public static void StartListening()
        {
            int port = int.Parse(ConfigurationManager.AppSettings.Get("Port"));
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
            _serverSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                _serverSocket.Bind(localEndPoint);
                _serverSocket.Listen(100);
                Console.WriteLine("Waiting for a connection...");

                Task.Factory.StartNew(() => {
                    Socket clientSocket = null;
                    
                    while (true)
                    {
                        clientSocket = _serverSocket.Accept();
                        lock(_clientSocketDictionary){_clientSocketDictionary.Add(clientSocket, 0);}

                        Task.Factory.StartNew(() =>
                        {
                            StartReceiving(clientSocket);
                        });
                        Console.WriteLine("Connected! Remote end point: {0}", clientSocket.LocalEndPoint.ToString());
                    }
                });

                _exit.WaitOne();
                foreach (KeyValuePair<Socket, int> kvp in _clientSocketDictionary)
                {
                    kvp.Key.Shutdown(SocketShutdown.Both);
                    kvp.Key.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private static void StartReceiving(Socket clientSocket)
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
                        receivedBytes = clientSocket.Receive(buffer, BufferSize, SocketFlags.None);
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
                        receivedBytes = clientSocket.Receive(buffer, BufferSize, SocketFlags.None);
                        unreceivedBytes -= receivedBytes;
                        stream.Write(buffer, 0, receivedBytes);
                    }

                    if (unreceivedBytes == 0) // all fragments received
                    {
                        Task.Factory.StartNew(() =>
                        {
                            ProcessPacket(clientSocket, packageType, new MemoryStream(stream.ToArray()));
                        });

                        newPackage = true;
                    }
                }
            }
            catch (Exception e)
            {
                int clientID = _clientSocketDictionary[clientSocket];
                List<int> contactIdList = UserDAO.GetContacIDListByUserID(clientID);

                lock (_clientSocketDictionary){_clientSocketDictionary.Remove(clientSocket);}

                Socket recipientSocket = null;
                ContactIsOffline contactIsOffline = new ContactIsOffline() { ContactID = clientID };
                byte[] data = null;

                contactIdList.ForEach((id)=>
                {
                    if(_clientSocketDictionary.ContainsValue(id))
                    {
                        lock (_clientSocketDictionary) 
                        {
                            recipientSocket = _clientSocketDictionary.FirstOrDefault(s => s.Value == id).Key;
                        }
                        
                        if (recipientSocket != null)
                        {
                            data = contactIsOffline.CreateTransferablePackage();
                            Task.Factory.StartNew(() => 
                            {
                                Send(recipientSocket, data); 
                            });
                        }
                    }
                });

                stream.Close();
                Console.WriteLine(e.ToString());
            }
        }
        private static void ProcessPacket(Socket clientSocket, PackageType packageType, Stream stream)
        {
            switch (packageType)
            {
                case PackageType.Message:
                    {
                        SkypeNetLogic.Package.Message msg = SkypeNetLogic.Package.Message.Deserialize<SkypeNetLogic.Package.Message>(stream, true);

                        if (msg != null)
                        {
                            lock (_clientSocketDictionary)
                            {
                                msg.SenderID = _clientSocketDictionary[clientSocket];

                                if (_clientSocketDictionary.ContainsValue(msg.RecipientID))
                                {
                                    Socket recipientSocket = _clientSocketDictionary.FirstOrDefault(s => s.Value == msg.RecipientID).Key;

                                    if (recipientSocket != null)
                                    {
                                        Send(recipientSocket, msg.CreateTransferablePackage());
                                    }
                                    else
                                    {
                                        lock (_unsentMessages) { _unsentMessages.Add(msg); }
                                    }
                                }
                                else 
                                {
                                    lock (_unsentMessages) { _unsentMessages.Add(msg); }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Message deserialization failure!");
                        }
                    }
                    break;
                case PackageType.AuthenticationRequest:
                    {
                        AuthenticationRequest authentication = AuthenticationRequest.Deserialize<AuthenticationRequest>(stream, true);

                        if (authentication != null)
                        {
                            AuthenticationResponse ar = UserDAO.Authenticate(authentication.Login, authentication.Password);
                            
                            if (ar.HasError == false)
                            {
                                ContactIsOnline contactIsOnline = new ContactIsOnline();
                                contactIsOnline.ContactID = ar.Profile.Id;
                                byte[] byteArr = contactIsOnline.CreateTransferablePackage();
                                Socket recipientSocket = null;

                                lock (_clientSocketDictionary) 
                                {
                                    _clientSocketDictionary[clientSocket] = ar.Profile.Id;

                                    ar.ContactList.ForEach((i) => 
                                    {
                                        if (_clientSocketDictionary.ContainsValue(i.Id))
                                        {
                                            i.IsOnline = true;
                                            recipientSocket = _clientSocketDictionary.FirstOrDefault(s => s.Value == i.Id).Key;

                                            if (recipientSocket != null)
                                            {
                                                Task.Factory.StartNew(() =>
                                                {
                                                    Send(recipientSocket, byteArr);
                                                });
                                            }
                                        }
                                    });
                                }

                                int clientID = ar.Profile.Id;

                                try
                                {
                                    ar.UnreceivedMessages = new List<SkypeNetLogic.Package.Message>();
                                    foreach (SkypeNetLogic.Package.Message msg in _unsentMessages)
                                    {
                                        if (msg.RecipientID == clientID)
                                            ar.UnreceivedMessages.Add(msg);
                                            //Send(clientSocket, PackageHelper.Serialize(PackageType.Message, msg));
                                    }

                                    Send(clientSocket, ar.CreateTransferablePackage());

                                    lock (_unsentMessages)
                                    {
                                        _unsentMessages.RemoveAll((i) =>
                                        {
                                            return i.RecipientID == clientID ? true : false;
                                        });
                                    }
                                    Console.WriteLine("Authenticated! " + ar.Profile.Id.ToString());
                                }
                                catch
                                {
                                    Console.WriteLine("Authentication failure!");
                                }
                            }
                            else
                            {
                                Send(clientSocket, ar.CreateTransferablePackage());
                                Console.WriteLine("Invalid password or login!");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Authentication deserialization failure!");
                        }
                    }
                    break;
                case PackageType.RegistrationRequest:
                    {
                        RegistrationRequest registrationRequest = Package.Deserialize<RegistrationRequest>(stream, true);

                        if (registrationRequest != null)
                        {
                            string newImgName = null;

                            if (registrationRequest.ImageBytes != null)
                            {
                                newImgName = Path.GetRandomFileName();
                                File.WriteAllBytes(UserDAO.ImagePath + newImgName, registrationRequest.ImageBytes);
                            }

                            RegistrationResponse registrationResponse = UserDAO.Register(registrationRequest.Login, registrationRequest.Password, registrationRequest.Email, newImgName);
                            byte[] bytyData = registrationResponse.CreateTransferablePackage();
                            
                            Send(clientSocket, bytyData);
                        }
                        else
                        {
                            Console.WriteLine("Registration request deserialization failure!");
                        }
                    }
                    break;
                case PackageType.SearchRequest:
                    {
                        SearchRequest searchRequest = Package.Deserialize<SearchRequest>(stream, true);

                        if (searchRequest != null)
                        {
                            SearchResponse searchResponse = UserDAO.GetContactListBySearchStr(searchRequest.SearchStr);
                            int requesterID = _clientSocketDictionary[clientSocket];
                            byte[] byteData = null;

                            searchResponse.ContactList.Remove(new SkypeNetLogic.Model.User() { Id = requesterID }); // Removing requester from search result
                            //searchResponse.ContactList.ForEach((i) =>
                            //{
                            //    if (_clientSocketDictionary.ContainsValue(i.Id))
                            //        i.IsOnline = true;
                            //});
                            byteData = searchResponse.CreateTransferablePackage();
                            Send(clientSocket, byteData);
                        }
                        else
                        {
                            Console.WriteLine("Search request deserialization failure!");
                        }
                    }
                    break;
                case PackageType.ContactRequest:
                    {
                        ContactRequest contactRequest = Package.Deserialize<ContactRequest>(stream, true);

                        if (contactRequest != null)
                        {
                            Console.WriteLine("Server: contact request received!");
                            int clientID = 0;
                            
                            lock(_clientSocketDictionary)
                            {
                                clientID = _clientSocketDictionary[clientSocket];
                            }

                            SkypeNetLogic.Model.User contact = UserDAO.GetContactByUserID(clientID);

                            if (contact != null)
                            {
                                ContactResponse contactResponse = new ContactResponse();
                                contactResponse.Contact = contact;

                                if (_clientSocketDictionary.ContainsValue(contactRequest.ContactID))
                                {
                                    Socket recipientSocket = null;

                                    lock (_clientSocketDictionary)
                                    {
                                        recipientSocket = _clientSocketDictionary.FirstOrDefault(s => s.Value == contactRequest.ContactID).Key;
                                    }

                                    if (recipientSocket != null)
                                    {
                                        Send(recipientSocket, contactResponse.CreateTransferablePackage());
                                    }
                                }
                            }
                            else 
                            {
                                Console.WriteLine("Invalid UserID!");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Contact request deserialization failure!");
                        }
                    }
                    break;
                case PackageType.AddingContact:
                    {
                        AddingContact addingContact = Package.Deserialize<AddingContact>(stream, true);

                        if (addingContact != null)
                        {
                            Console.WriteLine("Adding friend!");

                            int clientID = 0;

                            lock (_clientSocketDictionary)
                            {
                                clientID = _clientSocketDictionary[clientSocket];
                            }

                            if (UserDAO.AddContact(clientID, addingContact.ContactID) && UserDAO.AddContact(addingContact.ContactID, clientID))
                            {
                                SkypeNetLogic.Model.User contact = UserDAO.GetContactByUserID(clientID);

                                if (contact != null)
                                {
                                    NewContact newContact = new NewContact();
                                    newContact.Contact = contact;

                                    if (_clientSocketDictionary.ContainsValue(addingContact.ContactID))
                                    {
                                        Socket recipientSocket = null;

                                        lock (_clientSocketDictionary)
                                        {
                                            recipientSocket = _clientSocketDictionary.FirstOrDefault(s => s.Value == addingContact.ContactID).Key;
                                        }

                                        if (recipientSocket != null)
                                        {
                                            Send(recipientSocket, newContact.CreateTransferablePackage());
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Invalid UserID!");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Contact request deserialization failure!");
                        }
                    }
                    break;
                case PackageType.RemovingContactRequest:
                    {
                        RemovingContactRequest removingContactRequest = Package.Deserialize<RemovingContactRequest>(stream, true);

                        if (removingContactRequest != null)
                        {
                            Console.WriteLine("Removing contact!");

                            lock (_clientSocketDictionary)
                            {
                                removingContactRequest.SenderID = _clientSocketDictionary[clientSocket];
                            }

                            if (UserDAO.RemoveContactPair(removingContactRequest.SenderID, removingContactRequest.ContactID))
                            {
                                if (_clientSocketDictionary.ContainsValue(removingContactRequest.ContactID))
                                {
                                    Socket recipientSocket = null;

                                    lock (_clientSocketDictionary)
                                    {
                                        recipientSocket = _clientSocketDictionary.FirstOrDefault(s => s.Value == removingContactRequest.ContactID).Key;
                                    }

                                    if (recipientSocket != null)
                                    {
                                        RemovingContactResponse removingContactResponse = new RemovingContactResponse();

                                        removingContactResponse.ContactID = removingContactRequest.SenderID;
                                        Send(recipientSocket, removingContactResponse.CreateTransferablePackage());
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Contact request deserialization failure!");
                        }
                    }
                    break;
            }
        }
        private static void Send(Socket clientSocket, byte[] byteData)
        {
            int byteNumber = byteData.Length;
            int sentByteNumber = 0;

            try
            {
                lock (clientSocket)
                {
                    while (sentByteNumber < byteNumber)
                    {
                        Console.WriteLine("Sending message..");
                        sentByteNumber += clientSocket.Send(byteData, sentByteNumber, byteNumber - sentByteNumber, SocketFlags.None);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public static void ShutdownAll()
        {
            _exit.Set();
        }
    }
}
