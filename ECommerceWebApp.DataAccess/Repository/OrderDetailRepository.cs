using ECommerceWebApp.DataAccess.Data;
using ECommerceWebApp.Models;
using ECommerceWebApp.DataAccess.Repository.IRespository;


namespace ECommerceWebApp.DataAccess.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private ApplicationDbContext _db;
        public OrderDetailRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }
        

        public void Update(OrderDetail obj)
        {
            _db.OrderDetails.Update(obj);
        }
    }
}
