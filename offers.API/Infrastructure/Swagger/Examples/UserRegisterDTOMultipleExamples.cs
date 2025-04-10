using offers.Application.Models.DTO;
using offers.Application.Models.Response;
using Swashbuckle.AspNetCore.Filters;

namespace offers.API.Infrastructure.Swagger.Examples
{
    public class UserRegisterDTOMultipleExamples : IMultipleExamplesProvider<UserRegisterDTO>
    {
        public IEnumerable<SwaggerExample<UserRegisterDTO>> GetExamples()
        {
            yield return SwaggerExample.Create("Basic User", new UserRegisterDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "599889988",
                Password = "P@ssw0rd123"
            });

            yield return SwaggerExample.Create("User with complex password", new UserRegisterDTO
            {
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice.smith@example.com",
                PhoneNumber = "599889988",
                Password = "C0mpl3x!Pass2024"
            });

            yield return SwaggerExample.Create("Test User", new UserRegisterDTO
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test.user@sample.org",
                PhoneNumber = "599889988",
                Password = "Test123!"
            });

            yield return SwaggerExample.Create("Minimal Valid User", new UserRegisterDTO
            {
                FirstName = "Liam",
                LastName = "Nguyen",
                Email = "liam.nguyen@mail.com",
                PhoneNumber = "599889988",
                Password = "Password123."
            });
        }
    }
}
