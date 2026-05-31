
using System.ComponentModel.DataAnnotations;

namespace OnlineStoreWeb.Models
{
    public class CheckoutModel
    {
        [Required]
        public string CustomerName { get; set; }

        [Required]
        public string Phone { get; set; }



        public string? Address { get; set; }

        public string? Notes { get; set; }

        public string? City { get; set; }

        public string? Area { get; set; }

        public string? DeliveryNotes { get; set; }

    }
}