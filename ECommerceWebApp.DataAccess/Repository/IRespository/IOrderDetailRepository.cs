using ECommerceWebApp.Models;


namespace ECommerceWebApp.DataAccess.Repository.IRespository
{
    public interface IOrderDetailRepository : IRepository<OrderDetail>
    {
        void Update(OrderDetail obj);
    }
}
