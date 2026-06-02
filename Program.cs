using Microsoft.EntityFrameworkCore;
using PetCareShop.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!context.Products.Any())
    {
        context.Products.AddRange(
            new PetCareShop.Models.Product
            {
                Name = "Chó Corgi Pembroke",
                Category = "Chó cảnh",
                Description = "Corgi chân ngắn đáng yêu, thông minh, phù hợp nuôi trong gia đình.",
                Price = 12000000,
                ImageUrl = "/images/products/corgi.jpg"
            },
            new PetCareShop.Models.Product
            {
                Name = "Mèo Anh Lông Ngắn",
                Category = "Mèo cảnh",
                Description = "Mèo Anh lông ngắn mặt tròn, hiền lành, dễ chăm sóc.",
                Price = 9000000,
                ImageUrl = "/images/products/british-cat.jpg"
            },
            new PetCareShop.Models.Product
            {
                Name = "Pate Cho Mèo Cao Cấp",
                Category = "Thức ăn",
                Description = "Pate dinh dưỡng, hỗ trợ tiêu hóa và tăng sức đề kháng cho mèo.",
                Price = 45000,
                ImageUrl = "/images/products/cat-food.jpg"
            },
            new PetCareShop.Models.Product
            {
                Name = "Hạt Dinh Dưỡng Cho Chó",
                Category = "Thức ăn",
                Description = "Thức ăn hạt dành cho chó mọi lứa tuổi, giàu protein và vitamin.",
                Price = 250000,
                ImageUrl = "/images/products/dog-food.jpg"
            },
            new PetCareShop.Models.Product
            {
                Name = "Vòng Cổ Thú Cưng",
                Category = "Phụ kiện",
                Description = "Vòng cổ mềm mại, chắc chắn, nhiều màu sắc cho chó mèo.",
                Price = 79000,
                ImageUrl = "/images/products/collar.jpg"
            },
            new PetCareShop.Models.Product
            {
                Name = "Nhà Cây Cho Mèo",
                Category = "Phụ kiện",
                Description = "Nhà cây nhiều tầng cho mèo leo trèo, vui chơi và nghỉ ngơi.",
                Price = 850000,
                ImageUrl = "/images/products/cat-tree.jpg"
            },
            new PetCareShop.Models.Product
            {
                Name = "Chó Poodle Tiny",
                Category = "Chó cảnh",
                Description = "Poodle nhỏ nhắn, thông minh, ít rụng lông, dễ huấn luyện.",
                Price = 8000000,
                ImageUrl = "/images/products/poodle.jpg"
            },
            new PetCareShop.Models.Product
            {
                Name = "Mèo Scottish Fold",
                Category = "Mèo cảnh",
                Description = "Mèo tai cụp đáng yêu, tính cách ngoan ngoãn và thân thiện.",
                Price = 11000000,
                ImageUrl = "/images/products/scottish-fold.jpg"
            }
        );

        context.SaveChanges();
    }
}
app.Run();