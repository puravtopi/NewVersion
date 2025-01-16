using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using PainTrax.Web.Helper;
using PainTrax.Web.Models;
using PainTrax.Web.Services;
using PainTrax.Web.ViewModel;
using System.Diagnostics;

namespace PainTrax.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IMapper _mapper;
        private readonly LoginServices _services = new LoginServices();
        private readonly GroupsService _groupservices = new GroupsService();
        private readonly DashboardService _dashboardservices = new DashboardService();
        private readonly ISession _session = null;
        private readonly IHttpContextAccessor _httpContextAccessor = null;
        private readonly Common _commonservices = new Common();
        private readonly UserService _userService = new UserService();
        private readonly SettingsService _setting = new SettingsService();


        public HomeController(ILogger<HomeController> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _session = _httpContextAccessor.HttpContext.Session;
        }

        public IActionResult Index()
        {

         

            //TblCompany tbl = new TblCompany()
            //{
            //    Address = "123",
            //    CreatedBy = 1,
            //    CreatedDate = DateTime.Now,
            //    Email = "purav.topi@gmail.com",
            //    Id = 1,
            //    IsActive = true,
            //    Name = "Purav",
            //    Phone = "789"

            //};

            //var t = _mapper.Map<CompanyVM>(tbl);

            //var respose = _service.Save(t);
            try
            {
                int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);

                ViewBag.TotalPatient = _dashboardservices.GetTotalPatient(cmpid.Value, 4);
                ViewBag.TodaysPatient = _dashboardservices.GetTotalPatient(cmpid.Value, 1);
                ViewBag.TodaysAttorny = _dashboardservices.GetTotalAttorny(cmpid.Value);
                ViewBag.TodaysIncCo = _dashboardservices.GetTotalInsuranceCompany(cmpid.Value);
                ViewBag.TotalClaimNo = _dashboardservices.GetTotalClaimNo(cmpid.Value);

            }
            catch (Exception ex)
            {
                SaveLog(ex, "Index");
            }
            return View();
        }

        public IActionResult Login()
        {

         
            if (Request.Cookies["LoginCookie"] != null)
            {
                var savedLogin = JsonConvert.DeserializeObject<LoginVM>(Request.Cookies["LoginCookie"]);

                // Pre-populate login fields with saved credentials
                return View(savedLogin);
            }
            ViewBag.Success = true;
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginVM login, bool remember)
        {
            try
            {

                var response = _services.CompanyLogin(login);

                if (response.Success)
                {

                    HttpContext.Session.SetInt32(SessionKeys.SessionCmpId, response.Model.cmp_id.Value);
                    HttpContext.Session.SetInt32(SessionKeys.SessionCmpUserId, response.Model.Id.Value);
                    HttpContext.Session.SetString(SessionKeys.SessionCmpEmail, response.Model.emailid);
                    HttpContext.Session.SetString(SessionKeys.SessionUserName, response.Model.uname);
                    HttpContext.Session.SetString(SessionKeys.SessionDesignation, response.Model.desig_name);
                    HttpContext.Session.SetInt32(SessionKeys.SessionUserId, response.Model.Id.Value);

                    var setting = _setting.GetOne(response.Model.cmp_id.Value);

                    if (setting != null)
                    {
                        HttpContext.Session.SetInt32(SessionKeys.SessionLocationId, setting.location);
                        HttpContext.Session.SetInt32(SessionKeys.SessionPageSize, setting.page_size);
                        HttpContext.Session.SetString(SessionKeys.SessionDateFormat, setting.dateformat);
                        HttpContext.Session.SetString(SessionKeys.SessionPageBreak, setting.pageBreakForInjection.ToString().ToLower());
                        HttpContext.Session.SetString(SessionKeys.SessionIsDaignosis, setting.isdaignosisshow.ToString().ToLower());
                        HttpContext.Session.SetString(SessionKeys.SessionDaignosisFoundStatment, setting.foundStatment == null ? "" : setting.foundStatment);
                        HttpContext.Session.SetString(SessionKeys.SessionDaignosisNotFoundStatment, setting.notfoundStatment == null ? "" : setting.notfoundStatment);
                        HttpContext.Session.SetString(SessionKeys.SessionInjectionAsSeparateBlock, setting.injectionAsSeparateBlock.ToString().ToLower());


                    }
                    else
                    {
                        HttpContext.Session.SetInt32(SessionKeys.SessionLocationId, 0);
                        HttpContext.Session.SetInt32(SessionKeys.SessionPageSize, 25);
                        HttpContext.Session.SetString(SessionKeys.SessionDateFormat, "MM/dd/yyyy");
                        HttpContext.Session.SetString(SessionKeys.SessionPageBreak, "false");
                        HttpContext.Session.SetString(SessionKeys.SessionIsDaignosis, "false");
                    }


                    //get menu access from assign group

                    tbl_groups objgroup = new tbl_groups() { Id = response.Model.groupid };
                    var groupDetails = _groupservices.GetOne(objgroup);

                    if (groupDetails != null)
                    {
                        HttpContext.Session.SetString(SessionKeys.SessionPagesAccess, groupDetails.pages_name);
                        HttpContext.Session.SetString(SessionKeys.SessionRoleAccess, groupDetails.role_name);
                        HttpContext.Session.SetString(SessionKeys.SessionFormsAccess, groupDetails.form_name==null?"": groupDetails.form_name);
                    }


                    // Check if "Remember Me" checkbox is checked
                    if (remember)
                    {
                        // Set a cookie with user's login information
                        var options = new CookieOptions
                        {
                            Expires = DateTime.Now.AddDays(7) // Cookie expires in 7 days
                        };
                        Response.Cookies.Append("LoginCookie", JsonConvert.SerializeObject(login), options);
                    }

                    return RedirectToAction("GetProvider");
                }
                else
                {
                    ViewBag.Success = false;
                }
            }
            catch (Exception ex)
            {
                SaveLog(ex, "Login");
            }
            return View(login);
        }

        public IActionResult AdminLogin()
        {
            ViewBag.Success = true;
            return View();
        }

        [HttpPost]
        public IActionResult AdminLogin(LoginVM login)
        {
            try
            {
                var response = _services.AdminLogin(login);

                if (response.Success)
                {
                    HttpContext.Session.SetInt32(SessionKeys.SessionCmpUserId, response.Model.id);
                    HttpContext.Session.SetString(SessionKeys.SessionUserName, response.Model.uname);
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    ViewBag.Success = false;
                }
            }
            catch (Exception ex)
            {
                SaveLog(ex, "AdminLogin");
            }
            return View(login);
        }

        public IActionResult AdminDashboard()
        {
            ViewBag.Success = true;
            return View();
        }

        public IActionResult GetProvider()
        {
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
            //ViewBag.locList
            var data = _commonservices.GetLocations(cmpid.Value);

            List<SelectListItem> lst = new List<SelectListItem>();

            int defaultlocation = HttpContext.Session.GetInt32(SessionKeys.SessionLocationId).Value;

            foreach (var item in data)
            {
                var obj = new SelectListItem()
                {
                    Text = item.Text,
                    Value = item.Value,
                    Selected = item.Value == defaultlocation.ToString() ? true : false
                };
                lst.Add(obj);

            }
            ViewBag.locList = lst;

            var providers = _userService.GetProviders(cmpid.Value);
            ViewBag.providerList = providers;
            return View();
        }

        [HttpPost]
        public IActionResult GetProvider(GetProviderVM model, [FromForm] string[] selectedProviders)
        {


            HttpContext.Session.SetInt32(SessionKeys.SessionLocationId, model.locationid == null ? 0 : model.locationid.Value);
            HttpContext.Session.SetInt32(SessionKeys.SessionSelectedProviderId, model.providerid == null ? 0 : model.providerid.Value);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Logout()
        {
            _session.Clear();
            /* foreach (var item in _httpContextAccessor.HttpContext.Request.Cookies.Keys)
             {
                 _httpContextAccessor.HttpContext.Response.Cookies.Delete(item);
             }*/
            return RedirectToAction("Login", "Home");
        }
        public IActionResult AdminLogout()
        {
            _session.Clear();
            /* foreach (var item in _httpContextAccessor.HttpContext.Request.Cookies.Keys)
             {
                 _httpContextAccessor.HttpContext.Response.Cookies.Delete(item);
             }*/
            return RedirectToAction("AdminLogin", "Home");
        }

        [HttpPost]
        public IActionResult TotalPatient(int type)
        {
            int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
            var cnt = _dashboardservices.GetTotalPatient(cmpid.Value, type);
            return Json(cnt);
        }

        public IActionResult SessionExpired()
        {
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }
        public IActionResult ResetPassword()
        {
            return View();
        }


        #region private Method
        private void SaveLog(Exception ex, string actionname)
        {
            var msg = "";
            if (ex.InnerException == null)
            {
                _logger.LogError(ex.Message);
                msg = ex.Message;
            }
            else
            {
                _logger.LogError(ex.InnerException.Message);
                msg = ex.InnerException.Message;
            }
            var logdata = new tbl_log
            {
                CreatedDate = DateTime.Now,
                CreatedBy = HttpContext.Session.GetInt32(SessionKeys.SessionCmpUserId),
                Message = msg,
            };
            new LogService().Insert(logdata);
        }
        #endregion 
    }
}