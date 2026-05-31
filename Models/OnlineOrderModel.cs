namespace OnlineStoreWeb.Models
{
    public class OnlineOrderModel
    {
        public int Id { get; set; }

        public string CustomerName { get; set; }

        public string Phone { get; set; }

        public decimal Total { get; set; }

        public string OrderStatus { get; set; }

        public string PaymentStatus { get; set; }

        public string DeliveryStatus { get; set; }

        public DateTime OrderDate { get; set; }

        public int TotalOrders { get; set; }

        public int PendingOrders { get; set; }

        public int PaidOrders { get; set; }

        public decimal TotalSales { get; set; }
    }
}