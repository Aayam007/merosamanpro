using Kachuwa.Data.Crud.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MeroSaman.Models
{
    [Table("Store")]
    public class Store
    {
        [Key]
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public int IdentityId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber1 { get; set; }
        public string PhoneNumber2 { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string District { get; set; }
        public string Municipality { get; set; }
        public string Street { get; set; }
        public string Logo { get; set; }
        [AutoFill(AutoFillProperty.CurrentDate)]
        public DateTime AddedOn { get; set; }

    }
}
