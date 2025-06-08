using AutoMapper;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using System; // Để sử dụng DateTime và TimeSpan cho các ánh xạ
using System.Linq; // Để sử dụng .FirstOrDefault() trong MapFrom (đã có)

namespace DecalXeAPI.MappingProfiles
{
    public class MainMappingProfile : Profile
    {
        public MainMappingProfile()
        {
            // Ánh xạ cho Role (Dòng này đã được thêm vào và là điểm thiếu sót trước đó)
            CreateMap<Role, RoleDto>(); // Ánh xạ đơn giản từ Role Model sang RoleDto

            // Ánh xạ cho Account
            CreateMap<Account, AccountDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : string.Empty));

            // Ánh xạ cho Employee
            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store != null ? src.Store.StoreName : string.Empty))
                .ForMember(dest => dest.AccountUsername, opt => opt.MapFrom(src => src.Account != null ? src.Account.Username : null))
                .ForMember(dest => dest.AccountRoleName, opt => opt.MapFrom(src => src.Account != null && src.Account.Role != null ? src.Account.Role.RoleName : null));

            // Ánh xạ cho Customer
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

            // Ánh xạ cho DecalService
            CreateMap<DecalService, DecalServiceDto>()
                .ForMember(dest => dest.DecalTypeName, opt => opt.MapFrom(src => src.DecalType != null ? src.DecalType.DecalTypeName : string.Empty));

            // Ánh xạ cho ServiceProduct
            CreateMap<ServiceProduct, ServiceProductDto>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.DecalService != null ? src.DecalService.ServiceName : string.Empty))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : string.Empty));

            // Ánh xạ cho DecalTemplate
            CreateMap<DecalTemplate, DecalTemplateDto>()
                .ForMember(dest => dest.DecalTypeName, opt => opt.MapFrom(src => src.DecalType != null ? src.DecalType.DecalTypeName : string.Empty));

            // Ánh xạ cho ServiceDecalTemplate
            CreateMap<ServiceDecalTemplate, ServiceDecalTemplateDto>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.DecalService != null ? src.DecalService.ServiceName : string.Empty))
                .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.DecalTemplate != null ? src.DecalTemplate.TemplateName : string.Empty));

            // Ánh xạ cho CustomServiceRequest
            CreateMap<CustomServiceRequest, CustomServiceRequestDto>()
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FirstName + " " + src.Customer.LastName : string.Empty))
                .ForMember(dest => dest.SalesEmployeeFullName, opt => opt.MapFrom(src => src.SalesEmployee != null ? src.SalesEmployee.FirstName + " " + src.SalesEmployee.LastName : null))
                .ForMember(dest => dest.OrderID, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderID : null)) // Thêm ánh xạ OrderID và Description của Order
                .ForMember(dest => dest.CustomServiceRequestDescription, opt => opt.MapFrom(src => src.Description)); // Lấy trực tiếp từ Description của CustomServiceRequest


            // Ánh xạ cho Design
            CreateMap<Design, DesignDto>()
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderStatus : null))
                .ForMember(dest => dest.DesignerFullName, opt => opt.MapFrom(src => src.Designer != null ? src.Designer.FirstName + " " + src.Designer.LastName : null));

            // Ánh xạ cho Order
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FirstName + " " + src.Customer.LastName : string.Empty))
                .ForMember(dest => dest.AssignedEmployeeFullName, opt => opt.MapFrom(src => src.AssignedEmployee != null ? src.AssignedEmployee.FirstName + " " + src.AssignedEmployee.LastName : null))
                .ForMember(dest => dest.CustomServiceRequestID, opt => opt.MapFrom(src => src.CustomServiceRequest != null ? src.CustomServiceRequest.CustomRequestID : null))
                .ForMember(dest => dest.CustomServiceRequestDescription, opt => opt.MapFrom(src => src.CustomServiceRequest != null ? src.CustomServiceRequest.Description : null));


            // Ánh xạ cho OrderDetail
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

        }
    }
}