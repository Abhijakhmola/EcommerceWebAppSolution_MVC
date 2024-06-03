using Razorpay.Api;

namespace EcommerceWebApp.Service
{

	public class RazorpayService
	{
		private readonly string _key;
		private readonly string _secret;

		public RazorpayService(IConfiguration configuration)
		{
			_key = configuration["Razorpay:KeyId"];
			_secret = configuration["Razorpay:KeySecret"];
		}

		public Order CreateOrder(decimal amount, string currency = "INR")
		{
			RazorpayClient client = new RazorpayClient(_key, _secret);

			Dictionary<string, object> options = new Dictionary<string, object>
		{
			{ "amount", amount * 100 }, // Amount in paise
            { "currency", currency },
		};

			Order order = client.Order.Create(options);
			return order;
		}
	}

}
