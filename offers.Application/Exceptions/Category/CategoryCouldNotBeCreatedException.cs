using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions.Category
{
    public class CategoryCouldNotBeCreatedException : Exception
    {
        public List<string> Errors { get; }
        public CategoryCouldNotBeCreatedException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
