using Kachuwa.Core.DI;
using Kachuwa.Identity.Service;
using Kachuwa.Web.Service;
using MeroSaman.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MeroSaman.Core
{
    public class ServiceRegistrar : IServiceRegistrar
    {
        public void Register(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddSingleton<ICatagoryService, CatagoryService>();
            serviceCollection.AddSingleton<IProductService, ProductService>();
            serviceCollection.AddSingleton<IFileService, FileService>();
            serviceCollection.AddSingleton<ICounrtyService, CountryService>();
            serviceCollection.AddSingleton<IStateService, StateService>();
            serviceCollection.AddSingleton<IDistrictService, DistrictService>();
            serviceCollection.AddSingleton<IMunicipalityService, MunicipalityService>();
             serviceCollection.AddSingleton<IStoreService, StoreSerivice>();
            serviceCollection.AddScoped<IMenuService, MenuService>();
            serviceCollection.AddSingleton<ICartListService, CartListService>();
        }

        public void Update(IServiceCollection serviceCollection)
        {
            
        }
    }
}
