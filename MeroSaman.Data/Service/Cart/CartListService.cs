using Dapper;
using Kachuwa.Data;
using MeroSaman.Models;

namespace MeroSaman.Service
{

    public class CartListService : ICartListService
    {
        public CrudService<CartList> CartCrudService { get; set; } = new CrudService<CartList>();
    }

}
