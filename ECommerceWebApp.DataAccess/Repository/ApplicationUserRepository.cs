using ECommerceWebApp.DataAccess.Data;
using ECommerceWebApp.Models;
using ECommerceWebApp.DataAccess.Repository.IRespository;


namespace ECommerceWebApp.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;
        public ApplicationUserRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }
    }
}
