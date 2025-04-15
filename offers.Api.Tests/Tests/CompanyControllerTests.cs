using FluentAssertions.Execution;
using FluentAssertions;
using offers.Application.Models.DTO;
using offers.Application.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Azure;
using offers.Api.Tests.Tests.helper;

namespace offers.Api.Tests.Tests
{
    public class CompanyControllerTests : IClassFixture<OffersApiWebApplicationFactory>
    {
        private readonly HttpClient httpClient;
        private readonly string _baseRequestUrl;
        private readonly JsonSerializerOptions _jsonSerializerOption = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        public CompanyControllerTests(OffersApiWebApplicationFactory factory)
        {
            httpClient = factory.CreateClient();
            _baseRequestUrl = "api/v1/Company";
        }

        [Fact]
        public async Task OfferPost_ShouldReturnCreatedAtAction()
        {
            var loginResponse = await TestHelper.LogInAsCompany(httpClient);
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var form = new MultipartFormDataContent
            {
                { new StringContent("Test Offer"), "Name" },
                { new StringContent("This is a test offer."), "Description" },
                { new StringContent("5"), "Count" },
                { new StringContent("199.99"), "Price" },
                { new StringContent("1"), "CategoryId" },
                { new StringContent(DateTime.UtcNow.AddDays(7).ToString("o")), "ArchiveAt" }
            };

            var offerResponse = await httpClient.PostAsync($"{_baseRequestUrl}/offers", form);
            offerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var offerBody = await offerResponse.Content.ReadAsStringAsync();
            var createdOffer = JsonSerializer.Deserialize<OfferResponseModel>(offerBody, _jsonSerializerOption);

            using (new AssertionScope())
            {
                createdOffer.Should().NotBeNull();
                createdOffer!.Name.Should().Be("Test Offer");
                createdOffer.Price.Should().Be(199.99m);
                createdOffer.Count.Should().Be(5);
            }
        }

        [Fact]
        public async Task GetMyOffers_ShouldReturnMyOffers()
        {
            var loginResponse = await TestHelper.LogInAsCompany(httpClient);
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var offerResponses = await httpClient.GetAsync($"{_baseRequestUrl}/Offers");

            var offerBody = await offerResponses.Content.ReadAsStringAsync();
            var createdOffers = JsonSerializer.Deserialize<List<OfferResponseModel>>(offerBody, _jsonSerializerOption);

            using (new AssertionScope())
            {
                offerResponses.StatusCode.Should().Be(HttpStatusCode.OK);
                createdOffers.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task GetMyOffer_ShouldReturnMyOffer()
        {
            var loginResponse = await TestHelper.LogInAsCompany(httpClient);
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var form = new MultipartFormDataContent
            {
                { new StringContent("Test Offer"), "Name" },
                { new StringContent("This is a test offer."), "Description" },
                { new StringContent("5"), "Count" },
                { new StringContent("199.99"), "Price" },
                { new StringContent("1"), "CategoryId" },
                { new StringContent(DateTime.UtcNow.AddDays(7).ToString("o")), "ArchiveAt" }
            };

            var offerResponse = await httpClient.PostAsync($"{_baseRequestUrl}/Offers", form);
            var offerBody = await offerResponse.Content.ReadAsStringAsync();
            var createdOffer = JsonSerializer.Deserialize<OfferResponseModel>(offerBody, _jsonSerializerOption);


            var offerGetResponse = await httpClient.GetAsync($"{_baseRequestUrl}/Offers/{createdOffer.Id}");

            var offerGetBody = await offerGetResponse.Content.ReadAsStringAsync();
            var offer = JsonSerializer.Deserialize<OfferResponseModel>(offerGetBody, _jsonSerializerOption);

            using (new AssertionScope())
            {
                offerGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                offer.Id.Should().Be(createdOffer.Id);
            }
        }

        [Fact]
        public async Task DeleteOffer_ShouldDeleteTheOffer()
        {
            var loginResponse = await TestHelper.LogInAsCompany(httpClient);
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var form = new MultipartFormDataContent
            {
                { new StringContent("Test Offer"), "Name" },
                { new StringContent("This is a test offer."), "Description" },
                { new StringContent("5"), "Count" },
                { new StringContent("199.99"), "Price" },
                { new StringContent("1"), "CategoryId" },
                { new StringContent(DateTime.UtcNow.AddDays(7).ToString("o")), "ArchiveAt" }
            };

            var offerResponse = await httpClient.PostAsync($"{_baseRequestUrl}/Offers", form);
            var offerBody = await offerResponse.Content.ReadAsStringAsync();
            var createdOffer = JsonSerializer.Deserialize<OfferResponseModel>(offerBody, _jsonSerializerOption);

            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);
            var deleteResponse = await httpClient.DeleteAsync($"{_baseRequestUrl}/offers/{createdOffer.Id}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await httpClient.GetAsync($"{_baseRequestUrl}/Offers/{createdOffer.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ChangeCompanyPicture_ShouldUpdateSuccessfully()
        {
            var loginResponse = await TestHelper.LogInAsCompany(httpClient);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var imageBytes = Encoding.UTF8.GetBytes("fake-image-content");
            var fileContent = new ByteArrayContent(imageBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            var form = new MultipartFormDataContent
            {
                { fileContent, "Photo", "testpic.jpg" }
            };

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResponse.Token);
            var response = await httpClient.PatchAsync($"{_baseRequestUrl}/change-picture", form);
            var responseContent = await response.Content.ReadAsStringAsync();
            using (new AssertionScope())
            {
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task ChangeOfferPicture_ShouldUpdateSuccessfully()
        {
            var loginResponse = await TestHelper.LogInAsCompany(httpClient);
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var form = new MultipartFormDataContent
            {
                { new StringContent("Test Offer"), "Name" },
                { new StringContent("This is a test offer."), "Description" },
                { new StringContent("5"), "Count" },
                { new StringContent("199.99"), "Price" },
                { new StringContent("1"), "CategoryId" },
                { new StringContent(DateTime.UtcNow.AddDays(7).ToString("o")), "ArchiveAt" }
            };

            var offerResponse = await httpClient.PostAsync($"{_baseRequestUrl}/Offers", form);
            var offerBody = await offerResponse.Content.ReadAsStringAsync();
            var createdOffer = JsonSerializer.Deserialize<OfferResponseModel>(offerBody, _jsonSerializerOption);

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var imageBytes = Encoding.UTF8.GetBytes("fake-image-content");
            var fileContent = new ByteArrayContent(imageBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            var formPhoto = new MultipartFormDataContent
            {
                { fileContent, "Photo", "testpic.jpg" }
            };

            var response = await httpClient.PatchAsync($"{_baseRequestUrl}/offers/{createdOffer.Id}/change-picture", formPhoto);

            using (new AssertionScope())
            {
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task GetCurrentCompany_ShouldReturnTheCurrentCompany()
        {
            var loginResponse = await TestHelper.LogInAsCompany(httpClient);
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var companyResponse = await httpClient.GetAsync($"{_baseRequestUrl}");
            var companyBody = await companyResponse.Content.ReadAsStringAsync();
            var companyResponseModel = JsonSerializer.Deserialize<CompanyResponseModel>(companyBody, _jsonSerializerOption);

            using (new AssertionScope())
            {
                companyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                companyResponseModel.Id.Should().Be(loginResponse.Id);
            }
        }

    }
}
