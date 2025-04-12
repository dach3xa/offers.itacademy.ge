using Azure;
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

        public async Task CategoryPost_ShouldReturnCreatedAtAction()
        {
            var model = new CategoryDTO
            {
                Name = "TestCategory",
                Description = "PostingATestCategory"
            };

            var json = JsonSerializer.Serialize(model);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var getResponse = await httpClient.PostAsync($"{_baseRequestUrl}/category", data);
            var responseContent = await getResponse.Content.ReadAsStringAsync();

            var category = JsonSerializer.Deserialize<CategoryResponseModel>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            //assert
            using (new AssertionScope())
            {
                getResponse.StatusCode.Should().Be(HttpStatusCode.Created);
                category.Name.Should().Be("TestCategory");
            }
        }

        public async Task GetAllUsers_ShouldReturnUsers()
        {
            var getResponse = await httpClient.GetAsync($"{_baseRequestUrl}/users").ConfigureAwait(true);
            var content = await getResponse.Content.ReadAsStringAsync().ConfigureAwait(true);
            var result = JsonSerializer.Deserialize<AccountResponseModel>(content, _jsonSerializerOption);

            //assert
            using (new AssertionScope())
            {
                getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                result!.Id.Should().Be(1);
            }
        }

    }
}
