using offers.API.Models;
using Swashbuckle.AspNetCore.Filters;

namespace offers.API.Infrastructure.Swagger.Examples
{

    public class OfferDTOMultipleExamples : IMultipleExamplesProvider<OfferDTO>
    {
        public IEnumerable<SwaggerExample<OfferDTO>> GetExamples()
        {
            yield return SwaggerExample.Create(
            "Bakery Clearance",
            new OfferDTO
            {
                Name = "End-of-Day Croissant Pack",
                Description = "Fresh croissants left over from the day, heavily discounted.",
                Count = 5,
                Price = 3.99m,
                CategoryId = 1,
                ArchiveAt = DateTime.UtcNow.AddHours(2),
                PhotoURL = "https://example.com/images/croissants.jpg"
            });

            yield return SwaggerExample.Create(
                "Sushi Leftovers",
                new OfferDTO
                {
                    Name = "Evening Sushi Combo",
                    Description = "Mixed sushi rolls that didn’t sell today. Safe to eat and delicious.",
                    Count = 2,
                    Price = 7.50m,
                    CategoryId = 2,
                    ArchiveAt = DateTime.UtcNow.AddHours(1),
                    PhotoURL = "https://example.com/images/sushi-combo.jpg"
                });

            yield return SwaggerExample.Create(
                "Fruit Clearance Box",
                new OfferDTO
                {
                    Name = "Mixed Fruits (Slightly Ripe)",
                    Description = "Box of assorted ripe fruits nearing expiration — great for smoothies.",
                    Count = 1,
                    Price = 4.25m,
                    CategoryId = 3,
                    ArchiveAt = DateTime.UtcNow.AddHours(3),
                    PhotoURL = "https://example.com/images/fruit-box.jpg"
                });
        }
    }
}
