
using System.Linq;
using System.Threading.Tasks;
using Kachuwa.Log;
using MeroSaman.Models;
using MeroSaman.Service;
using Microsoft.AspNetCore.Mvc;


namespace MeroSaman.Controllers.Admin
{
    [Area("Admin")]
    [Route("[controller]/[action]")]
    public class DashboardController : Controller
    {
        private readonly ICatagoryService _catagoryService;
        private readonly ILogger _logger;
        private readonly IProductService _productService;

        public DashboardController(ICatagoryService catagoryService, ILogger logger, IProductService productService)
        {
            _catagoryService = catagoryService;
            _productService = productService;

        }
        public async Task<IActionResult> Index()
        {
            try
            {
                DashboardViewModel dashboard = new DashboardViewModel();
                var cat = await _catagoryService.CategoryCrudService.GetListAsync("Where IsActive=@IsActive", new { IsActive = 1 });
                var pro = await _productService.ProductCrudService.GetListAsync("Where IsActive=@IsActive", new { IsActive = 1 });
                dashboard.catagory_count = cat.Count();
                dashboard.product_count = pro.Count();
                return View(dashboard);
            }
            catch (System.Exception e)
            {

                throw e;
            }
          
        }
    }
}