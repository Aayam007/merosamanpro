using Dapper;
using Kachuwa.Data;
using MeroSaman.Models;

namespace MeroSaman.Service
{
    public class StateService : IStateService
    {
        public CrudService<State> StateCrudService { get; set; } = new CrudService<State>();

    }
}
