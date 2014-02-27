using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Core.DomainServices;

namespace UI.MVC4.Controllers.Web
{
    public class TestController : Controller
    {
        private readonly ICryptoService _cryptoService;

        public TestController(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }

        [Authorize(Roles = "GlobalAdmin")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Encrypt(string input)
        {
            if(!string.IsNullOrEmpty(input))
                ViewBag.Encryption = _cryptoService.Encrypt(input);
            return View();
        }

    }
}
