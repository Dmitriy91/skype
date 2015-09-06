using NetworkPackets.Packet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Text;


namespace SkypeServer.DAO
{
    public static class UserDAO
    {
        public static string ImagePath { get; private set; }

        static UserDAO()
        {
            try
            {
                ImagePath = ConfigurationManager.AppSettings.Get("Images");
            }
            catch (Exception)
            { 
                //an error message goes here)
            }
        }

        public static AuthenticationResponse Authenticate(string login, string password)
        {
            SkypeDataClassesDataContext sdc = new SkypeDataClassesDataContext();
            ISingleResult<SP_AuthenticateResult> authenticationResult = sdc.SP_Authenticate(login, password);
            AuthenticationResponse ar = new AuthenticationResponse();            
            int returnedValue = (int)authenticationResult.ReturnValue;

            if (returnedValue == 1)
            {
                NetworkPackets.Model.User profile = null;

                foreach (SP_AuthenticateResult user in authenticationResult)
                    profile = new NetworkPackets.Model.User 
                    { 
                        Id = user.UserID, 
                        Login = user.Login, 
                        Email = user.Email,
                        ImageBytes = (user.ImageName == null? null: GetImgByteArrByName(user.ImageName))
                    };

                ar.Profile = profile;
                ar.ContactList = GetContactListByUserID(profile.Id);
            }
            else
            {
                ar.HasError = true;

                if (returnedValue == 0)
                    ar.LoginError = "Doesn't exist!";

                if (returnedValue == -1)
                    ar.PasswordError = "Invalid password!";
            }

            return ar;
        }

        public static RegistrationResponse Register(string login, string password, string email, string imgName)
        {
            SkypeDataClassesDataContext sdc = new SkypeDataClassesDataContext();
            int returnedValue = sdc.SP_Register(login, email, password, imgName);
            RegistrationResponse registrationResponse = new RegistrationResponse();

            if (returnedValue == 1)
            {
                registrationResponse.HasError = false;
                return registrationResponse;
            }
            
            registrationResponse.HasError = true;
            registrationResponse.LoginError = "Already Exists!";
            
            return registrationResponse;
        }

        public static List<NetworkPackets.Model.User> GetContactListByUserID(int userID)
        {
            SkypeDataClassesDataContext sdc = new SkypeDataClassesDataContext();
            ISingleResult<SP_ContactList_SEL_byUserIDResult> contacts = sdc.SP_ContactList_SEL_byUserID(userID);
            List<NetworkPackets.Model.User> contactList = new List<NetworkPackets.Model.User>();

            foreach (SP_ContactList_SEL_byUserIDResult contact in contacts)
            {
                contactList.Add(new NetworkPackets.Model.User 
                { 
                    Id = contact.UserID, 
                    Login = contact.Login, 
                    Email = contact.Email,
                    ImageBytes = (contact.ImageName == null ? null : GetImgByteArrByName(contact.ImageName))
                });
            }

            return contactList;
        }

        public static NetworkPackets.Model.User GetContactByUserID(int userID)
        {
            SkypeDataClassesDataContext sdc = new SkypeDataClassesDataContext();
            ISingleResult<SP_Contact_SEL_byUserIDResult> contacts = sdc.SP_Contact_SEL_byUserID(userID);

            foreach (SP_Contact_SEL_byUserIDResult contact in contacts)
            {
                return new NetworkPackets.Model.User 
                { 
                    Id = contact.UserID, 
                    Login = contact.Login, 
                    Email = contact.Email,
                    ImageBytes = (contact.ImageName == null ? null : GetImgByteArrByName(contact.ImageName))
                };
            }

            return null;
        }

        public static SearchResponse GetContactListBySearchStr(string searchStr)
        {
            SkypeDataClassesDataContext sdc = new SkypeDataClassesDataContext();
            ISingleResult<SP_Contact_SEL_byLoginResult> contacts = sdc.SP_Contact_SEL_byLogin(searchStr);
            SearchResponse searchResponse = new SearchResponse();

            foreach (SP_Contact_SEL_byLoginResult contact in contacts)
            {
                searchResponse.ContactList.Add(new NetworkPackets.Model.User 
                {
                    Id = contact.UserID, 
                    Login = contact.Login, 
                    Email = contact.Email,
                    ImageBytes = (contact.ImageName == null ? null : GetImgByteArrByName(contact.ImageName))
                });
            }

            return searchResponse;
        }

        public static List<int> GetContacIDListByUserID(int userID)
        {
            SkypeDataClassesDataContext sdc = new SkypeDataClassesDataContext();
            ISingleResult<SP_ContactIDList_SEL_byUserIDResult> contacts = sdc.SP_ContactIDList_SEL_byUserID(userID);

            List<int> idList = new List<int>();

            foreach (SP_ContactIDList_SEL_byUserIDResult contact in contacts)
            {
                idList.Add(contact.ContactID);
            }

            return idList;
        }

        public static bool AddContact(int userID, int contactID)
        {
            SkypeDataClassesDataContext sdc = new SkypeDataClassesDataContext();
            int result = sdc.SP_Contact_INS(userID, contactID);

            if (result == 1)
                return true;

            return false;
        }

        public static bool RemoveContactPair(int userID, int contactID)
        {
            SkypeDataClassesDataContext sdc = new SkypeDataClassesDataContext();
            int result = sdc.SP_ContactPair_DEL(userID, contactID);

            if (result == 1)
                return true;

            return false;
        }

        #region Helper method
        private static byte[] GetImgByteArrByName(string name)
        {
            try
            {
                return File.ReadAllBytes(ImagePath + name);
            }
            catch(Exception) 
            {
                return null;
            }
        }
        #endregion
    }
}
