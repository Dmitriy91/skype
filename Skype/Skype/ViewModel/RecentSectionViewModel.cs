using Client;
using MicroMvvm;
using Skype.Model;
using Skype.Resources.Model;
using SkypeNetLogic.Enum;
using SkypeNetLogic.Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Skype.ViewModel
{
    public class RecentSectionViewModel : BaseViewModel
    {
        #region Fields
        private ContactSectionViewModel _contactSectionViewModel;
        private Contact _selectedContact;
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
                }
            }
        }
        public MultithreadContactObservableCollection Contacts { get; set; }
        #endregion

        #region Constractor
        public RecentSectionViewModel(ContactSectionViewModel contactSectionViewModel)
        {
            _contactSectionViewModel = contactSectionViewModel;
            Contacts = new MultithreadContactObservableCollection();
        }
        #endregion

        #region Commands
        private RelayCommand _acceptContactCommand;
        private RelayCommand _declineContactCommand;

        private void AcceptContact()
        {
            AddingContact addingContact = new AddingContact();
            Contact acceptedContact = SelectedContact;
            acceptedContact.StatusLogoBytes = App.GetStatusLogo(true);
            Contacts.Remove(acceptedContact);
            _contactSectionViewModel.Contacts.Add(acceptedContact);
            addingContact.ContactID = acceptedContact.Id;
            AsynchronousClientSocket.Send(addingContact.CreateTransferablePackage());
        }
        private bool CanExecuteAcceptContactCommand()
        {
            if (SelectedContact != null)
                return true;

            return false;
        }
        public ICommand AcceptContactCommand
        {
            get
            {
                if (_acceptContactCommand == null)
                    _acceptContactCommand = new RelayCommand((param) => { AcceptContact(); }, (param) => { return CanExecuteAcceptContactCommand(); });

                return _acceptContactCommand;
            }
        }

        private void DeclineContact()
        {
            Contacts.Remove(SelectedContact);
        }
        private bool CanExecuteDeclineContactCommand()
        {
            if (SelectedContact != null)
                return true;

            return false;
        }
        public ICommand DeclineContactCommand
        {
            get
            {
                if (_declineContactCommand == null)
                    _declineContactCommand = new RelayCommand((param) => { DeclineContact(); }, (param) => { return CanExecuteDeclineContactCommand(); });

                return _declineContactCommand;
            }
        }
        #endregion

        #region Methods
        public void ProcessContactResponse(ContactResponse contactResponse)
        {
            Contact tempContact = new Contact
            {
                Id = contactResponse.Contact.Id,
                Login = contactResponse.Contact.Login,
                Email = contactResponse.Contact.Email,
                AvatarBytes = (contactResponse.Contact.ImageBytes == null ? App.GetDefaultAvatar() : contactResponse.Contact.ImageBytes)
            };

            if (!Contacts.Contains(tempContact))
                Contacts.Add(tempContact);
        }
        #endregion
    }
}
