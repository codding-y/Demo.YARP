// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Yarp.Auth.Controllers
{
    [AllowAnonymous]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            ViewData["returnUrl"] = returnUrl;

            return View();
        }

        /// <summary>
        /// 处理来自Login.cshtml页面的输入以验证用户
        /// </summary>
        /// <param name="name"></param>
        /// <param name="myClaimValue"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Login(string name, string myClaimValue, string returnUrl)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, name),
                new Claim("myCustomClaim", myClaimValue)
            }, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            return SignIn(principal, new AuthenticationProperties()
            {
                RedirectUri = returnUrl
                // SignIn is the only one that requires a scheme: https://github.com/dotnet/aspnetcore/issues/23325
            }, CookieAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            return SignOut(new AuthenticationProperties()
            {
                RedirectUri = "/Account/LoggedOut",
            });
        }

        [HttpGet]
        public IActionResult LoggedOut()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
