using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.Models;
using System.Text.Json;

namespace OnlineStoreWeb.Controllers
{
    public class ProductsController : Controller
    {
        public async Task<IActionResult> Index()
        {
            List<ProductModel> products = new List<ProductModel>();

            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://localhost:7010/api/Products";

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

        public IActionResult AddToCart(string id, string name, decimal price)
        {
            List<CartItemModel> cart = new List<CartItemModel>();

            var sessionCart = HttpContext.Session.GetString("Cart");

            if (sessionCart != null)
            {
                cart = JsonSerializer.Deserialize<List<CartItemModel>>(sessionCart);
            }

            var existingItem = cart.FirstOrDefault(x => x.Id == id);

            if (existingItem != null)
            {
                existingItem.Qty++;
            }
            else
            {
                cart.Add(new CartItemModel
                {
                    Id = id,
                    Name = name,
                    Price = price,
                    Qty = 1,
                    Image = "https://localhost:7010/api/Products/GetImage/" + id
                });
            }

            HttpContext.Session.SetString
            (
                "Cart",
                JsonSerializer.Serialize(cart)
            );

            return Redirect(Request.Headers["Referer"].ToString());
        }

        // PRODUCT DETAILS

        public async Task<IActionResult> Details(string id)
        {
            List<ProductModel> products = new List<ProductModel>();

            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://localhost:7010/api/Products";

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

            var product = products.FirstOrDefault(x => x.Id == id);

            return View(product);
        }


        public IActionResult Cart()
        {
            List<CartItemModel> cart = new List<CartItemModel>();

            var sessionCart = HttpContext.Session.GetString("Cart");

            if (sessionCart != null)
            {
                cart = JsonSerializer.Deserialize<List<CartItemModel>>(sessionCart);
            }

            return View(cart);
        }

        public IActionResult IncreaseQty(string id)
        {
            var sessionCart = HttpContext.Session.GetString("Cart");

            if (sessionCart != null)
            {
                var cart = JsonSerializer.Deserialize<List<CartItemModel>>(sessionCart);

                var item = cart.FirstOrDefault(x => x.Id == id);

                if (item != null)
                {
                    item.Qty++;
                }

                HttpContext.Session.SetString
                (
                    "Cart",
                    JsonSerializer.Serialize(cart)
                );
            }

            return RedirectToAction("Cart");
        }

        public IActionResult DecreaseQty(string id)
        {
            var sessionCart = HttpContext.Session.GetString("Cart");

            if (sessionCart != null)
            {
                var cart = JsonSerializer.Deserialize<List<CartItemModel>>(sessionCart);

                var item = cart.FirstOrDefault(x => x.Id == id);

                if (item != null)
                {
                    item.Qty--;

                    if (item.Qty <= 0)
                    {
                        cart.Remove(item);
                    }
                }

                HttpContext.Session.SetString
                (
                    "Cart",
                    JsonSerializer.Serialize(cart)
                );
            }

            return RedirectToAction("Cart");
        }
    }
}