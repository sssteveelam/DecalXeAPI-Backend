using AutoMapper;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using System;
using System.Linq;

namespace DecalXeAPI.MappingProfiles
{
    public class MainMappingProfile : Profile
    {
        public MainMappingProfile()
        {
            // --- ÁNH XẠ CÁC BẢNG CƠ BẢN & CHUNG ---
            CreateMap<Role, RoleDto>();
            CreateMap<Store, StoreDto>();
            CreateMap<DecalType, DecalTypeDto>();
            CreateMap<CreateDesignCommentDto, DesignComment>();
            CreateMap<CreateCustomerDto, Customer>();
            CreateMap<UpdateCustomerDto, Customer>();
            CreateMap<UpdateCustomServiceRequestDto, CustomServiceRequest>();
            CreateMap<CreateDecalServiceDto, DecalService>();
            CreateMap<UpdateDecalServiceDto, DecalService>();
            CreateMap<CreateDecalTemplateDto, DecalTemplate>();
            CreateMap<UpdateDecalTemplateDto, DecalTemplate>();
            CreateMap<CreateDecalTypeDto, DecalType>();
            CreateMap<UpdateDecalTypeDto, DecalType>();            
            CreateMap<CreateDepositDto, Deposit>();
            CreateMap<CreateDesignDto, Design>();
            CreateMap<UpdateDesignDto, Design>();            
            CreateMap<CreateDesignWorkOrderDto, DesignWorkOrder>();
            CreateMap<UpdateDesignWorkOrderDto, DesignWorkOrder>();
            CreateMap<CreateEmployeeDto, Employee>();
            CreateMap<UpdateEmployeeDto, Employee>();
            CreateMap<CreateFeedbackDto, Feedback>();
            CreateMap<UpdateFeedbackDto, Feedback>();
            CreateMap<CreateOrderDetailDto, OrderDetail>();
            CreateMap<UpdateOrderDetailDto, OrderDetail>();
            CreateMap<CreateOrderDto, Order>();
            CreateMap<UpdateOrderDto, Order>();
            CreateMap<CreatePaymentDto, Payment>();
            CreateMap<UpdatePaymentDto, Payment>();
            CreateMap<CreatePrintingPriceDetailDto, PrintingPriceDetail>();
            CreateMap<UpdatePrintingPriceDetailDto, PrintingPriceDetail>();
            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductDto, Product>();
            CreateMap<CreateStoreDto, Store>();
            CreateMap<UpdateStoreDto, Store>();
            CreateMap<CreateTechLaborPriceDto, TechLaborPrice>();
            CreateMap<UpdateTechLaborPriceDto, TechLaborPrice>();

            CreateMap<CreateServiceVehicleModelProductDto, ServiceVehicleModelProduct>();
            CreateMap<UpdateServiceVehicleModelProductDto, ServiceVehicleModelProduct>();
            CreateMap<CreateVehicleModelDto, VehicleModel>();
            CreateMap<UpdateVehicleModelDto, VehicleModel>();
            CreateMap<VehicleModelDecalType, VehicleModelDecalTypeDto>()
                .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.VehicleModel != null ? src.VehicleModel.ModelName : string.Empty))
                .ForMember(dest => dest.DecalTypeName, opt => opt.MapFrom(src => src.DecalType != null ? src.DecalType.DecalTypeName : string.Empty));

            CreateMap<CreateWarrantyDto, Warranty>();
            CreateMap<UpdateWarrantyDto, Warranty>();            
            // DecalXeAPI/MappingProfiles/MainMappingProfile.cs
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : string.Empty));
                
            CreateMap<Account, AccountDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : string.Empty));
            CreateMap<UpdateAccountDto, Account>(); 


            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store != null ? src.Store.StoreName : string.Empty))
                .ForMember(dest => dest.AccountUsername, opt => opt.MapFrom(src => src.Account != null ? src.Account.Username : null))
                .ForMember(dest => dest.AccountRoleName, opt => opt.MapFrom(src => src.Account != null && src.Account.Role != null ? src.Account.Role.RoleName : null));
            CreateMap<Customer, CustomerDto>()
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
                .ForMember(dest => dest.AccountUsername, opt => opt.MapFrom(src => src.Account != null ? src.Account.Username : null))
                .ForMember(dest => dest.AccountRoleName, opt => opt.MapFrom(src => src.Account != null && src.Account.Role != null ? src.Account.Role.RoleName : null));

            // --- ÁNH XẠ CÁC BẢNG LIÊN QUAN ĐẾN XE (ĐÃ SỬA TÊN) ---
            CreateMap<VehicleBrand, VehicleBrandDto>();
            CreateMap<VehicleModel, VehicleModelDto>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.VehicleBrand != null ? src.VehicleBrand.BrandName : string.Empty));

            CreateMap<CustomerVehicle, CustomerVehicleDto>()
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FirstName + " " + src.Customer.LastName : string.Empty))
                .ForMember(dest => dest.VehicleModelName, opt => opt.MapFrom(src => src.VehicleModel != null ? src.VehicleModel.ModelName : string.Empty))
                .ForMember(dest => dest.VehicleBrandName, opt => opt.MapFrom(src => src.VehicleModel != null && src.VehicleModel.VehicleBrand != null ? src.VehicleModel.VehicleBrand.BrandName : string.Empty));

            // --- ÁNH XẠ CÁC BẢNG LIÊN QUAN ĐẾN DỊCH VỤ VÀ DECAL ---
            CreateMap<DecalService, DecalServiceDto>()
                .ForMember(dest => dest.DecalTypeName, opt => opt.MapFrom(src => src.DecalType != null ? src.DecalType.DecalTypeName : string.Empty))
                .ForMember(dest => dest.PrintingPriceDetailID, opt => opt.MapFrom(src => src.PrintingPriceDetail != null ? src.PrintingPriceDetail.ServiceID : null));
            CreateMap<DecalTemplate, DecalTemplateDto>()
                .ForMember(dest => dest.DecalTypeName, opt => opt.MapFrom(src => src.DecalType != null ? src.DecalType.DecalTypeName : string.Empty));
            
            
            CreateMap<VehicleModelDecalTemplate, VehicleModelDecalTemplateDto>()
                 .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.VehicleModel != null ? src.VehicleModel.ModelName : string.Empty))
                 .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.DecalTemplate != null ? src.DecalTemplate.TemplateName : string.Empty));

            // --- ÁNH XẠ CÁC BẢNG NGHIỆP VỤ (ORDER, DESIGN, PAYMENT...) ---
            CreateMap<CustomServiceRequest, CustomServiceRequestDto>()
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FirstName + " " + src.Customer.LastName : string.Empty))
                .ForMember(dest => dest.SalesEmployeeFullName, opt => opt.MapFrom(src => src.SalesEmployee != null ? src.SalesEmployee.FirstName + " " + src.SalesEmployee.LastName : null))
                .ForMember(dest => dest.OrderID, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderID : null))
                .ForMember(dest => dest.CustomServiceRequestDescription, opt => opt.MapFrom(src => src.Description));

            // HUYNH ĐÃ SỬA LẠI ÁNH XẠ CHO DESIGN ĐỂ BỎ ORDERSTATUS
            CreateMap<Design, DesignDto>()
                .ForMember(dest => dest.DesignerFullName, opt => opt.MapFrom(src => src.Designer != null ? src.Designer.FirstName + " " + src.Designer.LastName : null));

            // HUYNH ĐÃ SỬA LẠI HOÀN CHỈNH ÁNH XẠ CHO ORDER
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FirstName + " " + src.Customer.LastName : string.Empty))
                .ForMember(dest => dest.AssignedEmployeeFullName, opt => opt.MapFrom(src => src.AssignedEmployee != null ? src.AssignedEmployee.FirstName + " " + src.AssignedEmployee.LastName : null))
                .ForMember(dest => dest.CustomServiceRequestID, opt => opt.MapFrom(src => src.CustomServiceRequest != null ? src.CustomServiceRequest.CustomRequestID : null))
                .ForMember(dest => dest.CustomServiceRequestDescription, opt => opt.MapFrom(src => src.CustomServiceRequest != null ? src.CustomServiceRequest.Description : null))
                .ForMember(dest => dest.ChassisNumber, opt => opt.MapFrom(src => src.CustomerVehicle != null ? src.CustomerVehicle.ChassisNumber : null)) // Đã sửa LicensePlate -> ChassisNumber
                .ForMember(dest => dest.VehicleModelName, opt => opt.MapFrom(src => src.CustomerVehicle != null && src.CustomerVehicle.VehicleModel != null ? src.CustomerVehicle.VehicleModel.ModelName : null)) // Sửa CarModel -> VehicleModel
                .ForMember(dest => dest.VehicleBrandName, opt => opt.MapFrom(src => src.CustomerVehicle != null && src.CustomerVehicle.VehicleModel != null && src.CustomerVehicle.VehicleModel.VehicleBrand != null ? src.CustomerVehicle.VehicleModel.VehicleBrand.BrandName : null)) // Sửa CarModel -> VehicleModel
                .ForMember(dest => dest.IsCustomDecal, opt => opt.MapFrom(src => src.IsCustomDecal));

            CreateMap<OrderDetail, OrderDetailDto>()
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderStatus : null))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.DecalService != null ? src.DecalService.ServiceName : string.Empty));
            
            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderStatus : null));
            CreateMap<Feedback, FeedbackDto>()
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderStatus : null))
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FirstName + " " + src.Customer.LastName : string.Empty));
            // Trong file MainMappingProfile.cs
            CreateMap<Warranty, WarrantyDto>()
                .ForMember(dest => dest.ChassisNumber, opt => opt.MapFrom(src => src.CustomerVehicle != null ? src.CustomerVehicle.ChassisNumber : string.Empty))
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.CustomerVehicle != null && src.CustomerVehicle.Customer != null ? src.CustomerVehicle.Customer.FirstName + " " + src.CustomerVehicle.Customer.LastName : string.Empty));

            CreateMap<OrderStageHistory, OrderStageHistoryDto>()
                .ForMember(dest => dest.ChangedByEmployeeFullName, opt => opt.MapFrom(src => src.ChangedByEmployee != null ? src.ChangedByEmployee.FirstName + " " + src.ChangedByEmployee.LastName : null));
            CreateMap<PrintingPriceDetail, PrintingPriceDetailDto>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.DecalService != null ? src.DecalService.ServiceName : string.Empty));
            CreateMap<DesignComment, DesignCommentDto>()
                .ForMember(dest => dest.SenderUsername, opt => opt.MapFrom(src => src.SenderAccount != null ? src.SenderAccount.Username : null))
                .ForMember(dest => dest.SenderRoleName, opt => opt.MapFrom(src => src.SenderAccount != null && src.SenderAccount.Role != null ? src.SenderAccount.Role.RoleName : null));
           

            // --- CÁC ÁNH XẠ CHO DTO ĐẦU VÀO (INPUT) ---
            CreateMap<CreateCustomServiceRequestDto, CustomServiceRequest>()
                .ForMember(dest => dest.CustomRequestID, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.RequestDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(src => "New"))
                .ForMember(dest => dest.OrderID, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.SalesEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore());
            CreateMap<ConvertCsrToOrderDto, Order>()
                .ForMember(dest => dest.OrderID, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.EstimatedCost))
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => "New"))
                .ForMember(dest => dest.CustomerID, opt => opt.Ignore())
                .ForMember(dest => dest.CustomServiceRequest, opt => opt.Ignore())
                .ForMember(dest => dest.IsCustomDecal, opt => opt.MapFrom(src => src.IsCustomDecal));
            CreateMap<RegisterDto, Account>()
                .ForMember(dest => dest.AccountID, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password));


            CreateMap<Deposit, DepositDto>();
            CreateMap<DesignWorkOrder, DesignWorkOrderDto>();
            CreateMap<TechLaborPrice, TechLaborPriceDto>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.DecalService != null ? src.DecalService.ServiceName : string.Empty))
                .ForMember(dest => dest.VehicleModelName, opt => opt.MapFrom(src => src.VehicleModel != null ? src.VehicleModel.ModelName : string.Empty));
            CreateMap<ServiceVehicleModelProduct, ServiceVehicleModelProductDto>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.DecalService != null ? src.DecalService.ServiceName : string.Empty))
                .ForMember(dest => dest.VehicleModelName, opt => opt.MapFrom(src => src.VehicleModel != null ? src.VehicleModel.ModelName : string.Empty))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : string.Empty));


            // Thêm các ánh xạ cho DTO chi tiết
            CreateMap<AdminDetail, AdminDetailDto>();
            CreateMap<ManagerDetail, ManagerDetailDto>();
            CreateMap<SalesPersonDetail, SalesPersonDetailDto>();
            CreateMap<DesignerDetail, DesignerDetailDto>();
            CreateMap<TechnicianDetail, TechnicianDetailDto>();

            
        }
    }
}