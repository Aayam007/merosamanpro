using System;
using Kachuwa.Data.Crud.Attribute;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Kachuwa.Identity.Models
{
    [Table("AppUser")]
    public class AppUser
    {
        [Key]
        public long AppUserId { get; set; }

        [IgnoreUpdate]
        public long IdentityUserId { get; set; }

        [Required]
        public string FirstName { get; set; }

        //[Required]
        public string LastName { get; set; }

        public string Bio { get; set; }

        [Required]
        //[IgnoreUpdate]
        [EmailAddress]
        public string Email { get; set; }

        public string Address { get; set; }

        [Required]
        [Phone(ErrorMessage = "Invalid phone number!")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Invalid phone number!")]
        public string PhoneNumber { get; set; }
        public string DOB { get; set; }
        public string ProfilePicture { get; set; }
        public string Gender { get; set; }

        public string DeviceId { get; set; }
        public string DeviceOS { get; set; }

        public bool IsActive { get; set; }
        [IgnoreInsert]
        [AutoFill(false)]
        public bool IsDeleted { get; set; }
        [AutoFill(AutoFillProperty.CurrentDate)]
        [IgnoreUpdate]
        public DateTime AddedOn { get; set; }

        [AutoFill(AutoFillProperty.CurrentUser)]
        [IgnoreUpdate]
        public string AddedBy { get; set; }

        [IgnoreAll]
        public int RowTotal { get; set; }
      
        public string City { get; set; }
        public string Country { get; set; }

        [IgnoreAll]
        public string FullName
        {
            get
            {
                return FirstName + "  " + LastName;
            }
        }
      
    }
}
