﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Domain.Models
{
    public class CompanyDetail
    {
        public int AccountId { get; set; }
        public string CompanyName { get; set; }
        public bool IsActive { get; set; }
        public string PhotoURL { get; set; }
        public Account Account { get; set; }
    }
}
