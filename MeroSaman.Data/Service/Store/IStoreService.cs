
using Kachuwa.Data;
using MeroSaman.Models;


namespace MeroSaman.Service
{
    public interface IStoreService
    {
        CrudService<Store> StoreCrudService { get; set; }
    }
}
