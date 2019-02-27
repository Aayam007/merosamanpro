using Kachuwa.Data.Crud.Attribute;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MeroSaman.Models
{
    [Table("Product")]
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Select Catagory")]
        public int CatagoryId { get; set; }
        public int IdentityId { get; set; }

        [Required(ErrorMessage = "Enter Product Name!")]
        public string Name { get; set; }
        public string ShortDescription { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }


        public string Descripton { get; set; }
        [IgnoreAll]
        public string CatagoryName { get; set; }

        [AutoFill(AutoFillProperty.CurrentDate)]
        public DateTime AddedOn { get; set; }
        public IFormFile ImageUrl  { get; set; }
        public string Image { get; set; }
        public int Price { get; set; }
        public string PriceUnit { get; set; }
        public string ManufactureBy { get; set; }
        public string Quantity { get; set; }
        public string QuantityUnit { get; set; }
        public string Offer { get; set; }
        public string Discounted { get; set; }
        public string AddedBy { get; set; }
        public IFormFileCollection ImageList { get; set; }

    }
}