using MeroSaman.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Kachuwa.Data;

namespace MeroSaman.Service
{
    public class ProductService : IProductService
    {
        public CrudService<Product> ProductCrudService { get; set; } = new CrudService<Product>();
        public CrudService<ProductImage> ProductImageService { get; set; } = new CrudService<ProductImage>();
        public CrudService<AdminProduct> AdminProductCrudService { get; set; } = new CrudService<AdminProduct>();

    }
}
