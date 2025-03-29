using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Models
{
    public class TransactionResponseModel
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public decimal Paid { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public int OfferId { get; set; }
        public string OfferName { get; set; }
    }
}
