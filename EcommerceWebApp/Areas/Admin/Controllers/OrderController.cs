using ECommerceWebApp.DataAccess.Repository.IRespository;
using ECommerceWebApp.Models;
using ECommerceWebApp.Models.ViewModels;
using ECommerceWebApp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;
using Razorpay.Api.Errors;
using System.Security.Claims;

namespace EcommerceWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RazorPaySettings _razorpaySettings;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork, RazorPaySettings razorPaySettings)
        {
            _unitOfWork = unitOfWork;
            _razorpaySettings = razorPaySettings;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };
            return View(OrderVM);
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.Postalcode = OrderVM.OrderHeader.Postalcode;
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            orderHeader.OrderStatus = SD.StatusShipped;
            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();

            TempData["success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }
      
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                try
                {
                    var client = new RazorpayClient(_razorpaySettings.KeyId, _razorpaySettings.KeySecret);
                    var payment = client.Payment.Fetch(orderHeader.PaymentId);

                    var options = new Dictionary<string, object>
            {
                { "payment_id", orderHeader.PaymentId },
                { "amount", (orderHeader.OrderTotal * 100).ToString() }, // Amount in paise
                
            };

                    var refund = client.Payment.Refund(options);

                    // Handle successful refund (optional)
                    // ...

                    _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
                    _unitOfWork.Save();
                    TempData["success"] = "Order Cancelled Successfully.";
                }
                catch (Exception ex)
                {
                    // Log the exception details (including raw response if logging is enabled)
                    // ...

                    // Handle the error gracefully (e.g., retry, notify user)
                    TempData["error"] = "An error occurred during refund. Please try again later.";
                }
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
            OrderVM.OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeader.Id == OrderVM.OrderHeader.Id, includeProperties: "Product");

            //razorpay payment logic
            var options = new Dictionary<string, object>
        {
            { "amount", (OrderVM.OrderHeader.OrderTotal * 100).ToString() }, // amount in the smallest currency unit
            { "currency", "INR" },
            { "receipt", OrderVM.OrderHeader.Id.ToString() },
            { "payment_capture", 1 } // auto capture
        };
            var client = new RazorpayClient(_razorpaySettings.KeyId, _razorpaySettings.KeySecret);
            var order = client.Order.Create(options);
            OrderVM.OrderHeader.SessionId = order["id"].ToString();
            _unitOfWork.Save();

            // Store order information in TempData for use in the Payment view
            TempData["OrderHeaderId"] = OrderVM.OrderHeader.Id.ToString();
            TempData["OrderTotal"] = OrderVM.OrderHeader.OrderTotal.ToString();
            TempData["OrderId"] = OrderVM.OrderHeader.SessionId;
            return RedirectToAction("Payment");

//            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        public IActionResult Payment()
        {
            // Retrieve order information from TempData and parse the values back to their original types
            if (TempData["OrderHeaderId"] is string orderHeaderIdStr && int.TryParse(orderHeaderIdStr, out int orderHeaderId) &&
                TempData["OrderTotal"] is string orderTotalStr && double.TryParse(orderTotalStr, out double orderTotal) &&
                TempData["OrderId"] is string orderId)
            {
                var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId);
                _unitOfWork.Save();

                var model = new PaymentVM
                {
                    OrderHeaderId = orderHeaderId,
                    OrderTotal = orderTotal,
                    OrderId = orderId,
                    RazorpayKey = _razorpaySettings.KeyId,
                    // You can pass additional properties if needed
                };

                return View(model);
            }

            // Handle cases where TempData values are missing or parsing fails
            return RedirectToAction(nameof(Order));
        }
        public IActionResult PaymentConfirmation(int id, string paymentId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");

            if (!string.IsNullOrEmpty(paymentId))
            {
                var client = new RazorpayClient(_razorpaySettings.KeyId, _razorpaySettings.KeySecret);
                var payment = client.Payment.Fetch(paymentId);

                if (payment["status"].ToString().Equals("captured", StringComparison.OrdinalIgnoreCase))
                {
                    _unitOfWork.OrderHeader.UpdateRazorpayPaymentId(orderHeader.Id, orderHeader.SessionId!, paymentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, orderHeader.OrderStatus, SD.PaymentStatusApproved);

                }
                else
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
                _unitOfWork.Save();
            }

           

            return View(id);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusPending);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.StatusInProcess);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.StatusApproved);
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.StatusShipped);
                    break;
                default:
                    break;
            }
            return Json(new { data = objOrderHeaders });
        }

        #endregion
    }
}
