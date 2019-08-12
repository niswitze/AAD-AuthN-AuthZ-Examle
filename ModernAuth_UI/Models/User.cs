using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ModernAuth_UI.Models
{
    public class User
    {
        public string DisplayName { get; set; }
        public string Mail { get; set; }
        public string UserPrincipalName { get; set; }
    }
}