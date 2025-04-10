using offers.Application.Models.DTO;
using offers.Application.Models.Response;
using Swashbuckle.AspNetCore.Filters;

namespace offers.API.Infrastructure.Swagger.Examples
{
    public class AccountLoginDTOMultipleExamples : IMultipleExamplesProvider<AccountLoginDTO>
    {
        public IEnumerable<SwaggerExample<AccountLoginDTO>> GetExamples()
        {
            yield return SwaggerExample.Create("Admin Login", new AccountLoginDTO
            {
                Email = "randomuser@example.com",
                Password = "Dachidachi1."
            });

            yield return SwaggerExample.Create("User - John Doe", new AccountLoginDTO
            {
                Email = "john.doe@example.com",
                Password = "P@ssw0rd123"
            });

            yield return SwaggerExample.Create("User - Alice Smith", new AccountLoginDTO
            {
                Email = "alice.smith@example.com",
                Password = "C0mpl3x!Pass2024"
            });

            yield return SwaggerExample.Create("User - Test User", new AccountLoginDTO
            {
                Email = "test.user@sample.org",
                Password = "Test123!"
            });

            yield return SwaggerExample.Create("User - Liam Nguyen", new AccountLoginDTO
            {
                Email = "liam.nguyen@mail.com",
                Password = "Password123"
            });

            yield return SwaggerExample.Create("Company - InnoTech", new AccountLoginDTO
            {
                Email = "contact@innotech.com",
                Password = "Innovate@2024"
            });

            yield return SwaggerExample.Create("Company - BrandWise", new AccountLoginDTO
            {
                Email = "info@brandwise.io",
                Password = "Branding123!"
            });
        }
    }
}
