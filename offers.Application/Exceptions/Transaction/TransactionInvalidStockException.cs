using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions.Transaction
{
    public class TransactionInvalidStockException : Exception
    {
        public List<string> Errors { get; }
        public TransactionInvalidStockException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
