using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class PetCareShopContext(DbContextOptions<PetCareShopContext> options) : IdentityDbContext<PetCareShop.Data.ApplicationUser>(options)
{
}

