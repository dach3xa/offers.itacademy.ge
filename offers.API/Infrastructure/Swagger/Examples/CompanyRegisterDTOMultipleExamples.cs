using offers.Application.Models.DTO;
using offers.Application.Models.Response;
using Swashbuckle.AspNetCore.Filters;

namespace offers.API.Infrastructure.Swagger.Examples
{
    public class CompanyRegisterDTOMultipleExamples : IMultipleExamplesProvider<CompanyRegisterDTO>
    {
        public IEnumerable<SwaggerExample<CompanyRegisterDTO>> GetExamples()
        {
            yield return SwaggerExample.Create("Tech Startup", new CompanyRegisterDTO
            {
                CompanyName = "InnoTech",
                Email = "contact@innotech.com",
                PhoneNumber = "599889988",
                Password = "Innovate@2024",
            });

            yield return SwaggerExample.Create("Marketing Agency", new CompanyRegisterDTO
            {
                CompanyName = "BrandWise",
                Email = "info@brandwise.io",
                PhoneNumber = "599889988",
                Password = "Branding123!",
            });

            yield return SwaggerExample.Create("E-Commerce Platform", new CompanyRegisterDTO
            {
                CompanyName = "ShopifyClone",
                Email = "support@shopifyclone.net",
                PhoneNumber = "599889988",
                Password = "EcomSecure!9",
            });

            yield return SwaggerExample.Create("Consulting Firm", new CompanyRegisterDTO
            {
                CompanyName = "StratEdge",
                Email = "hello@stratedge.org",
                PhoneNumber = "599889988",
                Password = "Consult@2023",
            });
        }
    }
}
