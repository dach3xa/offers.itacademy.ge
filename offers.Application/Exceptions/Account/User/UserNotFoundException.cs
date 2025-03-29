using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions.Account.User
{
    public class UserNotFoundException : Exception
    {
        public List<string> Errors { get; }
        public UserNotFoundException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
