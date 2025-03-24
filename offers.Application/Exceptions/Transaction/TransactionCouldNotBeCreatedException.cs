using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions.Transaction
{
    public class TransactionCouldNotBeCreatedException : Exception
    {
        public List<string> Errors { get; }
        public TransactionCouldNotBeCreatedException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
