using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Application_Security_Practical.Models
{
    public class ApplicationUser: IdentityUser
    {

		

		[Required, MaxLength(50)]
		[Display(Name = "Full Name")]
		public string FullName { get; set; } = string.Empty;

		[Required]
		public string Gender { get; set; } = string.Empty;

		[Required]
		public string DeliveryAddress { get; set; } = string.Empty;

		[Required]
		[Display(Name = "Credit Card ")]
		public string CreditCardNo { get; set; } = string.Empty;

		[Required]
		public string AboutMe { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ImageURL { get; set; }

		public string salt { get; set; }

    }
}
