using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions.Offer
{
    public class OfferCouldNotBeDeletedException : Exception
    {
        public List<string> Errors { get; }
        public OfferCouldNotBeDeletedException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
