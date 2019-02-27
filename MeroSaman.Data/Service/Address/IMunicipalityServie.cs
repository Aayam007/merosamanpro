using Kachuwa.Data;
using MeroSaman.Models;

namespace MeroSaman.Service
{
   public interface IMunicipalityService
    {
        CrudService<Municipality> MunicipalityCrudService { get; set; }
    }
}
