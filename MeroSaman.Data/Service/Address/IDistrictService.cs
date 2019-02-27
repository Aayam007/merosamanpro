using Kachuwa.Data;
using MeroSaman.Models;

namespace MeroSaman.Service
{
   public interface IDistrictService
    {
        CrudService<District> DistrictCrudService { get; set; }

    }
}
