using Kachuwa.Identity.Extensions;
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

   
    public class AdminTopBarViewComponent : ViewComponent
    {
        private readonly ICatagoryService _catagoryService;
        private readonly ILogger _logger;
        public AdminTopBarViewComponent(ICatagoryService catagoryService, ILogger logger)
        {
            _catagoryService = catagoryService;
            _logger = logger;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var user = User.Identity.GetUserName();
                ViewBag.name = user;
                return View();
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
            
        }
    }
}
