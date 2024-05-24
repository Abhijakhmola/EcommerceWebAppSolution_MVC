﻿

namespace ECommerceWebApp.DataAccess.Repository.IRespository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        void Save();
    }
}