using System;
using System.Collections.Generic;
using System.Text;

namespace Score247Management.Core.Users.Models
{
    public class User
    {
        public string UserId { get; }
        public string Username { get; }
        public string Password { get; }
        public UserRole Role { get; }
        public string Status { get; }
    }
}