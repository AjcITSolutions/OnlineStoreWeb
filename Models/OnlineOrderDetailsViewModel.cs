namespace OnlineStoreWeb.Models
{
    public class OnlineOrderDetailsViewModel
    {
        public int Id { get; set; }

        public string CustomerName { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Area { get; set; }

        public string Notes { get; set; }

        public string DeliveryNotes { get; set; }

        public decimal Total { get; set; }

        public string OrderStatus { get; set; }

        public string PaymentStatus { get; set; }

        public string DeliveryStatus { get; set; }

        public List<OnlineOrderItemModel> Items { get; set; }
    }
}