using offers.Application.Models.Response;
using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Web.Models
{
    public class OfferSearchViewModel
    {
        public List<CategoryResponseModel> Categories;
        public List<OfferResponseModel> Offers;
        public List<int> SelectedCategoryIds;
    }
}
