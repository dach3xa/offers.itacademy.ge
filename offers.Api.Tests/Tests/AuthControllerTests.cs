using FluentAssertions;
using FluentAssertions.Execution;
using offers.Application.Models.DTO;
using offers.Application.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace offers.Api.Tests.Tests
{
    public class AuthControllerTests : IClassFixture<OffersApiWebApplicationFactory>
    {
        private readonly HttpClient httpClient;
        private readonly string _baseRequestUrl;
        private readonly JsonSerializerOptions _jsonSerializerOption = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        public AuthControllerTests(OffersApiWebApplicationFactory factory)
        {
            httpClient = factory.CreateClient();
            _baseRequestUrl = "api/v1/Auth";
        }

        [Fact]
        public async Task UserRegister_ShouldReturnCreatedAtAction()
        {
            var email = $"testuser_{Guid.NewGuid()}@example.com";

            var user = new UserRegisterDTO
            {
                FirstName = "test",
                LastName = "testlast",
                PhoneNumber = "599858078",
                Password = "Testtest1.",
                Email = email,
            };
            var json = JsonSerializer.Serialize(user);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{_baseRequestUrl}/user/register", data);

            var registerBody = await response.Content.ReadAsStringAsync();
            var createdUser = JsonSerializer.Deserialize<UserResponseModel>(registerBody, _jsonSerializerOption);

            using (new AssertionScope())
            {
                response.StatusCode.Should().Be(HttpStatusCode.Created);
                createdUser!.Email.Should().Be(email);
            }
        }

        [Fact]
        public async Task CompanyRegister_ShouldReturnCreatedAtAction()
        {
            var email = $"testCompany_{Guid.NewGuid()}@example.com";

            var formData = new Dictionary<string, string>
            {
                { "Email", email },
                { "Password", "StrongPass123!" },
                { "CompanyName", "Test Corp" },
                { "PhoneNumber", "599858078" }
            };

            var content = new FormUrlEncodedContent(formData);

            var response = await httpClient.PostAsync($"{_baseRequestUrl}/company/register", content);

            var registerBody = await response.Content.ReadAsStringAsync();
            var createdUser = JsonSerializer.Deserialize<UserResponseModel>(registerBody, _jsonSerializerOption);

            using (new AssertionScope())
            {
                response.StatusCode.Should().Be(HttpStatusCode.Created);
                createdUser!.Email.Should().Be(email);
            }
        }

        [Fact]
        public async Task LogIn_ShouldReturnLoginResponse()
        {
            var email = $"testuser_{Guid.NewGuid()}@example.com";

            var user = new UserRegisterDTO
            {
                FirstName = "test",
                LastName = "testlast",
                PhoneNumber = "599858078",
                Password = "Testtest1.",
                Email = email,
            };
            var json = JsonSerializer.Serialize(user);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{_baseRequestUrl}/user/register", data);

            var registerBody = await response.Content.ReadAsStringAsync();
            var createdUser = JsonSerializer.Deserialize<UserResponseModel>(registerBody, _jsonSerializerOption);


            var login = new AccountLoginDTO
            {
                Password = "Testtest1.",
                Email = email
            };

            var jsonLogin = JsonSerializer.Serialize(login);
            var dataLogin = new StringContent(jsonLogin, Encoding.UTF8, "application/json");

            var loginResponse = await httpClient.PostAsync($"{_baseRequestUrl}/login", dataLogin);

            var loginResponseBody = await loginResponse.Content.ReadAsStringAsync();
            var loginResponseModel = JsonSerializer.Deserialize<LoginResponseDTO>(loginResponseBody, _jsonSerializerOption);

            using (new AssertionScope())
            {
                loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                loginResponseModel!.Email.Should().Be(email);
            }
        }
    }
}
