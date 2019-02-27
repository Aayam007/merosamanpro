using Kachuwa.Data.Crud.Attribute;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MeroSaman.Models
{
    [Table("AdminProduct")]
    public class AdminProduct
    {
        [Key]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Select Catagory")]
        public int CatagoryId { get; set; }
        public int IdentityId { get; set; }
        [Required(ErrorMessage = "Enter Product Name!")]
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        [IgnoreAll]
        public string CatagoryName { get; set; }
        [AutoFill(AutoFillProperty.CurrentDate)]
        public DateTime AddedOn { get; set; }
        public IFormFile ImageUrl  { get; set; }
        public string Image { get; set; }    
    }
}