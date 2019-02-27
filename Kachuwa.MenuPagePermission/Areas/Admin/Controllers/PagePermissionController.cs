using Kachuwa.Data.Extension;
using Kachuwa.Web;
using Kachuwa.Web.Notification;
using Kachuwa.Web.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Kachuwa.Identity.Service;

namespace KachuwaPermission.Controllers
{
    [Area("Admin")]
    //[Authorize(PolicyConstants.PagePermission)]
    public class PagePermissionController : BaseController
    {
        private readonly IIdentityRoleService _roles;
        private readonly IPageService _pageService;
        private readonly INotificationService _inotificationService;
        private readonly IMenuService _menuService;
        public PagePermissionController(IIdentityRoleService roles, INotificationService notificationService, IMenuService menuService, IPageService pageService)
        {
            _roles = roles;
            _inotificationService = notificationService;
            _menuService = menuService;
            _pageService = pageService;
        }

        [Route("page/permission")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var data = await _pageService.PermissionCrudService.GetListAsync("where AllowAccess = @AllowAccess", new { AllowAccess = true });
                ViewBag.ActualData = data;
                var rolemodel = new PagePermission
                {
                    RoleLists = await _roles.RoleService.GetListAsync(),
                    Pagelist = await _pageService.CrudService.GetListAsync()
                };
                return View(rolemodel);
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        [HttpPost]
        [Route("page/permission")]
        public async Task<IActionResult> New(PagePermission model)
        {
            try
            {
                model.AutoFill();
                if (ModelState.IsValid)
                {
                    var menulist = await _pageService.PermissionCrudService.GetAsync("Where PageId=@PageId and RoleId=@RoleId", new { PageId = model.PageId, RoleId = model.RoleId });
                    if (menulist != null)
                    {
                        menulist.AllowAccess = model.AllowAccess;
                        var data = await _pageService.PermissionCrudService.UpdateAsync(menulist);
                    }
                    else
                    {
                        var data = await _pageService.PermissionCrudService.InsertAsync(model);
                    }
                    return View();
                }
                else
                {
                    return View();
                }
            }
            catch (Exception e)
            {
                return View();
            }
        }
    }
}
