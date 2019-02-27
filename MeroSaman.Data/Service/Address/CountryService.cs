using Kachuwa.Data;
using MeroSaman.Models;

namespace MeroSaman.Service
{
    public class CountryService : ICounrtyService
    {
        public CrudService<Country> CountryCrudService { get; set; } = new CrudService<Country>();
    }
}
