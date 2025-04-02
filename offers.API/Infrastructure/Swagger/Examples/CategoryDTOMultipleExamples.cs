using offers.API.Models;
using Swashbuckle.AspNetCore.Filters;

namespace offers.API.Infrastructure.Swagger.Examples
{
    public class CategoryDTOMultipleExamples : IMultipleExamplesProvider<CategoryDTO>
    {
        public IEnumerable<SwaggerExample<CategoryDTO>> GetExamples()
        {
            yield return SwaggerExample.Create("Electronics", new CategoryDTO
            {
                Name = "Electronics",
                Description = "Devices like phones, laptops, and TVs."
            });

            yield return SwaggerExample.Create("Books", new CategoryDTO
            {
                Name = "Books",
                Description = "All kinds of printed and digital reading material."
            });

            yield return SwaggerExample.Create("Clothing", new CategoryDTO
            {
                Name = "Clothing",
                Description = "Clothing for men, women, and kids."
            });

            yield return SwaggerExample.Create("Furniture", new CategoryDTO
            {
                Name = "Furniture",
                Description = "Home and office furniture items."
            });
        }
    }
}
