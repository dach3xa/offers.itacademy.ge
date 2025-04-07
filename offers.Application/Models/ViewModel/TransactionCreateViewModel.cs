using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Models.ViewModel
{
    public class TransactionCreateViewModel
    {
        [Required(ErrorMessage = "count is required")]
        [Range(1, int.MaxValue, ErrorMessage = "The value must be greater than 0.")]
        public int Count { get; set; }
    }
}
