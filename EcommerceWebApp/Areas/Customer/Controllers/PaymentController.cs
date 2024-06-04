using EcommerceWebApp.Service;
using ECommerceWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceWebApp.Areas.Customer.Controllers
{
	[Route("[controller]")]
	public class PaymentController : Controller
	{
		private readonly RazorpayService _razorpayService;

		public PaymentController(RazorpayService razorpayService)
		{
			_razorpayService = razorpayService;
		}

		[Route("[action]")]
		public IActionResult Index(PaymentViewModel paymentViewModel)
		{
			
			return View(paymentViewModel);
		}

	}
}
