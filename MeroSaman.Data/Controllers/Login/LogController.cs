using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Kachuwa.Log;
using Kachuwa.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace MeroSaman.Core.Controllers
{
    //[Authorize(Roles = "Admin,Developer")]
    //[Authorize("Test")]
    //[Route("[controller]/[action]")]
    [Area("Admin")]
    public class LogController : BaseController
    {
        public ILoggerService LoggerService { get; }
        private readonly IHostingEnvironment _hostingEnvironment;
        public LogController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        [Route("admin/Log")]
        public async Task<IActionResult> IndexAsync(int offset = 1, int limit = 20)
        {
            var Basedir = _hostingEnvironment.ContentRootPath + "\\Logs\\";
            var _todaysDate = DateTime.Now.ToString("yyyy_MM_dd");

            if (!Directory.Exists(Basedir))
            {
                Directory.CreateDirectory(Basedir);
            }
            var LogFilePath = Basedir + _todaysDate + ".log";
 
            string filecontent = System.IO.File.ReadAllText(LogFilePath);
            ViewBag.FileContent = filecontent;
            return View();

            //var dailyLogs = LoggerService.GetTodaysLogs(offset, limit);
            //return Json(dailyLogs.Logs);
        }
      
        [Route("log/ByDate")]
        [Route("log/ByDate/page")]
        [Route("log/ByDate/test")]
        public IActionResult ByDate(int offset = 1, int limit = 20,DateTime? day=null)
        {
            //day = day ?? DateTime.Now;
           // var dailyLogs = LoggerService.GetLogs(offset, limit, day.GetValueOrDefault(DateTime.Now));
            return Json(true);
        }
    }
}
