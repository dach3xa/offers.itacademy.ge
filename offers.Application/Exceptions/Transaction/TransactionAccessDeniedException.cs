using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions.Transaction
{
    public class TransactionAccessDeniedException : Exception
    {
        public List<string> Errors { get; }
        public TransactionAccessDeniedException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
