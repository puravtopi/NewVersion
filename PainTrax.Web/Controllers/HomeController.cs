﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly DashboardService _dashboardservices = new DashboardService();
        private readonly ISession _session = null;
        private readonly IHttpContextAccessor _httpContextAccessor = null;
        private readonly Common _commonservices = new Common();
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

            }
            catch(Exception ex)
            {
                SaveLog(ex,"Index");
            }
            return View();
        }

        public IActionResult Login()
        {
            var encrypt = EncryptionHelper.Encrypt("ram");
            var dcrypt = EncryptionHelper.Decrypt("u8cMaogbLx2JzhgJ0/kjugaNkaaYjvUT83FMJwS0VTs=");

            ViewBag.Success = true;
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginVM login)
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

                    var setting = _setting.GetOne(response.Model.cmp_id.Value);

                    if (setting != null)
                    {
                        HttpContext.Session.SetInt32(SessionKeys.SessionLocationId, setting.location);
                        HttpContext.Session.SetInt32(SessionKeys.SessionPageSize, setting.page_size);
                        HttpContext.Session.SetString(SessionKeys.SessionDateFormat, setting.dateformat);
                    }
                    else
                    {
                        HttpContext.Session.SetInt32(SessionKeys.SessionLocationId, 0);
                        HttpContext.Session.SetInt32(SessionKeys.SessionPageSize, 25);
                        HttpContext.Session.SetString(SessionKeys.SessionDateFormat, "MM/dd/yyyy");
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
            return View();
        }

        [HttpPost]
        public IActionResult GetProvider(GetProviderVM model)
        {
            HttpContext.Session.SetInt32(SessionKeys.SessionLocationId, model.locationid);
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
            foreach (var item in _httpContextAccessor.HttpContext.Request.Cookies.Keys)
            {
                _httpContextAccessor.HttpContext.Response.Cookies.Delete(item);
            }
            return RedirectToAction("Login", "Home");
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