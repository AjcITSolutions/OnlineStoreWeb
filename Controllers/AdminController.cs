using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OnlineStoreWeb.Models;

namespace OnlineStoreWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;

        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Orders(
           string search,
           string status,
           int page = 1)
        {

            if (HttpContext.Session.GetString("Admin") != "true")
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            int pageSize = 5;
            int totalOrdersCount = 0;
            List<OnlineOrderModel> orders =
                new List<OnlineOrderModel>();

            string connStr =
                _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection con =
                new SqlConnection(connStr))
            {
                con.Open();



            
      


                string countQuery =
    "SELECT COUNT(*) FROM online_orders";

                SqlCommand countCmd =
                    new SqlCommand(countQuery, con);

                totalOrdersCount =
                    (int)countCmd.ExecuteScalar();

                int skip =
                    (page - 1) * pageSize;

                string query = @"
        SELECT *
        FROM online_orders
     WHERE
(
    customer_name LIKE @search
    OR phone LIKE @search
    OR CAST(id AS NVARCHAR) LIKE @search
)
AND
(
    @status = ''
    OR order_status = @status
    OR payment_status = @status
    OR delivery_status = @status
)
       ORDER BY id DESC
OFFSET @skip ROWS
FETCH NEXT @pageSize ROWS ONLY";

                SqlCommand cmd =
                    new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@search",
                    "%" + (search ?? "") + "%");
                cmd.Parameters.AddWithValue("@status",
                 status ?? "");
                cmd.Parameters.AddWithValue("@skip", skip);

                cmd.Parameters.AddWithValue("@pageSize",
                    pageSize);
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

            ViewBag.TotalOrders =
                orders.Count;

            ViewBag.PendingOrders =
                orders.Count(x => x.OrderStatus == "Pending");

            ViewBag.PaidOrders =
                orders.Count(x => x.PaymentStatus == "Paid");

            ViewBag.TotalSales =
                orders.Sum(x => x.Total);

            ViewBag.CurrentPage = page;

            ViewBag.TotalPages =
                (int)Math.Ceiling(
                    (double)totalOrdersCount / pageSize);

            return View(orders);
        }

        public IActionResult OrderDetails(int id)
        {
            if (HttpContext.Session.GetString("Admin") != "true")
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            OnlineOrderDetailsViewModel model =
                new OnlineOrderDetailsViewModel();

            model.Items =
                new List<OnlineOrderItemModel>();

            string connStr =
                _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection con =
                new SqlConnection(connStr))
            {
                con.Open();

                // ORDER INFO

                string orderQuery = @"
        SELECT *
        FROM online_orders
        WHERE id=@id";

                SqlCommand orderCmd =
                    new SqlCommand(orderQuery, con);

                orderCmd.Parameters.AddWithValue("@id", id);

                SqlDataReader dr =
                    orderCmd.ExecuteReader();

                if (dr.Read())
                {
                    model.Id =
                        Convert.ToInt32(dr["id"]);

                    model.CustomerName =
                        dr["customer_name"].ToString();

                    model.Phone =
                        dr["phone"].ToString();

                    model.Address =
                        dr["address"].ToString();

                    model.City =
                        dr["city"].ToString();

                    model.Area =
                        dr["area"].ToString();

                    model.Notes =
                        dr["notes"].ToString();

                    model.DeliveryNotes =
                        dr["delivery_notes"].ToString();

                    model.Total =
                        dr["net_total"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(dr["net_total"]);
                    model.OrderStatus =
                         dr["order_status"].ToString();

                    model.PaymentStatus =
                        dr["payment_status"].ToString();

                    model.DeliveryStatus =
                        dr["delivery_status"].ToString();
                }

                dr.Close();

                // ORDER ITEMS

                string detailsQuery = @"
        SELECT *
        FROM online_order_details
        WHERE order_id=@id";

                SqlCommand detailsCmd =
                    new SqlCommand(detailsQuery, con);

                detailsCmd.Parameters.AddWithValue("@id", id);

                SqlDataReader detailsDr =
                    detailsCmd.ExecuteReader();

                while (detailsDr.Read())
                {
                    model.Items.Add(
                        new OnlineOrderItemModel
                        {
                            ProductName =
                                detailsDr["product_name"].ToString(),

                            Price =
                                Convert.ToDecimal(detailsDr["price"]),

                            Qty =
                                Convert.ToInt32(detailsDr["qty"]),

                            Total =
                                Convert.ToDecimal(detailsDr["total"])
                        });
                }
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult UpdateStatus
(
    int id,
    string orderStatus,
    string paymentStatus,
    string deliveryStatus
)
        {
            if (HttpContext.Session.GetString("Admin") != "true")
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            string connStr =
                _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection con =
                new SqlConnection(connStr))
            {
                string query = @"
        UPDATE online_orders
        SET
            order_status=@order_status,
            payment_status=@payment_status,
            delivery_status=@delivery_status
        WHERE id=@id";

                SqlCommand cmd =
                    new SqlCommand(query, con);

                cmd.Parameters.AddWithValue(
                    "@order_status",
                    orderStatus);

                cmd.Parameters.AddWithValue(
                    "@payment_status",
                    paymentStatus);

                cmd.Parameters.AddWithValue(
                    "@delivery_status",
                    deliveryStatus);

                cmd.Parameters.AddWithValue(
                    "@id",
                    id);

                con.Open();

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction(
                "OrderDetails",
                new { id = id });
        }
    }
}