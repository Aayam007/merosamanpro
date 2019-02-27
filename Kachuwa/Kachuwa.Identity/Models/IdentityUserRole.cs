using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kachuwa.Data.Crud.Attribute;

namespace Kachuwa.Identity.Models
{
    [Table("IdentityUserRole")]
    public class IdentityUserRole
    {
        [Key]
        public int IdentityRoleId { get; set; }
        public long UserId { get; set; }
       
        public long RoleId { get; set; }
    }
}