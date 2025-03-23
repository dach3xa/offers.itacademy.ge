using System.ComponentModel.DataAnnotations;

namespace offers.API.Models
{
    public class CategoryDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be 2–50 characters")]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage = "Description can't be more than 200 characters")]
        public string? Description { get; set; }
    }
}
