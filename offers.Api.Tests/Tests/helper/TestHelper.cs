using offers.Application.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using System.Net.Http.Headers;
using offers.Domain.Enums;
using offers.Domain.Models;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using offers.Application.RepositoryInterfaces;
using offers.Application.Models.Response;

namespace offers.Api.Tests.Tests.helper
{
    public static class TestHelper
    {
        private static readonly JsonSerializerOptions _jsonSerializerOption = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        private static async Task SeedCompany(HttpClient httpClient, IServiceProvider serviceProvider, string email)
        {
            using var scope = serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();

            var result = await repository.GetAsync(email, CancellationToken.None);

            if (result == null)
            {
                var formData = new MultipartFormDataContent
                {
                    { new StringContent(email), "Email" },
                    { new StringContent("Testpass1."), "Password" },
                    { new StringContent("Test Corp"), "CompanyName" },
                    { new StringContent("599111111"), "PhoneNumber" },
                };

                var registerResponse = await httpClient.PostAsync("api/v1/Auth/company/register", formData);
                registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
                var registerJson = await registerResponse.Content.ReadAsStringAsync();
                var registerResponseDto = JsonSerializer.Deserialize<CompanyResponseModel>(registerJson, _jsonSerializerOption);
                var adminLogin = await LoginAsAdminAsync(httpClient);
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminLogin.Token);

                var patchResponse = await httpClient.PatchAsync($"api/v1/Admin/companies/{registerResponseDto.Id}/confirm", null);
                patchResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        public static async Task<LoginResponseDTO> LogInAsCompany(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            string email = "CompanyExample@gmail.com";
            await SeedCompany(httpClient, serviceProvider, email);

            var loginDto = new AccountLoginDTO
            {
                Email = email,
                Password = "Testpass1."
            };

            var loginJson = JsonSerializer.Serialize(loginDto);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
            var loginResponse = await httpClient.PostAsync("api/v1/Auth/login", loginContent);
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginBody = await loginResponse.Content.ReadAsStringAsync();
            var loginData = JsonSerializer.Deserialize<LoginResponseDTO>(loginBody, _jsonSerializerOption);

            return loginData;
        }

        private static async Task SeedUser(HttpClient httpClient, IServiceProvider serviceProvider, string email)
        {
            using var scope = serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();

            var result = await repository.GetAsync(email, CancellationToken.None);

            if( result == null)
            {
                var userRegister = new UserRegisterDTO
                {
                    Email = email,
                    Password = "Testpass1.",
                    FirstName = "Tes",
                    LastName = "test",
                    PhoneNumber = "599111111"
                };

                var RegisterJson = JsonSerializer.Serialize(userRegister);
                var RegisterContent = new StringContent(RegisterJson, Encoding.UTF8, "application/json");
                var registerResponse = await httpClient.PostAsync("api/v1/Auth/user/register", RegisterContent);
                registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            }

        }
        private static async Task Deposit(HttpClient httpClient, IServiceProvider serviceProvider, LoginResponseDTO loginResponse)
        {
            if (serviceProvider == null)
            {
                throw new Exception("Service provider has to be registered for this method to work");
            }

            using var scope = serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();

            var result = await repository.GetAsync(loginResponse.Email, CancellationToken.None);
            if(result.UserDetail.Balance == 0)
            {
                var deposit = new DepositRequestDTO
                {
                    Amount = 2000
                };
                var DepositJson = JsonSerializer.Serialize(deposit);
                var DepositContent = new StringContent(DepositJson, Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", loginResponse.Token);
                await httpClient.PatchAsync("api/v1/User/deposit", DepositContent);
            }
        }

        public static async Task<LoginResponseDTO> LogInAsUserAsync(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            var email = "UserExample@gmail.com";
            await SeedUser(httpClient, serviceProvider, email);
            var loginDto = new AccountLoginDTO
            {
                Email = email,
                Password = "Testpass1."
            };
            var loginJson = JsonSerializer.Serialize(loginDto);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
            var loginResponse = await httpClient.PostAsync("api/v1/Auth/login", loginContent);
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginBody = await loginResponse.Content.ReadAsStringAsync();
            var loginData = JsonSerializer.Deserialize<LoginResponseDTO>(loginBody, _jsonSerializerOption);
            await Deposit(httpClient, serviceProvider, loginData);

            return loginData;
        }

        public static async Task<LoginResponseDTO> LoginAsAdminAsync(HttpClient httpClient)
        {
            string email = "randomuser@example.com";
            string password = "Dachidachi1.";
            var loginDto = new AccountLoginDTO
            {
                Email = email,
                Password = password
            };
            var loginJson = JsonSerializer.Serialize(loginDto);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
 
            var loginResponse = await httpClient.PostAsync("api/v1/Auth/login", loginContent);

            var loginBody = await loginResponse.Content.ReadAsStringAsync();
            var loginData = JsonSerializer.Deserialize<LoginResponseDTO>(loginBody, _jsonSerializerOption);
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            return loginData;
        }
    }
}
