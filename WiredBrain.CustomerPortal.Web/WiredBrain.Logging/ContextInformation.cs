using System;
using System.Collections.Generic;
using System.Text;

namespace WiredBrain.Logging
{
    public class ContextInformation
    {
        public string Host { get; set; }
        public string Method { get; set; }
        public string RemoteIpAddress { get; set; }
        public string Protocol { get; set; }
        public UserInformation UserInfo { get; set; } 
    }

    public class UserInformation
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public Dictionary<string, List<string>> UserClaims { get; set; }
    }
}
