using ECommerceWebApp.DataAccess.Data;
using ECommerceWebApp.Models;
using ECommerceWebApp.DataAccess.Repository.IRespository;


namespace ECommerceWebApp.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>,IShoppingCartRepository
    {
        private ApplicationDbContext _db;
        public ShoppingCartRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }
        

        public void Update(ShoppingCart obj)
        {
            _db.ShoppingCarts.Update(obj);
        }
    }
}
