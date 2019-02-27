using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kachuwa.Data.Crud.Attribute;
using Kachuwa.Identity.Models;

namespace Kachuwa.Web.Model
{
    [Table("MenuPermission")]
    public class MenuPermission
    {
        [Key]
        public Int64 MenuPermissionId { get; set; }
        public int MenuId { get; set; }
        public bool AllowAccessForAll { get; set; }
        public bool AllowAccess { get; set; }
        public int RoleId { get; set; }
        [AutoFill(AutoFillProperty.CurrentDate)]
        public DateTime AddedOn { get; set; }
        [AutoFill(AutoFillProperty.CurrentUser)]
        public string AddedBy { get; set; }
        [IgnoreAll]
        public int RowTotal { get; set; }
        [IgnoreAll]
        public bool IsSelected { get; set; }
        [IgnoreAll]
        public IEnumerable<IdentityRole> RoleLists { get; set; }
        [IgnoreAll]
        public IEnumerable<Menu> Menulists { get; set; }
    }

}