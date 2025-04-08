using offers.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Models.Response
{
    public class CompanyResponseModel : AccountResponseModel
    {
        public string CompanyName { get; set; }
        public string PhotoURL { get; set; }
        public bool IsActive { get; set; }
    }
}
