﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Exceptions.Funds
{
    public class InsufficientFundsException : Exception
    {
        public List<string> Errors { get; }
        public InsufficientFundsException(string Message, List<string> errors = null)
            : base(Message)
        {
            Errors = errors;
        }
    }
}
