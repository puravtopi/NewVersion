using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MS.Services;

namespace PainTrax.Web.Controllers
{
    public class AppStatusController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ILogger<AppStatusController> _logger;
        private readonly AppStatusService _services = new AppStatusService();
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;

        public AppStatusController(IMapper mapper, ILogger<AppStatusController> logger, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            _mapper = mapper;
            _logger = logger;
            Environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
