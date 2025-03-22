using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Domain.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public UserDetail? UserDetail { get; set; }
        public CompanyDetail? CompanyDetail { get; set; }
    }
}
