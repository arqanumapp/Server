using ArqanumServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArqanumServer.Controllers
{
    public class CaptchaController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
