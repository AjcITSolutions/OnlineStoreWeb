
using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.Models;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Localization;

namespace OnlineStoreWeb.Controllers
{
    public class HomeController : Controller
    {



public IActionResult ChangeLanguage(string culture)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            });

            return RedirectToAction("Index", "Home");
        }

    private readonly ILogger<HomeController> _logger;


        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            List<ProductModel> products = new List<ProductModel>();

            using (HttpClient client = new HttpClient())
            {
              //  string apiUrl = "https://localhost:7010/api/Products";
                string apiUrl = "https://ataonlinestoreapi.runasp.net/api/Products";
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    products = JsonSerializer.Deserialize<List<ProductModel>>
                    (
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }
                    );
                }
            }

            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }


    }
}