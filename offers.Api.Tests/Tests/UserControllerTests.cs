using FluentAssertions.Execution;
using FluentAssertions;
using offers.Api.Tests.Tests.helper;
using offers.Application.Models.DTO;
using offers.Application.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace offers.Api.Tests.Tests
{
    public class UserControllerTests : IClassFixture<OffersApiWebApplicationFactory>
    {
        private readonly HttpClient httpClient;
        private readonly string _baseRequestUrl;
        private readonly JsonSerializerOptions _jsonSerializerOption = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        private readonly OffersApiWebApplicationFactory _factory;

        public UserControllerTests(OffersApiWebApplicationFactory factory)
        {
            httpClient = factory.CreateClient();
            _baseRequestUrl = "api/v1/User";
            _factory = factory;
        }

        [Fact]
        public async Task GetOffersByCategories_ShouldReturnFilteredOffers()
        {
            var category = new CategoryDTO
            {
                Name = $"TestCategory_{Guid.NewGuid()}",
                Description = "Test description"
            };

            var categoryJson = JsonSerializer.Serialize(category);
            var categoryContent = new StringContent(categoryJson, Encoding.UTF8, "application/json");

            var postCategoryResponse = await httpClient.PostAsync("api/v1/Admin/Category", categoryContent);
            postCategoryResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var categoryBody = await postCategoryResponse.Content.ReadAsStringAsync();
            var createdCategory = JsonSerializer.Deserialize<CategoryResponseModel>(categoryBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var loginResponse = await TestHelper.LogInAsCompany(httpClient, serviceProvider);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var form = new MultipartFormDataContent
            {
                { new StringContent("Filtered Offer"), "Name" },
                { new StringContent("Offer for category filtering test"), "Description" },
                { new StringContent("2"), "Count" },
                { new StringContent("49.99"), "Price" },
                { new StringContent(createdCategory.Id.ToString()), "CategoryId" },
                { new StringContent(DateTime.UtcNow.AddDays(7).ToString("o")), "ArchiveAt" }
            };

            var offerResponse = await httpClient.PostAsync("api/v1/Company/offers", form);
            offerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var offerBody = await offerResponse.Content.ReadAsStringAsync();
            var createdOffer = JsonSerializer.Deserialize<OfferResponseModel>(offerBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            httpClient.DefaultRequestHeaders.Authorization = null;

            var getFiltered = await httpClient.GetAsync($"{_baseRequestUrl}/offers?categoryIds={createdCategory.Id}&pageNumber=1&pageSize=10");
            getFiltered.StatusCode.Should().Be(HttpStatusCode.OK);

            var filteredBody = await getFiltered.Content.ReadAsStringAsync();
            var filteredOffers = JsonSerializer.Deserialize<List<OfferResponseModel>>(filteredBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            using (new AssertionScope())
            {
                filteredOffers.Should().NotBeNull();
                filteredOffers!.Should().ContainSingle(o => o.Id == createdOffer.Id);
                filteredOffers[0].Name.Should().Be("Filtered Offer");
                filteredOffers[0].CategoryId.Should().Be(createdCategory.Id);
            }
        }

        [Fact]
        public async Task CreateTransaction_ShouldReturnCreatedAtAction()
        {
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var loginResponse = await TestHelper.LogInAsCompany(httpClient, serviceProvider);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var form = new MultipartFormDataContent
            {
                { new StringContent("Filtered Offer"), "Name" },
                { new StringContent("Offer for category filtering test"), "Description" },
                { new StringContent("2"), "Count" },
                { new StringContent("50.00"), "Price" },
                { new StringContent("1"), "CategoryId" },
                { new StringContent(DateTime.UtcNow.AddDays(7).ToString("o")), "ArchiveAt" }
            };

            var offerResponse = await httpClient.PostAsync("api/v1/Company/offers", form);
            offerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var offerBody = await offerResponse.Content.ReadAsStringAsync();
            var createdOffer = JsonSerializer.Deserialize<OfferResponseModel>(offerBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            var loginResponseUser = await TestHelper.LogInAsUserAsync(httpClient, serviceProvider);

            var Transaction = new TransactionDTO
            {
                Count = 1,
                Paid = 50,
                OfferId = createdOffer.Id
            };

            var transactionJson = JsonSerializer.Serialize(Transaction);
            var transactionContent = new StringContent(transactionJson, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResponseUser.Token);
            var transactionResponse = await httpClient.PostAsync($"{_baseRequestUrl}/transaction", transactionContent);
            var transactionBody = await transactionResponse.Content.ReadAsStringAsync();
            var transaction = JsonSerializer.Deserialize<TransactionResponseModel>(transactionBody, _jsonSerializerOption);

            using (new AssertionScope())
            {
                transaction.AccountId.Should().Be(loginResponseUser.Id);
                transaction.OfferId.Should().Be(createdOffer.Id);
            }
        }

        [Fact]
        public async Task Deposit_ShouldDeposit()
        {
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var userLoginResponse = await TestHelper.LogInAsUserAsync(httpClient, serviceProvider);
            var deposit = new DepositRequestDTO
            {
                Amount = 2000
            };
            var depositJson = JsonSerializer.Serialize(deposit);
            var depositContent = new StringContent(depositJson, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", userLoginResponse.Token);
            var depositResponse = await httpClient.PatchAsync("api/v1/User/deposit", depositContent);
            depositResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", userLoginResponse.Token);
            var userResponse = await httpClient.GetAsync($"{_baseRequestUrl}");
            var userBody = await userResponse.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserResponseModel>(userBody, _jsonSerializerOption);
            
            using (new AssertionScope())
            {
                user.Balance.Should().BeGreaterThan(2000);
            }
        }

        [Fact]
        public async Task GetCurrentUser_ShouldReturnCurrentUser()
        {
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var userLoginResponse = await TestHelper.LogInAsUserAsync(httpClient, serviceProvider);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", userLoginResponse.Token);
            var userResponse = await httpClient.GetAsync($"{_baseRequestUrl}");
            var userBody = await userResponse.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserResponseModel>(userBody, _jsonSerializerOption);

            using (new AssertionScope())
            {
                user.Id.Should().Be(userLoginResponse.Id);
                user.Email.Should().Be(userLoginResponse.Email);
            }
        }

        [Fact]
        public async Task GetMyTransaction_ShouldReturnMyTransaction()
        {
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var loginResponse = await TestHelper.LogInAsCompany(httpClient, serviceProvider);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var form = new MultipartFormDataContent
            {
                { new StringContent("Filtered Offer"), "Name" },
                { new StringContent("Offer for category filtering test"), "Description" },
                { new StringContent("2"), "Count" },
                { new StringContent("50.00"), "Price" },
                { new StringContent("1"), "CategoryId" },
                { new StringContent(DateTime.UtcNow.AddDays(7).ToString("o")), "ArchiveAt" }
            };

            var offerResponse = await httpClient.PostAsync("api/v1/Company/offers", form);
            offerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var offerBody = await offerResponse.Content.ReadAsStringAsync();
            var createdOffer = JsonSerializer.Deserialize<OfferResponseModel>(offerBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            var loginResponseUser = await TestHelper.LogInAsUserAsync(httpClient, serviceProvider);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResponseUser.Token);

            var Transaction = new TransactionDTO
            {
                Count = 1,
                Paid = 50,
                OfferId = createdOffer.Id
            };

            var transactionJson = JsonSerializer.Serialize(Transaction);
            var transactionContent = new StringContent(transactionJson, Encoding.UTF8, "application/json");
            var transactionResponse = await httpClient.PostAsync($"{_baseRequestUrl}/transaction", transactionContent);
            var transactionBody = await transactionResponse.Content.ReadAsStringAsync();
            var transaction = JsonSerializer.Deserialize<TransactionResponseModel>(transactionBody, _jsonSerializerOption);

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResponseUser.Token);
            var myTransactionResponse = await httpClient.GetAsync($"{_baseRequestUrl}/transactions/{transaction.Id}");
            var myTransactionBody = await myTransactionResponse.Content.ReadAsStringAsync();
            var myTransaction = JsonSerializer.Deserialize<TransactionResponseModel>(myTransactionBody, _jsonSerializerOption);

            using (new AssertionScope())
            {
                myTransaction.Id.Should().Be(transaction.Id);
                myTransaction.AccountId.Should().Be(loginResponseUser.Id);
                myTransaction.OfferId.Should().Be(createdOffer.Id);
            }
        }

        [Fact]
        public async Task GetMyTransactions_ShouldReturnMyTransactions()
        {
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var loginResponseUser = await TestHelper.LogInAsUserAsync(httpClient, serviceProvider);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResponseUser.Token);
            var myTransactionsResponse = await httpClient.GetAsync($"{_baseRequestUrl}/transactions/");
            var myTransactionsBody = await myTransactionsResponse.Content.ReadAsStringAsync();
            var myTransactions = JsonSerializer.Deserialize<List<TransactionResponseModel>>(myTransactionsBody, _jsonSerializerOption);

            using (new AssertionScope())
            {
                myTransactions.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task RefundTransaction_ShouldRefundTheTransaction()
        {
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var loginResponse = await TestHelper.LogInAsCompany(httpClient, serviceProvider);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var form = new MultipartFormDataContent
            {
                { new StringContent("Filtered Offer"), "Name" },
                { new StringContent("Offer for category filtering test"), "Description" },
                { new StringContent("2"), "Count" },
                { new StringContent("50.00"), "Price" },
                { new StringContent("1"), "CategoryId" },
                { new StringContent(DateTime.UtcNow.AddDays(7).ToString("o")), "ArchiveAt" }
            };

            var offerResponse = await httpClient.PostAsync("api/v1/Company/offers", form);
            offerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var offerBody = await offerResponse.Content.ReadAsStringAsync();
            var createdOffer = JsonSerializer.Deserialize<OfferResponseModel>(offerBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            var loginResponseUser = await TestHelper.LogInAsUserAsync(httpClient, serviceProvider);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResponseUser.Token);

            var Transaction = new TransactionDTO
            {
                Count = 1,
                Paid = 50,
                OfferId = createdOffer.Id
            };

            var transactionJson = JsonSerializer.Serialize(Transaction);
            var transactionContent = new StringContent(transactionJson, Encoding.UTF8, "application/json");
            var transactionResponse = await httpClient.PostAsync($"{_baseRequestUrl}/transaction", transactionContent);
            var transactionBody = await transactionResponse.Content.ReadAsStringAsync();
            var transaction = JsonSerializer.Deserialize<TransactionResponseModel>(transactionBody, _jsonSerializerOption);

            var transactionRefundResponse = await httpClient.DeleteAsync($"{_baseRequestUrl}/transactions/{transaction.Id}");
            transactionRefundResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var transactionRefundGetResponse = await httpClient.GetAsync($"{_baseRequestUrl}/transactions/{transaction.Id}");
            transactionRefundGetResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
