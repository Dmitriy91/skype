using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkPackets.Enum
{
    public enum PacketType 
    { 
        None, 
        RegistrationRequest, 
        RegistrationResponse,
        AuthenticationRequest, 
        AuthenticationResponse, 
        SearchRequest, 
        SearchResponse, 
        ContactRequest, 
        ContactResponse, 
        AddingContact, 
        NewContact, 
        Message, 
        File, 
        ContactIsOnline, 
        ContactIsOffline, 
        RemovingContactRequest, 
        RemovingContactResponse
    }
}
