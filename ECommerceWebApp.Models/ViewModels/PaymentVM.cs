

namespace ECommerceWebApp.Models.ViewModels
{
    public class PaymentVM
    {
        public int OrderHeaderId { get; set; }
        public double OrderTotal { get; set; }
        public string OrderId { get; set; }
        public string RazorpayKey { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerContact { get; set; }
    }

}
