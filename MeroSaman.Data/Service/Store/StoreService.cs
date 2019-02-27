
using Kachuwa.Data;
using MeroSaman.Models;


namespace MeroSaman.Service
{
    public class StoreSerivice : IStoreService
    {
        public CrudService<Store> StoreCrudService { get; set; } = new CrudService<Store>();

    }
}
