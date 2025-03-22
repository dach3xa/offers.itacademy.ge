using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Models
{
    public class AccountResponseModel
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }   
    }
}
