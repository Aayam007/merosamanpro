using Kachuwa.Data;
using MeroSaman.Models;

namespace MeroSaman.Service
{
   public interface ICounrtyService
    {
        CrudService<Country> CountryCrudService { get; set; }

    }
}
