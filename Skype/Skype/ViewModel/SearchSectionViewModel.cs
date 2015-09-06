using Client;
using MicroMvvm;
using Skype.Model;
using Skype.Resources.Model;
using NetworkPackets.Enum;
using NetworkPackets.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
namespace Skype.ViewModel
{
    public class SearchSectionViewModel : BaseViewModel
    {
        #region Fields
        private ContactSectionViewModel _contactSectionViewModel;
        private string _searchStr = string.Empty;
        private Contact _selectedContact;
        private MultithreadContactObservableCollection _contacts;

        #endregion

        #region Properties
        public string SearchStr
        {
            get
            {
                return _searchStr;
            }
            set
            {
                _searchStr = value;
                NotifyPropertyChanged("SearchStr");
            }
        }
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
        #endregion

        #region Constructor
        public SearchSectionViewModel(ContactSectionViewModel contactSectionViewModel)
        {
            _contactSectionViewModel = contactSectionViewModel;
        }
        #endregion

        #region Commands
        private RelayCommand _searchCommand;
        private RelayCommand _requestContactCommand;

        void RequestContact()
        {
            ContactRequest contactRequest = new ContactRequest();

            contactRequest.ContactID = SelectedContact.Id;
            Contacts.Remove(SelectedContact);
            SelectedContact = null;
            AsynchronousClientSocket.Send(contactRequest.CreateTransferablePacket());
        }
        bool CanExecuteRequestContactCommand()
        {
            if (SelectedContact != null)
            {
                if (!_contactSectionViewModel.Contacts.Any((i) => { return i.Id == SelectedContact.Id ? true : false; }))
                {
                    return true;
                }
            }

            return false;
        }
        public ICommand RequestContactCommand
        {
            get
            {
                if (_requestContactCommand == null)
                    _requestContactCommand = new RelayCommand((param) => { RequestContact(); }, (param) => { return CanExecuteRequestContactCommand(); });

                return _requestContactCommand;
            }
        }

        void Search()
        {
            SearchRequest searchRequest = new SearchRequest();
            
            searchRequest.SearchStr = SearchStr;
            AsynchronousClientSocket.Send(searchRequest.CreateTransferablePacket());
        }
        bool CanExecuteSearchCommand()
        {
            if (string.IsNullOrWhiteSpace(SearchStr))
                return false;

            return true;
        }
        public ICommand SearchCommand
        {
            get
            {
                if (_searchCommand == null)
                    _searchCommand = new RelayCommand((param) => { Search(); }, (param) => { return CanExecuteSearchCommand(); });

                return _searchCommand;
            }
        }
        #endregion

        #region Methods
        public void ProcessSearchResponse(SearchResponse searchResponse)
        {
            MultithreadContactObservableCollection tempContacts = new MultithreadContactObservableCollection(); 

            searchResponse.ContactList.ForEach((i) =>
            {
                tempContacts.Add(new Contact
                {
                    Id = i.Id,
                    Login = i.Login,
                    Email = i.Email,
                    AvatarBytes = (i.ImageBytes == null ? App.GetDefaultAvatar() : i.ImageBytes)
                });
            });

            Contacts = tempContacts;
        }
        #endregion
    }
}
