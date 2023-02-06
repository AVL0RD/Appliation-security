using Application_Security_Practical.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.DataProtection;

namespace Application_Security_Practical.Pages
{

    public class IndexModel : PageModel
    {
   
        private readonly IHttpContextAccessor contxt;

        private readonly UserManager<ApplicationUser> userManager;

        public ApplicationUser User { get; set; } = new();

        public IndexModel(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            contxt = httpContextAccessor;
            this.userManager = userManager;

        }

        
        public async Task<IActionResult> OnGetAsync()
        {
            User = await userManager.GetUserAsync(HttpContext.User);
            var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
            var protector = dataProtectionProvider.CreateProtector("MySecretKey");


            if (User == null)
            {
                return RedirectToPage("/Login");
            }
            else
            {
                User = new ApplicationUser()
                {
                    FullName = User.FullName,
                    UserName = User.Email,
                    Email = User.Email,
                    CreditCardNo = protector.Unprotect(User.CreditCardNo),
                    Gender = User.Gender,
                    DeliveryAddress = User.DeliveryAddress,
                    AboutMe = User.AboutMe,
                    PhoneNumber = User.PhoneNumber,
                    ImageURL = User.ImageURL
                    
                };
                contxt.HttpContext.Session.SetString("Status", "Successful");
                return Page();
            }
        }


        //public Onpost()
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
        //        var protector = dataProtectionProvider.CreateProtector("MySecretKey");
        //    }


        //}
    }
}