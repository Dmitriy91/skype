using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skype.Model;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MicroMvvm;
using System.Windows.Media.Imaging;
using System.Windows;
using Client;
using Skype.Resources.Model;
using System.Windows.Controls;
using System.Threading;
using NetworkPackets.Packet;
using NetworkPackets.Model;

namespace Skype.ViewModel
{
    public class ContactSectionViewModel : BaseViewModel
    {
        #region Fields
        private ProfileSectionViewModel _profileSectionViewModel;
        private Dictionary<int, MultithreadMessageBoxObservableCollection> _contactToChat;
        private string _messageBody = string.Empty;
        private MultithreadMessageBoxObservableCollection _currentChat;
        private MultithreadContactObservableCollection _contacts;
        private Contact _selectedContact;
        Visibility _contactHeaderVisibility = Visibility.Hidden;
        private static readonly object _lock = new object();//used for thread synchronization 
        private static ManualResetEvent _contactsLoaded = new ManualResetEvent(false);
        #endregion

        #region Properties
        public Contact SelectedContact
        {
            get
            {
                return _selectedContact;
            }
            set
            {
                if (value != _selectedContact)
                {
                    _selectedContact = value;
                    NotifyPropertyChanged("SelectedContact");
                    SwitchChat();
                    ContactHeaderVisibility = Visibility.Visible;
                }
            }
        }
        public MultithreadContactObservableCollection Contacts
        {
            get
            {
                return _contacts;
            }
            set
            {
                if (value != _contacts)
                {
                    _contacts = value;
                    NotifyPropertyChanged("Contacts");
                }
            }
        }
        public MultithreadMessageBoxObservableCollection CurrentChat
        {
            get
            {
                return _currentChat;
            }
            set
            {
                if (value != _currentChat)
                {
                    _currentChat = value;
                    NotifyPropertyChanged("CurrentChat");
                }
            }
        }
        public string MessageBody
        {
            get
            {
                return _messageBody;
            }
            set
            {
                if (value != _messageBody)
                {
                    _messageBody = value;
                    NotifyPropertyChanged("MessageBody");
                }
            }
        }
        public Visibility ContactHeaderVisibility
        {
            get
            {
                return _contactHeaderVisibility;
            }
            set
            {
                if (value != _contactHeaderVisibility)
                {
                    _contactHeaderVisibility = value;
                    NotifyPropertyChanged("ContactHeaderVisibility");
                }
            }
        }
        #endregion

        #region Constructor
        public ContactSectionViewModel(ProfileSectionViewModel profileSectionViewModel)
        {
            _profileSectionViewModel = profileSectionViewModel;
            _contactToChat = new Dictionary<int, MultithreadMessageBoxObservableCollection>();
        }
        #endregion

        #region Commands
        private RelayCommand _switchChatCommand;
        private RelayCommand _sendCommand;
        private RelayCommand _removeContactCommand;
        private RelayCommand _newLineCommand;

        private void SwitchChat()
        {
                if (!_contactToChat.ContainsKey(SelectedContact.Id))
                    _contactToChat.Add(SelectedContact.Id, new MultithreadMessageBoxObservableCollection());

                if (CurrentChat != _contactToChat[SelectedContact.Id])
                    CurrentChat = _contactToChat[SelectedContact.Id];
        }
        private bool CanExecuteSwitchChatCommand()
        {
            if (SelectedContact != null)
                return true;

            return false;
        }
        public ICommand SwitchChatCommand 
        {
            get 
            {
                if (_switchChatCommand == null)
                    _switchChatCommand = new RelayCommand((param) => { SwitchChat(); }, (param)=>{ return CanExecuteSwitchChatCommand(); });

                return _switchChatCommand;
            }
        }

        private void Send(object param)
        {
            Message message = new Message()
            {
                MessageBody = MessageBody,
                RecipientID = SelectedContact.Id,
                SendingDateTime = DateTime.Now
            };

            AsynchronousClientSocket.Send(message.CreateTransferablePacket());
            MessageBody = string.Empty;

            lock (_lock)
            {
                CurrentChat.Add(new MessageSection()
                {
                    Message = message.MessageBody,
                    SendingDateTimeStr = message.SendingDateTime.ToString(),
                    Sender = _profileSectionViewModel.Login,
                    AvatarBytes = _profileSectionViewModel.AvatarBytes
                });
                
                ListBox lb = param as ListBox;

                lb.ScrollIntoView(CurrentChat.Last());
            }
        }
        private bool CanExecuteSendCommand()
        {
            if (!string.IsNullOrWhiteSpace(MessageBody) && SelectedContact != null)
                return true;

            return false;
        }
        public ICommand SendCommand
        {
            get
            {
                if (_sendCommand == null)
                    _sendCommand = new RelayCommand( Send, (param) => { return CanExecuteSendCommand(); });

                return _sendCommand;
            }
        }

        private void RemoveContact()
        {
            lock (_lock)
            {
                Contact removedContact = Contacts.FirstOrDefault((contact) =>
                {
                    return contact.Id == SelectedContact.Id;
                });

                if (removedContact != null)
                {
                    Contacts.Remove(removedContact);
                    CurrentChat = null;
                    _contactToChat.Remove(removedContact.Id);
                    RemovingContactRequest RemovingContactRequest = new RemovingContactRequest()
                    {
                        ContactID = removedContact.Id
                    };
                    AsynchronousClientSocket.Send(RemovingContactRequest.CreateTransferablePacket());
                }
            }
        }
        private bool CanExecuteRemoveContactCommand()
        {
            if (SelectedContact != null)
                return true;

            return false;
        }
        public ICommand RemoveContactCommand
        {
            get
            {
                if (_removeContactCommand == null)
                    _removeContactCommand = new RelayCommand((param) => { RemoveContact(); }, (param) => { return CanExecuteRemoveContactCommand(); });

                return _removeContactCommand;
            }
        }

        private void AddNewLine(object param)
        {
            MessageBody += Environment.NewLine;
            TextBox textBox = param as TextBox;
            textBox.CaretIndex = MessageBody.Length - 1;
        }
        private bool CanExecuteNewLineCommand()
        {
            if (string.IsNullOrWhiteSpace(MessageBody))
                return false;

            return true;
        }
        public ICommand NewLineCommand
        {
            get
            {
                if (_newLineCommand == null)
                    _newLineCommand = new RelayCommand( AddNewLine, (param) => { return CanExecuteNewLineCommand(); });

                return _newLineCommand;
            }
        }
        #endregion

        #region Methods
        public void LoadContacts(List<User> users)
        {
            lock (_lock)
            {
                MultithreadContactObservableCollection tempContacts = new MultithreadContactObservableCollection();

                users.ForEach((i) =>
                {
                    tempContacts.Add(new Contact
                    {
                        Id = i.Id,
                        Login = i.Login,
                        Email = i.Email,
                        AvatarBytes = (i.ImageBytes == null ? App.GetDefaultAvatar() : i.ImageBytes),
                        StatusLogoBytes = App.GetStatusLogo(i.IsOnline)
                    });
                });

                Contacts = tempContacts;
                _contactsLoaded.Set();
            }
        }
        public void ProcessNewContact(NewContact newContact)
        {
            _contactsLoaded.WaitOne();
            lock (_lock)
            {
                Contacts.Add(new Contact
                {
                    Id = newContact.Contact.Id,
                    Login = newContact.Contact.Login,
                    Email = newContact.Contact.Email,
                    AvatarBytes = (newContact.Contact.ImageBytes == null ? App.GetDefaultAvatar() : newContact.Contact.ImageBytes),
                    StatusLogoBytes = App.GetStatusLogo(true)
                });
            }
        }
        public void ProcessIncomingMessage(Message incomingMessage)
        {
            _contactsLoaded.WaitOne();
            lock (_lock)
            {
                Contact senderContact = Contacts.FirstOrDefault((contact) =>
                {
                    return contact.Id == incomingMessage.SenderID;
                });

                if (senderContact != null)
                {
                    SelectedContact = senderContact;

                    CurrentChat.Add(new Skype.Model.MessageSection()
                    {
                        Message = incomingMessage.MessageBody,
                        SendingDateTimeStr = incomingMessage.SendingDateTime.ToString(),
                        Sender = senderContact.Login,
                        AvatarBytes = Contacts.FirstOrDefault((i) => { return i.Id == incomingMessage.SenderID; }).AvatarBytes
                    });
                }
            }
        }
        public void ProcessOfflineContact(ContactIsOffline contactIsOffline)
        {
            _contactsLoaded.WaitOne();
            lock (_lock)
            {
                Contact senderContact = Contacts.FirstOrDefault((contact) =>
                {
                    return contact.Id == contactIsOffline.ContactID;
                });

                if (senderContact != null)
                {
                    senderContact.StatusLogoBytes = App.GetStatusLogo(false);
                }
            }
        }
        public void ProcessOnlineContact(ContactIsOnline contactIsOnline)
        {
            _contactsLoaded.WaitOne();
            lock (_lock)
            {
                Contact senderContact = Contacts.FirstOrDefault((contact) =>
                {
                    return contact.Id == contactIsOnline.ContactID;
                });

                if (senderContact != null)
                {
                    senderContact.StatusLogoBytes = App.GetStatusLogo(true);
                }
            }
        }
        public void ProcessRemovingContactResponse(RemovingContactResponse removingContactResponse)
        {
            _contactsLoaded.WaitOne();
            lock (_lock)
            {
                Contact removedContact = Contacts.FirstOrDefault((contact) =>
                {
                    return contact.Id == removingContactResponse.ContactID;
                });

                if (removedContact != null)
                {
                    Contacts.Remove(removedContact);
                    CurrentChat = null;
                    _contactToChat.Remove(removedContact.Id);
                }
            }
        }
        #endregion
    }
}
