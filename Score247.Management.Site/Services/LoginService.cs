namespace Score247.Management.Site.Services
{
    public interface ILoginService
    {
        bool ExistUser(string username, string password);
    }

    public class LoginService : ILoginService
    {
        public bool ExistUser(string username, string password)

        {
            if (username == "score247" && password == "score@247")
            {
                return true;
            }

            return false;
        }
    }
}