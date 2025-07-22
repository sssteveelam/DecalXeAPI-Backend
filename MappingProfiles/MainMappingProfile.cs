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

            CreateMap<CreateOrderDto, Order>()
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount));

            CreateMap<UpdateOrderDto, Order>();
            CreateMap<CreatePaymentDto, Payment>();
            CreateMap<UpdatePaymentDto, Payment>();
            CreateMap<CreateStoreDto, Store>();
            CreateMap<UpdateStoreDto, Store>();
            CreateMap<CreateTechLaborPriceDto, TechLaborPrice>();
            CreateMap<UpdateTechLaborPriceDto, TechLaborPrice>();

            CreateMap<CreateVehicleModelDto, VehicleModel>();
            CreateMap<UpdateVehicleModelDto, VehicleModel>();
            CreateMap<VehicleModelDecalType, VehicleModelDecalTypeDto>()
                .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.VehicleModel != null ? src.VehicleModel.ModelName : string.Empty))
                .ForMember(dest => dest.DecalTypeName, opt => opt.MapFrom(src => src.DecalType != null ? src.DecalType.DecalTypeName : string.Empty));

            CreateMap<CreateWarrantyDto, Warranty>();
            CreateMap<UpdateWarrantyDto, Warranty>();            
            // DecalXeAPI/MappingProfiles/MainMappingProfile.cs

            

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

            CreateMap<CreateCustomerVehicleDto, CustomerVehicle>();
            CreateMap<UpdateCustomerVehicleDto, CustomerVehicle>();

            // --- ÁNH XẠ CHO DESIGN TEMPLATE ITEMS ---
            CreateMap<DesignTemplateItem, DesignTemplateItemDto>();
            CreateMap<CreateDesignTemplateItemDto, DesignTemplateItem>();
            CreateMap<UpdateDesignTemplateItemDto, DesignTemplateItem>();

            // --- ÁNH XẠ CÁC BẢNG LIÊN QUAN ĐẾN DỊCH VỤ VÀ DECAL ---
            CreateMap<DecalService, DecalServiceDto>()
                .ForMember(dest => dest.DecalTypeName, opt => opt.MapFrom(src => src.DecalType != null ? src.DecalType.DecalTypeName : string.Empty));
            CreateMap<DecalTemplate, DecalTemplateDto>()
                .ForMember(dest => dest.DecalTypeName, opt => opt.MapFrom(src => src.DecalType != null ? src.DecalType.DecalTypeName : string.Empty));
            
            
            CreateMap<VehicleModelDecalTemplate, VehicleModelDecalTemplateDto>()
                 .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.VehicleModel != null ? src.VehicleModel.ModelName : string.Empty))
                 .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.DecalTemplate != null ? src.DecalTemplate.TemplateName : string.Empty));

           
            // HUYNH ĐÃ SỬA LẠI ÁNH XẠ CHO DESIGN ĐỂ BỎ ORDERSTATUS
            CreateMap<Design, DesignDto>()
                .ForMember(dest => dest.DesignerFullName, opt => opt.MapFrom(src => src.Designer != null ? src.Designer.FirstName + " " + src.Designer.LastName : null));

            // UPDATED: Order mapping without Customer relationship
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.AssignedEmployeeFullName, opt => opt.MapFrom(src => src.AssignedEmployee != null ? src.AssignedEmployee.FirstName + " " + src.AssignedEmployee.LastName : null))
                .ForMember(dest => dest.ChassisNumber, opt => opt.MapFrom(src => src.CustomerVehicle != null ? src.CustomerVehicle.ChassisNumber : null))
                .ForMember(dest => dest.VehicleModelName, opt => opt.MapFrom(src => src.CustomerVehicle != null && src.CustomerVehicle.VehicleModel != null ? src.CustomerVehicle.VehicleModel.ModelName : null))
                .ForMember(dest => dest.VehicleBrandName, opt => opt.MapFrom(src => src.CustomerVehicle != null && src.CustomerVehicle.VehicleModel != null && src.CustomerVehicle.VehicleModel.VehicleBrand != null ? src.CustomerVehicle.VehicleModel.VehicleBrand.BrandName : null))
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

            // Mapping cho Create và Update DTOs
            CreateMap<CreateOrderStageHistoryDto, OrderStageHistory>();
            CreateMap<UpdateOrderStageHistoryDto, OrderStageHistory>();
           
            CreateMap<DesignComment, DesignCommentDto>()
                .ForMember(dest => dest.SenderUsername, opt => opt.MapFrom(src => src.SenderAccount != null ? src.SenderAccount.Username : null))
                .ForMember(dest => dest.SenderRoleName, opt => opt.MapFrom(src => src.SenderAccount != null && src.SenderAccount.Role != null ? src.SenderAccount.Role.RoleName : null));
           

            
            
            CreateMap<RegisterDto, Account>()
                .ForMember(dest => dest.AccountID, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password));


            CreateMap<Deposit, DepositDto>();
            CreateMap<DesignWorkOrder, DesignWorkOrderDto>();
            CreateMap<TechLaborPrice, TechLaborPriceDto>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.DecalService != null ? src.DecalService.ServiceName : string.Empty))
                .ForMember(dest => dest.VehicleModelName, opt => opt.MapFrom(src => src.VehicleModel != null ? src.VehicleModel.ModelName : string.Empty));
          

            // Thêm các ánh xạ cho DTO chi tiết
            CreateMap<AdminDetail, AdminDetailDto>();
            CreateMap<ManagerDetail, ManagerDetailDto>();
            CreateMap<SalesPersonDetail, SalesPersonDetailDto>();
            CreateMap<DesignerDetail, DesignerDetailDto>();
            CreateMap<TechnicianDetail, TechnicianDetailDto>();

            
        }
    }
}