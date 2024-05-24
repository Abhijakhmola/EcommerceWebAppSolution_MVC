using ECommerceWebApp.Models;


namespace ECommerceWebApp.DataAccess.Repository.IRespository
{
    public interface IProductRepository:IRepository<Product>
    {
        void Update(Product obj);
    }
}
