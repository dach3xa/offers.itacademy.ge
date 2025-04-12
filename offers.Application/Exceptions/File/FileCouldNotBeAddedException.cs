using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions.File
{
    public class FileCouldNotBeAddedException : Exception
    {
        public List<string> Errors { get; }
        public FileCouldNotBeAddedException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
