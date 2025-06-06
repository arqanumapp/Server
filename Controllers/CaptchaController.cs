using Microsoft.AspNetCore.Mvc;

namespace ArqanumServer.Controllers
{
    public class CaptchaController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
