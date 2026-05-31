using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.Models;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace OnlineStoreWeb.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IConfiguration _configuration;

        public ProductsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task<IActionResult> Index(
           string search = "",
           int categoryId = 0)
        {
            List<ProductModel> products =
                new List<ProductModel>();

            using (HttpClient client =
                   new HttpClient())
            {
                string apiUrl =
                    "https://ataonlinestoreapi.runasp.net/api/Products"
                    + "?search="
                    + Uri.EscapeDataString(search ?? "")
                    + "&categoryId="
                    + categoryId;

                var response =
                    await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json =
                        await response.Content.ReadAsStringAsync();

                    products =
                        JsonSerializer.Deserialize<List<ProductModel>>
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
                  //  Image = "https://localhost:7010/api/Products/GetImage/" + id
                    Image = "https://ataonlinestoreapi.runasp.net/api/Products/GetImage/" + id
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
            ProductModel product = null;

            using (HttpClient client = new HttpClient())
            {
                string apiUrl =
                    "https://ataonlinestoreapi.runasp.net/api/Products/" + id;

                var response =
                    await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json =
                        await response.Content.ReadAsStringAsync();

                    product =
                        JsonSerializer.Deserialize<ProductModel>
                        (
                            json,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            }
                        );
                }
            }

            if (product == null)
            {
                return NotFound();
            }

            ///////////////////////////////SEO//////////////////////////////////

            ViewData["Title"] = product.Name;

            ViewData["Description"] =
                $"{product.Name} بأفضل سعر وجودة في الأردن من Loyal Store";

            //////////////////////////////////////////////////////////////////

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

        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Checkout(CheckoutModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var cartJson = HttpContext.Session.GetString("Cart");

                if (string.IsNullOrEmpty(cartJson))
                {
                    TempData["Error"] = "السلة فارغة";

                    return RedirectToAction("Cart");
                }

                var cart =
                    JsonSerializer.Deserialize<List<CartItemModel>>(cartJson);

                if (cart == null || !cart.Any())
                {
                    TempData["Error"] = "السلة فارغة";

                    return RedirectToAction("Cart");
                }

                decimal total = cart.Sum(x => x.Price * x.Qty);

                decimal deliveryFee = 0;

                switch (model.City)
                {
                    case "Amman":
                        deliveryFee = 2;
                        break;

                    case "Zarqa":
                        deliveryFee = 3;
                        break;

                    case "Irbid":
                        deliveryFee = 4;
                        break;

                    case "Aqaba":
                        deliveryFee = 5;
                        break;
                }

                decimal netTotal = total + deliveryFee;

                string connStr =
                    _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();

                    string orderQuery = @"
INSERT INTO online_orders
(
    customer_name,
    phone,
    address,
    notes,
    total,
    city,
    area,
    delivery_notes,
    delivery_fee,
    net_total
)
OUTPUT INSERTED.id
VALUES
(
    @customer_name,
    @phone,
    @address,
    @notes,
    @total,
    @city,
    @area,
    @delivery_notes,
    @delivery_fee,
    @net_total
)";

                    SqlCommand cmd =
                        new SqlCommand(orderQuery, con);

                    cmd.Parameters.AddWithValue("@customer_name",
                        model.CustomerName ?? "");

                    cmd.Parameters.AddWithValue("@phone",
                        model.Phone ?? "");

                    cmd.Parameters.AddWithValue("@address",
                        model.Address ?? "");

                    cmd.Parameters.AddWithValue("@notes",
                        model.Notes ?? "");

                    cmd.Parameters.AddWithValue("@total",
                        total);

                    cmd.Parameters.AddWithValue("@city",
                        model.City ?? "");

                    cmd.Parameters.AddWithValue("@area",
                        model.Area ?? "");

                    cmd.Parameters.AddWithValue("@delivery_notes",
                        model.DeliveryNotes ?? "");

                    cmd.Parameters.AddWithValue("@delivery_fee",
                        deliveryFee);

                    cmd.Parameters.AddWithValue("@net_total",
                        netTotal);

                    int orderId = Convert.ToInt32(cmd.ExecuteScalar());

                    foreach (var item in cart)
                    {
                        string detailsQuery = @"
INSERT INTO online_order_details
(
    order_id,
    product_id,
    product_name,
    price,
    qty,
    total
)
VALUES
(
    @order_id,
    @product_id,
    @product_name,
    @price,
    @qty,
    @total
)";

                        SqlCommand detailsCmd =
                            new SqlCommand(detailsQuery, con);

                        detailsCmd.Parameters.AddWithValue("@order_id",
                            orderId);

                        detailsCmd.Parameters.AddWithValue("@product_id",
                            item.Id);

                        detailsCmd.Parameters.AddWithValue("@product_name",
                            item.Name ?? "");

                        detailsCmd.Parameters.AddWithValue("@price",
                            item.Price);

                        detailsCmd.Parameters.AddWithValue("@qty",
                            item.Qty);

                        detailsCmd.Parameters.AddWithValue("@total",
                            item.Price * item.Qty);

                        detailsCmd.ExecuteNonQuery();
                    }
                }

                HttpContext.Session.Remove("Cart");

                TempData["Success"] = "تم إرسال الطلب بنجاح";

                return RedirectToAction("Success");
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "حدث خطأ أثناء حفظ الطلب، حاول مرة أخرى";

                return RedirectToAction("Checkout");
            }
        }
        public IActionResult Success()
        {
            return View();
        }

        public IActionResult TrackOrder()
        {
            return View();
        }

        [HttpPost]
        public IActionResult TrackOrder(string phone)
        {
            string connStr =
                _configuration.GetConnectionString("DefaultConnection");

           List<OnlineOrderModel> orders =
    new List<OnlineOrderModel>();

            using (SqlConnection con =
                new SqlConnection(connStr))
            {
                con.Open();

                string query = @"
SELECT  *
FROM online_orders
WHERE phone = @phone
ORDER BY id DESC";

                SqlCommand cmd =
                    new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@phone", phone);

                SqlDataReader dr =
                    cmd.ExecuteReader();

                while (dr.Read())
                {
                    orders.Add(new OnlineOrderModel
                    {
                        Id = Convert.ToInt32(dr["id"]),

                        CustomerName =
                            dr["customer_name"].ToString(),

                        Phone =
                            dr["phone"].ToString(),

                        Total =
                            dr["net_total"] == DBNull.Value
                            ? 0
                            : Convert.ToDecimal(dr["net_total"]),

                        OrderStatus =
                            dr["order_status"].ToString(),

                        PaymentStatus =
                            dr["payment_status"].ToString(),

                        DeliveryStatus =
                            dr["delivery_status"].ToString(),

                        OrderDate =
                            Convert.ToDateTime(dr["order_date"])
                    });
                }
            }

            return View(orders);
        }
    }
}