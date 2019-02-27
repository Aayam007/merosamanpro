using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace MeroSaman.Core.Controllers.Seller
{
    
  
    [Route("[controller]/[action]")]
  public  class SellerProductController : Controller
    {
        public IActionResult Product()
        {
            return View();
        }

    }
}
