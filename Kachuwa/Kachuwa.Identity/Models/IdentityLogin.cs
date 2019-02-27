using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kachuwa.Data.Crud.Attribute;

namespace Kachuwa.Identity.Models
{
    [Table("IdentityUserLogin")]
    public class IdentityLogin
    {
        [Key]
        public Int64 LoginProviderId { get; set; }
        public string LoginProvider { get; set; }

        public string ProviderKey { get; set; }

        public long UserId { get; set; }

        public string ProviderDisplayName { get; set; }
        public string Name { get; set; }
    }
}