using ECommerceWebApp.DataAccess.Data;
using ECommerceWebApp.Models;
using ECommerceWebApp.DataAccess.Repository.IRespository;


namespace ECommerceWebApp.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }
        

        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }

		public void UpdateStatus(int Id, string orderStatus, string? paymentStatus = null)
		{
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u=>u.Id==Id);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus=orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus=paymentStatus;
                }
            }
		}

		public void UpdateRazorpayPaymentId(int Id, string sessionId, string paymentIntentId)
		{
			var orderFromDb=_db.OrderHeaders.FirstOrDefault(u=>u.Id==Id);
            if(!string.IsNullOrEmpty(sessionId))
            {
                orderFromDb.SessionId=sessionId;    
            }  
            if(!string.IsNullOrEmpty(paymentIntentId))
            {
                orderFromDb.PaymentId=paymentIntentId;   
                orderFromDb.PaymentDate=DateTime.Now;   
            }   
		}
	}
}
