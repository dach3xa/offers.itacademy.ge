using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Domain.Models
{
    public class Offer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int AccountId { get; set; }
        public Account Account { get; set; }
        public DateTime ArchiveAt { get; set; }
        public bool IsArchived { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
