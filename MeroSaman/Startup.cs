
using System;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Kachuwa.Core.Extensions;
using Kachuwa.Data;
using Kachuwa.Data.Crud;
using Kachuwa.Log.Insight;
using Kachuwa.Web;
using Kachuwa.Web.Theme;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Kachuwa.Identity;
using Microsoft.AspNetCore.Authorization;
using Kachuwa.Web.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MeroSaman.Api;
using System.Threading.Tasks;
namespace MeroSaman
{
    public class Startup
    {
        private IHostingEnvironment hostingEnvironment;
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            hostingEnvironment = env;
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);
            var serviceProvider = services.BuildServiceProvider();

           //data based 
            IDatabaseFactory dbFactory = DatabaseFactories.SetFactory(Dialect.SQLServer, serviceProvider);
            services.AddSingleton(dbFactory);
            //registering theme
            services.RegisterThemeService(config =>
            {
                config.FrontendThemeName = "Default";
                config.BackendThemeName = "Admin";
                config.LayoutName = "_layout";
            });
            services.ConfigureKachuwa(setup =>
            {             
            });
            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
            });
            services.AddAuthentication().AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyConstants.PagePermission, policy => policy.Requirements.Add(new PagePermissionRequirement()));
            });
            services.RegisterKachuwaCoreServices(serviceProvider);
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, PagePermissionHandler>();

            services.AddAntiforgery(options =>
            {
            });
            services.AddMvc(options =>
            {
            }).AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.Formatting = Formatting.Indented;
            }).AddViewComponentsAsServices();
            services.AddDistributedMemoryCache();
            services.AddKachuwaIdentitySever(hostingEnvironment, Configuration);


            //dual authorization support
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                 .AddJwtBearer(options =>
                 {
                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidateIssuer = true,
                         ValidateAudience = true,
                         ValidateLifetime = true,
                         ValidateIssuerSigningKey = true,
                         ValidIssuer = "MeroSaman",
                         ValidAudience = "MeroSamanApi",
                         IssuerSigningKey = JwtSecurityKey.Create("Mero Saman Is Best Product")
                     };

                     options.Events = new JwtBearerEvents
                     {
                         OnAuthenticationFailed = context =>
                         {
                             Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message);
                             return Task.CompletedTask;
                         },
                         OnTokenValidated = context =>
                         {
                             Console.WriteLine("OnTokenValidated: " + context.SecurityToken);
                             return Task.CompletedTask;
                         }
                     };
                 });
           
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
            });
            services.AddLocalization();
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = Int64.MaxValue;
                options.ValueLengthLimit = Int32.MaxValue;
                options.MultipartHeadersLengthLimit = Int32.MaxValue;
                options.ValueCountLimit = Int32.MaxValue;
            });
          
        }
        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider,
            IHostingEnvironment env, ILoggerFactory loggerFactory,
            IOptions<ApplicationInsightsSettings> applicationInsightsSettings)
        {
            loggerFactory.UseKachuwaInsight(applicationInsightsSettings, serviceProvider);
            app.UseDeveloperExceptionPage();
            app.UseSession();
            app.UseStaticFiles();
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".log"] = "text/plain";
            provider.Mappings[".txt"] = "text/plain";
            app.UseFileServer(new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), @"Logs")),
                RequestPath = new PathString("/dev/logs"),
                EnableDirectoryBrowsing = true,
                StaticFileOptions =
                {
                    DefaultContentType = "text/plain",
                    ContentTypeProvider = provider
                }
            });
            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseStaticHttpContext();
            app.UseWebSockets();
            var supportedCultures = new[]
              {
                new CultureInfo("en-US"),
                new CultureInfo("hi-IN"),
               new CultureInfo("en-GB"),
                new CultureInfo("es-ES"),
                new CultureInfo("zh-CN"),
                new CultureInfo("es"),
                new CultureInfo("fr-FR"),
                new CultureInfo("fr"),
                new CultureInfo("ne-NP"),
            };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });
            app.UseKachuwaCore(serviceProvider);
          
            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

