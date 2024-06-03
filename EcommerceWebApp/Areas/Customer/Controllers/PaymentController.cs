using EcommerceWebApp.Service;
using ECommerceWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceWebApp.Areas.Customer.Controllers
{
	public class PaymentController : Controller
	{
		private readonly RazorpayService _razorpayService;

		public PaymentController(RazorpayService razorpayService)
		{
			_razorpayService = razorpayService;
		}

		public IActionResult Index(PaymentViewModel paymentViewModel)
		{
			
			return View(paymentViewModel);
		}

	}
}
