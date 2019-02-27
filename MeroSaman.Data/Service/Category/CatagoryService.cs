using MeroSaman.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Kachuwa.Data;

namespace MeroSaman.Service
{
    public class CatagoryService : ICatagoryService
    {
       
        public  CrudService<Catagory> CategoryCrudService { get; set; } = new CrudService<Catagory>();


    }
}
