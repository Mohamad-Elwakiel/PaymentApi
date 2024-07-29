using AutoMapper;
using PaymentApi.Models;

namespace PaymentApi.DTOs
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<PaymentDetail,PaymentDetailDto>().ReverseMap();
            CreateMap<SignUpModel, SignUpModelDto>().ReverseMap();
            CreateMap<LoginModel, LoginModelDto>().ReverseMap();
            CreateMap<PaymentDetail, StoredProcedureModel>().ReverseMap();
        }
    }
}
