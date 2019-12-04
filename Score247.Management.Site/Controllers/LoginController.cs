using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Score247.Management.Site.Services;

namespace Score247.Management.Site.Controllers
{
    [Authorize]
    public class LoginController : Controller
    {
        private readonly ILoginService loginService;

        public LoginController(ILoginService loginService)
        {
            this.loginService = loginService;
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/Match");
            }

            return View();
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            return View();
        }

        public ActionResult Revoke()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Login(IFormCollection collection)
        {
            var username = (string)collection["username"];
            var password = (string)collection["password"];

            if (loginService.ExistUser(username, password))
            {
                var claims = new List<Claim>
            {
              new Claim(ClaimTypes.Name, Guid.NewGuid().ToString())
            };

                var claimsIdentity = new ClaimsIdentity(
                  claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties();

                await HttpContext.SignInAsync(
                  CookieAuthenticationDefaults.AuthenticationScheme,
                  new ClaimsPrincipal(claimsIdentity),
                  authProperties);
            }

            return Redirect("/Match");
        }
    }
}