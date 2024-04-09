using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MS.Services;
using PainTrax.Web.Services;

namespace PainTrax.Web.Controllers
{
    public class AppProviderController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ILogger<AppProviderController> _logger;
        private readonly AppProviderRelService _services = new AppProviderRelService();
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;

        public AppProviderController(IMapper mapper, ILogger<AppProviderController> logger, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
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
