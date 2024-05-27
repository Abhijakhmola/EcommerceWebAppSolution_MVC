using ECommerceWebApp.Models;


namespace ECommerceWebApp.DataAccess.Repository.IRespository
{
    public interface ICompanyRepository : IRepository<Company>
    {
        void Update(Company obj);
    }
}
