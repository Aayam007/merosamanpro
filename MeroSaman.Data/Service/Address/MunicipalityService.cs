using Kachuwa.Data;
using MeroSaman.Models;

namespace MeroSaman.Service
{
    public class MunicipalityService : IMunicipalityService
    {
        public CrudService<Municipality> MunicipalityCrudService { get; set; } = new CrudService<Municipality>();
    }
}
