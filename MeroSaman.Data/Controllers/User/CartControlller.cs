using MeroSaman.Core.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeroSaman.Core.Controllers
{
    class CartControlller : Controller
    {
        private readonly IConfiguration configuration;
        public CartControlller (IConfiguration config)
        {
            this.configuration = config;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult NewCart()
        {
            return View();
        }
        public IActionResult EditCart()
        {
            return View();
        }
        public IActionResult Delete()
        {
            return View();
        }
       
    }
}
