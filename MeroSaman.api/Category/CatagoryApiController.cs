using Kachuwa.Log;
using Kachuwa.Web.API;
using MeroSaman.Models;
using MeroSaman.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

    public class CatagoryApiController : BaseApiController
    {
        private readonly ICatagoryService _catagoryService;
        public ILogger _logger;
        public CatagoryApiController(ICatagoryService catagoryService, ILogger logger)
        {
            _catagoryService = catagoryService;
            _logger = logger;
        }
        [Route("api/v1/list/catagory")]
        [HttpGet]
        public async Task<dynamic> catagorylist()
        {
            try
            {
                var data = await _catagoryService.CategoryCrudService.GetListAsync("Where IsActive=@IsActive",new { IsActive=1 });
                return HttpResponse(StatusCodes.Status200OK, "", data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                return ErrorResponse(StatusCodes.Status500InternalServerError, e.Message);
            }
        }      
    }
}