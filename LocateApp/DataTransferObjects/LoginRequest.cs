using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace LocateApp.DataTransferObjects
{
    [ExcludeFromCodeCoverage]
    public class LoginSendRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public int RememberMe { get; set; }
        public string ExPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string MacAdress { get; set; }
    }
}