using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Application_Security_Practical.Models;
using Microsoft.AspNetCore.DataProtection;
using ApplicationSecurityPractical.Models;
using ApplicationSecurityPractical.Classes;

namespace Application_Security_Practical.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

        public ApplicationUser User { get; set; } = new();

        private readonly SignInManager<ApplicationUser> signInManager;

        private readonly UserManager<ApplicationUser> userManager;

        private readonly GoogleCaptchaService _captchaService;
        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, GoogleCaptchaService captchaService)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            _captchaService = captchaService;

        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {

                var captchaResult = await _captchaService.VerifyToken(LModel.Token);
                if (!captchaResult)
                {
                    return Page();
                }

                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("MySecretKey");

               
                var user = await userManager.FindByEmailAsync(LModel.Email);

                if (user != null)
                {
                    var salt = protector.Unprotect(user.salt);

                    var pwdwithsalt = LModel.Password + salt;


                    var identityResult = await signInManager.PasswordSignInAsync(LModel.Email, pwdwithsalt, LModel.RememberMe, true);
                    if (identityResult != null)
                    {
                        if (identityResult.Succeeded)
                        {

                            //Create the security context
                            var claims = new List<Claim>
                        {

                        new Claim(ClaimTypes.Name, "c@c.com"),
                        new Claim(ClaimTypes.Email, "c@c.com"),
                        new Claim("Department", "HR")
                        };

                            //var i = new ClaimsIdentity(claims, "MyCookieAuth");
                            //ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(i);
                            //await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);

                        }
                        //redirect admin and user to different pages

                        //add later after identity 
                        else
                        {
                            //if user is locked out after failed attempts 
                            if (identityResult.IsLockedOut)
                            {
                                ModelState.AddModelError("LockoutError", "Your account is locked out. Kindly wait for 1 minute and try again");
                                return Page();
                            }
                            else
                            {

                                ModelState.AddModelError("CustomError", "Invalid Email or Password");
                                

                            }

                        }
                    }


                    return Page();
                    ModelState.AddModelError("CustomError", "User is null");



                }
            }
            return Redirect("/Index");

        }
    }
};