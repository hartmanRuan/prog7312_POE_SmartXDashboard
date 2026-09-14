using System;
using System.Collections.Generic;
using System.Text;

namespace SmartXDashboard.Models
{
    public class UserAccount
    {
        public string Username { get; set; }
        public string Password { get; set; } // In production, store hashes; plain text is fine for this list mock
        public string FullName { get; set; }
    }
}
