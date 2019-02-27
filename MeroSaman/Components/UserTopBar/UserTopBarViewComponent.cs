using Kachuwa.Log;
using MeroSaman.Models;
using MeroSaman.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeroSaman.ViewComponents
{

   
    public class UserTopBarViewComponent : ViewComponent
    {
        private readonly ICatagoryService _catagoryService;
        private readonly ILogger _logger;
        public UserTopBarViewComponent(ICatagoryService catagoryService, ILogger logger)
        {
            _catagoryService = catagoryService;
            _logger = logger;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var catagory = await _catagoryService.CategoryCrudService.GetListAsync();
                return View(catagory);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
            
        }
    }
}
