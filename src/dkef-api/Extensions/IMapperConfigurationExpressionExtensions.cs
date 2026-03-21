using AutoMapper;

using Dkef.Contracts.Mailgun;

using Dkef.Domain;

namespace Dkef.Extensions;

public static class IMapperConfigurationExpressionExtensions
{
    extension(IMapperConfigurationExpression source)
    {
        public void MapMailgunContracts()
        {
            source.CreateMap<InformationMessage, ContactInquiryDto>()
                .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.SenderEmail, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.SenderPhone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.ReceivedAt, opt => opt.MapFrom(src => DateTime.UtcNow.ToString("R")));
            source.CreateMap<ChangeEmailRequest, ChangeEmailDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ConfirmLink, opt => opt.MapFrom(src => src.ConfirmLink))
                .ForMember(dest => dest.ReceivedAt, opt => opt.MapFrom(src => DateTime.UtcNow.ToString("R")));
            source.CreateMap<ChangeEmailRequest, OldChangeEmailDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.NewEmail, opt => opt.MapFrom(src => src.NewEmail))
                .ForMember(dest => dest.RevokeLink, opt => opt.MapFrom(src => src.RevokeLink))
                .ForMember(dest => dest.ReceivedAt, opt => opt.MapFrom(src => DateTime.UtcNow.ToString("R")));
            source.CreateMap<ResetPasswordRequest, ResetPasswordDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ChangeLink, opt => opt.MapFrom(src => src.ChangeLink))
                .ForMember(dest => dest.ReceivedAt, opt => opt.MapFrom(src => DateTime.UtcNow.ToString("R")));
            source.CreateMap<NewMemberRegistered, NewMemberRegisteredDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.ReceivedAt, opt => opt.MapFrom(src => DateTime.UtcNow.ToString("R")));
        }
    }
}
