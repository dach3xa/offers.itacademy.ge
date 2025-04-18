﻿using FluentAssertions.Execution;
using FluentAssertions;
using offers.Application.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using offers.Application.Models.DTO;
using offers.Api.Tests.Tests.helper;
using offers.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace offers.Api.Tests.Tests
{
    public class GuestControllerTests : IClassFixture<OffersApiWebApplicationFactory>
    {
        private readonly HttpClient httpClient;
        private readonly string _baseRequestUrl;
        private readonly JsonSerializerOptions _jsonSerializerOption = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        private readonly OffersApiWebApplicationFactory _factory;
        public GuestControllerTests(OffersApiWebApplicationFactory factory)
        {
            httpClient = factory.CreateClient();
            _baseRequestUrl = "api/v1/Guest";
            _factory = factory; 
        }

        [Fact]
        public async Task GetAllCategories_ShouldReturnCategories()
        {
            var categoryResponses = await httpClient.GetAsync($"{_baseRequestUrl}/categories");

            var categoryBody = await categoryResponses.Content.ReadAsStringAsync();
            var createdCategories = JsonSerializer.Deserialize<List<CategoryResponseModel>>(categoryBody, _jsonSerializerOption);

            using (new AssertionScope())
            {
                categoryResponses.StatusCode.Should().Be(HttpStatusCode.OK);
                createdCategories.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task GetCategory_ShouldReturnCategory()
        {
            var loginResponse = await TestHelper.LoginAsAdminAsync(httpClient);
            var name = "Name_" + Guid.NewGuid().ToString("N");
            var model = new CategoryDTO
            {
                Name = name,
                Description = "PostingATestCategory"
            };

            var json = JsonSerializer.Serialize(model);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);
            var postResponse = await httpClient.PostAsync($"api/v1/Admin/category", data);
            var responseContent = await postResponse.Content.ReadAsStringAsync();

            var category = JsonSerializer.Deserialize<CategoryResponseModel>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );


            var categoryResponse = await httpClient.GetAsync($"{_baseRequestUrl}/categories/{category.Id}");

            var categoryBody = await categoryResponse.Content.ReadAsStringAsync();
            var createdCategory = JsonSerializer.Deserialize<CategoryResponseModel>(categoryBody, _jsonSerializerOption);

            using (new AssertionScope())
            {
                categoryResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                createdCategory.Id.Should().Be(category.Id);
            }
        }

        [Fact]
        public async Task GetAllOffers_ShouldReturnOffers()
        {
            var offerResponses = await httpClient.GetAsync($"{_baseRequestUrl}/offers");

            var offerBody = await offerResponses.Content.ReadAsStringAsync();
            var createdOffers = JsonSerializer.Deserialize<List<OfferResponseModel>>(offerBody, _jsonSerializerOption);

            using (new AssertionScope())
            {
                offerResponses.StatusCode.Should().Be(HttpStatusCode.OK);
                createdOffers.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task GetOffer_ShouldReturnOffer()
        {
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var loginResponse = await TestHelper.LogInAsCompany(httpClient, serviceProvider);

            var form = new MultipartFormDataContent
            {
                { new StringContent("Test Offer"), "Name" },
                { new StringContent("This is a test offer."), "Description" },
                { new StringContent("5"), "Count" },
                { new StringContent("199.99"), "Price" },
                { new StringContent("1"), "CategoryId" },
                { new StringContent(DateTime.UtcNow.AddDays(7).ToString("o")), "ArchiveAt" }
            };
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var offerResponse = await httpClient.PostAsync($"api/v1/Company/Offers", form);
            var offerBody = await offerResponse.Content.ReadAsStringAsync();
            offerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdOffer = JsonSerializer.Deserialize<OfferResponseModel>(offerBody, _jsonSerializerOption);

            var offerGetResponse = await httpClient.GetAsync($"{_baseRequestUrl}/offers/{createdOffer.Id}");
            var offerGetBody = await offerGetResponse.Content.ReadAsStringAsync();
            var OfferGetResponseModel = JsonSerializer.Deserialize<OfferResponseModel>(offerGetBody, _jsonSerializerOption);

            using (new AssertionScope())
            {
                offerGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                OfferGetResponseModel.Id.Should().Be(createdOffer.Id);
            }
        }
    }
}
