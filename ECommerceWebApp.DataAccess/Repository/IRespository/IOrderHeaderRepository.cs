using ECommerceWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceWebApp.DataAccess.Repository.IRespository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);

        void UpdateStatus(int Id,string orderStatus,string? paymentStatus=null);
        void UpdateRazorpayPaymentId(int Id,string sessionId,string paymentIntentId);
    }

}
