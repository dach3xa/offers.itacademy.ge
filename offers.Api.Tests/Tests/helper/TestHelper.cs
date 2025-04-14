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
            var formData = new Dictionary<string, string>
            {
                { "Email", email },
                { "Password", "Testpass1!" },
                { "CompanyName", "Test Corp" },
                { "PhoneNumber", "599111111" }
            };

            var registerContent = new FormUrlEncodedContent(formData);
            var registerResponse = await httpClient.PostAsync("api/v1/Auth/company/register", registerContent);
            registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var loginDto = new AccountLoginDTO
            {
                Email = email,
                Password = "Testpass1!"
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
                Password = "Testpass1!",
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
                Password = "Testpass1!"
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
    }
}
