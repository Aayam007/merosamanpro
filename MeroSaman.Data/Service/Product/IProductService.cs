using Kachuwa.Data;
using MeroSaman.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MeroSaman.Service
{ 
    public interface IProductService
    {
        CrudService<Product> ProductCrudService { get; set; }
        CrudService<ProductImage> ProductImageService { get; set; }
        CrudService<AdminProduct> AdminProductCrudService { get; set; }
    }
}