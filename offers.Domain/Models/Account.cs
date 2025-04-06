
using Microsoft.AspNetCore.Identity;
using offers.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Domain.Models
{
    public class Account : IdentityUser<int>
    {
        public AccountRole Role { get; set; }
        public UserDetail? UserDetail { get; set; }
        public CompanyDetail? CompanyDetail { get; set; }
        public List<Offer> Offers { get; set; } 
    }
}
