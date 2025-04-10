

using Microsoft.AspNetCore.Http;

namespace offers.Application.Models.DTO
{
    public class OfferDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public DateTime ArchiveAt { get; set; }
        public IFormFile Photo { get; set; }

    }
}
