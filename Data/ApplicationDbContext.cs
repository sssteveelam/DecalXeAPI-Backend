// DecalXeAPI/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Models;

namespace DecalXeAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // --- NHÓM QUẢN LÝ NGƯỜI DÙNG & NHÂN SỰ ---
        public DbSet<Role> Roles { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<AdminDetail> AdminDetails { get; set; }
        public DbSet<ManagerDetail> ManagerDetails { get; set; }
        public DbSet<SalesPersonDetail> SalesPersonDetails { get; set; }
        public DbSet<DesignerDetail> DesignerDetails { get; set; }
        public DbSet<TechnicianDetail> TechnicianDetails { get; set; }

        // --- NHÓM QUẢN LÝ XE ---
        public DbSet<VehicleBrand> VehicleBrands { get; set; }
        public DbSet<VehicleModel> VehicleModels { get; set; }
        public DbSet<CustomerVehicle> CustomerVehicles { get; set; }
        public DbSet<VehicleModelDecalTemplate> VehicleModelDecalTemplates { get; set; }

        public DbSet<VehicleModelDecalType> VehicleModelDecalTypes { get; set; }


        // --- NHÓM QUẢN LÝ SẢN PHẨM & DỊCH VỤ ---
        public DbSet<DecalType> DecalTypes { get; set; }
        public DbSet<DecalService> DecalServices { get; set; }
        public DbSet<DecalTemplate> DecalTemplates { get; set; }
        public DbSet<TechLaborPrice> TechLaborPrices { get; set; }

        // --- NHÓM QUẢN LÝ ĐƠN HÀNG & NGHIỆP VỤ ---
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Design> Designs { get; set; }
        public DbSet<DesignTemplateItem> DesignTemplateItems { get; set; }
        public DbSet<DesignWorkOrder> DesignWorkOrders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Deposit> Deposits { get; set; }
        public DbSet<Warranty> Warranties { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<OrderStageHistory> OrderStageHistories { get; set; }
        public DbSet<DesignComment> DesignComments { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- CẤU HÌNH CÁC MỐI QUAN HỆ PHỨC TẠP ---
            modelBuilder.Entity<Design>()
                .HasOne(d => d.DesignWorkOrder)
                .WithOne(dwo => dwo.Design)
                .HasForeignKey<DesignWorkOrder>(dwo => dwo.DesignID);

            // Configure Design -> DesignTemplateItem relationship
            modelBuilder.Entity<DesignTemplateItem>()
                .HasOne(dti => dti.Design)
                .WithMany(d => d.TemplateItems)
                .HasForeignKey(dti => dti.DesignId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DesignComment>()
                .HasOne(dc => dc.ParentComment)
                .WithMany(dc => dc.Replies!)
                .HasForeignKey(dc => dc.ParentCommentID)
                .OnDelete(DeleteBehavior.Restrict);

          
            modelBuilder.Entity<TechLaborPrice>()
                .HasKey(tlp => new { tlp.ServiceID, tlp.VehicleModelID });
        }
    }
}