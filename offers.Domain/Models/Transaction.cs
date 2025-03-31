using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Domain.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public decimal Paid { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public UserDetail User { get; set; }
        public int OfferId { get; set; }
        public Offer Offer { get; set; }
    }
}
