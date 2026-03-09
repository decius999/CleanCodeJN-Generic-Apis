using AutoMapper;
using CleanCodeJN.GenericApis.Sample.Core.Dtos;
using CleanCodeJN.GenericApis.Sample.Domain;

namespace CleanCodeJN.GenericApis.Sample.Extensions;

public static class AutomapperExtensions
{
    public static Action<IMapperConfigurationExpression> Mapping() => cfg =>
    {
        cfg.CreateMap<CustomerPostDto, Customer>()
            .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Name))
            .ForPath(x => x.AddressInfo.Street, opt => opt.MapFrom(x => x.Street))
            .ForPath(x => x.AddressInfo.HouseNo, opt => opt.MapFrom(x => x.HouseNo))
            .ForPath(x => x.AddressInfo.Zip, opt => opt.MapFrom(x => x.Zip))
            .ForPath(x => x.AddressInfo.City, opt => opt.MapFrom(x => x.City))
            .ReverseMap();

        cfg.CreateMap<CustomerPutDto, Customer>()
           .ForPath(x => x.AddressInfo.CustomerId, opt => opt.MapFrom(x => x.Id))
           .ForPath(x => x.AddressInfo.Street, opt => opt.MapFrom(x => x.Street))
           .ForPath(x => x.AddressInfo.HouseNo, opt => opt.MapFrom(x => x.HouseNo))
           .ForPath(x => x.AddressInfo.Zip, opt => opt.MapFrom(x => x.Zip))
           .ForPath(x => x.AddressInfo.City, opt => opt.MapFrom(x => x.City))
           .ReverseMap();
    };
}
