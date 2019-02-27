using System.Linq;
using System.Threading.Tasks;
using Kachuwa.Data.Extension;
using Kachuwa.Form;
using Kachuwa.Web;
using Kachuwa.Web.Model;
using Kachuwa.Web.Security;
using Kachuwa.Web.Service;
using MeroSaman.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeroSaman.Controllers
{
    [Area("Admin")]
    [Authorize(PolicyConstants.PagePermission)]
    public class MenuController : BaseController
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }
        #region Menu Crud
        [Route("admin/menu/page/{page?}")]
        [Route("admin/menu")]//default make it at last
        public async Task<IActionResult> Index([FromRoute]int page = 1, [FromQuery]string query = "")
        {
            ViewData["Page"] = page;
            int rowsPerPage = 10;
            //customized viewmodel with join
            var model = await _menuService.MenuCrudService.GetListPagedAsync(page, rowsPerPage, 1,
                "Where Name like @Query and IsDeleted=0", "Addedon desc", new { Query = "%" + query + "%" });
            return View(model);
        }

        [Route("admin/menu/new")]
        public async Task<IActionResult> New()
        {
            var menulist = await _menuService.MenuCrudService.GetListAsync("where ParentId=@ParentId", new { ParentId = 0 });
            var formDatasource = new FormDatasource();
            formDatasource.SetSource("ParentMenuList", menulist.Select(x => new FormInputItem()
            {
                Id = x.MenuId,
                Value = x.MenuId.ToString(),
                Label = x.Name
            }));
            ViewData["FormDataParentMenu"] = formDatasource;
            return View();
        }

        [HttpPost]
        [Route("admin/menu/new")]
        public async Task<IActionResult> New(Menu model)
        {
            if (ModelState.IsValid)
            {
                // model.Url = model.Url.TrimStart(new char[] { '/' });
                model.AutoFill();
                if (model.MenuId == 0)
                {
                    model.CssClass = "material-icons md-18";
                    model.IsSystem = true;
                    if (model.Url == null)
                    {
                        model.Url = "#";
                    }                   
                    await _menuService.MenuCrudService.InsertAsync<int>(model);
                }                    
                else
                    await _menuService.MenuCrudService.UpdateAsync(model);
                return RedirectToAction("Index");
            }
            else
            {
                return View(model);
            }
        }


        [Route("admin/menu/edit/{pageId}")]
        public async Task<IActionResult> Edit([FromRoute]int pageId)
        {
            var model = await _menuService.MenuCrudService.GetAsync(pageId);
            var menulist = await _menuService.MenuCrudService.GetListAsync("where ParentId=@ParentId", new { ParentId = 0 });
            var formDatasource = new FormDatasource();
            formDatasource.SetSource("ParentMenuList", menulist.Select(x => new FormInputItem()
            {
                Id = x.MenuId,
                Value = x.MenuId.ToString(),
                Label = x.Name,
                IsSelected = x.MenuId == model.ParentId
            }));
            ViewData["FormDataParentMenu"] = formDatasource;  
            if(model.Url == "#")
            {
                model.Url = null;
            }
            return View(model);
        }

        [HttpPost]
        [Route("admin/menu/edit")]
        public async Task<IActionResult> Edit(Menu model)
        {
            if (ModelState.IsValid)
            {
                model.AutoFill();
                if (model.MenuId == 0)
                    await _menuService.MenuCrudService.InsertAsync<int>(model);
                else
                {
                    model.CssClass = "material-icons md-18";
                    model.IsSystem = true;
                    if (model.Url == null)
                    {
                        model.Url = "#";
                    }
                    await _menuService.MenuCrudService.UpdateAsync(model);
                }                    
                return RedirectToAction("Index");
            }
            else
            {
                return View(model);
            }
        }

        [HttpPost]
        [Route("admin/menu/delete")]
        public async Task<JsonResult> Delete(int id)
        {
            var result = await _menuService.MenuCrudService.DeleteAsync(id);
            return Json(result);
        }
        #endregion



    }
}