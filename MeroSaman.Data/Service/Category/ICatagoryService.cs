using Kachuwa.Data;
using MeroSaman.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MeroSaman.Service
{
    public interface ICatagoryService
    {
        CrudService<Catagory> CategoryCrudService { get; set; }
    }
}
