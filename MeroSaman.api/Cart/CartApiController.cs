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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace MeroSamanApi.Controllers
{

    public class CartApiController : BaseApiController
    {
        private readonly ICartListService _cartListService;
        public ILogger _logger;
        public CartApiController(ICartListService cartListService, ILogger logger)
        {
            _cartListService = cartListService;
            _logger = logger;
        }
        [Route("api/v1/list/Cart")]
        [HttpPost]
        public async Task<dynamic> Cartlist(CartList cartList)
        {
            try
            {
                var stream = cartList.Token;
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(stream) as JwtSecurityToken;
                var Id = jsonToken.Claims.First(claim => claim.Type == "Id").Value;
                 cartList.IdentityId =Convert.ToInt32(Id);
                var data = await _cartListService.CartCrudService.InsertAsync(cartList);
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