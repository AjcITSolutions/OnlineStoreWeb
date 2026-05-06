namespace OnlineStoreWeb.Models
{
    public class CartItemModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Qty { get; set; }

        public string Image { get; set; }
    }
}