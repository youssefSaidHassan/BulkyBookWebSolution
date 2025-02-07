using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Product product)
        {
            var prductFromDb = _context.Products.FirstOrDefault(u => u.Id == product.Id);
        
            if (prductFromDb != null)
            {
                prductFromDb.Title = product.Title;
                prductFromDb.ISBN= product.ISBN;
                prductFromDb.Description = product.Description;
                prductFromDb.Price= product.Price;
                prductFromDb.ListPrice = product.ListPrice;
                prductFromDb.Price50 = product.Price50;
                prductFromDb.Price100 = product.Price100;
                prductFromDb.Author = product.Author;
                prductFromDb.CategoryId = product.CategoryId;
                prductFromDb.CoverTypeId = product.CoverTypeId;
                if (product.ImageUrl != null)
                {
                    prductFromDb.ImageUrl = product.ImageUrl;
                }
            }
        }
    }
}
