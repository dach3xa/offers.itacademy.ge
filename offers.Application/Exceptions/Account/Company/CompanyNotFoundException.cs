using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions.Account.Company
{
    public class CompanyNotFoundException : Exception
    {
        public List<string> Errors { get; }
        public CompanyNotFoundException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
