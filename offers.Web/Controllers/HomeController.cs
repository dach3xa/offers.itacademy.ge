using Microsoft.AspNetCore.Mvc;
using offers.Web.Models;
using System.Diagnostics;

namespace offers.Web.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }
}
