using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace demo.Controllers
{
    public class HomeController : Controller
    {
        private static readonly Random Rnd = new Random();

        public HomeController()
        {
        }

        public IActionResult Index()
        {
            if (0 == Rnd.Next(0, 4)) throw new Exception("Exception here");
            return View();
        }
    }
}
