using Kachuwa.Log;
using Kachuwa.Web.API;
using MeroSaman.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace MeroSamanApi.Controllers
{
    public class ProductController : BaseApiController
    {


        private readonly IProductService _productService;
        private readonly ICatagoryService _catagoryService;
        private readonly ILogger _logger;

        public ProductController(IProductService productService, ICatagoryService catagoryService, ILogger logger)
        {
            _productService = productService;
            _catagoryService = catagoryService;
            _logger = logger;
        }

        [Route("api/v1/list/product")]
        [HttpGet]
        public async Task<dynamic> ProductList(int catagoryId)
        {
            try
            {
                var data = await _productService.ProductCrudService.GetListAsync("Where CatagoryId=@CatagoryId",new { CatagoryId= catagoryId });
                return  HttpResponse(StatusCodes.Status200OK, "", data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                return ErrorResponse(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

    }
}