using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions.Deposit
{
    public class DepositCouldNotValidateException : Exception
    {
        public List<string> Errors { get; }
        public DepositCouldNotValidateException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
