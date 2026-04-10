using CoreBooking.Domain.Entities; // Ensure this matches your actual namespace
using Microsoft.EntityFrameworkCore;

namespace CoreBooking.Infrastructure.Data
{
    public class CoreBookingDbContext : DbContext
    {
        public CoreBookingDbContext(DbContextOptions<CoreBookingDbContext> options) : base(options)
        {
        }

        // 1. The Catalog Entities
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<ProductContent> ProductContents { get; set; }

        // 2. The User & Transaction Entities
        public DbSet<User> Users { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Optional: You can configure specific database rules here, 
            // like ensuring the AdapterKey is unique, or setting decimal precision for Price.
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(o => o.PriceAtTimeOfBooking)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Provider>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Providers)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Provider>()
                .HasIndex(p => p.AdapterKey)
                .IsUnique();


        }
    }
}