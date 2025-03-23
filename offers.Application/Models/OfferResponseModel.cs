using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Models
{
    public class OfferResponseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
        public Category Category { get; set; }
    }
}
