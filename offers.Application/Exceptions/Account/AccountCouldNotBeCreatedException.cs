using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions.Account
{
    public class AccountCouldNotBeCreatedException : Exception
    {
        public List<string> Errors { get; }
        public AccountCouldNotBeCreatedException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
