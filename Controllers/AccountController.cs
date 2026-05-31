
using Microsoft.AspNetCore.Mvc;

namespace OnlineStoreWeb.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (username == "admin" &&
               password == "123456")
            {
                HttpContext.Session.SetString(
                    "Admin",
                    "true");

                return RedirectToAction(
                    "Orders",
                    "Admin");
            }

            ViewBag.Error = "Invalid login";

            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Admin");

            return RedirectToAction(
                "Login");
        }
    }
}