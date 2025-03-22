using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions
{
    public class AccountAlreadyExistsException : Exception
    {
        public List<string> Errors { get; }
        public AccountAlreadyExistsException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
