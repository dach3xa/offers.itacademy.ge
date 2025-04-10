using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Models.Response
{
    public class OfferResponseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PhotoURL { get; set; }
        public bool IsArchived { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
        public DateTime ArchiveAt { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}
