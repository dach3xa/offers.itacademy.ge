using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions.Offer
{
    public class OfferNotFoundException : Exception
    {
        public List<string> Errors { get; }
        public OfferNotFoundException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
