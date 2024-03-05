using Microsoft.AspNetCore.Mvc;

namespace PainTrax.Web.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
