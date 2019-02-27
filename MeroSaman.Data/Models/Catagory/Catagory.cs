using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using Dapper;
using Kachuwa.Data.Crud.Attribute;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeroSaman.Models
{
    [Table("Catagory")]
    public class Catagory
    {
        [Key]
        public int CatagoryId { get; set; }

        [Required(ErrorMessage = "Enter Catagory Name!")]
        public string Name { get; set; }

       

        public bool IsActive { get; set; }

        [AutoFill(AutoFillProperty.CurrentDate)]
        public DateTime AddedOn { get; set; }
    }
   
}
