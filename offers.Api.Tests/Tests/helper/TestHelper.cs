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

namespace offers.Api.Tests.Tests.helper
{
    public static class TestHelper
    {
        private static readonly JsonSerializerOptions _jsonSerializerOption = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        public static async Task<LoginResponseDTO> LogInAsCompany(HttpClient httpClient)
        {
            var email = $"testcompany_{Guid.NewGuid()}@example.com";

            var formData = new MultipartFormDataContent
            {
                { new StringContent(email), "Email" },
                { new StringContent("Testpass1."), "Password" },
                { new StringContent("Test Corp"), "CompanyName" },
                { new StringContent("599111111"), "PhoneNumber" },
            };

            var registerResponse = await httpClient.PostAsync("api/v1/Auth/company/register", formData);
            registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

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

        public static async Task<LoginResponseDTO> LogInAsUserAsync(HttpClient httpClient)
        {
            var email = $"testcompany_{Guid.NewGuid()}@example.com";
            var userRegister = new UserRegisterDTO
            {
                Email =  email,
                Password = "Testpass1.",
                FirstName = "Tes",
                LastName = "test",
                PhoneNumber = "599111111"
            };

            var RegisterJson = JsonSerializer.Serialize(userRegister);
            var RegisterContent = new StringContent(RegisterJson, Encoding.UTF8, "application/json");
            var registerResponse = await httpClient.PostAsync("api/v1/Auth/user/register", RegisterContent);
            registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

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

            var deposit = new DepositRequestDTO
            {
                Amount = 2000
            };
            var DepositJson = JsonSerializer.Serialize(deposit);
            var DepositContent = new StringContent(DepositJson, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginData.Token);
            await httpClient.PatchAsync("api/v1/User/deposit", DepositContent);

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
