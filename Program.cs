using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;
using PetCareShop.Models;
using PetCareShop.Models.Interfaces;
using PetCareShop.Models.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var connectionString =
    builder.Configuration
        .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Không tìm thấy chuỗi kết nối DefaultConnection.");

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseSqlServer(connectionString));

builder.Services
    .AddDefaultIdentity<ApplicationUser>(
        options =>
        {
            options.SignIn
                .RequireConfirmedAccount = false;

            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password
                .RequireNonAlphanumeric = false;

            options.User.RequireUniqueEmail = true;
        })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(
    options =>
    {
        options.LoginPath =
            "/Identity/Account/Login";

        options.LogoutPath =
            "/Identity/Account/Logout";

        options.AccessDeniedPath =
            "/Identity/Account/AccessDenied";
    });

builder.Services.AddScoped<
    IProductRepository,
    ProductRepository>();

builder.Services.AddScoped<
    IShoppingCartRepository>(
        services =>
            ShoppingCartRepository.GetCart(
                services));

builder.Services.AddScoped<
    IOrderRepository,
    OrderRepository>();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout =
        TimeSpan.FromMinutes(30);

    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

using (var scope =
       app.Services.CreateScope())
{
    var services =
        scope.ServiceProvider;

    var context =
        services.GetRequiredService<
            ApplicationDbContext>();

    var roleManager =
        services.GetRequiredService<
            RoleManager<IdentityRole>>();

    var userManager =
        services.GetRequiredService<
            UserManager<ApplicationUser>>();

    string[] roleNames =
    {
        "Admin",
        "Customer"
    };

    foreach (string roleName in roleNames)
    {
        if (!await roleManager
                .RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(
                new IdentityRole(roleName));
        }
    }

    const string adminEmail =
        "admin@petcare.com";

    const string adminPassword =
        "123456";

    var adminUser =
        await userManager.FindByEmailAsync(
            adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FullName = "Quản trị viên",
            PhoneNumber = "0909999999",
            Address = "Đà Lạt, Việt Nam",
            CreatedAt = DateTime.Now
        };

        var createResult =
            await userManager.CreateAsync(
                adminUser,
                adminPassword);

        if (!createResult.Succeeded)
        {
            string errors = string.Join(
                "; ",
                createResult.Errors.Select(
                    error =>
                        error.Description));

            throw new InvalidOperationException(
                errors);
        }
    }

    if (!await userManager.IsInRoleAsync(
            adminUser,
            "Admin"))
    {
        await userManager.AddToRoleAsync(
            adminUser,
            "Admin");
    }

    if (!await context.Products.AnyAsync())
    {
        context.Products.AddRange(
            new Product
            {
                Name = "Chó Corgi Pembroke",
                Category = "Chó cảnh",
                Description =
                    "Corgi chân ngắn đáng yêu, thông minh và thân thiện.",
                Price = 12000000,
                ImageUrl =
                    "/images/products/corgi.jpg"
            },
            new Product
            {
                Name = "Mèo Anh Lông Ngắn",
                Category = "Mèo cảnh",
                Description =
                    "Mèo Anh mặt tròn, hiền lành và dễ chăm sóc.",
                Price = 9000000,
                ImageUrl =
                    "/images/products/british-cat.jpg"
            },
            new Product
            {
                Name = "Pate Cho Mèo Cao Cấp",
                Category = "Thức ăn",
                Description =
                    "Pate giàu dinh dưỡng dành cho mèo.",
                Price = 45000,
                ImageUrl =
                    "/images/products/cat-food.jpg"
            },
            new Product
            {
                Name = "Vòng Cổ Thú Cưng",
                Category = "Phụ kiện",
                Description =
                    "Vòng cổ mềm mại và chắc chắn.",
                Price = 79000,
                ImageUrl =
                    "/images/products/collar.jpg"
            }
        );

        await context.SaveChangesAsync();
    }
}

app.Run();