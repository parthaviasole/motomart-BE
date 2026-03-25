using Microsoft.EntityFrameworkCore;
using motomart_BE.Models;

namespace motomart_BE.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names to match Supabase conventions
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Product>().ToTable("products");
            modelBuilder.Entity<Address>().ToTable("addresses");

            // Use unique table names to avoid conflicts with existing tables
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("moto_orders");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.AddressId).HasColumnName("address_id");
                entity.Property(e => e.TotalAmount).HasColumnName("total_amount");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.PaymentType).HasColumnName("payment_type");
                entity.Property(e => e.Otp).HasColumnName("otp");
                entity.Property(e => e.RazorpayOrderId).HasColumnName("razorpay_order_id");
                entity.Property(e => e.RazorpayPaymentId).HasColumnName("razorpay_payment_id");
                entity.Property(e => e.RazorpaySignature).HasColumnName("razorpay_signature");
                entity.Property(e => e.PaymentStatus).HasColumnName("payment_status");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("moto_order_items");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.OrderId).HasColumnName("order_id");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.Price).HasColumnName("price");
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("moto_cart_items");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
            });

            // Additional configurations if needed
            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired();
            
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name);
        }
    }
}
