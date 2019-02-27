using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using Kachuwa.Caching;
using Kachuwa.Data;
using Kachuwa.Web.Model;
using Kachuwa.Web.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Dapper;
using Kachuwa.Data.Extension;

namespace Kachuwa.Web.Service
{

    public class MenuService : IMenuService
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ICacheService _cacheService;

        public CrudService<Menu> MenuCrudService { get; set; } = new CrudService<Menu>();
        public CrudService<MenuType> TypeCrudService { get; set; } = new CrudService<MenuType>();
        public CrudService<MenuSetting> SettingCrudService { get; set; } = new CrudService<MenuSetting>();
        public CrudService<MenuPermission> PermissionCrudService { get; set; } = new CrudService<MenuPermission>();
        private string _cacheKey = "Kachuwa.Permissions";

        public MenuService(IHostingEnvironment hostingEnvironment, ICacheService cacheService)
        {
            _hostingEnvironment = hostingEnvironment;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<MenuPermissionViewModel>> GetPermissionsFromCache()
        {
            var permissions = await _cacheService.GetAsync<IEnumerable<MenuPermissionViewModel>>(_cacheKey, async () =>
            {
                return await GetAllPermissions() ;
            });
            return permissions;
        }

        public  async Task<IEnumerable<MenuPermissionViewModel>> GetAllPermissions()
        {
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                await db.OpenAsync();
                return await db.QueryAsync<MenuPermissionViewModel>("select mp.*,m.Name,m.Url,ir.Name as RoleName from dbo.menupermission as mp " +
                    " inner join dbo.Menu as m on m.MenuId=mp.MenuId " +
                    " left join dbo.IdentityRole as ir on mp.RoleId=ir.Id ;");
            }
        }

        public async Task<int> SaveMenu(MenuViewModel model)
        {
            if (model.MenuId == 0)
            {
                model.AutoFill();
                var menuId = await MenuCrudService.InsertAsync<int>(model);
                foreach (var permission in model.Permissions)
                {
                    await PermissionCrudService.InsertAsync<int>(permission);
                }
                _cacheService.Remove(_cacheKey);
                await GetPermissionsFromCache();
                return menuId;
            }
            else
            {
                model.AutoFill();
                await MenuCrudService.UpdateAsync(model);
                await PermissionCrudService.DeleteAsync("Where MenuId=@MenuId", new { model.MenuId });
                foreach (var permission in model.Permissions)
                {
                    await PermissionCrudService.InsertAsync<int>(permission);
                }
                _cacheService.Remove(_cacheKey);
                await GetPermissionsFromCache();
                return model.MenuId;

            }
        }
        public async Task<IEnumerable<Menu>> GetAllMenuListCrudServiseAsync(List<string> userRoles, int idUser)
        {
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                await db.OpenAsync();
                try
                {
                    var lists = await db.QueryAsync<Menu>("select distinct M.Name,M.MenuId,M.Url,M.Icon,M.CssClass,M.IsChild,M.ParentId,M.MenuId,M.GroupName,M.Culture,M.IsBackend,M.IsSystem,M.IsActive, M.IsDeleted, M.AddedOn, M.AddedBy from Menu M INNER JOIN  MenuPermission MP ON   Mp.MenuId = M.MenuId  inner join dbo.IdentityRole ir on ir.Id =MP.RoleId  and ir.Name  in (@userRoles) and M.IsActive=1 and M.IsBackend=1 and MP.AllowAccess=1", new { userRoles });
                    return lists;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}
