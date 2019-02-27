using Kachuwa.Data;
using MeroSaman.Models;

namespace MeroSaman.Service
{
    public class DistrictService : IDistrictService
    {
        public CrudService<District> DistrictCrudService { get; set; } = new CrudService<District>();
    }
}

