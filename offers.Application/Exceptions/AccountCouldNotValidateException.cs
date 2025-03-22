using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions
{
    public class AccountCouldNotValidateException : Exception
    {
        public List<string> Errors { get; }
        public AccountCouldNotValidateException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
