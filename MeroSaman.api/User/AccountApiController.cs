using Kachuwa.Identity.Models;
using Kachuwa.Identity.Service;
using Kachuwa.Identity.ViewModels;
using Kachuwa.Log;
using Kachuwa.Web;
using Kachuwa.Web.API;
using Kachuwa.Web.Services;
using MeroSaman.Api;
using MeroSaman.Models;
using MeroSaman.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using IdentityUser = Kachuwa.Identity.Models.IdentityUser;

namespace MeroSamanApi.Controllers
{

    public class AccountApiController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IAppUserService _appUserService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;

        public AccountApiController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IAppUserService appUserService,
            IHostingEnvironment hostingEnvironment, IEmailSender emailSender, ILogger logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appUserService = appUserService;
            _hostingEnvironment = hostingEnvironment;
            _emailSender = emailSender;
            _logger = logger;
        }

        [AllowAnonymous]
        [Route("api/v1/token")]
        [HttpPost]
        public async Task<IActionResult> Token()
        {
            IActionResult response = Unauthorized();
            var token = new JwtTokenBuilder()
                          .AddSecurityKey(JwtSecurityKey.Create("Mero Saman Is Best Product"))
                          .AddSubject("james bond")
                          .AddIssuer("MeroSaman")
                          .AddAudience("MeroSamanApi")
                          .AddClaim("UserId", "111")
                          .AddExpiry(1)
                          .Build();

            response = Ok(new { token = token });
            return response;
        }

        [AllowAnonymous]
        [Route("api/v1/user/login")]
        [HttpPost]
        public async Task<IActionResult> LogIn(LoginViewModel user)
        {
            IActionResult response = Unauthorized();
            var result = await _signInManager.PasswordSignInAsync(user.Email, user.Password, user.RememberMe, lockoutOnFailure: false);
            var data = await _userManager.FindByEmailAsync(user.Email);
            if (result.Succeeded)
            {
                var token = new JwtTokenBuilder()
                              .AddSecurityKey(JwtSecurityKey.Create("Mero Saman Is Best Product"))
                              .AddSubject("james bond")
                              .AddIssuer("MeroSaman")
                              .AddAudience("MeroSamanApi")
                              .AddClaim("Id",data.Id.ToString())
                              .AddExpiry(1)
                              .Build();

                response = Ok(new { token = token });
            }
            return response;
        }

        [AllowAnonymous]
        [Route("api/v1/user/registration")]
        [HttpPost]
        public async Task<JsonResult> Register(UserViewModel model)
        {
            IActionResult response = Unauthorized();
            try
            {
                if (ModelState.IsValid)
                {
                    var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        response = Ok(new { message = "User name already exit" });
                        return Json(response);
                    }
                    var status = await _appUserService.SaveNewUserAsync(model);
                    if (status.HasError == false)
                    {
                        var myuser = await _userManager.FindByEmailAsync(model.Email);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(myuser);
                        var newcode = HttpUtility.UrlEncode(code);
                        EmailAddress emailAddress = new EmailAddress();
                        emailAddress.Email = model.Email;
                        var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account",
                        new { Email = model.Email, code = newcode }, protocol: HttpContext.Request.Scheme);
                        var appuser = await _appUserService.AppUserCrudService.GetAsync("Where IdentityUserId=@IdentityUserId", new { IdentityUserId = myuser.Id });
                        string FilePath = _hostingEnvironment.ContentRootPath + "\\EmailBody\\ClientsRegistration.html";
                        StreamReader str = new StreamReader(FilePath);
                        string MailText = str.ReadToEnd();
                        MailText = MailText.Replace("{CallUrl}", callbackUrl);

                        await _emailSender.SendEmailAsync("Confirm your email", MailText, emailAddress);
                        TempData["Message"] = "Confirmation Email has been send to your email. Please check email.";
                        TempData["MessageValue"] = "1";
                        response = Ok(new { message = "Confirm your email" });
                        return Json(response);
                    }
                    else
                    {
                        response = Ok(new { message = "Unable to register a user" });
                        return Json(response);
                    }
                }
                else
                {
                    response = Ok(new { message = "Unable to register a user" });
                    return Json(response);
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                response = Ok(new { message = "exception throw" });
                return Json(response);
                throw e;
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string Email, string code)
        {
            try
            {
                if (Email == null || code == null)
                {
                    return Redirect("/");
                }
                var user = await _userManager.FindByEmailAsync(Email);
                if (user == null)
                {
                    return Redirect("/account/register");
                    //throw new ApplicationException($"Unable to load user with ID '{Email}'.");
                }
                var newcode = HttpUtility.UrlDecode(code);
                var result = await _userManager.ConfirmEmailAsync(user, newcode);
                var userapp = await _appUserService.AppUserCrudService.GetAsync("Where IdentityUserId=@IdentityUserId", new { IdentityUserId = user.Id });
                if (userapp != null)
                {
                    await _appUserService.AppUserCrudService.UpdateAsync(userapp);
                }
                if (!result.Succeeded)
                {
                    var generatenew = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var newmycode = HttpUtility.UrlEncode(generatenew);
                    EmailAddress emailAddress = new EmailAddress();
                    emailAddress.Email = user.Email;
                    var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account",
                    new { Email = user.Email, code = newmycode }, protocol: HttpContext.Request.Scheme);
                    var appuser = await _appUserService.AppUserCrudService.GetAsync("Where IdentityUserId=@IdentityUserId", new { IdentityUserId = user.Id });

                    string FilePath = "E:\\Projects\\MeroPetBackend2\\MeroPetApp\\ClientsRegistration.html";
                    StreamReader str = new StreamReader(FilePath);
                    string MailText = str.ReadToEnd();
                    MailText = MailText.Replace("{CallUrl}", callbackUrl);

                    //var formate = string.Format($"<div class=\"get-started\"><div class=\"row\" style=\"margin-bottom: 40px;\"><div class=\"col-md-12 text-center\"><h1 style =\"font-weight: bold; color: #ff6c00;\"><img src=\"http://www.meropet.com/Image/logobrand.png?h=300%20&&%20w=300\" style=\"height: 40px; margin-right: 10px;\"><span style = \"font-weight: bold;\">MEROPET</span></h1><h5 style=\"display: block; margin-top: 20px;\">Welcome to Meropet</h5><p>Thank you for sigining up with MeroPet! Get started by adding your pet and veterinaries and get the effective care services for your pets.</p><a href =  " + callbackUrl + " style=\"background-color: #ff6c00; padding: 10px 25px; border-radius: 7px; margin: 30px auto; color: #fff; font-size: 16px; text-decoration: none; display: inline-block;\">Get Started</a></div></div><div class=\"row\" style=\"margin-bottom: 40px;\"><div class=\"col-md-6 text-left\"><p><strong>Download the App</strong></p><p>You’ll need the MeroPet app for your iphone, Android to start take care of your pets.</p><a style= \"color:#ff6c00; text-transform: uppercase;\">GET THE MEROPET APP!</a></div><div class=\"col-md-6 text-left\" style=\"margin-top: 20px;\"><p><img src = \"https://image.flaticon.com/icons/svg/174/174836.svg\" style=\"height:30px;\"><strong>&nbsp;&nbsp;Android</strong></p><p><img src = \"https://image.flaticon.com/icons/svg/831/831276.svg\" style=\"height:30px;\"><strong>&nbsp;&nbsp;Ios</strong></p></div></div><div class=\"row\"><div class=\"col-md-6 text-left\"><img src = \"https://images.pexels.com/photos/265667/pexels-photo-265667.jpeg?auto=compress&cs=tinysrgb&h=750&w=1260\" style=\"width: 100%; height:250px; object-fit: contain; margin: auto;\"></div><div class=\"col-md-6 text-left\"><p><strong>Visit Your web Dashboard</strong></p><ul><li>Add multiple pets, veterinary</li><li>Ask any pet related queries</li><li>Take suitable appointment to the veterinary you have selected</li><li>Add food and supplies for your pet</li><li>Get pet care services</li></ul><a  style= \"color:#ff6c00; text-transform: uppercase;\">Login to dashboard ></a></div></div><div class=\"row\" style=\"border-top: 1px solid #ccc; padding-top: 20px; margin-top: 40px;\"><div class=\"col-md-12 text-left\"><p><strong>Have more Question?</strong></p><p>Visit our online support center anytime to get answers or contact a representative.VISIT SUPPORT CENTER</ p><a  style=\"color:#ff6c00; text-transform: uppercase;\">Visit help center ></a></div></div><div class=\"row\" style=\"margin-top: 40px\"><div class=\"col-md-12 text-center\"><a style=\"background:#ff6c00;height:30px; width: 30px; border-radius: 50%; color: #fff; text-decoration: none; display: inline-block; padding-top: 3px;margin-right: 5px;text-align: center; vertical-align: middle;\"><i class=\"fa fa-facebook\"></i></a><a  style=\"background:#ff6c00;height:30px; width: 30px; border-radius: 50%; color: #fff; text-decoration: none; display: inline-block; padding-top: 3px;margin-right: 5px;text-align: center; vertical-align: middle;\"><i class=\"fa fa-twitter\"></i></a><p>© 2018 Bitspanda Technology, All right Reserved</p><p>Our mailing address is:<br/>Bitspanda technology Pvt. Ltd., BK Bhawan 4th floor, Balkumari, Lalitpur, Nepal</p></div></div></div>");
                    await _emailSender.SendEmailAsync("Confirm your email", MailText, emailAddress);
                    TempData["Message"] = "Confirmation Email has been send to your email. Please check email.";
                    TempData["MessageValue"] = "1";
                    return RedirectToAction(nameof(RegisterConfirm));
                }
                return View(result.Succeeded ? "ConfirmEmail" : "Error");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterConfirm()
        {
            return View();
        }

    }
}