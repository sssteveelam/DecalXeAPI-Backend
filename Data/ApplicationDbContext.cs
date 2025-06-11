using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Models; // Dòng này quan trọng để có thể sử dụng các Models
using System.Linq; // Để sử dụng Linq cho các truy vấn trong OnModelCreating

namespace DecalXeAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // --- DbSet cho các bảng đã có (giữ nguyên) ---
        public DbSet<Role> Roles { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<DecalType> DecalTypes { get; set; }
        public DbSet<TimeSlotDefinition> TimeSlotDefinitions { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<DecalService> DecalServices { get; set; }
        public DbSet<ServiceProduct> ServiceProducts { get; set; }
        public DbSet<DecalTemplate> DecalTemplates { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<TechnicianDailySchedule> TechnicianDailySchedules { get; set; }
        public DbSet<ScheduledWorkUnit> ScheduledWorkUnits { get; set; }
        public DbSet<CustomServiceRequest> CustomServiceRequests { get; set; }
        public DbSet<Design> Designs { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Warranty> Warranties { get; set; }

        public DbSet<ServiceDecalTemplate> ServiceDecalTemplates { get; set; }

        // --- DbSet cho các BẢNG MỚI từ yêu cầu review ---
        public DbSet<CarBrand> CarBrands { get; set; } // Bảng mới
        public DbSet<CarModel> CarModels { get; set; } // Bảng mới
        public DbSet<CustomerVehicle> CustomerVehicles { get; set; } // Bảng mới
        public DbSet<OrderStageHistory> OrderStageHistories { get; set; } // Bảng mới
        public DbSet<PrintingPriceDetail> PrintingPriceDetails { get; set; } // Bảng mới
        public DbSet<DesignComment> DesignComments { get; set; } // Bảng mới
        public DbSet<OrderCompletionImage> OrderCompletionImages { get; set; } // Bảng mới
        public DbSet<CarModelDecalTemplate> CarModelDecalTemplates { get; set; } // Bảng liên kết mới

        // Cấu hình các mối quan hệ chi tiết
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình mối quan hệ 1-1 giữa CustomServiceRequest và Order (đã có từ trước)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.CustomServiceRequest)
                .WithOne(csr => csr.Order)
                .HasForeignKey<CustomServiceRequest>(csr => csr.OrderID)
                .IsRequired(false); // OrderID trong CustomServiceRequest có thể null

            // Cấu hình mối quan hệ 1-1 giữa DecalService và PrintingPriceDetail
            // ServiceID của PrintingPriceDetail là cả PK và FK, trỏ về DecalService
            modelBuilder.Entity<PrintingPriceDetail>()
                .HasKey(ppd => ppd.ServiceID); // Định nghĩa ServiceID là Khóa Chính của PrintingPriceDetail

            modelBuilder.Entity<PrintingPriceDetail>()
                .HasOne(ppd => ppd.DecalService) // Một PrintingPriceDetail thuộc về một DecalService
                .WithOne(ds => ds.PrintingPriceDetail!) // Một DecalService có một PrintingPriceDetail
                .HasForeignKey<PrintingPriceDetail>(ppd => ppd.ServiceID); // Khóa ngoại là ServiceID trong PrintingPriceDetail

            // Cấu hình mối quan hệ tự tham chiếu trong DesignComment (ParentComment)
            modelBuilder.Entity<DesignComment>()
                .HasOne(dc => dc.ParentComment) // Một DesignComment có một ParentComment (ID)
                .WithMany(dc => dc.Replies!) // Một ParentComment có thể có nhiều Replies (dùng ! để bỏ qua warning nullability)
                .HasForeignKey(dc => dc.ParentCommentID)
                .IsRequired(false) // ParentCommentID có thể null
                .OnDelete(DeleteBehavior.Restrict); // Ngăn chặn xóa cascade để tránh vòng lặp hoặc lỗi.
                                                  // Nếu ParentComment bị xóa, các Replies sẽ không bị xóa theo.

            // Cấu hình mối quan hệ nhiều-nhiều cho CarModelDecalTemplate (qua bảng liên kết)
            // Vì CarModelDecalTemplateID là PK riêng, chỉ cần cấu hình mối quan hệ:
            modelBuilder.Entity<CarModelDecalTemplate>()
                .HasOne(cmdt => cmdt.CarModel)
                .WithMany(cm => cm.CarModelDecalTemplates!) // ! để bỏ qua warning nullability
                .HasForeignKey(cmdt => cmdt.ModelID);

            modelBuilder.Entity<CarModelDecalTemplate>()
                .HasOne(cmdt => cmdt.DecalTemplate)
                .WithMany(dt => dt.CarModelDecalTemplates!) // ! để bỏ qua warning nullability
                .HasForeignKey(cmdt => cmdt.TemplateID);
        }
    }
}