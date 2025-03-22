using Mapster;
using offers.API.Models.CompanyDTO;
using offers.API.Models.UserDTO;
using offers.Application.Models;
using offers.Domain.Models;
using System;
using System.Globalization;

namespace offers.API.Infrastructure.Mappings
{
    public static class MapsterConfiguration
    {
        public static void RegisterMaps(this IServiceCollection services)
        {

            TypeAdapterConfig<CompanyRegisterDTO, Account>
                .NewConfig()
                .Map(dest => dest.Id, src => 0)
                .Map(dest => dest.Role, src => "Company")
                .Map(dest => dest.PasswordHash, src => src.Password)
                .Map(dest => dest.CompanyDetail, src => new CompanyDetail()) 
                .AfterMapping((src, dest) =>
                {
                    dest.CompanyDetail.CompanyName = src.CompanyName;
                });


            TypeAdapterConfig<UserRegisterDTO, Account>
                .NewConfig()
                .Map(dest => dest.Id, src => 0)
                .Map(dest => dest.Role, src => "User")
                .Map(dest => dest.PasswordHash, src => src.Password)
                .Map(dest => dest.UserDetail, src => new UserDetail())
                .AfterMapping((src, dest) =>
                {
                    dest.UserDetail.FirstName = src.FirstName;
                    dest.UserDetail.LastName = src.LastName;
                });

            TypeAdapterConfig<Account, AccountResponseModel>
                .NewConfig();
        }
    }
}
