using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Models; // <-- ĐẢM BẢO DÒNG NÀY CÓ!
using System.Linq;
using System.Collections.Generic; // Cần cho ICollection và WithMany

namespace DecalXeAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // --- DbSet cho các bảng đã có (ĐÃ CẬP NHẬT THEO REVIEW2) ---
        public DbSet<Role> Roles { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<DecalType> DecalTypes { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<DecalService> DecalServices { get; set; }
        public DbSet<DecalTemplate> DecalTemplates { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<CustomServiceRequest> CustomServiceRequests { get; set; }
        public DbSet<Design> Designs { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Warranty> Warranties { get; set; }

        // --- DbSet cho các BẢNG MỚI (TỪ REVIEW1) VÀ ĐỔI TÊN THEO REVIEW2 ---
        public DbSet<VehicleBrand> VehicleBrands { get; set; } // <-- ĐỔI TÊN TỪ CarBrand
        public DbSet<VehicleModel> VehicleModels { get; set; } // <-- ĐỔI TÊN TỪ CarModel
        public DbSet<CustomerVehicle> CustomerVehicles { get; set; }
        public DbSet<OrderStageHistory> OrderStageHistories { get; set; }
        public DbSet<PrintingPriceDetail> PrintingPriceDetails { get; set; }
        public DbSet<DesignComment> DesignComments { get; set; }
        public DbSet<VehicleModelDecalTemplate> VehicleModelDecalTemplates { get; set; } // <-- ĐỔI TÊN TỪ CarModelDecalTemplate

        // --- MỚI TỪ REVIEW2: DbSet cho các bảng NHÓM VAI TRÒ NHÂN VIÊN TÁCH RỜI ---
        public DbSet<AdminDetail> AdminDetails { get; set; }
        public DbSet<ManagerDetail> ManagerDetails { get; set; }
        public DbSet<SalesPersonDetail> SalesPersonDetails { get; set; }
        public DbSet<DesignerDetail> DesignerDetails { get; set; }
        public DbSet<TechnicianDetail> TechnicianDetails { get; set; }

        // --- MỚI TỪ REVIEW2: DbSet cho các bảng LIÊN QUAN ĐẾN GIÁ/ĐẶT CỌC/DESIGN WORK ---
        public DbSet<Deposit> Deposits { get; set; }
        public DbSet<TechLaborPrice> TechLaborPrices { get; set; }
        public DbSet<DesignWorkOrder> DesignWorkOrders { get; set; }
        public DbSet<ServiceVehicleModelProduct> ServiceVehicleModelProducts { get; set; }

        public DbSet<ServiceDecalTemplate> ServiceDecalTemplates { get; set; }

        // Cấu hình các mối quan hệ chi tiết
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình mối quan hệ 1-1 giữa CustomServiceRequest và Order (giữ nguyên)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.CustomServiceRequest)
                .WithOne(csr => csr.Order)
                .HasForeignKey<CustomServiceRequest>(csr => csr.OrderID)
                .IsRequired(false);

            // Cấu hình mối quan hệ 1-1 giữa DecalService và PrintingPriceDetail (giữ nguyên)
            modelBuilder.Entity<PrintingPriceDetail>()
                .HasKey(ppd => ppd.ServiceID);

            modelBuilder.Entity<PrintingPriceDetail>()
                .HasOne(ppd => ppd.DecalService)
                .WithOne(ds => ds.PrintingPriceDetail!)
                .HasForeignKey<PrintingPriceDetail>(ppd => ppd.ServiceID);

            // Cấu hình mối quan hệ tự tham chiếu trong DesignComment (ParentComment) (giữ nguyên)
            modelBuilder.Entity<DesignComment>()
                .HasOne(dc => dc.ParentComment)
                .WithMany(dc => dc.Replies!)
                .HasForeignKey(dc => dc.ParentCommentID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình mối quan hệ nhiều-nhiều cho VehicleModelDecalTemplate
            modelBuilder.Entity<VehicleModelDecalTemplate>()
                .HasOne(vmdt => vmdt.VehicleModel)
                .WithMany(vm => (ICollection<VehicleModelDecalTemplate>)vm.VehicleModelDecalTemplates!) // Ép kiểu tường minh
                .HasForeignKey(vmdt => vmdt.ModelID);

            modelBuilder.Entity<VehicleModelDecalTemplate>()
                .HasOne(vmdt => vmdt.DecalTemplate)
                .WithMany(dt => (ICollection<VehicleModelDecalTemplate>)dt.VehicleModelDecalTemplates!) // Ép kiểu tường minh
                .HasForeignKey(vmdt => vmdt.TemplateID);

            // --- MỚI TỪ REVIEW2: Cấu hình mối quan hệ 1-1 cho các bảng chi tiết vai trò nhân viên ---
            modelBuilder.Entity<AdminDetail>()
                .HasKey(ad => ad.EmployeeID);
            modelBuilder.Entity<AdminDetail>()
                .HasOne(ad => ad.Employee)
                .WithOne(e => e.AdminDetail!)
                .HasForeignKey<AdminDetail>(ad => ad.EmployeeID);

            modelBuilder.Entity<ManagerDetail>()
                .HasKey(md => md.EmployeeID);
            modelBuilder.Entity<ManagerDetail>()
                .HasOne(md => md.Employee)
                .WithOne(e => e.ManagerDetail!)
                .HasForeignKey<ManagerDetail>(md => md.EmployeeID);

            modelBuilder.Entity<SalesPersonDetail>()
                .HasKey(spd => spd.EmployeeID);
            modelBuilder.Entity<SalesPersonDetail>()
                .HasOne(spd => spd.Employee)
                .WithOne(e => e.SalesPersonDetail!)
                .HasForeignKey<SalesPersonDetail>(spd => spd.EmployeeID);

            modelBuilder.Entity<DesignerDetail>()
                .HasKey(dd => dd.EmployeeID);
            modelBuilder.Entity<DesignerDetail>()
                .HasOne(dd => dd.Employee)
                .WithOne(e => e.DesignerDetail!)
                .HasForeignKey<DesignerDetail>(dd => dd.EmployeeID);

            modelBuilder.Entity<TechnicianDetail>()
                .HasKey(td => td.EmployeeID);
            modelBuilder.Entity<TechnicianDetail>()
                .HasOne(td => td.Employee)
                .WithOne(e => e.TechnicianDetail!)
                .HasForeignKey<TechnicianDetail>(td => td.EmployeeID);

            // --- MỚI TỪ REVIEW2: Cấu hình mối quan hệ 1-0..1 cho Design và DesignWorkOrder ---
            modelBuilder.Entity<DesignWorkOrder>()
                .HasKey(dwo => dwo.DesignID); // DesignID là PK của DesignWorkOrder

            modelBuilder.Entity<DesignWorkOrder>()
                .HasOne(dwo => dwo.Design)
                .WithOne(d => d.DesignWorkOrder!)
                .HasForeignKey<DesignWorkOrder>(dwo => dwo.DesignID);

            // --- MỚI TỪ REVIEW2: Cấu hình mối quan hệ nhiều-nhiều-nhiều cho ServiceVehicleModelProduct ---
            modelBuilder.Entity<ServiceVehicleModelProduct>()
                .HasOne(svmp => svmp.DecalService)
                .WithMany(ds => (ICollection<ServiceVehicleModelProduct>)ds.ServiceVehicleModelProducts!) // Ép kiểu tường minh
                .HasForeignKey(svmp => svmp.ServiceID);

            modelBuilder.Entity<ServiceVehicleModelProduct>()
                .HasOne(svmp => svmp.VehicleModel)
                .WithMany(vm => (ICollection<ServiceVehicleModelProduct>)vm.ServiceCarModelProducts!) // Ép kiểu tường minh
                .HasForeignKey(svmp => svmp.VehicleModelID);

            modelBuilder.Entity<ServiceVehicleModelProduct>()
                .HasOne(svmp => svmp.Product)
                .WithMany(p => (ICollection<ServiceVehicleModelProduct>)p.ServiceVehicleModelProducts!) // <-- ĐÃ SỬA: Đảm bảo tên NP khớp và ép kiểu tường minh
                .HasForeignKey(svmp => svmp.ProductID);

            // --- MỚI TỪ REVIEW2: Cấu hình mối quan hệ nhiều-nhiều cho TechLaborPrice ---
            modelBuilder.Entity<TechLaborPrice>()
                .HasOne(tlp => tlp.DecalService)
                .WithMany(ds => (ICollection<TechLaborPrice>)ds.TechLaborPrices!) // Ép kiểu tường minh
                .HasForeignKey(tlp => tlp.ServiceID);

            modelBuilder.Entity<TechLaborPrice>()
                .HasOne(tlp => tlp.VehicleModel)
                .WithMany(vm => (ICollection<TechLaborPrice>)vm.TechLaborPrices!) // Ép kiểu tường minh
                .HasForeignKey(tlp => tlp.VehicleModelID);

            // --- CẤU HÌNH CÁC MỐI QUAN HỆ CHO BẢNG ĐÃ BỊ XÓA ---
            // (Đã được xóa hoặc comment ở các bước trước đó)
        }
    }
}
