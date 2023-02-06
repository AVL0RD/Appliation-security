using Application_Security_Practical.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Application_Security_Practical
{
	public class MyDbContext : IdentityDbContext<ApplicationUser>
	{
		private readonly IConfiguration _configuration;
		public MyDbContext(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			string connectionString = _configuration.GetConnectionString("MyConnection");
			optionsBuilder.UseSqlServer(connectionString);
		}
	}
}
