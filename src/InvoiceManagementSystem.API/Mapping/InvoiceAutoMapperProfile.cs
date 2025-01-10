using AutoMapper;
using InvoiceManagementSystem.API.DTO;
using InvoiceManagementSystem.Data.Entity;
using InvoiceManagementSystem.Service.BusinessDomain;

namespace InvoiceManagementSystem.API.Mapping
{
    public class InvoiceAutoMapperProfile : Profile
    {
        public InvoiceAutoMapperProfile()
        {
            CreateMap<InvoiceDomain, InvoiceDto>().ReverseMap().ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<InvoiceDomain, InvoiceEntity>().ReverseMap().ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
