using Mapster;
using offers.API.Models;
using offers.Application.Models;
using offers.Domain.Enums;
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
                .Map(dest => dest.Role, src => AccountRole.Admin)
                .Map(dest => dest.PasswordHash, src => src.Password)
                .Map(dest => dest.CompanyDetail, src => new CompanyDetail()) 
                .AfterMapping((src, dest) =>
                {
                    dest.CompanyDetail.CompanyName = src.CompanyName;
                    dest.CompanyDetail.IsActive = false;
                });


            TypeAdapterConfig<UserRegisterDTO, Account>
                .NewConfig()
                .Map(dest => dest.Id, src => 0)
                .Map(dest => dest.Role, src => AccountRole.User)
                .Map(dest => dest.PasswordHash, src => src.Password)
                .Map(dest => dest.UserDetail, src => new UserDetail())
                .AfterMapping((src, dest) =>
                {
                    dest.UserDetail.FirstName = src.FirstName;
                    dest.UserDetail.LastName = src.LastName;
                    dest.UserDetail.Balance = 0;
                });

            TypeAdapterConfig<Account, AccountResponseModel>
                .NewConfig();

            TypeAdapterConfig<CategoryDTO, Category>
                .NewConfig();

            TypeAdapterConfig<OfferDTO, Offer>
                .NewConfig()
                .Map(dest => dest.Id, src => 0)
                .Map(dest => dest.CreatedAt, src => DateTime.Now)
                .Map(dest => dest.IsArchived, src => false);
        }
    }
}
