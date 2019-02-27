using Kachuwa.Identity.Extensions;
using Kachuwa.Log;
using Kachuwa.Web.Service;
using MeroSaman.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeroSaman.ViewComponents
{  
    public class AdminSideMenuViewComponent : ViewComponent
    {
        private readonly IMenuService _menuService;
        private readonly ILogger _logger;

        public AdminSideMenuViewComponent(IMenuService menuService, ILogger logger)
        {
            _menuService = menuService;
            _logger = logger;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var idUser = User.Identity.GetIdentityUserId();
                var roles = User.Identity.GetRoles();

                var user = User.Identity.GetUserName();
                ViewBag.name = user;
                List<string> rolevalue = roles.ToList();

                var data = await _menuService.GetAllMenuListCrudServiseAsync(rolevalue,(int)idUser);
                return View(data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            } 
            
        }
    }
}
