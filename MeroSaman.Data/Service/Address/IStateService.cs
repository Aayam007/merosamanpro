using Kachuwa.Data;
using MeroSaman.Models;

namespace MeroSaman.Service
{
   public interface IStateService
    {
        CrudService<State> StateCrudService { get; set; }

    }
}
