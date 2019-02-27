using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Kachuwa.Data.Extension;
using Kachuwa.Log;
using MeroSaman.Models;
using MeroSaman.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MeroSaman.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CatagoryController : Controller
    {
        private readonly ICatagoryService _catagoryService;
        private readonly ILogger _logger;
        public CatagoryController(ICatagoryService catagoryService, ILogger logger)
        {
            _catagoryService = catagoryService;
            _logger = logger;
        }

        [Route("catagory/page/{pageNo?}")]
        [Route("catagory")]
        public async Task<IActionResult> Index([FromRoute]int pageNo = 1, [FromQuery]string query = "")
        {
            try
            {
                ViewData["Page"] = pageNo;
                int rowsPerPage = 40;
                var data = await _catagoryService.CategoryCrudService.GetListPagedAsync(pageNo, rowsPerPage, 1, "Where Name like @Query and IsActive=@IsActive", "Addedon desc", new { IsActive = 1,  Query = "%" + query + "%" });
                return View(data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
        }
        [Route("catagory/new")]
        [HttpGet]
        public async Task<IActionResult> New()
        {
            try
            {
                var data = await _catagoryService.CategoryCrudService.GetListAsync("Where IsActive=@IsActive", new { IsActive = 1 });
                return View(data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw;
            }         
        }
        [HttpPost]
        [Route("catagory/new")]
        public async Task<JsonResult> New(Catagory[] catagorys)
        {
            try
            {
                foreach (var item in catagorys)
                {
                    if (item.Name!=null)
                    {
                        item.AutoFill();
                        await _catagoryService.CategoryCrudService.InsertAsync(item);
                    }
                }
                return Json(catagorys);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
        }

        [HttpGet]
        [Route("catagory/edit/{id}")]

        public async Task<IActionResult> Edit([FromRoute]int id)
        {
            try
            {
                if (id == 0)
                {
                    return NotFound();
                }
                var data = await _catagoryService.CategoryCrudService.GetAsync("Where CatagoryId=@CatagoryId", new { CatagoryId = id });
                return View(data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
        }

        [HttpPost]
        [Route("catagory/edit")]

        public async Task<IActionResult> Edit(Catagory catagory)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    catagory.AutoFill();
                    await _catagoryService.CategoryCrudService.UpdateAsync(catagory);
                    return RedirectToAction("Index");
                }
                return View(catagory);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }          
        }

        [Route("catagory/delete")]
        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            try
            {
                if (id > 0)
                {
                    await _catagoryService.CategoryCrudService.DeleteAsync("Where CatagoryId=@CatagoryId", new { CatagoryId = id });
                }
                return Json(StatusCodes.Status200OK);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }        
        }
    }
}