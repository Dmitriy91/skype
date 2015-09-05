using Skype.Model;
using Skype.Resources.Model;
using SkypeNetLogic.Model;
using SkypeNetLogic.Package;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Skype.ViewModel
{
    public class MainWinViewModel : BaseViewModel
    {
        #region Properties
        public MainWinMenuViewModel MainWinMenuViewModel { get; private set; }
        public ContactSectionViewModel ContactSectionViewModel { get; private set; }
        public ProfileSectionViewModel ProfileSectionViewModel { get; private set; }
        public SearchSectionViewModel SearchSectionModel { get; private set; }
        public RecentSectionViewModel RecentSectionViewModel { get; private set; }
        public static MainWinViewModel CurrentViewModel { get; private set; }
        #endregion

        #region Constructor
        public MainWinViewModel()
        {
            //initializing and binding sub view-models among themselves to provide access
            ProfileSectionViewModel = new ProfileSectionViewModel();
            ContactSectionViewModel = new ContactSectionViewModel(ProfileSectionViewModel);
            SearchSectionModel = new SearchSectionViewModel(ContactSectionViewModel);
            RecentSectionViewModel = new RecentSectionViewModel(ContactSectionViewModel);
            MainWinMenuViewModel = new MainWinMenuViewModel();
            CurrentViewModel = this;
            //Provide access to the current view-model instance for other windows
        }
        #endregion

        #region Methods
        public void LoadContacts(List<User> users)
        {
            ContactSectionViewModel.LoadContacts(users);
        }
        public void ProcessNewContact(NewContact newContact)
        {
            ContactSectionViewModel.ProcessNewContact(newContact);
        }
        public void ProcessIncomingMessage(Message incomingMessage)
        {
            ContactSectionViewModel.ProcessIncomingMessage(incomingMessage);
        }
        public void ProcessOfflineContact(ContactIsOffline contactIsOffline)
        {
            ContactSectionViewModel.ProcessOfflineContact(contactIsOffline);
        }
        public void ProcessOnlineContact(ContactIsOnline contactIsOnline)
        {
            ContactSectionViewModel.ProcessOnlineContact(contactIsOnline);
        }
        public void LoadProfile(SkypeNetLogic.Model.User user)
        {
            ProfileSectionViewModel.LoadProfile(user);
        }
        public void ProcessSearchResponse(SearchResponse searchResponse)
        {
            SearchSectionModel.ProcessSearchResponse(searchResponse);
        }
        public void ProcessContactResponse(ContactResponse contactResponse)
        {
            RecentSectionViewModel.ProcessContactResponse(contactResponse);
        }
        public void ProcessRemovingContactResponse(RemovingContactResponse removingContactResponse)
        {
            ContactSectionViewModel.ProcessRemovingContactResponse(removingContactResponse);
        }
        #endregion
    }
}
