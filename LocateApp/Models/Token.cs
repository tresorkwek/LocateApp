using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocateApp.Models
{
    public class Token
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public double expires_in { get; set; }
    }
}