using Kachuwa.Data.Crud.Attribute;
using MeroSaman.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MeroSaman.Models
{
    [Table("CartList")]
   public  class CartList
    {
        [Key]
        public int CartListId { get; set; }
        public int IdentityId { get; set; }
        public int ProductId { get; set; }
        [IgnoreAll]
        public string Token { get; set; }
    }
}
