using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kachuwa.Web.Model;
using Microsoft.AspNetCore.Mvc;

namespace Kachuwa.Web
{
    [Table("Page")]
    public class PageViewModel : SEO
    {
        public long PageId { get; set; }
        [Required]
        public string Name { get; set; }
        public new string Url { get; set; }
        public string Content { get; set; }
        public bool UseMasterLayout { get; set; }
        public bool IsPublished { get; set; }

        public bool IsNew { get; set; }
        public string OldUrl { get; set; }
        public bool IsBackend { get; set; }
        public bool IsSystem { get; set; }
    }
}