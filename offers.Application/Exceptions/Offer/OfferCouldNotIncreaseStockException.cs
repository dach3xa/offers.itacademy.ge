using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions.Offer
{
    public class OfferCouldNotIncreaseStockException : Exception
    {
        public List<string> Errors { get; }
        public OfferCouldNotIncreaseStockException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
