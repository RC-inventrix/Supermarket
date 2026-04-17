using CoreBooking.Domain.Entities;
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
        // public DbSet<User> Users { get; set; } // Safely removed user authentication
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision for financial calculations
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(o => o.PriceAtTimeOfBooking)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            // Configure relationship rules
            modelBuilder.Entity<Provider>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Providers)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Meat" },
                new Category { Id = 2, Name = "Vegetables" },
                new Category { Id = 3, Name = "Spices" }
            );

            // CHANGED: Removed the old AdapterKey unique index rule because 
            // the AdapterKey property no longer exists in our architecture!
        }
    }
}