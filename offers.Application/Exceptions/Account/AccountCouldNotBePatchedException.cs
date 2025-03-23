using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions.Account
{
    public class AccountCouldNotBePatchedException : Exception
    {
        public List<string> Errors { get; }
        public AccountCouldNotBePatchedException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
