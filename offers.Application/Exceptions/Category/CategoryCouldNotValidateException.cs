using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions.Category
{
    public class CategoryCouldNotValidateException : Exception
    {
        public List<string> Errors { get; }
        public CategoryCouldNotValidateException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
