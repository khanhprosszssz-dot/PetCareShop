using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetCareShop.Models;

namespace PetCareShop.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;

        public DbSet<ShoppingCartItem> ShoppingCartItems
        {
            get;
            set;
        } = null!;

        public DbSet<Order> Orders { get; set; } = null!;

        public DbSet<OrderDetail> OrderDetails { get; set; } = null!;

        public DbSet<Customer> Customers { get; set; } = null!;

        public DbSet<ProductReview> ProductReviews
        {
            get;
            set;
        } = null!;

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ShoppingCartItem>()
                .HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ShoppingCartItem>()
                .HasIndex(item => new
                {
                    item.ShoppingCartId,
                    item.ProductId
                })
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasOne(order => order.Customer)
                .WithMany()
                .HasForeignKey(order => order.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(detail => detail.Order)
                .WithMany(order => order.OrderDetails)
                .HasForeignKey(detail => detail.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductReview>()
                .HasOne(review => review.Product)
                .WithMany()
                .HasForeignKey(review => review.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductReview>()
                .HasOne(review => review.Customer)
                .WithMany()
                .HasForeignKey(review => review.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}