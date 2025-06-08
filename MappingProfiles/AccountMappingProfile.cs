using AutoMapper;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;

namespace DecalXeAPI.MappingProfiles
{
    public class AccountMappingProfile : Profile // Kế thừa từ lớp Profile của AutoMapper
    {
        public AccountMappingProfile()
        {
            // Định nghĩa ánh xạ từ Account Model sang AccountDto
           CreateMap<Account, AccountDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : ""));
            // Giải thích:
            // CreateMap<Nguồn, Đích>
            // .ForMember(thuộc tính của Đích, tùy chọn ánh xạ)
            // dest => dest.RoleName: Thuộc tính RoleName của AccountDto (Đích)
            // opt => opt.MapFrom(src => src.Role.RoleName): Lấy giá trị từ thuộc tính Role.RoleName của Account Model (Nguồn)
        }
    }
}