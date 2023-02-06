using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Application_Security_Practical.Models;
using Microsoft.Win32;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System;

namespace Application_Security_Practical.Pages
{
    [ValidateAntiForgeryToken]
    public class RegisterModel : PageModel
    {

        private UserManager<ApplicationUser> userManager { get; }
        private SignInManager<ApplicationUser> signInManager { get; }

        private readonly RoleManager<IdentityRole> roleManager;

        private IWebHostEnvironment environment;

        [BindProperty]
        public Register RModel { get; set; }

        [BindProperty]
        public IFormFile? Upload { get; set; }

        public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment environment)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.environment = environment;
        }



        public void OnGet()
        {
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("MySecretKey");

                //Generate random "salt"
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                byte[] saltByte = new byte[8];

                // Fills array of bytes with a cryptographically strong sequence of random values.
                rng.GetBytes(saltByte);
                var salt = Convert.ToBase64String(saltByte);
                
                string pwdwithsalt = RModel.Password + salt;

                if (Upload != null)
                {
                    if (Upload.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("Upload", "File size cannot exceed 2MB.");
                        return Page();
                    }
                    var uploadsFolder = "uploads";
                    var imageFile = Guid.NewGuid() + Path.GetExtension(Upload.FileName);
                    var imagePath = Path.Combine(environment.ContentRootPath, "wwwroot", uploadsFolder, imageFile);
                    if (!Directory.Exists(Path.GetDirectoryName(imagePath)))
                    {
                        // If the directory does not exist, create it.
                        Directory.CreateDirectory(Path.GetDirectoryName(imagePath));
                    }

                    // Attempt to create the file stream.
                    using var fileStream = new FileStream(imagePath, FileMode.Create);
                    await Upload.CopyToAsync(fileStream);
                    RModel.ImageURL = string.Format("/{0}/{1}", uploadsFolder, imageFile);
                }

                var user = new ApplicationUser()    
                {
                    FullName = System.Web.HttpUtility.HtmlEncode(RModel.FullName),
                    UserName = System.Web.HttpUtility.HtmlEncode(RModel.Email),
                    Email = System.Web.HttpUtility.HtmlEncode(RModel.Email),
                    CreditCardNo = protector.Protect(RModel.CreditCardNo),
                    Gender = System.Web.HttpUtility.HtmlEncode(RModel.Gender),
                    DeliveryAddress = System.Web.HttpUtility.HtmlEncode(RModel.DeliveryAddress),
                    AboutMe = System.Web.HttpUtility.HtmlEncode(RModel.DeliveryAddress),
                    PhoneNumber = System.Web.HttpUtility.HtmlEncode(RModel.MobileNumber),
                    salt = protector.Protect(salt),
                    PhoneNumberConfirmed = true, //phone number verify
                    EmailConfirmed = true, //emnail verify
                    ImageURL = RModel.ImageURL
                };

                //Create the Admin role if NOT exist
                IdentityRole role = await roleManager.FindByIdAsync("Admin");
                if (role == null)
                {
                    IdentityResult result2 = await roleManager.CreateAsync(new IdentityRole("Admin"));
                    if (!result2.Succeeded)
                    {
                        ModelState.AddModelError("", "Create role user failed");
                    }
                }

                //Login user after account creation
                var result = await userManager.CreateAsync(user, pwdwithsalt);

                if (result.Succeeded)
                {
                    //Add users to Admin Role
                    result = await userManager.AddToRoleAsync(user, "Admin");
                    //result = await userManager.AddToRoleAsync(user, "User");

                    await signInManager.SignInAsync(user, false);
                    return RedirectToPage("/Index");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return Page();
        }







    }
}
