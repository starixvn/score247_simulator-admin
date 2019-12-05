using Score247Management.Core.Enumerations;

namespace Score247Management.Core.Users.Models
{
    public class UserRole : Enumeration
    {
        ////ap – The match has ended after penalties
        public static readonly UserRole Admin = new UserRole(1, "admin");

        public UserRole()
        {
        }

        public UserRole(byte value, string displayName)
            : base(value, displayName)
        {
        }

        public UserRole(byte value)
            : base(value, value.ToString())
        {
        }
    }
}