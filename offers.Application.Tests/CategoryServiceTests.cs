using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using offers.Application.Exceptions;
using offers.Application.Exceptions.Account.Company;
using offers.Application.Exceptions.Category;
using offers.Application.RepositoryInterfaces;
using offers.Application.Services.Accounts;
using offers.Application.Services.Categories;
using offers.Application.UOF;
using offers.Domain.Enums;
using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Tests
{
    public class CategoryServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<ICategoryRepository> _repository;

        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _repository = new Mock<ICategoryRepository>();
            _unitOfWork = new Mock<IUnitOfWork>();
            _categoryService = new CategoryService(_repository.Object, _unitOfWork.Object);
        }

        [Fact(DisplayName = "when there is already a category with the same name create should throw category already exists excpetion")]
        public async Task CreateCategory_WhenCategoryNameExists_ShouldThrowCategoryAlreadyExistsException()
        {
            var category = new Category
            {
                Name = "someCategory",
                Description = "blablabla"
            };
            _repository
                .Setup(x => x.ExistsAsync(category.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var task =  () => _categoryService.CreateAsync(category, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CategoryAlreadyExistsException>(task);
            Assert.Equal("A category with this name already exists", exception.Message);
        }

        [Fact(DisplayName = "when there is no category with the same name create should create a new category")]
        public async Task CreateCategory_WhenCategoryNameDoesNotExist_ShouldReturnCategory()
        {
            var category = new Category
            {
                Name = "someCategory",
                Description = "blablabla"
            };
            _repository
                .Setup(x => x.ExistsAsync(category.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var categoryResponse = await _categoryService.CreateAsync(category, CancellationToken.None);

            using (new AssertionScope())
            {
                categoryResponse.Should().NotBeNull();
                categoryResponse.Name.Should().Be(category.Name);
            }
        }

        [Theory(DisplayName = "when id does not exist get category should throw category not found excpetion")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetCategory_WhenIdDoesNotExist_ShouldThrowCategoryNotFoundException(int id)
        {
            _repository
                .Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category)null);

            var task = () => _categoryService.GetAsync(id, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<CategoryNotFoundException>(task);
            Assert.Equal($"category with the id {id} was not found", exception.Message);
        }

        [Theory(DisplayName = "when id exists, get category should get the Category")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetCategory_WhenIdExists_ShouldReturnCategory(int id)
        {
            _repository
                .Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Category
                {
                    Id = id,
                    Name = "someCategory",
                    Description = "blablabla"
                });

            var category = await _categoryService.GetAsync(id, CancellationToken.None);

            using (new AssertionScope())
            {
                category.Should().NotBeNull();
                category.Id.Should().Be(id);
            }
        }
    }
}
