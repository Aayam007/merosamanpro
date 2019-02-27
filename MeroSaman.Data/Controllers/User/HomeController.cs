using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MeroSaman.Models;
using MeroSaman.Service;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using Kachuwa.Log;

namespace MeroSaman.Controllers
{

    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICatagoryService _catagoryService;
        private readonly ILogger _logger;
        public HomeController(IProductService productService, ICatagoryService catagoryService, ILogger logger)
        {
            _productService = productService;
            _catagoryService = catagoryService;
            _logger = logger;
        }
       
        public async Task<IActionResult> Index()
        {
            try
            {
                var product = await _productService.ProductCrudService.GetListAsync();
                foreach (var item in product)
                {
                    item.Image = "/Image/" + item.Image;
                    if (item.CatagoryId!=0)
                    {
                        var catagory = await _catagoryService.CategoryCrudService.GetAsync(item.CatagoryId);
                        item.CatagoryName = catagory.Name;
                    }
                   
                }
                return View(product);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw;
            }      
        }
       
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Detail([FromQuery]int id)
        {
            try
            {
                if (id == 0)
                {
                    return NotFound();
                }
                var data = await _productService.ProductCrudService.GetAsync(id);
                if (data == null)
                {
                    return NotFound();
                }
                return View(data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }     
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }     
    }
}
