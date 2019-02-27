using Kachuwa.Data.Extension;
using Kachuwa.Identity.Service;
using Kachuwa.Web;
using Kachuwa.Web.Model;
using Kachuwa.Web.Notification;
using Kachuwa.Web.Service;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KachuwaPermission.Controllers
{
    [Area("Admin")]
    //[Authorize(PolicyConstants.PagePermission)]
    public class MenuPermissionController : BaseController
    {
        private readonly IIdentityRoleService _roles;
        private readonly INotificationService _inotificationService;
        private readonly IMenuService _menuService;
        public MenuPermissionController(IIdentityRoleService roles, INotificationService notificationService, IMenuService menuService)
        {
            _roles = roles;
            _inotificationService = notificationService;
            _menuService = menuService;   
        }
        [Route("menu/permission")]
        public async Task<IActionResult> Index()
        {
            var data = await _menuService.PermissionCrudService.GetListAsync("where AllowAccess = @AllowAccess", new { AllowAccess = true});

            ViewBag.ActualData = data;
            var rolemodel = new MenuPermission
            {
                RoleLists = await _roles.RoleService.GetListAsync(),
                Menulists = await _menuService.MenuCrudService.GetListAsync(),
            }; 
            return View(rolemodel);                                            
        }
        [HttpPost]
        [Route("menu/permission/new")]
        public async Task<IActionResult> New(MenuPermission model)
        {
            model.AutoFill();      
            if (ModelState.IsValid)
            {
                var menulist= await _menuService.PermissionCrudService.GetAsync("Where MenuId=@MenuId and RoleId=@RoleId",new { MenuId=model.MenuId, RoleId=model.RoleId });
                if (menulist!=null)
                {
                    menulist.AllowAccess = model.AllowAccess;
                    var data = await _menuService.PermissionCrudService.UpdateAsync(menulist);
                }
                else
                {
                    var data = await _menuService.PermissionCrudService.InsertAsync(model);
                } 
                _inotificationService.Notify("Success", "Data has been saved successfully!", NotificationType.Success);
                return View();
            }
            else
            {
                return View();
            }
        }
    }
}
