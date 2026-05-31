namespace OnlineStoreWeb.Models
{
    public class OnlineOrderItemModel
    {
        public string ProductName { get; set; }

        public decimal Price { get; set; }

        public int Qty { get; set; }

        public decimal Total { get; set; }
    }
}