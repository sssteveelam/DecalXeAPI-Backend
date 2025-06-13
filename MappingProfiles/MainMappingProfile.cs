using AutoMapper;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using System; // Để sử dụng DateTime và TimeSpan cho các ánh xạ
using System.Linq; // Để sử dụng Linq trong MapFrom

namespace DecalXeAPI.MappingProfiles
{
    public class MainMappingProfile : Profile
    {
        public MainMappingProfile()
        {
            // --- ÁNH XẠ CÁC BẢNG CŨ (ĐÃ CÓ TỪ TRƯỚC) ---

            // Ánh xạ cho Role
            CreateMap<Role, RoleDto>();

            // Ánh xạ cho Account
            CreateMap<Account, AccountDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : string.Empty));

            // Ánh xạ cho Employee (Cập nhật để ánh xạ các trường mới)
            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store != null ? src.Store.StoreName : string.Empty))
                .ForMember(dest => dest.AccountUsername, opt => opt.MapFrom(src => src.Account != null ? src.Account.Username : null))
                .ForMember(dest => dest.AccountRoleName, opt => opt.MapFrom(src => src.Account != null && src.Account.Role != null ? src.Account.Role.RoleName : null));

            // Ánh xạ cho Customer (Cập nhật để ánh xạ các trường mới)
            CreateMap<Customer, CustomerDto>()
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
                .ForMember(dest => dest.AccountUsername, opt => opt.MapFrom(src => src.Account != null ? src.Account.Username : null))
                .ForMember(dest => dest.AccountRoleName, opt => opt.MapFrom(src => src.Account != null && src.Account.Role != null ? src.Account.Role.RoleName : null));

            // Ánh xạ cho Store
            CreateMap<Store, StoreDto>();

            // Ánh xạ cho DecalType
            CreateMap<DecalType, DecalTypeDto>();

            // Ánh xạ cho Product
            CreateMap<Product, ProductDto>();

            // Ánh xạ cho TimeSlotDefinition
            CreateMap<TimeSlotDefinition, TimeSlotDefinitionDto>();

            // Ánh xạ cho Promotion
            CreateMap<Promotion, PromotionDto>();

            // Ánh xạ cho DecalService (Cập nhật để ánh xạ các trường mới)
            CreateMap<DecalService, DecalServiceDto>()
                .ForMember(dest => dest.DecalTypeName, opt => opt.MapFrom(src => src.DecalType != null ? src.DecalType.DecalTypeName : string.Empty))
                .ForMember(dest => dest.PrintingPriceDetailID, opt => opt.MapFrom(src => src.PrintingPriceDetail != null ? src.PrintingPriceDetail.ServiceID : null)); // ID của chi tiết giá in

            // Ánh xạ cho ServiceProduct
            CreateMap<ServiceProduct, ServiceProductDto>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.DecalService != null ? src.DecalService.ServiceName : string.Empty))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : string.Empty));

            // Ánh xạ cho DecalTemplate (Cập nhật để ánh xạ các trường mới)
            CreateMap<DecalTemplate, DecalTemplateDto>()
                .ForMember(dest => dest.DecalTypeName, opt => opt.MapFrom(src => src.DecalType != null ? src.DecalType.DecalTypeName : string.Empty));

            // Ánh xạ cho ServiceDecalTemplate
            CreateMap<ServiceDecalTemplate, ServiceDecalTemplateDto>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.DecalService != null ? src.DecalService.ServiceName : string.Empty))
                .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.DecalTemplate != null ? src.DecalTemplate.TemplateName : string.Empty));

            // Ánh xạ cho CustomServiceRequest (Cập nhật để ánh xạ các trường mới)
            CreateMap<CustomServiceRequest, CustomServiceRequestDto>()
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FirstName + " " + src.Customer.LastName : string.Empty))
                .ForMember(dest => dest.SalesEmployeeFullName, opt => opt.MapFrom(src => src.SalesEmployee != null ? src.SalesEmployee.FirstName + " " + src.SalesEmployee.LastName : null))
                .ForMember(dest => dest.OrderID, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderID : null))
                .ForMember(dest => dest.CustomServiceRequestDescription, opt => opt.MapFrom(src => src.Description));

            // Ánh xạ cho Design (Cập nhật để ánh xạ các trường mới)
            CreateMap<Design, DesignDto>()
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderStatus : null))
                .ForMember(dest => dest.DesignerFullName, opt => opt.MapFrom(src => src.Designer != null ? src.Designer.FirstName + " " + src.Designer.LastName : null));

            // Ánh xạ cho Order (Cập nhật để ánh xạ các trường mới)
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FirstName + " " + src.Customer.LastName : string.Empty))
                .ForMember(dest => dest.AssignedEmployeeFullName, opt => opt.MapFrom(src => src.AssignedEmployee != null ? src.AssignedEmployee.FirstName + " " + src.AssignedEmployee.LastName : null))
                .ForMember(dest => dest.CustomServiceRequestID, opt => opt.MapFrom(src => src.CustomServiceRequest != null ? src.CustomServiceRequest.CustomRequestID : null))
                .ForMember(dest => dest.CustomServiceRequestDescription, opt => opt.MapFrom(src => src.CustomServiceRequest != null ? src.CustomServiceRequest.Description : null))
                .ForMember(dest => dest.LicensePlate, opt => opt.MapFrom(src => src.CustomerVehicle != null ? src.CustomerVehicle.LicensePlate : null))
                .ForMember(dest => dest.CarModelName, opt => opt.MapFrom(src => src.CustomerVehicle != null && src.CustomerVehicle.CarModel != null ? src.CustomerVehicle.CarModel.ModelName : null))
                .ForMember(dest => dest.CarBrandName, opt => opt.MapFrom(src => src.CustomerVehicle != null && src.CustomerVehicle.CarModel != null && src.CustomerVehicle.CarModel.CarBrand != null ? src.CustomerVehicle.CarModel.CarBrand.BrandName : null))
                .ForMember(dest => dest.IsCustomDecal, opt => opt.MapFrom(src => src.IsCustomDecal)) ; // <-- THÊM ÁNH XẠ NÀY

            // Ánh xạ cho OrderDetail (Cập nhật để ánh xạ các trường mới)
            CreateMap<OrderDetail, OrderDetailDto>()
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderStatus : null))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.DecalService != null ? src.DecalService.ServiceName : string.Empty));

            // Ánh xạ cho TechnicianDailySchedule
            CreateMap<TechnicianDailySchedule, TechnicianDailyScheduleDto>()
                .ForMember(dest => dest.EmployeeFullName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FirstName + " " + src.Employee.LastName : string.Empty));

            // Ánh xạ cho ScheduledWorkUnit
            CreateMap<ScheduledWorkUnit, ScheduledWorkUnitDto>()
                .ForMember(dest => dest.ScheduleDate, opt => opt.MapFrom(src => src.DailySchedule != null ? src.DailySchedule.ScheduleDate : DateTime.MinValue))
                .ForMember(dest => dest.SlotStartTime, opt => opt.MapFrom(src => src.TimeSlotDefinition != null ? src.TimeSlotDefinition.StartTime : TimeSpan.Zero))
                .ForMember(dest => dest.SlotEndTime, opt => opt.MapFrom(src => src.TimeSlotDefinition != null ? src.TimeSlotDefinition.EndTime : TimeSpan.Zero))
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderStatus : null));

            // Ánh xạ cho Payment
            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderStatus : null))
                .ForMember(dest => dest.PromotionName, opt => opt.MapFrom(src => src.Promotion != null ? src.Promotion.PromotionName : null));

            // Ánh xạ cho Feedback
            CreateMap<Feedback, FeedbackDto>()
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderStatus : null))
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FirstName + " " + src.Customer.LastName : string.Empty));

            // Ánh xạ cho Warranty
            CreateMap<Warranty, WarrantyDto>()
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderStatus : null));

            // --- ÁNH XẠ CÁC BẢNG MỚI TỪ YÊU CẦU REVIEW ---

            // Ánh xạ cho CarBrand
            CreateMap<CarBrand, CarBrandDto>();

            // Ánh xạ cho CarModel
            CreateMap<CarModel, CarModelDto>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.CarBrand != null ? src.CarBrand.BrandName : string.Empty));

            // Ánh xạ cho CustomerVehicle
CreateMap<CustomerVehicle, CustomerVehicleDto>()
    .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FirstName + " " + src.Customer.LastName : string.Empty))
    .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.CarModel != null ? src.CarModel.ModelName : string.Empty))
    .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.CarModel != null && src.CarModel.CarBrand != null ? src.CarModel.CarBrand.BrandName : string.Empty)); // <-- SỬA DÒNG NÀY
            // Ánh xạ cho OrderStageHistory
            CreateMap<OrderStageHistory, OrderStageHistoryDto>()
                .ForMember(dest => dest.ChangedByEmployeeFullName, opt => opt.MapFrom(src => src.ChangedByEmployee != null ? src.ChangedByEmployee.FirstName + " " + src.ChangedByEmployee.LastName : null));

            // Ánh xạ cho PrintingPriceDetail
            CreateMap<PrintingPriceDetail, PrintingPriceDetailDto>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.DecalService != null ? src.DecalService.ServiceName : string.Empty));

            // Ánh xạ cho DesignComment
            CreateMap<DesignComment, DesignCommentDto>()
                .ForMember(dest => dest.SenderUsername, opt => opt.MapFrom(src => src.SenderAccount != null ? src.SenderAccount.Username : null))
                .ForMember(dest => dest.SenderRoleName, opt => opt.MapFrom(src => src.SenderAccount != null && src.SenderAccount.Role != null ? src.SenderAccount.Role.RoleName : null));

            // Ánh xạ cho OrderCompletionImage
            CreateMap<OrderCompletionImage, OrderCompletionImageDto>()
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderStatus : string.Empty));

            // Ánh xạ cho CarModelDecalTemplate
            CreateMap<CarModelDecalTemplate, CarModelDecalTemplateDto>()
                .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.CarModel != null ? src.CarModel.ModelName : string.Empty))
                .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.DecalTemplate != null ? src.DecalTemplate.TemplateName : string.Empty));


            // Ánh xạ DTO đầu vào cho CustomServiceRequest
            CreateMap<CreateCustomServiceRequestDto, CustomServiceRequest>()
                .ForMember(dest => dest.CustomRequestID, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.RequestDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(src => "New"))
                .ForMember(dest => dest.OrderID, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.SalesEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore());

            // Ánh xạ DTO đầu vào cho ConvertCsrToOrderDto
            CreateMap<ConvertCsrToOrderDto, Order>()
                .ForMember(dest => dest.OrderID, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.EstimatedCost))
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => "New"))
                .ForMember(dest => dest.CustomerID, opt => opt.Ignore())
                .ForMember(dest => dest.CustomServiceRequest, opt => opt.Ignore())
                .ForMember(dest => dest.IsCustomDecal, opt => opt.MapFrom(src => src.IsCustomDecal));

            // Ánh xạ DTO đầu vào cho RegisterDto
            CreateMap<RegisterDto, Account>()
                .ForMember(dest => dest.AccountID, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password));
        }
    }
}