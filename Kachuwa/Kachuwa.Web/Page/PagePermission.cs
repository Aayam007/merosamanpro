using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kachuwa.Data.Crud.Attribute;
using Kachuwa.Identity.Models;

namespace Kachuwa.Web
{
    [Table("PagePermission")]
    public class PagePermission
    {
        [Key]
        public long PagePermissionId { get; set; }

        public long PageId { get; set; }

        public bool AllowAccessForAll { get; set; }

        public bool AllowAccess { get; set; }

        public long RoleId { get; set; }

        [AutoFill(AutoFillProperty.CurrentDate)]
        [IgnoreUpdate]
        public DateTime AddedOn { get; set; }
        [AutoFill(AutoFillProperty.CurrentUser)]
        [IgnoreUpdate]
        public string AddedBy { get; set; }

        public IEnumerable<IdentityRole> RoleLists { get; set; }
        public IEnumerable<Page> Pagelist { get; set; }
    }
}