using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions.Transaction
{
    public class TransactionInvalidAmountException : Exception
    {
        public List<string> Errors { get; }
        public TransactionInvalidAmountException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
