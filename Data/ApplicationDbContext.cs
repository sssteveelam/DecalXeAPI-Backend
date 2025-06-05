using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Models; // Dòng này quan trọng để có thể sử dụng các Models

namespace DecalXeAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // --- DbSet cho các bảng cơ bản (Bước 5.1) ---
        public DbSet<Role> Roles { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<DecalType> DecalTypes { get; set; }
        public DbSet<TimeSlotDefinition> TimeSlotDefinitions { get; set; }
        public DbSet<Product> Products { get; set; }

        // --- DbSet cho các bảng phụ thuộc cấp 1 (Bước 5.2) ---
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<DecalService> DecalServices { get; set; }
        public DbSet<ServiceProduct> ServiceProducts { get; set; }

        // --- DbSet cho các bảng phụ thuộc cấp 2 và 3 (Bước 5.3.1 và 5.3.2) ---
        public DbSet<DecalTemplate> DecalTemplates { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<TechnicianDailySchedule> TechnicianDailySchedules { get; set; }
        public DbSet<Promotion> Promotions { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ScheduledWorkUnit> ScheduledWorkUnits { get; set; }
        public DbSet<CustomServiceRequest> CustomServiceRequests { get; set; }
        public DbSet<Design> Designs { get; set; }

        // --- DbSet cho các bảng còn lại (Bước 5.3.3 này) ---
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Warranty> Warranties { get; set; }
        public DbSet<ServiceDecalTemplate> ServiceDecalTemplates { get; set; }

        // Override OnModelCreating để cấu hình các mối quan hệ phức tạp hơn
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình mối quan hệ 1-1 giữa Order và CustomServiceRequest
            // Đặt khóa ngoại trên CustomServiceRequest
            modelBuilder.Entity<Order>()
                .HasOne(o => o.CustomServiceRequest) // Order có một CustomServiceRequest
                .WithOne(csr => csr.Order) // CustomServiceRequest có một Order
                .HasForeignKey<CustomServiceRequest>(csr => csr.OrderID) // <-- SỬA TỪ csr.RequestID SANG csr.OrderID
                .IsRequired(false); // OrderID trong CustomServiceRequest có thể null
        }
    }
}