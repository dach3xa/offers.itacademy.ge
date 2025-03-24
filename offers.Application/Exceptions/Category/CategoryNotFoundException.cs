using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions.Category
{
    public class CategoryNotFoundException : Exception
    {
        public List<string> Errors { get; }
        public CategoryNotFoundException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
