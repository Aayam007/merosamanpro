using System;
using System.Linq;
using System.Threading.Tasks;
using Kachuwa.Data.Extension;
using Kachuwa.Log;
using MeroSaman.Models;
using MeroSaman.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using IdentityUser = Kachuwa.Identity.Models.IdentityUser;

namespace MeroSaman.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICatagoryService _catagoryService;
        private readonly Kachuwa.Storage.IStorageProvider _storageProvider;
        private readonly UserManager<IdentityUser> _usermanager;
        private readonly ILogger _logger;
        public ProductController(IProductService productService, ICatagoryService catagoryService, Kachuwa.Storage.IStorageProvider storageProvider,
            UserManager<IdentityUser> usermanager, ILogger logger)
        {
            _productService = productService;
            _catagoryService = catagoryService;
            _storageProvider = storageProvider;
            _usermanager = usermanager;
            _logger = logger;
        }
        [Route("product/page/{pageNo?}")]
        [Route("product")]
        public async Task<IActionResult> Index([FromRoute]int pageNo = 1, [FromQuery]string query = "")
        {
            try
            {
                var user = await _usermanager.GetUserAsync(User);
                ViewData["Page"] = pageNo;
                int rowsPerPage = 10;
                var data = await _productService.ProductCrudService.GetListPagedAsync(pageNo, rowsPerPage, 1, "Where Name like @Query and IdentityId=@IdentityId ", "Addedon desc", new { IdentityId = user.Id, Query = "%" + query + "%" });
                foreach (var item in data)
                {
                    var catagory = await _catagoryService.CategoryCrudService.GetAsync("Where CatagoryId=@CatagoryId", new { CatagoryId = item.CatagoryId });
                    item.CatagoryName = catagory.Name;
                }
                return View(data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
        }

        [HttpGet]
        [Route("product/new")]
        public async Task<IActionResult> New()
        {
            try
            {
                var user = await _usermanager.GetUserAsync(User);
                var data = await _catagoryService.CategoryCrudService.GetListAsync("Where IsActive=1 ", new { IsActive = 1 });
                ViewBag.catagory = data;

                var product = await _productService.ProductCrudService.GetListAsync("Where IsActive = @IsActive and IdentityId = @IdentityId", new { IsActive = 1, IdentityId = user.Id });
                ViewBag.product = product;
                return View();
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
        }
        [HttpPost]
        [Route("product/new")]
        public async Task<IActionResult> New(Product products)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _usermanager.GetUserAsync(User);
                    products.IdentityId = user.Id;
                    products.AutoFill();
                    products.Image = await _storageProvider.Save("Image", products.ImageUrl);
                    var id = await _productService.ProductCrudService.InsertAsync(products);
                    if (id != 0)
                    {
                        foreach (var item in products.ImageList)
                        {
                            ProductImage productImage = new ProductImage();
                            productImage.ProductId = (int)id;
                            productImage.Image = await _storageProvider.Save("Image", item);
                            await _productService.ProductImageService.InsertAsync(productImage);
                        }
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
        }
        [HttpGet]
        [Route("product/edit/{id}")]
        public async Task<IActionResult> Edit([FromRoute]int id)
        {
            try
            {
                var catagory = await _catagoryService.CategoryCrudService.GetListAsync("Where IsActive=@IsActive", new { IsActive = 1 });
                ViewBag.catagory = catagory;
                if (id == 0)
                {
                    return NotFound();
                }
                var imagelist = await _productService.ProductImageService.GetListAsync("Where ProductId= @ProductId", new { ProductId = id });
                ViewBag.imagelist = imagelist;
                var data = await _productService.ProductCrudService.GetAsync("Where ProductId=@ProductId", new { ProductId = id });
                if (data == null)
                {
                    return NotFound();
                }
                return View(data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
        }

        [HttpPost]
        [Route("product/edit")]
        public async Task<IActionResult> Edit(Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _usermanager.GetUserAsync(User);
                    product.IdentityId = user.Id;
                    product.AutoFill();
                    if (product.ImageUrl != null)
                    {
                        product.Image = await _storageProvider.Save("Image", product.ImageUrl);
                    }
                    await _productService.ProductCrudService.UpdateAsync(product);
                    if (product.ImageList.Count() > 0)
                    {
                        await _productService.ProductImageService.DeleteAsync("Where ProductId=@ProductId", new { ProductId = product.ProductId });
                        foreach (var item in product.ImageList)
                        {
                            ProductImage productImage = new ProductImage();
                            productImage.ProductId = (int)product.ProductId;
                            productImage.Image = await _storageProvider.Save("Image", item);
                            await _productService.ProductImageService.InsertAsync(productImage);
                        }
                    }
                    return RedirectToAction("Index");
                }
                return View(product);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
        }
        [HttpPost]
        [Route("product/delete")]
        public async Task<JsonResult> Delete(int id)
        {
            try
            {
                if (id > 0)
                {
                    await _productService.ProductCrudService.DeleteAsync("Where ProductId=@ProductId", new { ProductId = id });
                    return Json(StatusCodes.Status200OK);
                }
                else
                {
                    return Json(StatusCodes.Status404NotFound);
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
        }

        //########################ADMIN PRODUCT CRUD ###################################################//
        [Route("product/IndexAdmin")]
        public async Task<IActionResult> IndexAdmin([FromRoute]int pageNo = 1, [FromQuery]string query = "")
        {
            try
            {
                var user = await _usermanager.GetUserAsync(User);
                ViewData["Page"] = pageNo;
                int rowsPerPage = 10;
                var data = await _productService.AdminProductCrudService.GetListPagedAsync(pageNo, rowsPerPage, 1, "Where Name like @Query and IsActive=@IsActive and IdentityId=@IdentityId ", "Addedon desc", new { IsActive = 1, IdentityId = user.Id, Query = "%" + query + "%" });
                foreach (var item in data)
                {
                    var catagory = await _catagoryService.CategoryCrudService.GetAsync("Where CatagoryId=@CatagoryId", new { CatagoryId = item.CatagoryId });
                    item.CatagoryName = catagory.Name;
                }
                return View(data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
        }

        [HttpGet]
        [Route("product/NewAdmin")]
        public async Task<IActionResult> NewAdmin()
        {
            try
            {
                var user = await _usermanager.GetUserAsync(User);
                var data = await _catagoryService.CategoryCrudService.GetListAsync("Where IsActive=1 ", new { IsActive = 1 });
                ViewBag.catagory = data;

                var product = await _productService.AdminProductCrudService.GetListAsync("Where IsActive = @IsActive and IdentityId = @IdentityId", new { IsActive = 1, IdentityId = user.Id });
                ViewBag.product = product;
                return View();
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }

        }
        [HttpPost]
        [Route("product/NewAdmin")]
        public async Task<IActionResult> NewAdmin(AdminProduct adminProduct)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _usermanager.GetUserAsync(User);
                    adminProduct.IdentityId = user.Id;
                    adminProduct.IsActive = true;
                    adminProduct.AutoFill();
                    adminProduct.Image = await _storageProvider.Save("Image", adminProduct.ImageUrl);
                    var id = await _productService.AdminProductCrudService.InsertAsync(adminProduct);
                }
                return RedirectToAction("IndexAdmin");
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }

        }
        //for admin edit
        [HttpGet]
        [Route("product/editadmin/{id}")]
        public async Task<IActionResult> EditAdmin([FromRoute]int id)
        {
            try
            {
                var catagory = await _catagoryService.CategoryCrudService.GetListAsync("Where IsActive=@IsActive", new { IsActive = 1 });
                ViewBag.catagory = catagory;
                if (id == 0)
                {
                    return NotFound();
                }
                var data = await _productService.AdminProductCrudService.GetAsync("Where ProductId=@ProductId", new { ProductId = id });
                if (data == null)
                {
                    return NotFound();
                }
                return View(data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
        }
        [HttpPost]
        [Route("product/editadmin")]
        public async Task<IActionResult> EditAdmin(AdminProduct adminProduct)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _usermanager.GetUserAsync(User);
                    adminProduct.IdentityId = user.Id;
                    adminProduct.AutoFill();
                    if (adminProduct.ImageUrl != null)
                    {
                        adminProduct.Image = await _storageProvider.Save("Image", adminProduct.ImageUrl);
                    }


                    await _productService.AdminProductCrudService.UpdateAsync(adminProduct);
                    return RedirectToAction("IndexAdmin");
                }
                return View(adminProduct);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
        }

        //admin edit close
        [HttpPost]
        [Route("product/deleteadmin")]
        public async Task<JsonResult> DeleteAdmin(int id)
        {
            try
            {
                if (id > 0)
                {
                    await _productService.AdminProductCrudService.DeleteAsync("Where ProductId=@ProductId", new { ProductId = id });
                    return Json(StatusCodes.Status200OK);
                }
                else
                {
                    return Json(StatusCodes.Status404NotFound);
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
        }
    }
}