using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Kachuwa.Configuration;
using Kachuwa.Identity.Service;
using Kachuwa.Identity.ViewModels;
using Kachuwa.Localization;
using Microsoft.AspNetCore.Http;
using Kachuwa.Data.Extension;
using System.Web;
using Kachuwa.Data;
using System.Data.Common;
using Dapper;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Kachuwa.Web.Services;
using Kachuwa.Web;
using Microsoft.AspNetCore.Identity;
using Kachuwa.Identity.Models;
using IdentityUser = Kachuwa.Identity.Models.IdentityUser;
using Kachuwa.Log;
using MeroSaman.Service;
using MeroSaman.Models;

namespace MeroSaman.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private IEmailSender _emailSender;
        private readonly IAppUserService _appUserService;
        private readonly ILocaleResourceProvider _localeResourceProvider;
        private readonly KachuwaAppConfig _kachuwaConfig;
        private readonly IIdentityUserService _identityUserService;
        private readonly IIdentityRoleService _identityRoleService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private ILogger _logger;
        private IStoreService _storeService;


        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender,
            IHttpContextAccessor httpContextAccessor,
            IAuthenticationSchemeProvider schemeProvider
            , IOptionsSnapshot<KachuwaAppConfig> kachuwaConfig, ILocaleResourceProvider localeResourceProvider,
            IIdentityUserService identityUserService,
            IAppUserService appUserService,
            IIdentityRoleService identityRoleService,
            IHostingEnvironment hostingEnvironment, ILogger logger,
             IStoreService storeService

        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _localeResourceProvider = localeResourceProvider;
            _kachuwaConfig = kachuwaConfig.Value;
            _appUserService = appUserService;
            _identityUserService = identityUserService;
            _identityRoleService = identityRoleService;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
            _storeService = storeService;
        }
        [TempData]
        public string ErrorMessage { get; set; }
        public IAppUserService AppUserService { get; }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            try
            {
                ViewData["ReturnUrl"] = returnUrl;
                if (ModelState.IsValid)
                {
                    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        var user = await _userManager.FindByEmailAsync(model.Email);
                        var role = await _identityRoleService.GetUserRolesAsync(user.Id);
                        if (role.FirstOrDefault().Id == 1)
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            return Redirect("/Dashboard/index");
                        }
                    }
                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                    }
                    if (result.IsLockedOut)
                    {
                        return RedirectToAction(nameof(Lockout));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, _localeResourceProvider.Get("Invalid username or password."));
                        return View(model);
                    }
                }
                // If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }

        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var model = new LoginWith2faViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError(string.Empty, _localeResourceProvider.Get("Someone already has that email. Try another?"));
                        return View(model);
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
                        return RedirectToAction(nameof(RegisterConfirm));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, _localeResourceProvider.Get("Unable to Register your profile"));
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _localeResourceProvider.Get("Unable to Register your profile"));
                    return View(model);
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterConfirm()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/Home/Index");
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var UserName = info.Principal.FindFirstValue(ClaimTypes.MobilePhone);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);
            var firstname = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            var lastname = info.Principal.FindFirstValue(ClaimTypes.Surname);
            var dob = info.Principal.FindFirstValue(ClaimTypes.DateOfBirth);
            var gender = info.Principal.FindFirstValue(ClaimTypes.Gender);
            var identifier = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var picture = $"https://graph.facebook.com/{identifier}/picture?type=large";

            if (!string.IsNullOrEmpty(email))
            {
                var userId = await _userManager.FindByEmailAsync(email);
                if (userId != null)
                {
                    await _signInManager.SignInAsync(userId, true);
                    return Redirect("/home");
                }
                else
                {

                    var user = new IdentityUser { UserName = email, Email = email };
                    user.EmailConfirmed = true;
                    var result = await _userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        AppUser appUser = new AppUser();
                        var userInfo = await _userManager.FindByEmailAsync(email);
                        appUser.AutoFill();
                        appUser.IdentityUserId = userInfo.Id;
                        appUser.FirstName = firstname;
                        appUser.LastName = lastname;
                        appUser.Email = email;
                        appUser.ProfilePicture = picture;
                        appUser.Gender = gender;
                        appUser.DOB = dob;
                        var id = await _appUserService.AppUserCrudService.InsertAsync(appUser);
                        var roleIds = await _identityRoleService.RoleService.GetListAsync();
                        IdentityUserRole identityUserRole = new IdentityUserRole();
                        int roleid = 0;
                        foreach (var item in roleIds)
                        {
                            if (item.Name == "User")
                            {
                                roleid = (int)item.Id;
                                break;
                            }
                        }
                        identityUserRole.RoleId = roleid;
                        identityUserRole.UserId = userInfo.Id;
                        await _identityUserService.UserRoleService.InsertAsync(identityUserRole);
                        IdentityLogin identityLogin = new IdentityLogin();
                        identityLogin.UserId = user.Id;
                        identityLogin.ProviderKey = info.ProviderKey;
                        identityLogin.ProviderDisplayName = info.ProviderDisplayName;
                        identityLogin.LoginProvider = info.LoginProvider;
                        var result1 = await _identityUserService.LoginService.InsertAsync(identityLogin);
                        if (result1 != null)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return Redirect("/Home");
                        }
                        else
                        {
                            return View("Register");
                        }
                    }
                    else
                    {
                        return View("Register");
                    }
                }
            }
            else
            {
                //email address invalid
                return View("Register");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    throw new ApplicationException("Error loading external login information during confirmation.");
                }
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    AppUser appUser = new AppUser();
                    var userInfo = await _userManager.FindByEmailAsync(model.Email);
                    appUser.AutoFill();
                    appUser.FirstName = info.Principal.FindFirstValue(ClaimTypes.Name);
                    appUser.Email = info.Principal.FindFirstValue(ClaimTypes.Email);
                    var id = await _appUserService.AppUserCrudService.InsertAsync(appUser);
                    var roleIds = await _identityRoleService.RoleService.GetListAsync();
                    IdentityUserRole identityUserRole = new IdentityUserRole();
                    int roleid = 0;
                    foreach (var item in roleIds)
                    {
                        if (item.Name == "User")
                        {
                            roleid = (int)item.Id;
                            break;
                        }
                    }
                    identityUserRole.RoleId = roleid;
                    identityUserRole.UserId = userInfo.Id;
                    await _identityUserService.UserRoleService.InsertAsync(identityUserRole);
                    IdentityLogin identityLogin = new IdentityLogin();
                    identityLogin.UserId = user.Id;
                    identityLogin.ProviderKey = info.ProviderKey;
                    identityLogin.ProviderDisplayName = info.ProviderDisplayName;
                    identityLogin.LoginProvider = info.LoginProvider;
                    if (result.Succeeded)
                    {
                        var result1 = await _identityUserService.LoginService.InsertAsync(identityLogin);
                        if (result1 != null)
                        {

                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return Redirect("/Home");
                        }
                    }
                    AddErrors(result);
                }
                else
                {

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return Redirect("/Home");
                }
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View(nameof(ExternalLogin), model);
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
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }
                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.ResetPasswordCallbackLink(model.Email, code, Request.Scheme);
                var to = new List<EmailAddress>() {
                    new EmailAddress{
                    Email=model.Email }
            };
                var appuser = await _appUserService.AppUserCrudService.GetAsync("Where IdentityUserId=@IdentityUserId", new { IdentityUserId = user.Id });
                if (appuser != null)
                {
                    await _emailSender.SendEmailAsync("Reset Password",
                                      $"<div class=\"row email-noti\" style=\"background-color:#eee;padding:25px;margin:25px;font-family:'Lato';\"><div class=\"col-md-12\"><h3>Don't Worry! We all forget Sometimes.</h3><div class=\"forget-email\" style=\"border-top: 1px solid #ccc; border-bottom: 1px solid #ccc; margin: 30px auto; padding: 30px 0;\"><p>Hi, " + appuser.FullName + ",</p><p>You've recently asked to reset password for MeroPet account:<a style=\"color:#ff6c00\"> " + user.Email + "</a></p><p>To update your password, Please click the button below.</p><a href= " + callbackUrl + "  style=\"background-color: #ff6c00;padding:10px 25px;border-radius: 7px;margin: 30px auto; color: #fff; font-size: 16px; text-decoration:none;display: inline-block\"> Reset Password</a><div class=\"regards\"><p style=\"margin-bottom:7px\">Cheers,</p><p>MeroPet Team.</p></div></div><div class=\"text-center\"><p style = \"color: #999\"> This message is sent to you by</p><img src = \"http://www.meropet.com/Image/logobrand.png?h=300%20&amp;&amp;%20w=300\" style=\"height:20px;margin-right:10px\"><span style=\"font-weight: bold\">MEROPET</span></div></div></div>", to.ToArray());
                }
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string Email = null, string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            var model = new ResetPasswordViewModel { Code = code, Email = Email };
            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            AddErrors(result);
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterStore(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterStore(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                Store store = new Store();
                try
                {

                    var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError(string.Empty, _localeResourceProvider.Get("Someone already has that email. Try another?"));
                        return View(model);
                    }
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        var identityUser = await _userManager.FindByEmailAsync(model.Email);
                        store.StoreName = model.FirstName;
                        store.Email = model.Email;
                        store.PhoneNumber1 = model.PhoneNumber;
                        store.AutoFill();
                        store.IdentityId = identityUser.Id;
                        var dbFactory = DbFactoryProvider.GetFactory();
                        using (var db = (DbConnection)dbFactory.GetConnection())
                        {
                            await db.OpenAsync();
                            using (var dbTran = db.BeginTransaction())
                            {
                                try
                                {
                                    var id = await _storeService.StoreCrudService.InsertAsync<int>(db, store, dbTran, 10);
                                    int[] roleIds = { 3 };
                                    await _identityUserService.AddUserRoles(roleIds, identityUser.Id);
                                    var myuser = await _userManager.FindByEmailAsync(model.Email);
                                    dbTran.Commit();
                                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(myuser);
                                    var newcode = HttpUtility.UrlEncode(code);
                                    EmailAddress emailAddress = new EmailAddress();
                                    emailAddress.Email = model.Email;
                                    var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account",
                                    new { Email = model.Email, code = newcode }, protocol: HttpContext.Request.Scheme);
                                    //  var storedata = await _storeService.StoreCrudService.GetAsync("Where IdentityId=@IdentityId", new { IdentityId = myuser.Id });
                                    string FilePath = _hostingEnvironment.ContentRootPath + "\\EmailBody\\ClientsRegistration.html";
                                    StreamReader str = new StreamReader(FilePath);
                                    string MailText = str.ReadToEnd();
                                    MailText = MailText.Replace("{CallUrl}", callbackUrl);

                                    await _emailSender.SendEmailAsync("Confirm your email", MailText, emailAddress);
                                    TempData["Message"] = "Confirmation Email has been send to your email. Please check email.";
                                    TempData["MessageValue"] = "1";
                                }
                                catch (Exception e)
                                {
                                    dbTran.Rollback();
                                    await _userManager.DeleteAsync(user);
                                    _logger.Log(LogType.Error, () => e.Message, e);
                                    throw e;

                                }
                            }
                        }
                        return RedirectToAction(nameof(RegisterConfirm));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, _localeResourceProvider.Get("Unable to register your profile. Try another?"));
                        return View(model);
                    }
                }
                catch (Exception e)
                {
                    _logger.Log(LogType.Error, () => e.Message, e);
                    throw e;
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, _localeResourceProvider.Get("Unable to register your profile. Try another?"));
                return View(model);
            }

        }
        #region Helpers
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect("/");
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> GetCode(string code)
        {
            if (code == null)
            {
                return Json("Failed");
            }
            else
            {
                var List = await _appUserService.AppUserCrudService.GetAsync("where OwnerUniqId=@OwnerUniqId", new { OwnerUniqId = code });
                if (List != null)
                {
                    return Json(List.IdentityUserId);
                }
                else
                {
                    return Json("Invalid");
                }
            }
        }

        #endregion
    }
}
