using MeroSaman.Models;
using MeroSaman.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace MeroSaman.Core.Controllers
{

    class WishListController : Controller
    {
        public readonly IConfiguration _configuration;
        public readonly ICartListService _wishListService;
        public WishListController(IConfiguration config, ICartListService wishListService)
        {
            _configuration = config;
            _wishListService = wishListService;

        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult New()
        {
            return View();
        }
        [HttpPost]
        public JsonResult New (CartList wishList)
        {
            if(ModelState.IsValid)
            {
                //if(_wishListService.New(wishList)> 0)
                //{
                //    return Json("Index");
                //}
            }
            return Json(wishList);
        }
    }
}
