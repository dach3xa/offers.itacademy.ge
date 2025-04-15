using Azure;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using offers.Api.Tests.Tests.helper;
using offers.Application.Models.DTO;
using offers.Application.Models.Response;
using offers.Domain.Enums;
using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace offers.Api.Tests.Tests
{
    public class AdminControllerTests : IClassFixture<OffersApiWebApplicationFactory>
    {
        private readonly HttpClient httpClient;
        private readonly string _baseRequestUrl;
        private readonly JsonSerializerOptions _jsonSerializerOption = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        public AdminControllerTests(OffersApiWebApplicationFactory factory)
        {
            httpClient = factory.CreateClient();
            _baseRequestUrl = "api/v1/Admin";
        }

        [Fact]
        public async Task CategoryPost_ShouldReturnCreatedAtAction()
        {
            var name = $"TestCategory{Guid.NewGuid()}";
            var model = new CategoryDTO
            {
                Name = name,
                Description = "PostingATestCategory"
            };

            var json = JsonSerializer.Serialize(model);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var loginResponse = await TestHelper.LoginAsAdminAsync(httpClient);
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);
            var postResponse = await httpClient.PostAsync($"{_baseRequestUrl}/category", data);
            var responseContent = await postResponse.Content.ReadAsStringAsync();

            var category = JsonSerializer.Deserialize<CategoryResponseModel>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            //assert
            using (new AssertionScope())
            {
                postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
                postResponse.Headers.Location.Should().NotBeNull();
                category.Name.Should().Be(name);
            }
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnUsers()
        {
            var getResponse = await httpClient.GetAsync($"{_baseRequestUrl}/users").ConfigureAwait(true);
            var content = await getResponse.Content.ReadAsStringAsync().ConfigureAwait(true);
            var result = JsonSerializer.Deserialize<List<UserResponseModel>>(content, _jsonSerializerOption);

            //assert
            using (new AssertionScope())
            {
                getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                result.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task GetUser_ShouldReturnUser()
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

            var response = await httpClient.PostAsync($"api/v1/Auth/user/register", data);

            var registerBody = await response.Content.ReadAsStringAsync();
            var createdUser = JsonSerializer.Deserialize<UserResponseModel>(registerBody, _jsonSerializerOption);

            var getResponse = await httpClient.GetAsync($"{_baseRequestUrl}/users/{createdUser.Id}");
            var getresponseBody = await getResponse.Content.ReadAsStringAsync();
            var getUser = JsonSerializer.Deserialize<UserResponseModel>(getresponseBody, _jsonSerializerOption);
            using (new AssertionScope())
            {
                getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                getUser!.Id.Should().Be(createdUser.Id);
            }
        }

        [Fact]
        public async Task ConfirmCompany_ShouldConfirmTheCompany()
        {
            var email = $"testcompany_{Guid.NewGuid()}@example.com";

            var formData = new MultipartFormDataContent
            {
                { new StringContent(email), "Email" },
                { new StringContent("StrongPass123!"), "Password" },
                { new StringContent("Test Corp"), "CompanyName" },
                { new StringContent("599858078"), "PhoneNumber" }
            };

            var emptyFileContent = new ByteArrayContent(Array.Empty<byte>());
            emptyFileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            // ✅ Add file correctly
            formData.Add(emptyFileContent, "Photo", "empty.jpg");

            var response = await httpClient.PostAsync($"api/v1/Auth/company/register", formData);

            var registerBody = await response.Content.ReadAsStringAsync();
            var createdCompany = JsonSerializer.Deserialize<AccountResponseModel>(registerBody, _jsonSerializerOption);
            var loginResponse = await TestHelper.LoginAsAdminAsync(httpClient);
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);
            var confirmResponse = await httpClient.PatchAsync($"{_baseRequestUrl}/companies/{createdCompany.Id}/confirm", null);

            using (new AssertionScope())
            {
                confirmResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }

            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);
            var getResponse = await httpClient.GetAsync($"{_baseRequestUrl}/companies/{createdCompany.Id}");
            var companyContent = await getResponse.Content.ReadAsStringAsync();
            var company = JsonSerializer.Deserialize<CompanyResponseModel>(companyContent, _jsonSerializerOption);

            company.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetAllCompanies_ShouldReturnCompanies()
        {
            var getResponse = await httpClient.GetAsync($"{_baseRequestUrl}/companies").ConfigureAwait(true);
            var content = await getResponse.Content.ReadAsStringAsync().ConfigureAwait(true);
            var result = JsonSerializer.Deserialize<List<CompanyResponseModel>>(content, _jsonSerializerOption);

            //assert
            using (new AssertionScope())
            {
                getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                result.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task GetCompany_ShouldReturnCompany()
        {
            var email = $"testCompany_{Guid.NewGuid()}@example.com";

            var formData = new MultipartFormDataContent
            {
                { new StringContent(email), "Email" },
                { new StringContent("StrongPass123!"), "Password" },
                { new StringContent("Test Corp"), "CompanyName" },
                { new StringContent("599858078"), "PhoneNumber" }
            };

            var emptyFileContent = new ByteArrayContent(Array.Empty<byte>());
            emptyFileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var response = await httpClient.PostAsync($"api/v1/Auth/company/register", formData);

            var registerBody = await response.Content.ReadAsStringAsync();
            var createdCompany = JsonSerializer.Deserialize<CompanyResponseModel>(registerBody, _jsonSerializerOption);

            var getResponse = await httpClient.GetAsync($"{_baseRequestUrl}/companies/{createdCompany.Id}");
            var getresponseBody = await getResponse.Content.ReadAsStringAsync();
            var getCompany= JsonSerializer.Deserialize<CompanyResponseModel>(getresponseBody, _jsonSerializerOption);
            using (new AssertionScope())
            {
                getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                getCompany!.Id.Should().Be(createdCompany.Id);
            }
        }

    }
}
