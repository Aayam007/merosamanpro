using System.ComponentModel.DataAnnotations.Schema;
using Kachuwa.Data.Crud.Attribute;

namespace Kachuwa.Identity.Models
{
    [Table("IdentityUser")]
    public class IdentityUser:KachuwaIdentityUser
    {      
    }
}