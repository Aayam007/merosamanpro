using System.Reflection;
using Kachuwa.Core.DI;
using Kachuwa.Web;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Service
{
    public class PageMenuServiceRegistrar : IServiceRegistrar
    {
        private bool _isInstalled = false;
        public void Register(IServiceCollection serviceCollection, IConfiguration configuration)
        {
         serviceCollection.AddSingleton<IPageService, PageService>();
            var assp = new EmbeddedFileProvider(typeof(PageMenuServiceRegistrar).GetTypeInfo().Assembly);
            serviceCollection.Configure<RazorViewEngineOptions>(opts =>
            {
                opts.FileProviders.Add(assp);
            });
        }

        public void Update(IServiceCollection serviceCollection)
        {
            if (_isInstalled)
            {
                // var builder = serviceCollection.BuildServiceProvider();
                //  var settingService = builder.GetService<ISettingService>();
                // serviceCollection.AddSingleton(settingService.CrudService.Get(1));
            }
        }
    }
}