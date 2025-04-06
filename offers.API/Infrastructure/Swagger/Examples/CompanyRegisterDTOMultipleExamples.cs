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
                PhotoURL = "https://images.unsplash.com/photo-1537432376769-00aabc3d3e6b"
            });

            yield return SwaggerExample.Create("Marketing Agency", new CompanyRegisterDTO
            {
                CompanyName = "BrandWise",
                Email = "info@brandwise.io",
                PhoneNumber = "599889988",
                Password = "Branding123!",
                PhotoURL = "https://images.unsplash.com/photo-1581091870622-7e0cdfbb6799"
            });

            yield return SwaggerExample.Create("E-Commerce Platform", new CompanyRegisterDTO
            {
                CompanyName = "ShopifyClone",
                Email = "support@shopifyclone.net",
                PhoneNumber = "599889988",
                Password = "EcomSecure!9",
                PhotoURL = "https://images.unsplash.com/photo-1515169067865-d6f7efb04b03"
            });

            yield return SwaggerExample.Create("Consulting Firm", new CompanyRegisterDTO
            {
                CompanyName = "StratEdge",
                Email = "hello@stratedge.org",
                PhoneNumber = "599889988",
                Password = "Consult@2023",
                PhotoURL = "https://images.unsplash.com/photo-1551836022-d5d88e9218df"
            });
        }
    }
}
